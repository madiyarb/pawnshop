using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.PaymentSchedules;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Services.Contracts
{
    public class ContractPaymentScheduleService : IContractPaymentScheduleService
    {
        private readonly ContractPaymentScheduleRepository _contractPaymentScheduleRepository;
        private readonly UserRepository _userRepository;
        private readonly ContractRepository _contractRepository;
        private readonly IContractActionService _contractActionService;
        private readonly IPaymentScheduleService _paymentScheduleService;
        private readonly IContractService _contractService;
        private readonly RestructuredContractPaymentScheduleRepository _restructuredScheduleRepository;

        public ContractPaymentScheduleService(
            ContractPaymentScheduleRepository contractPaymentScheduleRepository,
            UserRepository userRepository,
            ContractRepository contractRepository,
            IContractActionService contractActionService,
            IPaymentScheduleService paymentScheduleService,
            IContractService contractService,
            RestructuredContractPaymentScheduleRepository restructuredScheduleRepository)
        {
            _contractPaymentScheduleRepository = contractPaymentScheduleRepository;
            _userRepository = userRepository;
            _contractRepository = contractRepository;
            _contractActionService = contractActionService;
            _paymentScheduleService = paymentScheduleService;
            _contractService = contractService;
            _restructuredScheduleRepository = restructuredScheduleRepository;
        }

        public ContractPaymentSchedule Get(int id)
        {
            ContractPaymentSchedule schedule = _contractPaymentScheduleRepository.Get(id);
            if (schedule == null)
                throw new PawnshopApplicationException($"График с идентификатором {id} не найден");

            return schedule;
        }

        public List<ContractPaymentSchedule> GetListByContractId(int contractId, bool withoutCheckContract = false)
        {
            if (!withoutCheckContract)
            {
                Contract contract = _contractRepository.GetOnlyContract(contractId);
                if (contract == null)
                    throw new PawnshopApplicationException($"Договор {contractId} не найден");
            }

            List<ContractPaymentSchedule> schedules = _contractPaymentScheduleRepository.GetListByContractId(contractId);
            return schedules;
        }

        public void Delete(int scheduleId, int authorId)
        {
            User author = _userRepository.Get(authorId);
            if (author == null)
                throw new PawnshopApplicationException($"Пользователь {authorId} не найден");

            ContractPaymentSchedule schedule = Get(scheduleId);
            Delete(schedule, author.Id);
        }

        public void Save(List<ContractPaymentSchedule> schedules, int contractId, int authorId, bool logDelete = true)
        {
            if (schedules == null)
                throw new ArgumentNullException(nameof(schedules));

            User author = _userRepository.Get(authorId);
            if (author == null)
                throw new PawnshopApplicationException($"Пользователь {authorId} не найден");

            Contract contract = _contractRepository.GetOnlyContract(contractId);
            if (contract == null)
                throw new PawnshopApplicationException($"Договор {contractId} не найден");

            using (IDbTransaction transaction = _contractPaymentScheduleRepository.BeginTransaction())
            {
                List<ContractPaymentSchedule> existingContractPaymentSchedules = GetListByContractId(contractId);
                Dictionary<int, ContractPaymentSchedule> existingSchedulesDict = existingContractPaymentSchedules.ToDictionary(s => s.Id, s => s);
                foreach (ContractPaymentSchedule schedule in schedules)
                {
                    schedule.ContractId = contract.Id;
                    ContractPaymentSchedule scheduleFromDB = null;
                    if (schedule.Id > 0)
                    {
                        if (!existingSchedulesDict.ContainsKey(schedule.Id))
                            throw new PawnshopApplicationException($"График {schedule.Id} не найден");

                        scheduleFromDB = existingSchedulesDict[schedule.Id];
                    }

                    Save(schedule, scheduleFromDB, author.Id);
                    existingSchedulesDict.Remove(schedule.Id);
                }

                foreach ((int id, ContractPaymentSchedule schedule) in existingSchedulesDict)
                    Delete(schedule, author.Id, logDelete);

                transaction.Commit();
            }
        }

        public void Save(ContractPaymentSchedule schedule, int authorId)
        {
            if (schedule == null)
                throw new ArgumentNullException(nameof(schedule));

            User author = _userRepository.Get(authorId);
            if (author == null)
                throw new PawnshopApplicationException($"Пользователь {authorId} не найден");

            ContractPaymentSchedule scheduleFromDB = null;
            if (schedule.Id > 0)
            {
                scheduleFromDB = Get(schedule.Id);
                if (scheduleFromDB == null)
                    throw new PawnshopApplicationException($"График {schedule.Id} не найден");
            }

            Save(schedule, scheduleFromDB, author.Id);
        }

        public ContractPaymentScheduleRevision GetRevision(int id, int revisionId)
        {
            Get(id);
            return _contractPaymentScheduleRepository.GetRevision(id, revisionId);
        }

        private void Delete(ContractPaymentSchedule schedule, int authorId, bool logChanges = true)
        {
            if (schedule == null)
                throw new ArgumentNullException(nameof(schedule));

            using (IDbTransaction transaction = _contractPaymentScheduleRepository.BeginTransaction())
            {
                schedule.Revision++;
                schedule.DeleteDate = DateTime.Now;
                _contractPaymentScheduleRepository.Update(schedule);
                if (logChanges)
                {
                    _contractPaymentScheduleRepository.LogChanges(schedule, authorId);
                }
                transaction.Commit();
            }
        }

        private void Save(ContractPaymentSchedule schedule, ContractPaymentSchedule scheduleFromDB, int authorId)
        {
            if (schedule == null)
                throw new ArgumentNullException(nameof(schedule));

            if (schedule.Id > 0)
            {
                if (scheduleFromDB == null)
                    throw new ArgumentNullException(nameof(scheduleFromDB));

                if (scheduleFromDB.Id != schedule.Id)
                    throw new ArgumentException($"Поле должно соответвовать с {nameof(schedule)}.{nameof(schedule.Id)}", nameof(scheduleFromDB));
            }
            else if (scheduleFromDB != null)
                throw new ArgumentException($"При {nameof(schedule)}{nameof(schedule.Id)} = 0, нельзя передавать этот параметр ", nameof(scheduleFromDB));

            using (IDbTransaction transaction = _contractPaymentScheduleRepository.BeginTransaction())
            {
                if (schedule.Id > 0)
                {
                    bool changed = scheduleFromDB.PenaltyCost != schedule.PenaltyCost
                        || scheduleFromDB.PercentCost != schedule.PercentCost
                        || scheduleFromDB.Prolongated != schedule.Prolongated
                        || scheduleFromDB.DebtLeft != schedule.DebtLeft
                        || scheduleFromDB.DebtCost != schedule.DebtCost
                        || scheduleFromDB.Canceled != schedule.Canceled
                        || scheduleFromDB.ContractId != schedule.ContractId
                        || scheduleFromDB.Date != schedule.Date
                        || scheduleFromDB.ActualDate != schedule.ActualDate
                        || scheduleFromDB.ActionId != schedule.ActionId
                        || scheduleFromDB.Period != schedule.Period
                        || scheduleFromDB.CreateDate != schedule.CreateDate
                        || scheduleFromDB.DeleteDate != schedule.DeleteDate
                        || scheduleFromDB.NextWorkingDate != schedule.NextWorkingDate;

                    if (changed)
                    {
                        scheduleFromDB.PenaltyCost = schedule.PenaltyCost;
                        scheduleFromDB.PercentCost = schedule.PercentCost;
                        scheduleFromDB.Prolongated = schedule.Prolongated;
                        scheduleFromDB.Revision++;
                        schedule.Revision = scheduleFromDB.Revision;
                        scheduleFromDB.DebtLeft = schedule.DebtLeft;
                        scheduleFromDB.DebtCost = schedule.DebtCost;
                        scheduleFromDB.Canceled = schedule.Canceled;
                        scheduleFromDB.ContractId = schedule.ContractId;
                        scheduleFromDB.Date = schedule.Date;
                        scheduleFromDB.ActualDate = schedule.ActualDate;
                        scheduleFromDB.ActionId = schedule.ActionId;
                        scheduleFromDB.Period = schedule.Period;
                        scheduleFromDB.CreateDate = schedule.CreateDate;
                        scheduleFromDB.DeleteDate = schedule.DeleteDate;
                        scheduleFromDB.NextWorkingDate = schedule.NextWorkingDate;
                        _contractPaymentScheduleRepository.Update(schedule);
                        _contractPaymentScheduleRepository.LogChanges(schedule, authorId);
                    
                        if (schedule is RestructuredContractPaymentSchedule scheduleRestructured)
                        {
                            _restructuredScheduleRepository.Update(scheduleRestructured);
                        }
                    }
                }
                else
                {
                    schedule.Revision = 1;
                    _contractPaymentScheduleRepository.Insert(schedule);
                    _contractPaymentScheduleRepository.LogChanges(schedule, authorId);

                    if (schedule is RestructuredContractPaymentSchedule scheduleRestructured)
                    {
                        _restructuredScheduleRepository.Insert(scheduleRestructured);
                    }
                }

                transaction.Commit();
            }
        }

        public async Task<List<ContractPaymentScheduleVersion>> GetScheduleVersions(int ContractId)
        {
            var versions = await _contractPaymentScheduleRepository.GetScheduleVersions(ContractId);
            if (versions != null)
            {
                if (versions.Count > 1)
                {
                    for (int i = versions.Count - 1; i > 0; i--)
                    {
                        versions[i].ScheduleDate = versions[i - 1].ScheduleDate;
                    }
                }
                if (versions.Count > 0)
                {
                    var contract = _contractRepository.GetOnlyContract(ContractId);
                    versions[0].ScheduleDate = contract.ContractDate;
                }
            }
            return versions;
        }

        public async Task<List<ContractPaymentScheduleVersion>> GetScheduleVersionsWithoutChangedControlDate(int ContractId)
        {
            var versions = await _contractPaymentScheduleRepository.GetScheduleVersionsWithoutChangedControlDate(ContractId);
            if (versions != null)
            {
                if (versions.Count > 1)
                {
                    for (int i = versions.Count - 1; i > 0; i--)
                    {
                        versions[i].ScheduleDate = versions[i - 1].ScheduleDate;
                    }
                }
                if (versions.Count > 0)
                {
                    var contract = _contractRepository.GetOnlyContract(ContractId);
                    versions[0].ScheduleDate = contract.ContractDate;
                }
            }
            return versions;
        }

        public async Task<List<RestructuredContractPaymentSchedule>> GetScheduleByAction(int ActionId)
        {
            return await _contractPaymentScheduleRepository.GetScheduleByAction(ActionId);
        }

        public async Task<List<ContractPaymentSchedule>> GetScheduleByActionForChangeDate(int ActionId)
        {
            return await _contractPaymentScheduleRepository.GetScheduleByActionForChangeDate(ActionId);
        }

        public async Task<List<ContractPaymentSchedule>> GetScheduleByHistory(int HistoryId)
        {
            return await _contractPaymentScheduleRepository.GetScheduleByHistory(HistoryId);
        }

        public async Task UpdateActionIdForPartialPayment(int ActionId, DateTime ActionDate, int ContractId)
        {
            await _contractPaymentScheduleRepository.UpdateActionIdForPartialPayment(ActionId, ActionDate, ContractId);
        }

        public async Task UpdateActionIdForPartialPaymentUnpaid(int ActionId, DateTime ActionDate, int ContractId, decimal penaltyCost, bool isEndPeriod)
        {
            await _contractPaymentScheduleRepository.UpdateActionIdForPartialPaymentUnpaid(ActionId, ActionDate, ContractId, penaltyCost, isEndPeriod);
        }

        public async Task<ContractPaymentSchedule> GetUnpaidSchedule(int ContractId)
        {
            return await _contractPaymentScheduleRepository.GetUnpaidSchedule(ContractId);
        }

        public async Task<ContractPaymentSchedule> GetNextPaymentSchedule(int ContractId, bool nowPeriodPayment = false)
        {
            return await _contractPaymentScheduleRepository.GetNextPaymentSchedule(ContractId, nowPeriodPayment);
        }

        public async Task<List<ContractPaymentSchedule>> GetScheduleRowsAfterPartialPayment(int ActionId, int ContractId)
        {
            return await _contractPaymentScheduleRepository.GetScheduleRowsAfterPartialPayment(ActionId, ContractId);
        }

        public async Task<List<ContractPaymentSchedule>> GetScheduleRowsBeforePartialPayment(int ContractId)
        {
            return await _contractPaymentScheduleRepository.GetScheduleRowsBeforePartialPayment(ContractId);
        }

        public async Task<int> InsertContractPaymentScheduleHistory(int ContractId, int ActionId, int Status)
        {
            return await _contractPaymentScheduleRepository.InsertContractPaymentScheduleHistory(ContractId, ActionId, Status);
        }

        public async Task InsertContractPaymentScheduleHistoryItems(int ContractPaymentScheduleHistoryId, ContractPaymentSchedule item)
        {
            await _contractPaymentScheduleRepository.InsertContractPaymentScheduleHistoryItems(ContractPaymentScheduleHistoryId, item);
        }

        public async Task UpdateContractPaymentScheduleHistoryStatus(int ContractId, int ActionId, int Status)
        {
            await _contractPaymentScheduleRepository.UpdateContractPaymentScheduleHistoryStatus(ContractId, ActionId, Status);
        }

        public async Task DeleteContractPaymentScheduleHistory(int ContractId, int ActionId)
        {
            await _contractPaymentScheduleRepository.DeleteContractPaymentScheduleHistory(ContractId, ActionId);
        }

        public async Task<List<ContractPaymentSchedule>> GetScheduleRowsAfterLastPartialPayment(int ContractId, int ActionId)
        {
            return await _contractPaymentScheduleRepository.GetScheduleRowsAfterLastPartialPayment(ContractId, ActionId);
        }

        public async Task<ContractPaymentSchedule> GetLastPartialPaymentScheduleRow(int ContractId, int ActionId)
        {
            return await _contractPaymentScheduleRepository.GetLastPartialPaymentScheduleRow(ContractId, ActionId);
        }

        public async Task<ContractPaymentSchedule> GetLastPartialPaymentScheduleHistoryRow(int ContractId, int ActionId)
        {
            return await _contractPaymentScheduleRepository.GetLastPartialPaymentScheduleHistoryRow(ContractId, ActionId);
        }

        public async Task RollbackScheduleToPreviousPartialPayment(int ContractId, int ActionId, decimal PartialPaymentCost)
        {
            await _contractPaymentScheduleRepository.RollbackScheduleToPreviousPartialPayment(ContractId, ActionId, PartialPaymentCost);
        }

        public async Task<decimal> GetAverageMonthlyPaymentAsync(int contractId)
        {
            return await _contractPaymentScheduleRepository.GetAverageMonthlyPaymentAsync(contractId);
        }

        public bool IsNeedUpdatePaymentSchedule(List<ContractPaymentSchedule> paymentSchedule, int contractId)
        {
            var dbPaymentSchedule = _contractPaymentScheduleRepository.GetListByContractId(contractId);

            if (!dbPaymentSchedule.Any())
                return true;

            var dbFirstPayment = dbPaymentSchedule.OrderBy(x => x.Date).FirstOrDefault();
            var currentFirstPayment = paymentSchedule.OrderBy(x => x.Date).FirstOrDefault();

            if (dbFirstPayment.Date != currentFirstPayment.Date ||
                dbFirstPayment.DebtCost != currentFirstPayment.DebtCost ||
                dbFirstPayment.PercentCost != currentFirstPayment.PercentCost ||
                dbFirstPayment.Period != currentFirstPayment.Period)
                return true;

            if (dbPaymentSchedule.Sum(x => x.DebtCost) != paymentSchedule.Sum(x => x.DebtCost) ||
                dbPaymentSchedule.Sum(x => x.PercentCost) != paymentSchedule.Sum(x => x.PercentCost) ||
                dbPaymentSchedule.Count() != paymentSchedule.Count())
                return true;

            return false;
        }

        public void UpdateFirstPaymentInfo(int contractId, Contract contract = null)
        {
            contract ??= _contractRepository.Get(contractId);

            if (contract == null)
                throw new PawnshopApplicationException($"Контракт {contractId} не найден!");

            if (contract.Setting == null)
                throw new PawnshopApplicationException($"У контракта {contractId} не найден продукт!");

            if (!contract.SignDate.HasValue)
                throw new PawnshopApplicationException($"У контракта {contractId} нет даты подписания!");

            var firstPayment = contract.PaymentSchedule.OrderBy(x => x.Date).FirstOrDefault();

            if (contract.ContractDate.Date == contract.SignDate.Value.Date)
                return;

            var payDays = Math.Abs((contract.SignDate.Value.Date - firstPayment.Date).Days);
            firstPayment.Period = payDays;

            if (contract.Setting.ScheduleType == ScheduleType.Annuity || contract.Setting.ScheduleType == ScheduleType.Discrete)
                firstPayment.PercentCost = Math.Round(payDays * (contract.Setting.LoanPercent * 30 * 12 / 365 / 100) * contract.LoanCost, 2);
            else if (contract.Setting.ScheduleType == ScheduleType.Differentiated)
                firstPayment.PercentCost = Math.Round(contract.LoanCost * payDays * (contract.Setting.LoanPercent / 100), 2);

            _contractPaymentScheduleRepository.Update(firstPayment);
        }

        public async Task<Contract> SaveBuilderByControlDate(ContractPaymentScheduleUpdateModel updateModel)
        {
            var contractId = updateModel.ContractId;
            Contract contract = _contractRepository.Get(contractId);
            var fPaymentDateString = contract.FirstPaymentDate?.ToString("dd.MM.yyy");
            if (contract == null)
                throw new PawnshopApplicationException($"Договор c id = {contractId} не найден");

            foreach (var item in contract.PaymentSchedule)
            {
                if (item.ActionId.HasValue)
                {
                    var act = await _contractActionService.GetAsync(item.ActionId.Value);
                    if (act != null)
                    {
                        if (act.ActionType != ContractActionType.PartialPayment)
                        {
                            item.ActionType = (int)act.ActionType;
                        }
                        else if (act.ActionType == ContractActionType.PartialPayment && Math.Round(act.Cost) == Math.Round(item.DebtCost))
                        {
                            item.ActionType = (int)act.ActionType;
                        }
                    }
                }
            }

            if (contract.PaymentSchedule.Any(x => x.ActionId == null && x.ActionType == null && x.Date == DateTime.Now.Date))
            {
                throw new PawnshopApplicationException($"У договора c id = {contract.Id} есть неоплаченный платеж");
            }

            _paymentScheduleService.BuildForChangeControlDate(contract, updateModel.ChangedControlDate.Value);

            var prevContract = _contractRepository.Get(contract.Id);

            var action = new ContractAction()
            {
                ActionType = ContractActionType.ControlDateChange,
                Date = DateTime.Now,
                Reason =
                    $"Изменение даты погашения {contract.ContractNumber} от {contract.ContractDate.ToString("dd.MM.yyyy")} с {fPaymentDateString} на {updateModel.ChangedControlDate?.ToString("dd.MM.yyyy")}",
                TotalCost = 0,
                Cost = 0,
                ContractId = contract.Id,
                AuthorId = 1,
                CreateDate = DateTime.Now
            };
            _contractActionService.Save(action);
            var historyId = await InsertContractPaymentScheduleHistory(contract.Id, action.Id, (int)ContractActionStatus.Canceled);
            foreach (var item in prevContract.PaymentSchedule)
            {
                if (item.ActionId.HasValue)
                {
                    var act = await _contractActionService.GetAsync(item.ActionId.Value);
                    if (act != null)
                    {
                        if (act.ActionType != ContractActionType.PartialPayment)
                        { item.ActionType = (int)act.ActionType; }
                        else
                        {
                            if (act.ActionType == ContractActionType.PartialPayment &&
                                Math.Round(act.Cost) == Math.Round(item.DebtCost))
                            { item.ActionType = (int)act.ActionType; }
                        }
                    }
                }
                await InsertContractPaymentScheduleHistoryItems(historyId, item);
            }

            //int authorId = _sessionContext.UserId;

            var payments = contract.PaymentSchedule.Where(x => x.ActionType != 40);
            var date = payments.First().Date;
            if (prevContract.FirstPaymentDate != date)
            {
                prevContract.FirstPaymentDate = date;
            }

            prevContract.PaymentSchedule = contract.PaymentSchedule;
            prevContract.MaturityDate = prevContract.PaymentSchedule.Max(x => x.Date);
            _contractService.CheckSchedule(prevContract, true);

            prevContract.NextPaymentDate = prevContract.PercentPaymentType == PercentPaymentType.EndPeriod
                ? prevContract.MaturityDate
                : prevContract.PaymentSchedule.Where(x => x.ActionId == null && x.Canceled == null).Min(x => x.Date);

            if (prevContract.PaymentSchedule.Where(x => prevContract.PercentPaymentType != PercentPaymentType.EndPeriod && x.Date == DateTime.Now.Date && x.ActionId == null && x.Canceled == null).Any())
            {
                throw new PawnshopApplicationException("Не возможно сделать ЧДП, т.к. по договору имеется не оплаченный платеж с сегодняшней датой оплаты. Вначале сделайте погашение имеющейся задолженности по договору через функционал оплаты(кнопка 'Оплата').");
            }

            return prevContract;
        }
    }
}
