using Hangfire;
using Newtonsoft.Json;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Data.Models.Restructuring;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Web.Engine.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pawnshop.Web.Engine.Jobs.AccountingJobs
{
    public class PenaltyAccrualJob
    {
        private static readonly object _object = new object();
        private const int THREAD_SLEEP_MILLISECONDS = 300000;
        private readonly IPenaltyAccrual _penaltyAccrualService;
        private readonly IContractService _contractService;
        private readonly IJobLog _jobLog;
        public PenaltyAccrualJob(IPenaltyAccrual penaltyAccrualService, IContractService contractService, IJobLog jobLog)
        {
            _penaltyAccrualService = penaltyAccrualService;
            _contractService = contractService;
            _jobLog = jobLog;
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
                    ContractFilter contractFilter = new ContractFilter
                    {
                        NextPaymentEndDate = today,
                        Statuses = new List<ContractStatus> { ContractStatus.Signed, ContractStatus.SoldOut }
                    };

                    List<Contract> contracts = _contractService.List(contractFilter);
                    _jobLog.Log("PenaltyAccrualJob", JobCode.Start, JobStatus.Success, EntityType.None, null, $"Получено {contracts.Count} договоров", null);
                    foreach (Contract contract in contracts)
                    {
                        if (processingContractIds.Contains(contract.Id))
                            continue;

                        processingContractIds.Add(contract.Id);
                        BackgroundJob.Enqueue<PenaltyAccrualJob>(x => x.ExecPenaltyAccrualForContract(contract.Id, today));
                    }
                }
                catch (Exception ex)
                {
                    string exceptionJson = JsonConvert.SerializeObject(ex);
                    _jobLog.Log("PenaltyAccrualJob", JobCode.Error, JobStatus.Failed, EntityType.None, null, null, exceptionJson);
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
                _jobLog.Log("PenaltyAccrualJob", JobCode.Error, JobStatus.Failed, EntityType.None, null, null, "Процесс не долждался своей очереди");
        }

        public void ExecuteOnAnyDate(DateTime date)
        {
            var processingContractIds = new HashSet<int>();
            try
            {
                DateTime today = date.Date;
                ContractFilter contractFilter = new ContractFilter
                {
                    NextPaymentEndDate = today,
                    Statuses = new List<ContractStatus> { ContractStatus.Signed, ContractStatus.SoldOut }
                };

                List<Contract> contracts = _contractService.List(contractFilter);
                _jobLog.Log("PenaltyAccrualJob", JobCode.Start, JobStatus.Success, EntityType.None, null, $"Получено {contracts.Count} договоров", null);
                foreach (Contract contract in contracts)
                {
                    if (processingContractIds.Contains(contract.Id))
                        continue;

                    processingContractIds.Add(contract.Id);
                    //ExecPenaltyAccrualForContract(contract.Id, today);
                    BackgroundJob.Enqueue<PenaltyAccrualJob>(x => x.ExecPenaltyAccrualForContract(contract.Id, today));
                }
            }
            catch (Exception ex)
            {
                string exceptionJson = JsonConvert.SerializeObject(ex);
                _jobLog.Log("PenaltyAccrualJob", JobCode.Error, JobStatus.Failed, EntityType.None, null, null, exceptionJson);
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
        public void ExecPenaltyAccrualForContract(int contractId, DateTime date)
        {
            var requestData1 = new { date };
            string requestData = JsonConvert.SerializeObject(requestData1);
            try
            {
                Contract contract = _contractService.Get(contractId);
                if (contract == null)
                    throw new PawnshopApplicationException($"Договор {contractId} не найден");

                if (contract.Status != ContractStatus.Signed && contract.Status != ContractStatus.SoldOut)
                    return;

                if (contract.InscriptionId.HasValue && contract.Inscription.Status != InscriptionStatus.Denied)
                {
                    if(!(contract.IsOffBalance && contract.UsePenaltyLimit && contract.Inscription?.Date >= Constants.PENY_LIMIT_DATE))
                    {
                        //если нет настройки продукта у договора или если у продукта нет типа продукта, то не начисляем
                        //не определяем по Contract.ProductType потому что есть договоры, где не проставлен тип продукта (баг после добора), но проставлена настройка продукта
                        //начисляем только по продуктам ДАМУ
                        if (!contract.SettingId.HasValue || !contract.Setting.ProductTypeId.HasValue || contract.Setting.ProductType.Code != Constants.PRODUCT_DAMU)
                            return;
                    }
                }

                _jobLog.Log("PenaltyAccrualJob.ExecPenaltyAccrualForContract", JobCode.Start, JobStatus.Success, EntityType.Contract, contract.Id, requestData, null);
                _penaltyAccrualService.Execute(contract, date, Constants.ADMINISTRATOR_IDENTITY);
                _jobLog.Log("PenaltyAccrualJob.ExecPenaltyAccrualForContract", JobCode.End, JobStatus.Success, EntityType.Contract, contract.Id, requestData, null);
            }
            catch (Exception ex)
            {
                string exJson = JsonConvert.SerializeObject(ex);
                _jobLog.Log("PenaltyAccrualJob.ExecPenaltyAccrualForContract", JobCode.Error, JobStatus.Failed, EntityType.Contract, contractId, requestData, exJson);
                throw;
            }
        }
    }
}
