using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Hangfire;
using Newtonsoft.Json;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Mail;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.PenaltyLimit;
using Pawnshop.Web.Engine.Audit;

namespace Pawnshop.Web.Engine.Jobs.AccountingJobs
{
    public class PenaltyLimitAccrualJob
    {
        private readonly ContractRepository _contractRepository;
        private readonly IPenaltyLimitAccrualService _penaltyLimitAccrualService;
        private readonly JobLog _jobLog;
        private const int THREAD_SLEEP_MILLISECONDS = 300000;
        private static readonly object _object = new object();
        private readonly IContractService _contractService;

        public PenaltyLimitAccrualJob(ContractRepository contractRepository, IPenaltyLimitAccrualService penaltyLimitAccrualService, JobLog jobLog, IContractService contractService)
        {
            _contractRepository = contractRepository;
            _penaltyLimitAccrualService = penaltyLimitAccrualService;
            _jobLog = jobLog;
            _contractService = contractService;
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
                    var today = DateTime.Today;
                    string requestData = $"Попытка насчисления лимитов пени для договоров, дата - {today:dd.MM.yyyy}";

                    _jobLog.Log("PenaltyLimitAccrualJob", JobCode.Start, JobStatus.Success, requestData: requestData);

                    var newAndAdditionContracts = _contractRepository.GetContractsOnDateForPenaltyAccrual(today);
                    _jobLog.Log("PenaltyLimitAccrualJob", JobCode.Begin, JobStatus.Success, EntityType.Contract, null, requestData: requestData, $"Получено {newAndAdditionContracts.Count} договоров");

                    foreach (int contractId in newAndAdditionContracts)
                    {
                        if (processingContractIds.Contains(contractId))
                            continue;

                        processingContractIds.Add(contractId);
                        BackgroundJob.Enqueue<PenaltyLimitAccrualJob>(x => x.ExecPenaltyLimitAccrualForContract(contractId, today));
                    }

                    var partialPaymentContracts = _contractRepository.GetContractsByParentContractDateForPenaltyAccrual(today);
                    _jobLog.Log("PenaltyLimitAccrualJob", JobCode.Begin, JobStatus.Success, EntityType.Contract, null, requestData: requestData, $"Получено {partialPaymentContracts.Count} договоров");

                    foreach (var contractId in partialPaymentContracts)
                    {
                        if (processingContractIds.Contains(contractId))
                            continue;

                        processingContractIds.Add(contractId);
                        BackgroundJob.Enqueue<PenaltyLimitAccrualJob>(x => x.ExecPenaltyLimitAccrualForContract(contractId, today));
                    }

                    _jobLog.Log("PenaltyLimitAccrualJob", JobCode.End, JobStatus.Success,
                        requestData:
                        @$"Успешно начислены лимиты пени. Количество договоров: {newAndAdditionContracts.Count + partialPaymentContracts.Count} (дата - {today:dd.MM.yyyy})");
                }
                catch (Exception ex)
                {
                    _jobLog.Log("PenaltyLimitAccrualJob", JobCode.Error, JobStatus.Failed, responseData: JsonConvert.SerializeObject(ex));
                    
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
                _jobLog.Log("PenaltyLimitAccrualJob", JobCode.Error, JobStatus.Failed, EntityType.None, null, null, "Процесс не долждался своей очереди");
        }


        public void ExecuteOnDate(DateTime date)
        {
            var processingContractIds = new HashSet<int>();
            try
            {
                var today = date.Date;
                string requestData = $"Попытка насчисления лимитов пени для договоров, дата - {today:dd.MM.yyyy}";

                _jobLog.Log("PenaltyLimitAccrualJob", JobCode.Start, JobStatus.Success, requestData: requestData);

                var newAndAdditionContracts = _contractRepository.GetContractsOnDateForPenaltyAccrual(today);
                _jobLog.Log("PenaltyLimitAccrualJob", JobCode.Begin, JobStatus.Success, EntityType.Contract, null, requestData: requestData, $"Получено {newAndAdditionContracts.Count} договоров");

                foreach (int contractId in newAndAdditionContracts)
                {
                    if (processingContractIds.Contains(contractId))
                        continue;

                    processingContractIds.Add(contractId);
                    //ExecPenaltyLimitAccrualForContract(contractId, today);
                    BackgroundJob.Enqueue<PenaltyLimitAccrualJob>(x => x.ExecPenaltyLimitAccrualForContract(contractId, today));
                }

                var partialPaymentContracts = _contractRepository.GetContractsByParentContractDateForPenaltyAccrual(today);
                _jobLog.Log("PenaltyLimitAccrualJob", JobCode.Begin, JobStatus.Success, EntityType.Contract, null, requestData: requestData, $"Получено {partialPaymentContracts.Count} договоров");

                foreach (var contractId in partialPaymentContracts)
                {
                    if (processingContractIds.Contains(contractId))
                        continue;

                    processingContractIds.Add(contractId);
                    //ExecPenaltyLimitAccrualForContract(contractId, today);
                    BackgroundJob.Enqueue<PenaltyLimitAccrualJob>(x => x.ExecPenaltyLimitAccrualForContract(contractId, today));
                }

                _jobLog.Log("PenaltyLimitAccrualJob", JobCode.End, JobStatus.Success,
                    requestData:
                    @$"Успешно начислены лимиты пени. Количество договоров: {newAndAdditionContracts.Count + partialPaymentContracts.Count} (дата - {today:dd.MM.yyyy})");
            }
            catch (Exception ex)
            {
                _jobLog.Log("PenaltyLimitAccrualJob", JobCode.Error, JobStatus.Failed, responseData: JsonConvert.SerializeObject(ex));

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
        public void ExecPenaltyLimitAccrualForContract(int contractId, DateTime date)
        {
            string requestData = JsonConvert.SerializeObject(new { date });
            try
            {
                Contract contract = _contractService.Get(contractId);
                if (contract == null)
                    throw new PawnshopApplicationException($"Договор {contractId} не найден");

                if (contract.ParentId.HasValue)
                {
                    contract.ParentContract = _contractService.Get(contract.ParentId.Value);

                    if (contract.ParentContract == null)
                        throw new PawnshopApplicationException($"Договор {contract.ParentId} не найден");
                }

                _jobLog.Log("PenaltyLimitAccrualJob.ExecPenaltyLimitAccrualForContract", JobCode.Start, JobStatus.Success, EntityType.Contract, contract.Id, requestData, null);

                if (contract.ParentContract is null)
                    _penaltyLimitAccrualService.Execute(contract, date, Constants.ADMINISTRATOR_IDENTITY);
                else
                    _penaltyLimitAccrualService.Execute(contract, contract.ParentContract, date, Constants.ADMINISTRATOR_IDENTITY);

                _jobLog.Log("PenaltyLimitAccrualJob.ExecPenaltyLimitAccrualForContract", JobCode.End, JobStatus.Success, EntityType.Contract, contract.Id, requestData, null);
            }
            catch (Exception ex)
            {
                string exJson = JsonConvert.SerializeObject(ex);
                _jobLog.Log("PenaltyLimitAccrualJob.ExecPenaltyLimitAccrualForContract", JobCode.Error, JobStatus.Failed, EntityType.Contract, contractId, requestData, exJson);
                throw;
            }
        }
    }
}