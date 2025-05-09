using Hangfire;
using Newtonsoft.Json;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Dictionaries;
using Pawnshop.Services.Exceptions;
using Pawnshop.Web.Engine.Audit;
using System;
using System.Collections.Generic;
using System.Threading;
using Pawnshop.Data.Models.Contracts.Inscriptions;

namespace Pawnshop.Web.Engine.Jobs.AccountingJobs
{
    public class InterestAccrualOnOverdueDebtJob
    {
        private static readonly object _object = new object();
        private readonly IInterestAccrual _interestAccrual;
        private readonly IContractService _contractService;
        private readonly IJobLog _jobLog;
        private const int THREAD_SLEEP_MILLISECONDS = 300000;

        public InterestAccrualOnOverdueDebtJob(IInterestAccrual interestAccrual, IContractService contractService, IJobLog jobLog)
        {
            _interestAccrual = interestAccrual;
            _contractService = contractService;
            _jobLog = jobLog;
        }

        [Queue("accruals")]
        public void Execute()
        {
            bool tryEnter = false;
            if (tryEnter = Monitor.TryEnter(_object, new TimeSpan(1, 0, 0)))
            {
                var processingContractIds = new HashSet<int>();
                try
                {
                    DateTime today = DateTime.Today;

                    _jobLog.Log("InterestAccrualOnOverdueDebtJob", JobCode.Start, JobStatus.Success, EntityType.Contract, null, null, null);
                    var contractStatuses = new HashSet<ContractStatus> { ContractStatus.Signed, ContractStatus.SoldOut };
                    List<Contract> contracts = new List<Contract>();
                    var neededPercentPaymentTypes = new List<PercentPaymentType> { PercentPaymentType.EndPeriod };

                    List<Contract> contractsTemp = _contractService.GetContractsByPaymentScheduleFilter(null, today.AddDays(-1), contractStatuses, neededPercentPaymentTypes);
                    contracts.AddRange(contractsTemp);

                    _jobLog.Log("InterestAccrualOnOverdueDebtJob", JobCode.Start, JobStatus.Success, EntityType.Contract, null, null, $"Получено {contracts.Count} договоров");

                    foreach (var contract in contracts)
                    {
                        processingContractIds.Add(contract.Id);
                        if (!contractStatuses.Contains(contract.Status))
                            continue;

                        BackgroundJob.Enqueue<InterestAccrualOnOverdueDebtJob>(x => x.OnAnyDate(contract.Id, today));
                    }
                }
                catch (Exception ex)
                {
                    string exJson = JsonConvert.SerializeObject(ex);
                    _jobLog.Log("InterestAccrualOnOverdueDebtJob", JobCode.Error, JobStatus.Failed, EntityType.None, null, null, exJson);
                    if (processingContractIds.Count == 0)
                        throw;
                }
                finally
                {
                    if (processingContractIds.Count > 0)
                        Thread.Sleep(THREAD_SLEEP_MILLISECONDS);

                    Monitor.Exit(_object);
                }
            }

            if (!tryEnter)
                _jobLog.Log("InterestAccrualOnOverdueDebtJob", JobCode.Error, JobStatus.Failed, EntityType.None, null, null, "Процесс не долждался своей очереди");
        }


        public void ExecuteOnAnyDate(DateTime date)
        {
            var processingContractIds = new HashSet<int>();
            try
            {
                DateTime today = date.Date;

                _jobLog.Log("InterestAccrualOnOverdueDebtJob", JobCode.Start, JobStatus.Success, EntityType.Contract, null, null, null);
                var contractStatuses = new HashSet<ContractStatus> { ContractStatus.Signed, ContractStatus.SoldOut };
                List<Contract> contracts = new List<Contract>();
                var neededPercentPaymentTypes = new List<PercentPaymentType> { PercentPaymentType.EndPeriod };

                List<Contract> contractsTemp = _contractService.GetContractsByPaymentScheduleFilter(null, today.AddDays(-1), contractStatuses, neededPercentPaymentTypes);
                contracts.AddRange(contractsTemp);

                _jobLog.Log("InterestAccrualOnOverdueDebtJob", JobCode.Start, JobStatus.Success, EntityType.Contract, null, null, $"Получено {contracts.Count} договоров");

                foreach (var contract in contracts)
                {
                    processingContractIds.Add(contract.Id);
                    if (!contractStatuses.Contains(contract.Status))
                        continue;
                    //OnAnyDate(contract.Id, today);
                    BackgroundJob.Enqueue<InterestAccrualOnOverdueDebtJob>(x => x.OnAnyDate(contract.Id, today));
                }
            }
            catch (Exception ex)
            {
                string exJson = JsonConvert.SerializeObject(ex);
                _jobLog.Log("InterestAccrualOnOverdueDebtJob", JobCode.Error, JobStatus.Failed, EntityType.None, null, null, exJson);
                if (processingContractIds.Count == 0)
                    throw;
            }
            finally
            {
                if (processingContractIds.Count > 0)
                    Thread.Sleep(THREAD_SLEEP_MILLISECONDS);

                Monitor.Exit(_object);
            }
        }

        [Queue("accruals")]
        public void OnAnyDate(int contractId, DateTime? accrualDate)
        {
            var request = new { accrualDate };
            string requestJson = JsonConvert.SerializeObject(request);
            bool throwException = true;
            try
            {
                _jobLog.Log("InterestAccrualOnOverdueDebtJob.OnAnyDate", JobCode.Start, JobStatus.Success, EntityType.Contract, contractId, requestJson, null);
                Contract contract = _contractService.Get(contractId);
                if (contract == null)
                {
                    throwException = false;
                    throw new PawnshopApplicationException($"Договор {contract.Id} не найден");
                }

                if (contract.Status != ContractStatus.Signed && contract.Status != ContractStatus.SoldOut)
                {
                    throwException = false;
                    throw new PawnshopApplicationException($"Договор {contract.Id} должен быть подписанным или выкупленным");
                }

                if (contract.InscriptionId.HasValue && contract.Inscription.Status != InscriptionStatus.Denied)
                {
                    if (contract.IsOffBalance && contract.UsePenaltyLimit && contract.Inscription.Date >= Constants.PENY_LIMIT_DATE)
                    {
                        _jobLog.Log("InterestAccrualOnOverdueDebtJob.OnAnyDate", JobCode.Begin, JobStatus.Success, EntityType.Contract, contractId, requestJson, "Имеется действующая исполнительная надпись. Начинается начисление на внебалансе");
                        _interestAccrual.ManualInterestAccrualOnOverdueDebt(contract, Constants.ADMINISTRATOR_IDENTITY, accrualDate ?? DateTime.Today);
                        _jobLog.Log("InterestAccrualOnOverdueDebtJob.OnAnyDate", JobCode.End, JobStatus.Success, EntityType.Contract, contractId, requestJson, null);

                    }
                    else
                    {
                        _jobLog.Log("InterestAccrualOnOverdueDebtJob.OnAnyDate", JobCode.Cancel, JobStatus.Success, EntityType.Contract, contractId, requestJson, $"Имеется действующая исполнительная надпись, созданная до {Constants.PENY_LIMIT_DATE}.");
                    }
                    return;

                }

                throwException = true;
                _interestAccrual.OnAnyDateOnOverdueDebt(contract, Constants.ADMINISTRATOR_IDENTITY, accrualDate ?? DateTime.Today);
                _jobLog.Log("InterestAccrualOnOverdueDebtJob.OnAnyDate", JobCode.End, JobStatus.Success, EntityType.Contract, contractId, requestJson, null);
            }
            catch (NothingToAccrualException ex)
            {
                // ничего не делаем
                string exJson = JsonConvert.SerializeObject(ex);
                _jobLog.Log("InterestAccrualOnOverdueDebtJob.OnAnyDate", JobCode.Error, JobStatus.Failed, EntityType.Contract, contractId, requestJson, exJson);
            }
            catch (Exception ex)
            {
                string exJson = JsonConvert.SerializeObject(ex);
                _jobLog.Log("InterestAccrualOnOverdueDebtJob.OnAnyDate", JobCode.Error, JobStatus.Failed, EntityType.Contract, contractId, requestJson, exJson);
                if (throwException)
                    throw;
            }
        }
    }
}
