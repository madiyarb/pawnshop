using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Contracts;
using Pawnshop.Web.Engine.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Membership;
using Pawnshop.AccountingCore.Models;
using System.Data;
using Pawnshop.Data.Models.Mintos;
using Pawnshop.Services;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Core;
using Newtonsoft.Json;
using Pawnshop.Web.Controllers.Api;
using Pawnshop.Web.Models.Contract;
using Pawnshop.Services.Crm;
using Pawnshop.Services.Models.Calculation;
using Pawnshop.Services.Calculation;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Services.PenaltyLimit;

namespace Pawnshop.Web.Engine.Services
{
    public class ContractActionProlongService : IContractActionProlongService
    {
        private readonly IContractActionOperationService _contractActionOperationService;
        private readonly IContractService _contractService;
        private readonly GroupRepository _groupRepository;
        private readonly UserRepository _userRepository;
        private readonly MintosContractRepository _mintosContractRepository;
        private readonly IEventLog _eventLog;
        private readonly MintosContractActionRepository _mintosContractActionRepository;
        private readonly ICrmPaymentService _crmPaymentService;
        private readonly IContractDutyService _contractDutyService;
        private readonly IContractPaymentScheduleService _contractPaymentScheduleService;
        private readonly IContractPaymentService _contractPaymentService;
        private readonly IPenaltyRateService _penaltyRateService;
        private readonly IContractActionService _contractActionService;

        private string _notEnoughPermissionError = "У вас недостаточно прав для проведения действий другим числом. Обратитесь к администратору.";
        private string _badDateError = "Дата не может быть меньше даты последнего действия по договору";

        public ContractActionProlongService(
            IContractActionOperationService contractActionOperationService,
            IContractService contractService,
            GroupRepository groupRepository,
            UserRepository userRepository,
            MintosContractRepository mintosContractRepository,
            IEventLog eventLog, IContractDutyService contractDutyService,
            MintosContractActionRepository mintosContractActionRepository,
            ICrmPaymentService crmPaymentService,
            IContractPaymentScheduleService contractPaymentScheduleService,
            IContractPaymentService contractPaymentService,
            IPenaltyRateService penaltyRateService,
            IContractActionService contractActionService)
        {
            _groupRepository = groupRepository;
            _contractActionOperationService = contractActionOperationService;
            _contractService = contractService;
            _userRepository = userRepository;
            _mintosContractRepository = mintosContractRepository;
            _eventLog = eventLog;
            _mintosContractActionRepository = mintosContractActionRepository;
            _crmPaymentService = crmPaymentService;
            _contractDutyService = contractDutyService;
            _contractPaymentScheduleService = contractPaymentScheduleService;
            _contractPaymentService = contractPaymentService;
            _penaltyRateService = penaltyRateService;
            _contractActionService = contractActionService;
        }

        public void Exec(ContractAction action, int authorId, int branchId, bool forceExpensePrepaymentReturn, bool autoApprove = false)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (action.Id != 0)
                throw new ArgumentException($"Свойство {nameof(action.Id)} должно равно 0", nameof(action));

            if (action.Rows == null)
                throw new ArgumentException($"Свойство {nameof(action.Rows)} не должно быть null", nameof(action));

            if (action.ActionType != ContractActionType.Prolong)
                throw new ArgumentException($"Свойство {nameof(action.ActionType)} должно быть {ContractActionType.Prolong}", nameof(action));

            Group branch = _groupRepository.Get(branchId);
            if (branch == null)
                throw new PawnshopApplicationException($"Филиал {branchId} не найден");

            User author = _userRepository.Get(authorId);
            if (author == null)
                throw new PawnshopApplicationException($"Автор {authorId} не найден");

            Contract contract = _contractService.Get(action.ContractId);
            if (contract == null)
                throw new PawnshopApplicationException($"Договор {action.ContractId}");

            if (contract.Status != ContractStatus.Signed)
                throw new PawnshopApplicationException($"Договор {contract.Id} должен был подписан");

            if (contract.PercentPaymentType != PercentPaymentType.EndPeriod)
                throw new PawnshopApplicationException($"Для данного договора({contract.Id}) невозможна пролонгация");

            if (contract.LoanPeriod <= 0)
                throw new PawnshopApplicationException("Период займа договора меньше или равно 0, ожидалось получить значение больше 0");

            if (action.Date.Date != DateTime.Now.Date && !author.ForSupport)
                throw new PawnshopApplicationException(_notEnoughPermissionError);

            if (autoApprove)
            {
                var incompleteExists = _contractActionService.IncopleteActionExists(contract.Id).Result;
                if (incompleteExists)
                    throw new PawnshopApplicationException("В договоре имеется неподтвержденное действие");
            }

            action.ExtraExpensesCost = action.ExtraExpensesCost ?? 0;
            ContractDutyCheckModel prolongContractDutyCheckModel = new ContractDutyCheckModel
            {
                ActionType = ContractActionType.Prolong,
                ContractId = contract.Id,
                Cost = action.Cost,
                Date = action.Date,
                PayTypeId = action.PayTypeId,
            };

            ContractDuty prolongDuty = _contractDutyService.GetContractDuty(prolongContractDutyCheckModel);
            if (prolongDuty == null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(prolongContractDutyCheckModel)} не будет равен null");

            List<ContractActionRow> prolongRows = prolongDuty.Rows;
            if (prolongRows == null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(prolongRows)} не будет равен null");

            List<ContractActionRow> actionProlongRows = action.Rows.ToList();
            if (prolongRows.Count == 0)
                return;

            List<ContractActionRow> actionProlongRowsFromDuty = prolongRows;
            var actionProlongRowsSet = actionProlongRows.ToHashSet();
            foreach (ContractActionRow prolongRowFromDB in prolongRows)
            {
                var prolongRowsToDelete = new List<ContractActionRow>();
                foreach (ContractActionRow prolongRow in actionProlongRowsSet)
                {
                    bool isSimilar =
                        prolongRowFromDB.Id == prolongRow.Id
                        && prolongRowFromDB.CreditAccountId == prolongRow.CreditAccountId
                        && prolongRowFromDB.DebitAccountId == prolongRow.DebitAccountId
                        && prolongRowFromDB.Cost == prolongRow.Cost
                        && prolongRowFromDB.BusinessOperationSettingId == prolongRow.BusinessOperationSettingId
                        && prolongRowFromDB.ActionId == prolongRow.ActionId;

                    if (isSimilar)
                        prolongRowsToDelete.Add(prolongRow);
                }

                foreach (ContractActionRow prolongRowToDelete in prolongRowsToDelete)
                {
                    actionProlongRowsSet.Remove(prolongRowToDelete);
                }
            }

            if (actionProlongRowsSet.Count > 0)
                throw new PawnshopApplicationException("Проводки действия не сходятся с реальными проводками действия 'Продление' с базы");

            decimal prolongRowsAmount = actionProlongRows.Sum(r => r.Cost);
            decimal extraExpensesCost = action.ExtraExpensesCost ?? 0;
            decimal actualExtraExpensesCost = _contractService.GetExtraExpensesCost(contract.Id, action.Date);
            if (extraExpensesCost != actualExtraExpensesCost)
                throw new PawnshopApplicationException("Суммы доп расходов не сходится с присланным доп расходом");

            using (IDbTransaction transaction = _groupRepository.BeginTransaction())
            {
                OrderStatus? orderStatus = null;
                if (autoApprove)
                    orderStatus = OrderStatus.Approved;

                _contractActionOperationService.Register(contract, action, authorId, branchId: branchId, forceExpensePrepaymentReturn: forceExpensePrepaymentReturn, orderStatus: orderStatus);

                if (autoApprove)
                    ExecOnApprove(action, authorId, contract);
                
                transaction.Commit();
            }
        }

        public void ExecOnApprove(ContractAction action, int authorId, Contract contract = null)
        {

            if (contract == null)
            {
                contract = _contractService.Get(action.ContractId);
            }

            if (contract == null)
                throw new PawnshopApplicationException($"Договор {action.ContractId} не найден");

            if (contract.Status != ContractStatus.Signed)
                throw new PawnshopApplicationException($"Договор {contract.Id} должен был подписан");

            if (contract.PercentPaymentType != PercentPaymentType.EndPeriod)
                throw new PawnshopApplicationException($"Для данного договора({contract.Id}) невозможна пролонгация");

            if (contract.LoanPeriod <= 0)
                throw new PawnshopApplicationException("Период займа договора меньше или равно 0, ожидалось получить значение больше 0");

            if (action.Date.Date < contract.Actions.Max(x => x.Date))
                throw new PawnshopApplicationException(_badDateError);

            User author = _userRepository.Get(authorId);
            if (author == null)
                throw new PawnshopApplicationException($"Автор {authorId} не найден");

            using (IDbTransaction transaction = _groupRepository.BeginTransaction())
            {
                _crmPaymentService.Enqueue(contract);

                contract.ProlongDate = action.Date;
                contract.MaturityDate = action.Date.AddDays(contract.LoanPeriod);
                contract.NextPaymentDate = contract.MaturityDate;
                ContractPaymentSchedule lastPayedSchedule = contract
                    .PaymentSchedule.Where(
                    s => !s.Canceled.HasValue
                    && !s.DeleteDate.HasValue
                    && s.ActualDate.HasValue
                    && s.ActionId.HasValue
                    && s.ActualDate.Value.Date < action.Date).OrderByDescending(s => s.Date).FirstOrDefault();

                DateTime previousProlongDate = lastPayedSchedule != null ?
                    lastPayedSchedule.ActualDate.Value : contract.ContractDate.Date;
                int daysBetweenCurrentProlongAndPreviousProlong = (action.Date - previousProlongDate).Days;
                if (daysBetweenCurrentProlongAndPreviousProlong < 0)
                    throw new PawnshopApplicationException($"{nameof(daysBetweenCurrentProlongAndPreviousProlong)} не может быть меньше нуля");

                decimal? changedPercentCost = null;

                if (daysBetweenCurrentProlongAndPreviousProlong < contract.LoanPeriod)
                {
                    DateTime beginDate = previousProlongDate.Date.AddDays(1);
                    beginDate = beginDate > action.Date ? action.Date : beginDate;

                    List<CashOrder> payedCashOrders = _contractPaymentService.GetPayedInterest(contract.Id, action.Date, beginDate);
                    decimal changedPercentCostTemp = payedCashOrders.Sum(co => co.StornoId.HasValue ? -co.OrderCost : co.OrderCost);

                    if (changedPercentCostTemp <= 0)
                        throw new PawnshopApplicationException($"Высчитанный оплаченный процент должен быть больше нуля");

                    changedPercentCost = changedPercentCostTemp;
                }

                if (changedPercentCost.HasValue)
                    changedPercentCost = Math.Round(changedPercentCost.Value, 2);

                ContractPaymentSchedule schedule = contract
                    .PaymentSchedule.Where(
                    s => !s.Canceled.HasValue
                    && !s.DeleteDate.HasValue
                    && !s.ActionId.HasValue).SingleOrDefault();
                if (schedule == null)
                    throw new PawnshopApplicationException("Не найдена будущая дата графика");

                int calculatedPeriod = (action.Date.Date - contract.ContractDate).Days;
                if (!contract.Locked)
                    calculatedPeriod++;

                if (lastPayedSchedule != null)
                    calculatedPeriod = (action.Date.Date - lastPayedSchedule.ActualDate.Value).Days;

                if (calculatedPeriod < 0)
                    throw new PawnshopApplicationException($"Период пролонгируемого графика{schedule.Id} не может быть меньше нуля, расчет периода оказался отрицательным");

                decimal contractDue = 0;
                contractDue += _contractService.GetAccountBalance(action.ContractId, action.Date);
                contractDue += _contractService.GetOverdueAccountBalance(action.ContractId, action.Date);

                decimal loanCost = Math.Round(contractDue, 2);
                schedule.Prolongated = schedule.Date;
                schedule.ActualDate = action.Date;
                schedule.ActionId = action.Id;
                schedule.DebtCost = 0;
                schedule.DebtLeft = loanCost;
                schedule.PercentCost = changedPercentCost ?? schedule.PercentCost;
                schedule.PenaltyCost =
                    action.Rows.Where(x => (x.PaymentType == AmountType.DebtPenalty || x.PaymentType == AmountType.LoanPenalty) && x.DebitAccountId != null).Sum(r => r.Cost);
                schedule.Period = action.Date.Date > schedule.Date ? contract.LoanPeriod : calculatedPeriod;
                
                contract.LoanPercentCost = Math.Round(contractDue * contract.LoanPercent / 100, 4, MidpointRounding.AwayFromZero);
                decimal percentCostForNewScheduleItem = Math.Round(contract.LoanPercentCost * contract.LoanPeriod, 2);
                contract.PaymentSchedule.Add(new ContractPaymentSchedule
                {
                    PercentCost = percentCostForNewScheduleItem,
                    DebtLeft = 0,
                    DebtCost = loanCost,
                    Date = contract.MaturityDate,
                    Revision = 1,
                    Period = contract.LoanPeriod
                });

                _contractService.Save(contract);
                _contractPaymentScheduleService.Save(contract.PaymentSchedule, contract.Id, author.Id);

                _penaltyRateService.IncreaseRates(contract, action.Date, authorId, action);

                transaction.Commit();
            }
            ScheduleMintosPaymentUpload(_contractService.Get(action.ContractId), action);
        }

        private void ScheduleMintosPaymentUpload(Contract contract, ContractAction action)
        {
            var mintosContracts = _mintosContractRepository.GetByContractId(contract.Id);
            MintosContract mintosContract = null;
            if (mintosContracts.Count > 1)
            {
                mintosContract = mintosContracts.Where(x => x.MintosStatus.Contains("active")).FirstOrDefault();
                if (mintosContracts.Where(x => x.MintosStatus.Contains("active")).Count() > 1)
                {
                    _eventLog.Log(
                        EventCode.MintosContractUpdate,
                        EventStatus.Failed,
                        EntityType.MintosContract,
                        mintosContract.Id,
                        JsonConvert.SerializeObject(mintosContracts),
                        $"Договор {contract.ContractNumber}({contract.Id}) выгружен в Mintos {mintosContracts.Where(x => x.MintosStatus.Contains("active")).Count()} раз(а)");
                }
            }
            else if (mintosContracts.Count == 1)
            {
                mintosContract = mintosContracts.FirstOrDefault();
                if (mintosContract != null &&
                    mintosContracts.Where(x => x.MintosStatus.Contains("active")).Count() == 0)
                {
                    _eventLog.Log(
                        EventCode.MintosContractUpdate,
                        EventStatus.Failed,
                        EntityType.MintosContract,
                        mintosContract.Id,
                        JsonConvert.SerializeObject(mintosContracts),
                        $"Договор {contract.ContractNumber}({contract.Id}) выгружен в Mintos, но не проверен/утвержден модерацией Mintos)");
                }
            }
            else
            {
                return;
            }

            if (mintosContracts == null || mintosContract == null) return;
            if (!mintosContract.MintosStatus.Contains("active")) return;

            try
            {
                MintosContractAction mintosAction = new MintosContractAction(action);

                using (var transaction = _mintosContractRepository.BeginTransaction())
                {
                    mintosAction.MintosContractId = mintosContract.Id;

                    _mintosContractActionRepository.Insert(mintosAction);

                    transaction.Commit();
                }
            }
            catch (Exception e)
            {
                _eventLog.Log(
                    EventCode.MintosContractUpdate,
                    EventStatus.Failed,
                    EntityType.MintosContract,
                    mintosContract.Id,
                    responseData: e.Message);
            }
        }
    }
}
