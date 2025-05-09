using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Newtonsoft.Json;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Web.Engine.Jobs.MigrationJobs;

namespace Pawnshop.Web.Engine.Jobs.AccountingJobs
{
    public class InterestAccrualJob
    {
        private static readonly object _object = new object();
        private readonly IInterestAccrual _interestAccrual;
        private readonly IContractService _contractService;
        private readonly IJobLog _jobLog;
        private const int THREAD_SLEEP_MILLISECONDS = 300000;

        public InterestAccrualJob(IInterestAccrual interestAccrual, IContractService contractService, IJobLog jobLog)
        {
            _interestAccrual = interestAccrual;
            _contractService = contractService;
            _jobLog = jobLog;
        }

        [Queue("accruals")]
        public void EnqueueAllOnAnyDate()
        {
            bool tryEnter = false;
            if (tryEnter = Monitor.TryEnter(_object, new TimeSpan(1, 0, 0)))
            {
                var processingContractIds = new HashSet<int>();
                try
                {
                    DateTime today = DateTime.Today;
                    _jobLog.Log("InterestAccrualJob", JobCode.Start, JobStatus.Success, EntityType.Contract, null, null, null);
                    var contractStatuses = new HashSet<ContractStatus> { ContractStatus.Signed, ContractStatus.SoldOut };
                    var contracts = _contractService.GetContractsByPaymentScheduleFilter(today, null, contractStatuses);
                    _jobLog.Log("InterestAccrualJob", JobCode.Start, JobStatus.Success, EntityType.Contract, null, null, $"Получено {contracts.Count} договоров");
                    foreach (var contract in contracts)
                    {
                        if (processingContractIds.Contains(contract.Id))
                            continue;

                        processingContractIds.Add(contract.Id);
                        if (!contractStatuses.Contains(contract.Status))
                            continue;
                        BackgroundJob.Enqueue<InterestAccrualJob>(x => x.OnAnyDate(contract.Id, today));
                    }
                }
                catch (Exception ex)
                {
                    string exJson = JsonConvert.SerializeObject(ex);
                    _jobLog.Log("InterestAccrualJob", JobCode.Error, JobStatus.Failed, EntityType.None, null, null, exJson);
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
                _jobLog.Log("InterestAccrualJob", JobCode.Error, JobStatus.Failed, EntityType.None, null, null, "Процесс не долждался своей очереди");
        }

        public void EnqueueOnSomeDate(DateTime date)
        {
            var processingContractIds = new HashSet<int>();
            try
            {
                DateTime today = date.Date;
                _jobLog.Log("InterestAccrualJob", JobCode.Start, JobStatus.Success, EntityType.Contract, null, null, null);
                var contractStatuses = new HashSet<ContractStatus> { ContractStatus.Signed, ContractStatus.SoldOut };
                var contracts = _contractService.GetContractsByPaymentScheduleFilter(today, null, contractStatuses);
                _jobLog.Log("InterestAccrualJob", JobCode.Start, JobStatus.Success, EntityType.Contract, null, null, $"Получено {contracts.Count} договоров");
                foreach (var contract in contracts)
                {
                    if (processingContractIds.Contains(contract.Id))
                        continue;

                    processingContractIds.Add(contract.Id);
                    if (!contractStatuses.Contains(contract.Status))
                        continue;
                    //OnAnyDate(contract.Id, today);
                    BackgroundJob.Enqueue<InterestAccrualJob>(x => x.OnAnyDate(contract.Id, today));
                }
            }
            catch (Exception ex)
            {
                string exJson = JsonConvert.SerializeObject(ex);
                _jobLog.Log("InterestAccrualJob", JobCode.Error, JobStatus.Failed, EntityType.None, null, null, exJson);
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
            try
            {
                _jobLog.Log("InterestAccrualJob.OnAnyDate", JobCode.Start, JobStatus.Success, EntityType.Contract, contractId, requestJson, null);
                var contract = _contractService.Get(contractId);
                _interestAccrual.OnAnyDateAccrual(contract, Constants.ADMINISTRATOR_IDENTITY, accrualDate ?? DateTime.Now);
                _jobLog.Log("InterestAccrualJob.OnAnyDate", JobCode.End, JobStatus.Success, EntityType.Contract, contractId, requestJson, null);
            }
            catch (Exception ex)
            {
                string exJson = JsonConvert.SerializeObject(ex);
                _jobLog.Log("InterestAccrualJob.OnAnyDate", JobCode.Error, JobStatus.Failed, EntityType.Contract, contractId, requestJson, exJson);
                throw;
            }
        }

        [Queue("accruals")]
        public void OnAnyDateRestructured(int contractId, DateTime? accrualDate)
        {
            var request = new { accrualDate };
            string requestJson = JsonConvert.SerializeObject(request);
            try
            {
                _jobLog.Log("InterestAccrualJob.OnAnyDate", JobCode.Start, JobStatus.Success, EntityType.Contract, contractId, requestJson, null);
                var contract = _contractService.Get(contractId);
                _interestAccrual.OnAnyDateAccrual(contract, Constants.ADMINISTRATOR_IDENTITY, accrualDate ?? DateTime.Now);
                _jobLog.Log("InterestAccrualJob.OnAnyDate", JobCode.End, JobStatus.Success, EntityType.Contract, contractId, requestJson, null);
            }
            catch (Exception ex)
            {
                string exJson = JsonConvert.SerializeObject(ex);
                _jobLog.Log("InterestAccrualJob.OnAnyDate", JobCode.Error, JobStatus.Failed, EntityType.Contract, contractId, requestJson, exJson);
                throw;
            }
        }
    }
}