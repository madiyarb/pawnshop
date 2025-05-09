using Pawnshop.Web.Engine.Audit;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Core;
using Pawnshop.Services.PenaltyLimit;
using Pawnshop.Services.Contracts;
using Pawnshop.Services;
using Pawnshop.Data.Models.AccountingCore;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Web.Engine.Jobs.AccountingJobs
{
    public class PenaltyRateDecreaseJob
    {
        private static readonly object _object = new object();
        private readonly IPenaltyRateService _penaltyRateService;
        private readonly IContractService _contractService;
        private readonly JobLog _jobLog;
        private IDictionaryWithSearchService<AccountingCore.Models.AccrualBase, AccrualBaseFilter> _accrualBaseService;
        private const int THREAD_SLEEP_MILLISECONDS = 300000;

        public PenaltyRateDecreaseJob(IPenaltyRateService penaltyRateService, JobLog jobLog, IContractService contractService,
            IDictionaryWithSearchService<AccountingCore.Models.AccrualBase, AccrualBaseFilter> accrualBaseService)
        {
            _jobLog = jobLog;
            _penaltyRateService = penaltyRateService;
            _contractService = contractService;
            _accrualBaseService = accrualBaseService;
        }

        [Queue("accruals")]
        public void Execute()
        {
            bool tryEnter = false;
            if (tryEnter = Monitor.TryEnter(_object, new TimeSpan(0, 30, 0)))
            {
                var processingContractIds = new HashSet<int>();
                try
                {
                    DateTime today = DateTime.Today;
                    string requestData = $"Уменьшение ставки пени для договоров, дата - {today:dd.MM.yyyy}";
                    _jobLog.Log("PenaltyRateDecreaseJob", JobCode.Start, JobStatus.Success, requestData: requestData);
                    var contractsForDecrease = _contractService.GetContractsForDecreasePenaltyRates(today);
                    _jobLog.Log("PenaltyRateDecreaseJob", JobCode.Begin, JobStatus.Success, EntityType.Contract, null, requestData: requestData, $"Получено {contractsForDecrease.Count} договоров");
                    foreach (var contract in contractsForDecrease)
                    {
                        var accrualSettings = _accrualBaseService.List(new ListQueryModel<AccrualBaseFilter> { Page = null, Model = new AccrualBaseFilter { AccrualType = AccrualType.PenaltyAccrual, IsActive = true, ContractClass = contract.ContractClass } }).List;
                        if (processingContractIds.Contains(contract.Id))
                            continue;

                        processingContractIds.Add(contract.Id);
                        
                        BackgroundJob.Enqueue<PenaltyRateDecreaseJob>(x => x.DecreasePenaltyRatesForContract(contract, today, accrualSettings));
                    }
                }
                catch (Exception ex)
                {
                    _jobLog.Log("PenaltyRateDecreaseJob", JobCode.Error, JobStatus.Failed, EntityType.None, null, null, JsonConvert.SerializeObject(ex));
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
                _jobLog.Log("PenaltyRateDecreaseJob", JobCode.Error, JobStatus.Failed, EntityType.None, null, null, "Процесс не долждался своей очереди");
        }

        public void ExecuteOnDate(DateTime date)
        {
            var processingContractIds = new HashSet<int>();
            try
            {
                DateTime today = date.Date;
                string requestData = $"Уменьшение ставки пени для договоров, дата - {today:dd.MM.yyyy}";
                _jobLog.Log("PenaltyRateDecreaseJob", JobCode.Start, JobStatus.Success, requestData: requestData);
                var contractsForDecrease = _contractService.GetContractsForDecreasePenaltyRates(today);
                _jobLog.Log("PenaltyRateDecreaseJob", JobCode.Begin, JobStatus.Success, EntityType.Contract, null, requestData: requestData, $"Получено {contractsForDecrease.Count} договоров");
                foreach (var contract in contractsForDecrease)
                {
                    var accrualSettings = _accrualBaseService.List(new ListQueryModel<AccrualBaseFilter> { Page = null, Model = new AccrualBaseFilter { AccrualType = AccrualType.PenaltyAccrual, IsActive = true, ContractClass = contract.ContractClass } }).List;
                    if (processingContractIds.Contains(contract.Id))
                        continue;

                    processingContractIds.Add(contract.Id);
                    //DecreasePenaltyRatesForContract(contract, today, accrualSettings);
                    BackgroundJob.Enqueue<PenaltyRateDecreaseJob>(x => x.DecreasePenaltyRatesForContract(contract, today, accrualSettings));
                }
            }
            catch (Exception ex)
            {
                _jobLog.Log("PenaltyRateDecreaseJob", JobCode.Error, JobStatus.Failed, EntityType.None, null, null, JsonConvert.SerializeObject(ex));
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
        public void DecreasePenaltyRatesForContract(Contract contract, DateTime date, List<AccountingCore.Models.AccrualBase> accrualSettings)
        {
            try
            {
                string requestData = $"Попытка уменьшение ставки пени для Договора {contract.Id} на дату - {date:dd.MM.yyyy}";
                _jobLog.Log("PenaltyRateDecreaseJob.DecreasePenaltyRatesForContract", JobCode.Start, JobStatus.Success, EntityType.Contract, contract.Id, requestData, null);
                _penaltyRateService.DecreaseRates(contract, date, Constants.ADMINISTRATOR_IDENTITY, accrualSettings);
                _jobLog.Log("PenaltyRateDecreaseJob.DecreasePenaltyRatesForContract", JobCode.End, JobStatus.Success, EntityType.Contract, contract.Id, requestData, null);
            }
            catch (Exception ex)
            {
                _jobLog.Log("PenaltyRateDecreaseJob.DecreasePenaltyRatesForContract", JobCode.Error, JobStatus.Failed, responseData: JsonConvert.SerializeObject(ex));
                throw;
            }
        }
    }
}
