using System;
using System.Collections.Generic;
using Pawnshop.Data.Access;
using System.Linq;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Core;
using Newtonsoft.Json;
using Pawnshop.Data.Models.Crm;
using Pawnshop.Core.Options;
using Microsoft.Extensions.Options;
using Pawnshop.Services.Contracts;
using Pawnshop.Core.Queries;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Services.CreditLines;
using Pawnshop.Services.Calculation;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Services.Models.Calculation;
using Pawnshop.Data.Models.Contracts;
using Serilog;

namespace Pawnshop.Web.Engine.Jobs
{
    public class CrmGeneratePaymentQueue
    {
        private readonly CrmPaymentRepository _crmPaymentRepository;
        private readonly EnviromentAccessOptions _options;
        private readonly IContractService _contractService;
        private readonly AccountRepository _accountRepository;
        private readonly ContractPaymentScheduleRepository _contractPaymentScheduleRepository;
        private readonly CollectionStatusRepository _collectionStatusRepository;
        private readonly JobLog _jobLog;
        private readonly ICreditLineService _creditLineService;
        private readonly IContractAmount _contractAmount;
        private readonly IContractDutyService _contractDutyService;
        private readonly ILogger _logger;
        private readonly IContractPaymentScheduleService _contractPaymentScheduleService;

        public CrmGeneratePaymentQueue(
            CrmPaymentRepository crmPaymentRepository,
            IOptions<EnviromentAccessOptions> options,
            IContractService contractService,
            AccountRepository accountRepository,
            ContractPaymentScheduleRepository contractPaymentScheduleRepository,
            CollectionStatusRepository collectionStatusRepository,
            JobLog jobLog,
            ICreditLineService creditLineService,
            IContractAmount contractAmount,
            IContractDutyService contractDutyService,
            ILogger logger,
            IContractPaymentScheduleService contractPaymentScheduleService)
        {
            _crmPaymentRepository = crmPaymentRepository;
            _options = options.Value;
            _contractService = contractService;
            _accountRepository = accountRepository;
            _contractPaymentScheduleRepository = contractPaymentScheduleRepository;
            _collectionStatusRepository = collectionStatusRepository;
            _jobLog = jobLog;
            _creditLineService = creditLineService;
            _contractAmount = contractAmount;
            _contractDutyService = contractDutyService;
            _logger = logger;
            _contractPaymentScheduleService = contractPaymentScheduleService;
        }

        public void Execute()
        {
            if (!_options.CrmPaymentUpload) return;
            try
            {
                _jobLog.Log("CrmGeneratePaymentQueue", JobCode.Start, JobStatus.Success, EntityType.BitrixGeneratePaymentQueue);

                List<int> payments = new List<int>();
                payments.AddRange(_crmPaymentRepository.GeneratePaymentQueue());

                _crmPaymentRepository.Insert(
                    payments.Select(x =>
                        CheckPayment(x)
                            ? new CrmUploadPayment { ContractId = x, CreateDate = DateTime.Now }
                            : new CrmUploadPayment { ContractId = x, UploadDate = DateTime.Now, CreateDate = DateTime.Now }
                    ).ToList()
                );

                _jobLog.Log("CrmGeneratePaymentQueue", JobCode.End, JobStatus.Success, EntityType.BitrixGeneratePaymentQueue);
            }
            catch (Exception e)
            {
                _jobLog.Log("CrmGeneratePaymentQueue", JobCode.Error, JobStatus.Failed, EntityType.BitrixGeneratePaymentQueue, responseData: JsonConvert.SerializeObject(e));
            }
        }

        public bool CheckPayment(int? contractId)
        {
            if (contractId == null || contractId == 0) return false;
            var contract = _contractService.GetOnlyContract(contractId.Value);
            var delayDayCount = (contract.BuyoutDate.HasValue || !contract.NextPaymentDate.HasValue) ? 0 : (DateTime.Now.Date - contract.NextPaymentDate.Value.Date).Days;
            
            // просроченные, выкупленные или реализованные контракты выгружаются как обычно
            if (delayDayCount > 0 || contract.Status > ContractStatus.Signed)
                return true;

            if (contract.ContractClass == ContractClass.Tranche ||
                contract.ContractClass == ContractClass.CreditLine)
            {
                var creditLineId = contract.ContractClass == ContractClass.CreditLine ? contract.Id :
                    _contractService.GetCreditLineId(contract.Id).Result;
                var distribution = _creditLineService.GetCurrentlyDebtForCreditLine(creditLineId).Result;
                var tranches = _contractService.GetAllSignedTranches(creditLineId).Result;
                decimal nextPaymentSum = 0;
                foreach(var tranche in tranches)
                {
                    var nextPaymentScheduleItem = _contractPaymentScheduleService.GetNextPaymentSchedule(tranche.Id).Result;
                    if (nextPaymentScheduleItem != null)
                    {
                        nextPaymentSum += nextPaymentScheduleItem.DebtCost + nextPaymentScheduleItem.PercentCost;
                    }
                }

                // если на авансе КЛ достаточно денег для погашения ежемесячного платежа по всем траншам, тогда на обзвон не выгружаем
                if (distribution.SummaryPrepaymentBalance >= nextPaymentSum)
                {
                    return false;
                }
            }
            else
            {
                if (contract.PercentPaymentType != PercentPaymentType.EndPeriod)
                {
                    var fullContract = _contractService.Get(contractId.Value);
                    _contractAmount.Init(fullContract, DateTime.Now);

                    // если на авансе достаточно денег для погашения ежемесячного платежа, тогда на обзвон не выгружаем
                    if (_contractAmount.DisplayAmount == 0)
                    {
                        return false;
                    }
                }
                else
                {
                    decimal depoBalance = _contractService.GetPrepaymentBalance(contract.Id);
                    ContractDuty prolongContractDuty = _contractDutyService.GetContractDuty(new ContractDutyCheckModel
                    {
                        ActionType = ContractActionType.Prolong,
                        ContractId = contract.Id,
                        Date = DateTime.Now.AddDays(1).Date,
                        PayTypeId = 2 // касса
                    });

                    // если на авансе достаточно денег для погашения ежемесячного платежа, тогда на обзвон не выгружаем
                    if (depoBalance >= prolongContractDuty.Cost)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}