using Pawnshop.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Core;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Data.Models.Audit;
using Newtonsoft.Json;
using System.Threading;
using Pawnshop.Services.Dictionaries;

namespace Pawnshop.Web.Engine.Jobs.AccountingJobs
{
    public class TakeAwayToDelayJob
    {
        private const int THREAD_SLEEP_MILLISECONDS = 300000;
        private static readonly object _object = new object();
        private readonly IContractService _contractService;
        private readonly IJobLog _jobLog;
        private readonly ITakeAwayToDelay _takeAwayTo;
        private readonly IHolidayService _holidayService;
        private readonly IContractPaymentScheduleService _paymentScheduleService;

        public TakeAwayToDelayJob(IContractService contractService, IJobLog jobLog,
            ITakeAwayToDelay takeAwayTo, IHolidayService holidayService, IContractPaymentScheduleService paymentScheduleService)
        {
            _contractService = contractService;
            _jobLog = jobLog;
            _takeAwayTo = takeAwayTo;
            _holidayService = holidayService;
            _paymentScheduleService = paymentScheduleService;
        }

        [Queue("accruals")]
        public void Execute()
        {
            DateTime date = DateTime.Today;
            DateTime takeAwayDate = date.AddDays(-1);
            string requestData = $"Попытка выноса на просрочку договоров, дата - {date:dd.MM.yyyy}";
            bool tryEnter = false;
            if (tryEnter = Monitor.TryEnter(_object, new TimeSpan(0, 30, 0)))
            {
                var processingContractIds = new HashSet<int>();
                try
                {
                    var contractStatuses = new HashSet<ContractStatus> { ContractStatus.Signed, ContractStatus.SoldOut };
                    _jobLog.Log("TakeAwayToDelayJob", JobCode.Start, JobStatus.Success, EntityType.Contract, null, requestData, null);
                    var fromDate = _holidayService.GetFirstPreviousHolidayFromDate(takeAwayDate);
                    if (fromDate == default)
                    {
                        fromDate = takeAwayDate;
                    }
                    List<Contract> contracts = _contractService.GetContractsByPaymentScheduleFilter(fromDate, takeAwayDate, contractStatuses);
                    _jobLog.Log("TakeAwayToDelayJob", JobCode.Begin, JobStatus.Success, EntityType.Contract, null, requestData: requestData, $"Получено {contracts.Count} договоров");
                    foreach (Contract contract in contracts)
                    {
                        if (processingContractIds.Contains(contract.Id))
                            continue;

                        processingContractIds.Add(contract.Id);
                        if (!contractStatuses.Contains(contract.Status))
                            continue;

                        //EnqueuePerContract(contract.Id, takeAwayDate);
                        BackgroundJob.Enqueue<TakeAwayToDelayJob>(x => x.EnqueuePerContract(contract.Id, takeAwayDate));
                    }
                }
                catch (Exception ex)
                {
                    string exceptionJson = JsonConvert.SerializeObject(ex);
                    _jobLog.Log("TakeAwayToDelayJob", JobCode.Error, JobStatus.Failed, EntityType.None, null, requestData: requestData, responseData: exceptionJson);
                    if (processingContractIds.Count == 0)
                        throw;
                }
                finally
                {
                    if(processingContractIds.Count > 0)
                        Thread.Sleep(THREAD_SLEEP_MILLISECONDS);

                    Monitor.Exit(_object);
                }
            }

            if (!tryEnter)
                _jobLog.Log("TakeAwayToDelayJob", JobCode.Error, JobStatus.Failed, EntityType.None, null, requestData, "Процесс не долждался своей очереди");
        }


        public void ExecuteOnDate(DateTime date)
        {
            date = date.Date;
            DateTime takeAwayDate = date.AddDays(-1);
            string requestData = $"Попытка выноса на просрочку договоров, дата - {date:dd.MM.yyyy}";
            var processingContractIds = new HashSet<int>();
            try
            {
                var contractStatuses = new HashSet<ContractStatus> { ContractStatus.Signed, ContractStatus.SoldOut };
                _jobLog.Log("TakeAwayToDelayJob", JobCode.Start, JobStatus.Success, EntityType.Contract, null, requestData, null);
                List<Contract> contracts = _contractService.GetContractsByPaymentScheduleFilter(takeAwayDate, takeAwayDate, contractStatuses);
                _jobLog.Log("TakeAwayToDelayJob", JobCode.Begin, JobStatus.Success, EntityType.Contract, null, requestData: requestData, $"Получено {contracts.Count} договоров");
                foreach (Contract contract in contracts)
                {
                    if (processingContractIds.Contains(contract.Id))
                        continue;

                    processingContractIds.Add(contract.Id);
                    if (!contractStatuses.Contains(contract.Status))
                        continue;

                    //EnqueuePerContract(contract.Id, date);
                    BackgroundJob.Enqueue<TakeAwayToDelayJob>(x => x.EnqueuePerContract(contract.Id, date));
                }
            }
            catch (Exception ex)
            {
                string exceptionJson = JsonConvert.SerializeObject(ex);
                _jobLog.Log("TakeAwayToDelayJob", JobCode.Error, JobStatus.Failed, EntityType.None, null, requestData: requestData, responseData: exceptionJson);
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
        public void EnqueuePerContract(int contractId, DateTime valueDate)
        {
            string requestData = $"Попытка выноса на просрочку договора {contractId}, дата проводки - {valueDate:dd.MM.yyyy}";
            try
            {
                Contract contract = _contractService.GetOnlyContract(contractId);
                contract.PaymentSchedule = _paymentScheduleService.GetListByContractId(contract.Id, true);
                
                if (contract == null)
                    throw new PawnshopApplicationException($"Договор {contractId} не найден");

                if (contract.Status != ContractStatus.Signed && contract.Status != ContractStatus.SoldOut)
                    return;

                var scheduleDate = contract.PaymentSchedule
                            .Where(x => !x.ActualDate.HasValue && x.Date <= valueDate)
                            .OrderByDescending(x => x.Date)
                            .FirstOrDefault().Date;

                if (scheduleDate == default)
                {
                    string responseDataStop = $"Не найдена дата для выноса по договору {contractId} на дату проводки - {valueDate:dd.MM.yyyy})";
                    _jobLog.Log("TakeAwayToDelayJob.EnqueuePerContract", JobCode.Start, JobStatus.Success, EntityType.Contract, contractId, requestData, responseDataStop);
                    return;
                }

                string responseDataStart = $"Попытка выноса договора {contractId}(дата - {scheduleDate:dd.MM.yyyy}, дата проводки - {valueDate:dd.MM.yyyy})";
                _jobLog.Log("TakeAwayToDelayJob.EnqueuePerContract", JobCode.Start, JobStatus.Success, EntityType.Contract, contractId, requestData, responseDataStart);

                _takeAwayTo.TakeAwayToDelay(contract, scheduleDate, valueDate, Constants.ADMINISTRATOR_IDENTITY, false);
                
                string responseData = $"Попытка выноса договора {contractId}(дата - {scheduleDate:dd.MM.yyyy}), дата проводки - {valueDate:dd.MM.yyyy} прошла успешно";
                _jobLog.Log("TakeAwayToDelayJob.EnqueuePerContract", JobCode.End, JobStatus.Success, EntityType.Contract, contract.Id, requestData, responseData);
            }
            catch (Exception ex)
            {
                string exceptionJson = JsonConvert.SerializeObject(ex);
                _jobLog.Log("TakeAwayToDelayJob.EnqueuePerContract", JobCode.Error, JobStatus.Failed, EntityType.Contract, contractId, requestData, responseData: exceptionJson);
                throw;
            }
        }
    }
}
