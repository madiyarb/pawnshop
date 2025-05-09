using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Pawnshop.Web.Engine.Audit;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Mintos;
using Pawnshop.Data.Models.Mintos.UploadModels;
using System.Dynamic;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Mintos.AnswerModels;
using System.Globalization;
using Hangfire;
using Pawnshop.Core.Options;
using Microsoft.Extensions.Options;
using Pawnshop.Web.Engine.Services;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Services.Contracts;

namespace Pawnshop.Web.Engine.Jobs
{
    public class MintosContractUploadJob
    {
        private readonly EventLog _eventLog;
        private readonly MintosUploadQueueRepository _queueRepository;
        private readonly IContractService _contractService;
        private readonly ContractRepository _contractRepository;
        private readonly ClientRepository _clientRepository;
        private readonly GroupRepository _groupRepository;
        private readonly OrganizationRepository _organizationRepository;
        private readonly MintosConfigRepository _mintosConfigRepository;
        private readonly CurrencyRepository _currencyRepository;
        private readonly MintosContractRepository _mintosContractRepository;
        private readonly JobLog _jobLog;
        private readonly EnviromentAccessOptions _options;
        private readonly MintosApi.MintosApi _mintosApi;
        private readonly IClientContactService _clientContactService;
        private readonly IVerificationService _verificationService;
        public MintosContractUploadJob(EventLog eventLog, MintosUploadQueueRepository queueRepository, IContractService contractService, ClientRepository clientRepository,
            GroupRepository groupRepository, OrganizationRepository organizationRepository, MintosConfigRepository mintosConfigRepository, CurrencyRepository currencyRepository,
            MintosContractRepository mintosContractRepository, IOptions<EnviromentAccessOptions> options, JobLog jobLog, IClientContactService clientContactService,
            MintosApi.MintosApi mintosApi, IVerificationService verificationService, ContractRepository contractRepository)
        {
            _eventLog = eventLog;
            _queueRepository = queueRepository;
            _clientRepository = clientRepository;
            _groupRepository = groupRepository;
            _organizationRepository = organizationRepository;
            _mintosConfigRepository = mintosConfigRepository;
            _currencyRepository = currencyRepository;
            _mintosContractRepository = mintosContractRepository;
            _options = options.Value;
            _jobLog = jobLog;
            _mintosApi = mintosApi;
            _clientContactService = clientContactService;
            _verificationService = verificationService;
            _contractService = contractService;
            _contractRepository = contractRepository;
        }

        /// <summary>
        /// Выгрузка договоров в Mintos
        /// </summary>
        [DisableConcurrentExecution(10 * 60)]
        public void Execute()
        {
            if (!_options.MintosUpload) return;
            var contractUploadQueue = _queueRepository.List(query: new { Status = MintosUploadStatus.Await });
            if (contractUploadQueue.Count == 0) return;

            foreach (var contractToUpload in contractUploadQueue)
            {
                try
                {
                    var contract = _contractService.Get(contractToUpload.ContractId);

                    if (_mintosContractRepository.CheckForBlackList(contract.Id))
                    {
                        _eventLog.Log(
                            EventCode.MintosContractUpload,
                            EventStatus.Failed,
                            EntityType.Contract,
                            contractToUpload.ContractId,
                            JsonConvert.SerializeObject(contractToUpload),
                            "Договор находится в черном списке для выгрузки в Mintos. Попробуйте позже.");
                        contractToUpload.Status = MintosUploadStatus.Canceled;
                        _queueRepository.Update(contractToUpload);
                        continue;
                    }

                    if (contract.Client.IsPolitician || contract.Client?.IsInBlackList == true || !contract.Client.LegalForm.IsIndividual || !contract.Client.IdentityNumberIsValid)
                    {
                        List<string> errors = new List<string>();
                        var client = contract.Client;
                        if (client.IsPolitician) errors.Add("Является PEP");
                        if ((client?.IsInBlackList == true)) errors.Add("В черном списке");
                        if (!client.LegalForm.IsIndividual) errors.Add("Не является физлицом");
                        if (!client.IdentityNumberIsValid) errors.Add("ИИН не валиден");
                        _eventLog.Log(
                            EventCode.MintosContractUpload,
                            EventStatus.Failed,
                            EntityType.Contract,
                            contractToUpload.ContractId,
                            JsonConvert.SerializeObject(contract.Client),
                            string.Concat("Клиент не удовлетворяет требованиям Mintos: ", string.Join(',', errors)));
                        contractToUpload.Status = MintosUploadStatus.Canceled;
                        _queueRepository.Update(contractToUpload);
                        continue;
                    }

                    var mintosContracts = _mintosContractRepository.GetByContractId(contract.Id);
                    if (mintosContracts.Count>0 && mintosContracts.Where(x => x.MintosStatus.Contains("active")).Count() > 0)
                    {
                        _eventLog.Log(
                            EventCode.MintosContractUpload,
                            EventStatus.Failed,
                            EntityType.Contract,
                            contractToUpload.ContractId,
                            JsonConvert.SerializeObject(contractToUpload),
                            "Договор уже выгружен в Mintos и активен. Попробуйте выкупить и затем повторно выгрузить договор.");
                        contractToUpload.Status = MintosUploadStatus.Canceled;
                        _queueRepository.Update(contractToUpload);
                        continue;
                    }


                    var parentContractId = contract.ParentId; //_contractRepository.GetParentContract(contract.Id);
                    Contract parentContract = null;
                    if (parentContractId.HasValue)
                    {
                        parentContract = _contractService.Get((int)parentContractId);
                        parentContract.Client = _clientRepository.Get(parentContract.ClientId);
                    }
                    var branch = _groupRepository.Get(contract.BranchId);

                    var uploadConfig = _mintosConfigRepository.Find(new
                    {
                        branch.OrganizationId,
                        contractToUpload.CurrencyId
                    });
                   

                    if (uploadConfig == null || !uploadConfig.NewUploadAllowed)
                    {
                        _eventLog.Log(
                            EventCode.MintosContractUpload,
                            EventStatus.Failed,
                            EntityType.Contract,
                            contractToUpload.ContractId,
                            JsonConvert.SerializeObject(contractToUpload),
                            "Настройка для выгрузки не найдена или отключена выгрузка новых договоров");
                        contractToUpload.Status = MintosUploadStatus.Canceled;
                        _queueRepository.Update(contractToUpload);
                        continue;
                    }

                    MintosContractUploadModel mintosContractModelToUpload = null;
                    string json = string.Empty;
                    try
                    {
                        ClientContact defaultContact = _verificationService.GetDefaultContact(contract.ClientId, false);
                        mintosContractModelToUpload = new MintosContractUploadModel(contract,
                            uploadConfig.InvestorInterestRate, uploadConfig.Currency.Code, uploadConfig.Currency.ExchangeRate, parentContract, defaultContact);
                        json = JsonConvert.SerializeObject(mintosContractModelToUpload);
                    }
                    catch (Exception e)
                    {
                        _eventLog.Log(
                            EventCode.MintosContractUpload,
                            EventStatus.Failed,
                            EntityType.Contract,
                            contractToUpload.ContractId,
                            JsonConvert.SerializeObject(contractToUpload),
                            e.Message);
                        contractToUpload.Status = MintosUploadStatus.Error;
                        _queueRepository.Update(contractToUpload);
                        continue;
                    }

                    string response = string.Empty;
                    try
                    {
                        response = _mintosApi.TryPost("loans", uploadConfig.ApiKey, mintosContractModelToUpload,EventCode.MintosContractUpload, EntityType.Contract, contractToUpload.ContractId);
                    }
                    catch (Exception e)
                    {
                        _eventLog.Log(
                            EventCode.MintosContractUpload,
                            EventStatus.Failed,
                            EntityType.Contract,
                            contractToUpload.ContractId,
                            JsonConvert.SerializeObject(contractToUpload),
                            e.Message);
                        contractToUpload.Status = MintosUploadStatus.Error;
                        _queueRepository.Update(contractToUpload);
                        continue;
                    }

                    var mintosAnswer = JsonConvert.DeserializeObject<AnswerContractModel>(response);
                    Currency contractCurrency = _currencyRepository.Find(new { IsDefault = true });

                    MintosContract mintosContract = mintosAnswer.ConvertToMintosContract(contractCurrency, contractToUpload, uploadConfig);

                    _mintosContractRepository.Insert(mintosContract);

                    _eventLog.Log(
                        EventCode.MintosContractUpload,
                        EventStatus.Success,
                        EntityType.Contract,
                        contract.Id,
                        json,
                        JsonConvert.SerializeObject(mintosContract));

                    contractToUpload.UploadDate = DateTime.Now;
                    contractToUpload.Status = MintosUploadStatus.Success;
                    contractToUpload.MintosContractId = mintosContract.Id;
                    _queueRepository.Update(contractToUpload);
                }
                catch (Exception e)
                {
                    _eventLog.Log(
                        EventCode.MintosContractUpload,
                        EventStatus.Failed,
                        EntityType.Contract,
                        contractToUpload.ContractId,
                        JsonConvert.SerializeObject(contractToUpload),
                        e.Message);
                }
                
            }

        }
    }
}
