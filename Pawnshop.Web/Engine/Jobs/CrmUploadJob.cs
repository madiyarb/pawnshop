using System;
using System.Collections.Generic;
using Pawnshop.Data.Access;
using Pawnshop.Web.Engine.MessageSenders;
using Pawnshop.Data.Models.Contracts;
using System.Dynamic;
using Pawnshop.Core.Exceptions;
using System.Text.RegularExpressions;
using System.Linq;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Core;
using Newtonsoft.Json;
using Hangfire;
using Pawnshop.Data.Models.Crm;
using Pawnshop.Core.Options;
using Microsoft.Extensions.Options;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Web.Engine.Services.Interfaces;
using System.Threading.Tasks;
using Pawnshop.Services.Crm;
using System.Net.Http;

namespace Pawnshop.Web.Engine.Jobs
{
    public class CrmUploadJob
    {
        private readonly CrmUploadContractRepository _crmUploadContractRepository;
        private readonly ContractRepository _contractRepository;
        private readonly GroupRepository _groupRepository;
        private readonly OrganizationRepository _organizationRepository;
        private readonly EmailSender _emailSender;
        private readonly ClientRepository _clientRepository;
        private readonly EventLog _eventLog;
        private readonly EnviromentAccessOptions _options;
        private readonly IVerificationService _verificationService;
        private readonly IClientContactService _clientContactService;
        private readonly AttractionChannelRepository _attractionChannelRepository;
        private readonly ContractActionRepository _contractActionRepository;

        private readonly OuterServiceSettingRepository _outerServiceSettings;
        private readonly ICrmUploadService _crmUploadService;
        private readonly JobLog _jobLog;
        private readonly CrmStatusesRepository _crmStatusesRepository;

        public CrmUploadJob(
            CrmUploadContractRepository crmUploadContractRepository,
            ContractRepository contractRepository,
            GroupRepository groupRepository,
            OrganizationRepository organizationRepository,
            EmailSender emailSender,
            ClientRepository clientRepository,
            EventLog eventLog,
            IOptions<EnviromentAccessOptions> options,
            IVerificationService verificationService,
            IClientContactService clientContactService,
            AttractionChannelRepository attractionChannelRepository,
            ContractActionRepository contractActionRepository,

            OuterServiceSettingRepository outerServiceSettings,
            ICrmUploadService crmUploadService,
            JobLog jobLog,
            CrmStatusesRepository crmStatusesRepository)
        {
            _crmUploadContractRepository = crmUploadContractRepository;
            _contractRepository = contractRepository;
            _groupRepository = groupRepository;
            _organizationRepository = organizationRepository;
            _emailSender = emailSender;
            _clientRepository = clientRepository;
            _eventLog = eventLog;
            _options = options.Value;
            _verificationService = verificationService;
            _clientContactService = clientContactService;
            _attractionChannelRepository = attractionChannelRepository;
            _contractActionRepository = contractActionRepository;

            _outerServiceSettings = outerServiceSettings;
            _crmUploadService = crmUploadService;
            _jobLog = jobLog;
            _crmStatusesRepository = crmStatusesRepository;
        }

        [DisableConcurrentExecution(10)]
        public async Task Execute()
        {
            if (!_options.CrmUpload) return;

            List<CrmUploadContract> contractsToUploadIds = _crmUploadContractRepository.Find();
            if (contractsToUploadIds == null) return;

            foreach (var contractToUploadId in contractsToUploadIds)
            {
                await CrmUpload(contractToUploadId);
            }
        }

        [Queue("crm")]
        private async Task CrmUpload(CrmUploadContract crmModel)
        {
            int contractId = crmModel.ContractId;
            var contract = _contractRepository.GetOnlyContract(crmModel.ContractId);
            if (contract.ContractClass == ContractClass.CreditLine)
            {
                contract = await _contractRepository.GetFirstTranche(contractId);
            }
            contract.Client = _clientRepository.Get(contract.ClientId);
            contract.Branch = _groupRepository.Get(contract.BranchId);

            var url = _outerServiceSettings.Find(new { Code = "CRM_DEAL_GET" }).URL;

            try
            {
                _jobLog.Log("CrmUploadJob", JobCode.Start, JobStatus.Success, EntityType.BitrixUpload);

                // Контакт/Клиент
                var crmContact = await FillContacts(contract.Client);

                var crmContracts = await FillContracts(crmModel, contract, crmContact);

                var contractToUpload = await FillContractToUpload(crmModel, contract, crmContact, crmContracts);

                #region Ищем договор/сделку, подходящую под условие или создаем новую
                // если не нашли сделок в CRM, создаем новый договор

                var crmDeal = await _crmUploadService.BitrixAPI(url, objectId: contractToUpload.Id);
                crmModel.ClientCrmId = crmDeal.result.CONTACT_ID == null ? 0 : (int)crmDeal.result.CONTACT_ID;
                crmModel.ContractCrmId = crmDeal.result.ID == null ? 0 : (int)crmDeal.result.ID;
                if (contractToUpload == null) contractToUpload = new CrmContract();
                contractToUpload.CategoryId = crmDeal.result.CATEGORY_ID == null ? 0 : (int)crmDeal.result.CATEGORY_ID;

                _clientRepository.UpdateCrmInfo(contract.ClientId, (int)crmModel.ClientCrmId); //Записываем В Clients Значение идентификатора контакта
                _contractRepository.UpdateCrmInfo(contract.Id, (int)crmModel.ContractCrmId); //Записываем В Contracts Значение идентификатора сделки
                crmModel.UploadDate = DateTime.Now;
                _crmUploadContractRepository.Update(crmModel);
                #endregion

                _jobLog.Log("CrmUploadJob", JobCode.End, JobStatus.Success, EntityType.BitrixUpload);
            }
            catch (Exception e)
            {
                _jobLog.Log("CrmUploadJob", JobCode.Error, JobStatus.Failed, EntityType.BitrixUpload, responseData: JsonConvert.SerializeObject(e), requestData: JsonConvert.SerializeObject(crmModel));
            }
        }

        private async Task<CrmContact> FillContacts(Client client)
        {
            var crmContact = new CrmContact();
            var contactId = await _crmUploadService.CreateOrUpdateContactInCrm(client);
            crmContact.Id = contactId;
            return crmContact;
        }

        private async Task<List<CrmContract>> FillContracts(CrmUploadContract crmModel, Contract contract, CrmContact crmContact)
        {
            var crmContracts = new List<CrmContract>();
            dynamic rooter = new ExpandoObject();

            if (crmModel.BitrixId != -999)
            {
                var result = await _crmUploadService.BitrixAPI(_outerServiceSettings.Find(new { Code = "CRM_DEAL_GET" }).URL, objectId: crmModel.BitrixId);
                if (result != null)
                {
                    DateTime signDate = DateTime.Now;
                    try
                    {
                        if (result.result.UF_CRM_1631676899 is DateTime)
                        {
                            signDate = result.result.UF_CRM_1631676899;
                            _eventLog.Log(EventCode.BitrixDataSuccess, EventStatus.Success, EntityType.Contract, contract.Id, string.Concat("crmModel:", JsonConvert.SerializeObject(crmModel)), string.Concat("SignDate is DateTime = ", JsonConvert.SerializeObject(result.result.UF_CRM_1631676899)));
                        }
                        else
                        {
                            if (!DateTime.TryParse(JsonConvert.SerializeObject(result.result.UF_CRM_1631676899), out signDate))
                            {
                                signDate = DateTime.Now;
                            }
                            _eventLog.Log(EventCode.BitrixDataSuccess, EventStatus.Success, EntityType.Contract, contract.Id, string.Concat("crmModel:", JsonConvert.SerializeObject(crmModel)), string.Concat("SignDate is String = ", JsonConvert.SerializeObject(result.result.UF_CRM_1631676899)));
                        }
                    }
                    catch (Exception e)
                    {
                        _eventLog.Log(EventCode.BitrixDataTypeError, EventStatus.Failed, EntityType.Contract, contract.Id, string.Concat("crmModel:", JsonConvert.SerializeObject(crmModel)), string.Concat("Exception Message = ", e.Message, ". StackTrace = ", e.StackTrace));
                        signDate = DateTime.Now;
                    }

                    var status = Regex.Replace((string)result.result.STAGE_ID, @"[C]\d*[:]", String.Empty);

                    crmContracts.Add(new CrmContract()
                    {
                        Id = result.result.ID == null ? 0 : (int)result.result.ID,
                        Title = result.result.TITLE,
                        StageId = result.result.STAGE_ID,
                        CategoryId = result.result.CATEGORY_ID == null ? 0 : (int)result.result.CATEGORY_ID,
                        ContractId = result.result.UF_CRM_1554465200529,
                        ContactId = result.result.CONTACT_ID == null ? 0 : (int)result.result.CONTACT_ID,
                        Opportunity = result.result.OPPORTUNITY == null ? 0 : (decimal)result.result.OPPORTUNITY,
                        SignDate = signDate,
                        AttractionChannel = result.result.UF_CRM_1602525272410,
                        Status = _crmStatusesRepository.FindStatus(new { CrmName = status, StatusTypeId = _crmStatusesRepository.FindStatusType(new { Code = "Contract" }).Id }),
                    });
                }
            }
            else if (crmModel.ContractCrmId == null)
            {

                rooter.filter = new ExpandoObject();
                rooter.filter.CONTACT_ID = crmContact.Id;
                rooter.filter.CATEGORY_ID = contract.Branch.BitrixCategoryId;
                rooter.filter.STAGE_ID = "C81:EXECUTING";

                var result = await _crmUploadService.BitrixAPI(_outerServiceSettings.Find(new { Code = "CRM_DEAL_LIST" }).URL, filter: rooter.filter);
                if (result.total >= 1)
                {
                    foreach (var deal in result.result)
                    {
                        if (String.IsNullOrEmpty((string)deal.OPPORTUNITY)) deal.OPPORTUNITY = 0;
                        if (String.IsNullOrEmpty((string)deal.UF_CRM_1602525272410)) deal.UF_CRM_1602525272410 = String.Empty;
                        DateTime signDate = DateTime.Now;
                        try
                        {
                            if (deal.UF_CRM_1631676899 is DateTime)
                            {
                                signDate = deal.UF_CRM_1631676899;
                                _eventLog.Log(EventCode.BitrixDataSuccess, EventStatus.Success, EntityType.Contract, contract.Id, string.Concat("crmModel:", JsonConvert.SerializeObject(crmModel)), string.Concat("SignDate is DateTime = ", JsonConvert.SerializeObject(deal.UF_CRM_1631676899)));
                            }
                            else
                            {
                                if (!DateTime.TryParse(JsonConvert.SerializeObject(deal.UF_CRM_1631676899), out signDate))
                                {
                                    signDate = DateTime.Now;
                                }
                                _eventLog.Log(EventCode.BitrixDataSuccess, EventStatus.Success, EntityType.Contract, contract.Id, string.Concat("crmModel:", JsonConvert.SerializeObject(crmModel)), string.Concat("SignDate is String = ", JsonConvert.SerializeObject(deal.UF_CRM_1631676899)));
                            }
                        }
                        catch (Exception e)
                        {
                            _eventLog.Log(EventCode.BitrixDataTypeError, EventStatus.Failed, EntityType.Contract, contract.Id, string.Concat("crmModel:", JsonConvert.SerializeObject(crmModel)), string.Concat("Exception Message = ", e.Message, ". StackTrace = ", e.StackTrace));
                            signDate = DateTime.Now;
                        }

                        var status = Regex.Replace((string)deal.STAGE_ID, @"[C]\d*[:]", String.Empty);

                        crmContracts.Add(new CrmContract()
                        {
                            Id = deal.ID == null ? 0 : (int)deal.ID,
                            Title = deal.TITLE,
                            StageId = deal.STAGE_ID,
                            CategoryId = deal.CATEGORY_ID == null ? 0 : (int)deal.CATEGORY_ID,
                            ContractId = deal.UF_CRM_1554465200529,
                            ContactId = deal.CONTACT_ID == null ? 0 : (int)deal.CONTACT_ID,
                            Opportunity = deal.OPPORTUNITY == null ? 0 : (decimal)deal.OPPORTUNITY,
                            SignDate = signDate,
                            AttractionChannel = deal.UF_CRM_1602525272410,
                            Status = _crmStatusesRepository.FindStatus(new { CrmName = status, StatusTypeId = _crmStatusesRepository.FindStatusType(new { Code = "Contract" }).Id })
                        });
                    }
                }
            }
            else
            {
                var result = await _crmUploadService.BitrixAPI(_outerServiceSettings.Find(new { Code = "CRM_DEAL_GET" }).URL, objectId: crmModel.ContractCrmId);
                if (!(result == null || result.result == null))
                {
                    DateTime signDate = DateTime.Now;
                    try
                    {
                        if (result.result.UF_CRM_1631676899 is DateTime)
                        {
                            signDate = result.result.UF_CRM_1631676899;
                            _eventLog.Log(EventCode.BitrixDataSuccess, EventStatus.Success, EntityType.Contract, contract.Id, string.Concat("crmModel:", JsonConvert.SerializeObject(crmModel)), string.Concat("SignDate is DateTime = ", JsonConvert.SerializeObject(result.result.UF_CRM_1631676899)));
                        }
                        else
                        {
                            if (!DateTime.TryParse(JsonConvert.SerializeObject(result.result.UF_CRM_1631676899), out signDate))
                            {
                                signDate = DateTime.Now;
                            };
                            _eventLog.Log(EventCode.BitrixDataSuccess, EventStatus.Success, EntityType.Contract, contract.Id, string.Concat("crmModel:", JsonConvert.SerializeObject(crmModel)), string.Concat("SignDate is String = ", JsonConvert.SerializeObject(result.result.UF_CRM_1631676899)));
                        }
                    }
                    catch (Exception e)
                    {
                        _eventLog.Log(EventCode.BitrixDataTypeError, EventStatus.Failed, EntityType.Contract, contract.Id, string.Concat("crmModel:", JsonConvert.SerializeObject(crmModel)), string.Concat("Exception Message = ", e.Message, ". StackTrace = ", e.StackTrace));
                        signDate = DateTime.Now;
                    }

                    var status = Regex.Replace((string)result.result.STAGE_ID, @"[C]\d*[:]", String.Empty);

                    crmContracts.Add(new CrmContract()
                    {
                        Id = int.TryParse(result.result.ID, out int resultId) ? resultId : 0,
                        Title = result.result.TITLE,
                        StageId = result.result.STAGE_ID,
                        CategoryId = int.TryParse(result.result.CATEGORY_ID, out int categoryId) ? categoryId : 0,
                        ContractId = result.result.UF_CRM_1554465200529,
                        ContactId = int.TryParse(result.result.CONTACT_ID, out int contactId) ? contactId : 0,
                        Opportunity = int.TryParse(result.result.OPPORTUNITY, out int opportunity) ? opportunity : 0,
                        SignDate = signDate,
                        AttractionChannel = result.result.UF_CRM_1602525272410,
                        Status = _crmStatusesRepository.FindStatus(new { CrmName = status, StatusTypeId = _crmStatusesRepository.FindStatusType(new { Code = "Contract" }).Id })
                    });
                }
            }

            return crmContracts;
        }

        private async Task<CrmContract> FillContractToUpload(CrmUploadContract crmModel, Contract contract, CrmContact crmContact, List<CrmContract> crmContracts)
        {
            var contractToUpload = new CrmContract();

            if (crmModel.BitrixId != -999)
            {
                // Находим сделку по BitrixId
                contractToUpload = crmContracts.FirstOrDefault();
                await _crmUploadService.UpdateCrmContract(contract, contractToUpload, crmModel.BitrixId);
                crmModel.ContractCrmId = contractToUpload.Id;
            }
            else if (crmContracts.Count == 0)
            {
                //создаем сделку
                crmModel.ClientCrmId = crmContact.Id;
                contractToUpload.Id = await _crmUploadService.CreateCrmContract(crmModel.ClientCrmId.Value, contract);
                crmModel.ContractCrmId = contractToUpload.Id;
            }
            else
            {
                //находим сделку по Title, CrmId или ContractId
                contractToUpload = crmContracts.Where(x =>
                        (x.Id == contract.CrmId) || //сверяем идентификатор в CRM и идентификатор, сохраненный у нас
                        (x.ContractId != null && (Int32.TryParse(x.ContractId, out var ContractId) ? ContractId : -1) == contract.Id) ||//сверяем выгруженный идентификатор договора с нашим
                        (x.ContractId != null && x.ContractId.Contains(contract.ContractNumber)) ||//сверяем выгруженный номер договора с нашим
                        (x.Title != null && x.Title.Contains(contract.ContractNumber)))//ищем номер договора в заголовке сделки 
                    .OrderByDescending(x => x.Id).FirstOrDefault();//самый новый

                //если договор уже выгружен в CRM, обновляем его
                if (contractToUpload != null && contractToUpload.Id > 0)
                {
                    //обновляем сделку
                    await _crmUploadService.UpdateCrmContract(contract, contractToUpload, crmModel.BitrixId);
                    crmModel.ContractCrmId = contractToUpload.Id;
                }
                else
                {
                    var wonStatusId = _crmStatusesRepository.FindStatus(new { crmName = "WON", StatusTypeId = _crmStatusesRepository.FindStatusType(new { Code = "Contract" }).Id }).BitrixId;
                    //ищем сделки в стадии меньше "Кредит оформлен"(сделка выиграна)
                    if (crmContracts.Where(x => x.Status.BitrixId == wonStatusId).Count() > 0)
                    {
                        if (crmContracts.Where(x => x.Status.BitrixId < wonStatusId)
                                .Count() >= 1)
                        {
                            //если несколько сделок со статусом меньше "Кредит оформлен"(сделка выиграна)
                            crmContracts = crmContracts
                                .Where(x => x.Status.BitrixId < wonStatusId).ToList();
                            //если есть сделка в нашем филиале, фильтруем
                            if (crmContracts.Where(x => x.CategoryId == contract.Branch.BitrixCategoryId).Count() >
                                0)
                            {
                                crmContracts = crmContracts
                                    .Where(x => x.CategoryId == contract.Branch.BitrixCategoryId).ToList();
                            }

                            //ищем самую новую сделку
                            if (contractToUpload == null) contractToUpload = new CrmContract();
                            contractToUpload = crmContracts.OrderByDescending(x => x.Id).FirstOrDefault();

                            //обновляем сделку
                            await _crmUploadService.UpdateCrmContract(contract, contractToUpload, crmModel.BitrixId);
                        }
                        else
                        {
                            //создаем сделку(если нет сделок со статусом меньше "Кредит оформлен")
                            if (contractToUpload == null) contractToUpload = new CrmContract();
                            contractToUpload.Id = await _crmUploadService.CreateCrmContract(crmContact.Id.Value, contract);
                        }
                    }
                    else
                    {
                        //создаем сделку
                        if (contractToUpload == null) contractToUpload = new CrmContract();
                        contractToUpload.Id = await _crmUploadService.CreateCrmContract(crmContact.Id.Value, contract);
                    }
                }
            }

            return contractToUpload;
        }
    }
}
