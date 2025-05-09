using System;
using System.Collections.Generic;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Core;
using Newtonsoft.Json;
using Pawnshop.Data.Models.Crm;
using Pawnshop.Core.Options;
using Microsoft.Extensions.Options;
using Hangfire;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Calculation;
using Pawnshop.AccountingCore.Models;
using System.Net.Http.Headers;
using System.Net.Http;
using Pawnshop.Services.Crm;
using System.Threading.Tasks;

namespace Pawnshop.Web.Engine.Jobs
{
    public class CrmUploadPaymentJob
    {
        private readonly CrmPaymentRepository _crmPaymentRepository;
        private readonly GroupRepository _groupRepository;
        private readonly EventLog _eventLog;
        private readonly ClientRepository _clientRepository;
        private readonly EnviromentAccessOptions _options;
        private readonly IContractActionRowBuilder _contractAmount;
        private readonly IContractService _contractService;
        private readonly ContractTransferRepository _contractTransferRepository;
        private readonly InscriptionRepository _inscriptionRepository;
        private readonly JobLog _jobLog;
        private readonly ICrmUploadService _crmUploadService;

        public CrmUploadPaymentJob(
            CrmPaymentRepository crmPaymentRepository,
            GroupRepository groupRepository,
            EventLog eventLog,
            ClientRepository clientRepository,
            IContractActionRowBuilder contractAmount,
            IOptions<EnviromentAccessOptions> options,
            IContractService contractService,
            ContractTransferRepository contractTransferRepository,
            InscriptionRepository inscriptionRepository,
            JobLog jobLog,

            ICrmUploadService crmUploadService
            )
        {
            _crmPaymentRepository = crmPaymentRepository;
            _groupRepository = groupRepository;
            _eventLog = eventLog;
            _clientRepository = clientRepository;
            _contractAmount = contractAmount;
            _options = options.Value;
            _contractService = contractService;
            _contractTransferRepository = contractTransferRepository;
            _inscriptionRepository = inscriptionRepository;
            _jobLog = jobLog;

            _crmUploadService = crmUploadService;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        public async Task Execute()
        {
            if (!_options.CrmPaymentUpload) return;
            try
            {
                _jobLog.Log("CrmUploadPaymentJob", JobCode.Start, JobStatus.Success, EntityType.BitrixUploadPayment);

                var tsoBranch = _groupRepository.Find(new { Name = "TSO" });
                List<CrmUploadPayment> payments = _crmPaymentRepository.Find();

                foreach (CrmUploadPayment payment in payments)
                {
                    payment.QueueDate = DateTime.Now;
                    _crmPaymentRepository.Update(payment);

                    await GenerateAndUpload(payment.Id);
                }

                _jobLog.Log("CrmUploadPaymentJob", JobCode.End, JobStatus.Success, EntityType.BitrixUploadPayment);
            }
            catch (Exception e)
            {
                _jobLog.Log("CrmUploadPaymentJob", JobCode.Error, JobStatus.Failed, EntityType.BitrixUploadPayment, responseData: JsonConvert.SerializeObject(e));
            }
        }

        [Queue("crm")]
        public async Task GenerateAndUpload(int id)
        {
            if (!_options.CrmPaymentUpload) return;

            var payment = _crmPaymentRepository.Get(id);
            if (payment == null)
                throw new ArgumentNullException(nameof(payment));

            try
            {
                var contract = GetContractDetails(payment.ContractId);
                _contractAmount.Init(contract, DateTime.Now);

                var loanCostLeft = _contractAmount.GetLoanCostLeft();
                var loanPercentCost = _contractAmount.BuyoutRow.PercentAmount;
                var penaltyPercentCost = _contractAmount.BuyoutRow.PenaltyAmount;
                var prepayment = _contractAmount.PrepaymentCost;
                var buyoutAmount = _contractAmount.BuyoutAmount;
                var prolongAmount = _contractAmount.DisplayAmountWithoutPrepayment;

                if (contract.CrmPaymentId.HasValue)
                {
                    bool isUpdated = await _crmUploadService.UpdateDeal(contract, loanCostLeft, loanPercentCost, penaltyPercentCost, prepayment, buyoutAmount, prolongAmount);
                    if (!isUpdated)
                        await _crmUploadService.CreateDeal(contract, loanCostLeft, loanPercentCost, penaltyPercentCost, prepayment, buyoutAmount, prolongAmount);
                }
                else
                {
                    await _crmUploadService.CreateDeal(contract, loanCostLeft, loanPercentCost, penaltyPercentCost, prepayment, buyoutAmount, prolongAmount);
                }

                payment.UploadDate = DateTime.Now;
                _crmPaymentRepository.Update(payment);
            }
            catch (Exception e)
            {
                _eventLog.Log(EventCode.BitrixUpload, EventStatus.Failed, EntityType.Contract, payment.ContractId,
                    responseData: e.Message + e.StackTrace);
            }
        }

        private Contract GetContractDetails(int contractId)
        {
            var contract = _contractService.GetOnlyContract(contractId);

            if (contract.InscriptionId > 0)
                contract.Inscription = _inscriptionRepository.GetInscriptionByContractId(contract.Id, contract.InscriptionId.Value);

            contract.Client = _clientRepository.Get(contract.ClientId);
            contract.Positions = _contractService.GetPositionsByContractId(contract.Id);
            contract.PaymentSchedule = _contractService.GetOnlyPaymentSchedule(contract.Id);
            contract.ContractTransfers = _contractTransferRepository.GetContactTransfersByContractId(contract.Id);
            contract.Branch = _groupRepository.Get(contract.BranchId);

            return contract;
        }
    }
}
