using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Calculation;
using Pawnshop.Services.Crm;
using Pawnshop.Services.Expenses;
using Pawnshop.Services.Models.Calculation;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using Pawnshop.Services.PenaltyLimit;

namespace Pawnshop.Services.Contracts
{
    public class ContractPaymentService : IContractPaymentService
    {
        private readonly IDictionaryWithSearchService<PaymentOrder, PaymentOrderFilter> _paymentOrderService;
        private readonly IDictionaryWithSearchService<Account, AccountFilter> _accountService;
        private readonly IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> _accountSettingService;
        private List<Account> _accounts;
        private readonly ICrmPaymentService _crmPaymentService;
        private readonly ContractRepository _contractRepository;
        private readonly IContractActionCheckService _contractActionCheckService;
        private readonly IContractService _contractService;
        private readonly IExpenseService _expenseService;
        private readonly IContractExpenseService _contractExpenseService;
        private readonly IContractActionOperationService _contractActionOperationService;
        private readonly IBusinessOperationSettingService _businessOperationSettingService;
        private readonly ICashOrderService _cashOrderService;
        private readonly IContractDutyService _contractDutyService;
        private readonly IContractPaymentScheduleService _contractPaymentScheduleService;
        private readonly IPenaltyRateService _penaltyRateService;
        private readonly IContractCloseService _contractCloseService;
        private readonly IContractActionService _contractActionService;

        public ContractPaymentService(IDictionaryWithSearchService<PaymentOrder, PaymentOrderFilter> paymentOrderService,
            IDictionaryWithSearchService<Account, AccountFilter> accountService,
            IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> accountSettingService,
            ICrmPaymentService crmPaymentService, ContractRepository contractRepository,
            IContractActionCheckService contractActionCheckService, IContractService contractService,
            IExpenseService expenseService, IContractExpenseService contractExpenseService,
            IContractActionOperationService contractActionOperationService,
            IBusinessOperationSettingService businessOperationSettingService,
            ICashOrderService cashOrderService, IContractDutyService contractDutyService,
            IContractPaymentScheduleService contractPaymentScheduleService,
            IPenaltyRateService penaltyRateService,
            IContractCloseService contractCloseService,
            IContractActionService contractActionService)
        {
            _paymentOrderService = paymentOrderService;
            _accountService = accountService;
            _accountSettingService = accountSettingService;
            _crmPaymentService = crmPaymentService;
            _contractRepository = contractRepository;
            _contractActionCheckService = contractActionCheckService;
            _contractService = contractService;
            _expenseService = expenseService;
            _contractExpenseService = contractExpenseService;
            _contractActionOperationService = contractActionOperationService;
            _businessOperationSettingService = businessOperationSettingService;
            _cashOrderService = cashOrderService;
            _contractDutyService = contractDutyService;
            _contractPaymentScheduleService = contractPaymentScheduleService;
            _penaltyRateService = penaltyRateService;
            _contractCloseService = contractCloseService;
            _contractActionService = contractActionService;
        }

        private void OrderAccounts()
        {
            var orders = _paymentOrderService
                .List(new ListQueryModel<PaymentOrderFilter> { Model = new PaymentOrderFilter { IsActive = true }, Page = null }).List
                .OrderBy(x => x.SequenceNumber).ToList();

            _accounts = _accounts
                .Join(orders, a => a.AccountSettingId, o => o.AccountSettingId, (a, o) => new { order = o, account = a })
                .OrderBy(x => x.order.SequenceNumber)
                .Select(r => r.account).ToList();
        }

        public List<ContractActionRow> Preview(decimal cost, int contractId)
        {
            _accounts = _accountService.List(new ListQueryModel<AccountFilter>
            { Model = new AccountFilter { ContractId = contractId }, Page = null }).List;

            OrderAccounts();

            var leftCost = cost;
            var result = new List<ContractActionRow>();

            foreach (var account in _accounts)
            {
                if (leftCost <= 0) break;
                AccountSetting accountSetting = _accountSettingService.GetAsync(account.AccountSettingId).Result;

                if (accountSetting?.DefaultAmountType == null) throw new ArgumentNullException(nameof(accountSetting), $"Не определён вид суммы для счета {account.Code} - {account.Name}({account.AccountNumber})");

                ContractActionRow row = new ContractActionRow
                {
                    PaymentType = accountSetting.DefaultAmountType.Value,
                    CreditAccountId = account.Id,
                    CreditAccount = account
                };

                row.Cost = leftCost > account.Balance ? account.Balance : leftCost;
                leftCost -= row.Cost;
                result.Add(row);
                if (leftCost <= 0) break;
            }

            return result;
        }

        public void Payment(ContractAction action, int branchId, int authorId, bool forceExpensePrepaymentReturn, bool autoApprove)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (action.Id != 0)
                throw new ArgumentException($"Свойство {nameof(action.Id)} должно быть равно нулю", nameof(action));

            if (action.ActionType != ContractActionType.Payment)
                throw new ArgumentException($"Свойство {nameof(action.ActionType)} должно быть равно {ContractActionType.Payment}", nameof(action));

            if (action.Rows == null)
                throw new ArgumentException($"Свойство {nameof(action.Rows)} не должно быть null", nameof(action));

            if (action.ExtraExpensesIds == null)
                throw new ArgumentException($"Свойство {nameof(action.ExtraExpensesIds)} не должно быть null", nameof(action));

            if (action.ExtraExpensesCost.HasValue && action.ExtraExpensesCost.Value < 0)
                throw new ArgumentException($"Свойство {nameof(action.ExtraExpensesCost)} должно быть положительным числом(>=0)", nameof(action));

            _contractActionCheckService.ContractActionCheck(action);
            var contract = _contractRepository.Get(action.ContractId);
            if (!contract.SettingId.HasValue && contract.Status != ContractStatus.Signed)
                throw new InvalidOperationException("Оплата недоступна по данному договору");

            if (contract.SettingId.HasValue && contract.Setting.InitialFeeRequired.HasValue && !(contract.Setting.InitialFeeRequired > 0)
                 && contract.Status != ContractStatus.AwaitForInitialFee
                 && action.IsInitialFee.HasValue && action.IsInitialFee.Value)
                throw new InvalidOperationException($"Настройки продукта не предполагают внесение первоначального взноса");

            if (autoApprove)
            {
                var incompleteExists = _contractActionService.IncopleteActionExists(contract.Id).Result;
                if (incompleteExists)
                    throw new PawnshopApplicationException("В договоре имеется неподтвержденное действие");
            }

            action.AuthorId = authorId;
            action.ActionType = ContractActionType.Payment;
            action.CreateDate = DateTime.Now;

            decimal payableExtraContractExpensesCost = action.ExtraExpensesCost ?? 0;
            decimal prepaymentBalance = _contractService.GetPrepaymentBalance(action.ContractId);

            decimal totalCost = action.TotalCost;
            totalCost += payableExtraContractExpensesCost;

            decimal surplus = action.Cost - totalCost;
            decimal prepaymentCost = 0;
            if (Math.Round(surplus, 2) < 0)
                throw new PawnshopApplicationException($"{nameof(action)}.{nameof(action.Cost)} меньше чем сумма {nameof(action.TotalCost)}");

            decimal prepaymentAndTotalCostDifference = prepaymentBalance - totalCost;
            if (prepaymentAndTotalCostDifference < 0)
                prepaymentCost = Math.Abs(prepaymentAndTotalCostDifference);

            // если договор дискретный
            if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
            {
                var prolongationContractDutyModel = new ContractDutyCheckModel
                {
                    ActionType = ContractActionType.Prolong,
                    Cost = totalCost,
                    Date = action.Date,
                    PayTypeId = action.PayTypeId,
                    ContractId = action.ContractId
                };

                ContractDuty prolongationContractDuty = _contractDutyService.GetContractDuty(prolongationContractDutyModel);
                if (prolongationContractDuty == null)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(prolongationContractDuty)} не будет null");

                if (totalCost >= prolongationContractDuty.Cost)
                    throw new PawnshopApplicationException("Указанная в оплате сумма достаточна для продления, воспользуйтесь кнопкой продления");
            }

            using (IDbTransaction transaction = _contractRepository.BeginTransaction())
            {
                OrderStatus? orderStatus = null;
                if (autoApprove)
                    orderStatus = OrderStatus.Approved;

                _contractActionOperationService.Register(contract, action, authorId, branchId: branchId, forceExpensePrepaymentReturn: forceExpensePrepaymentReturn, orderStatus: orderStatus);

                //увеличиваем лимит кредитной линии на сумму ОД оплаты
                if (contract.ContractClass == ContractClass.Tranche)
                {
                    var creditLineContract = _contractRepository.Get(contract.CreditLineId.Value);
                    decimal cost = action.Rows.Where(x => x.PaymentType == AmountType.Debt || x.PaymentType == AmountType.OverdueDebt).Sum(x => x.Cost);
                    if (cost > 0)
                    {
                        ContractActionRow creditLineActionRow = new ContractActionRow
                        {
                            PaymentType = AmountType.CreditLineLimit,
                            Cost = cost
                        };
                        var creditLineActionRows = new List<ContractActionRow>();
                        creditLineActionRows.Add(creditLineActionRow);
                        ContractAction creditLineAction = new ContractAction()
                        {
                            ActionType = ContractActionType.Payment,
                            Date = action.Date,
                            Reason = $"Увеличение лимита КЛ при оплате договора займа {contract.ContractNumber} от {contract.ContractDate.ToString("dd.MM.yyyy")}",
                            TotalCost = cost,
                            Cost = cost,
                            ContractId = creditLineContract.Id,
                            ParentActionId = action.Id,
                            ParentAction = action,
                            PayTypeId = action.PayTypeId ?? null,
                            RequisiteId = action.RequisiteId ?? null,
                            RequisiteCost = action.PayTypeId.HasValue ? (int)action.Cost : 0,
                            Checks = action.Checks,
                            Files = action.Files
                        };
                        creditLineAction.Rows = creditLineActionRows.ToArray();
                        _contractActionOperationService.Register(creditLineContract, creditLineAction, authorId, branchId, forceExpensePrepaymentReturn: forceExpensePrepaymentReturn, orderStatus: orderStatus);
                        action.ChildActionId = creditLineAction.Id;
                        _contractActionService.Save(action);
                    }
                }

                if (autoApprove)
                    ExecuteOnApprove(action, branchId, authorId, forceExpensePrepaymentReturn);
            }
        }

        public ContractAction PaymentWithReturnContractAction(ContractAction action, int branchId, int authorId, bool forceExpensePrepaymentReturn, bool autoApprove)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (action.Id != 0)
                throw new ArgumentException($"Свойство {nameof(action.Id)} должно быть равно нулю", nameof(action));

            if (action.ActionType != ContractActionType.Payment)
                throw new ArgumentException($"Свойство {nameof(action.ActionType)} должно быть равно {ContractActionType.Payment}", nameof(action));

            if (action.Rows == null)
                throw new ArgumentException($"Свойство {nameof(action.Rows)} не должно быть null", nameof(action));

            if (action.ExtraExpensesIds == null)
                throw new ArgumentException($"Свойство {nameof(action.ExtraExpensesIds)} не должно быть null", nameof(action));

            if (action.ExtraExpensesCost.HasValue && action.ExtraExpensesCost.Value < 0)
                throw new ArgumentException($"Свойство {nameof(action.ExtraExpensesCost)} должно быть положительным числом(>=0)", nameof(action));

            _contractActionCheckService.ContractActionCheck(action);
            var contract = _contractRepository.Get(action.ContractId);
            if (!contract.SettingId.HasValue && contract.Status != ContractStatus.Signed)
                throw new InvalidOperationException("Оплата недоступна по данному договору");

            if (contract.SettingId.HasValue && contract.Setting.InitialFeeRequired.HasValue && !(contract.Setting.InitialFeeRequired > 0)
                 && contract.Status != ContractStatus.AwaitForInitialFee
                 && action.IsInitialFee.HasValue && action.IsInitialFee.Value)
                throw new InvalidOperationException($"Настройки продукта не предполагают внесение первоначального взноса");

            if (autoApprove)
            {
                var incompleteExists = _contractActionService.IncopleteActionExists(contract.Id).Result;
                if (incompleteExists)
                    throw new PawnshopApplicationException("В договоре имеется неподтвержденное действие");
            }

            action.AuthorId = authorId;
            action.ActionType = ContractActionType.Payment;
            action.CreateDate = DateTime.Now;

            decimal payableExtraContractExpensesCost = action.ExtraExpensesCost ?? 0;
            decimal prepaymentBalance = _contractService.GetPrepaymentBalance(action.ContractId);

            decimal totalCost = action.TotalCost;
            totalCost += payableExtraContractExpensesCost;

            decimal surplus = action.Cost - totalCost;
            decimal prepaymentCost = 0;
            if (Math.Round(surplus, 2) < 0)
                throw new PawnshopApplicationException($"{nameof(action)}.{nameof(action.Cost)} меньше чем сумма {nameof(action.TotalCost)}");

            decimal prepaymentAndTotalCostDifference = prepaymentBalance - totalCost;
            if (prepaymentAndTotalCostDifference < 0)
                prepaymentCost = Math.Abs(prepaymentAndTotalCostDifference);

            // если договор дискретный
            if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
            {
                var prolongationContractDutyModel = new ContractDutyCheckModel
                {
                    ActionType = ContractActionType.Prolong,
                    Cost = totalCost,
                    Date = action.Date,
                    PayTypeId = action.PayTypeId,
                    ContractId = action.ContractId
                };

                ContractDuty prolongationContractDuty = _contractDutyService.GetContractDuty(prolongationContractDutyModel);
                if (prolongationContractDuty == null)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(prolongationContractDuty)} не будет null");

                if (totalCost >= prolongationContractDuty.Cost)
                    throw new PawnshopApplicationException("Указанная в оплате сумма достаточна для продления, воспользуйтесь кнопкой продления");
            }

            using (IDbTransaction transaction = _contractRepository.BeginTransaction())
            {
                OrderStatus? orderStatus = null;
                if (autoApprove)
                    orderStatus = OrderStatus.Approved;

                _contractActionOperationService.Register(contract, action, authorId, branchId: branchId, forceExpensePrepaymentReturn: forceExpensePrepaymentReturn, orderStatus: orderStatus);

                //увеличиваем лимит кредитной линии на сумму ОД оплаты
                if (contract.ContractClass == ContractClass.Tranche)
                {
                    var creditLineContract = _contractRepository.Get(contract.CreditLineId.Value);
                    decimal cost = action.Rows.Where(x => x.PaymentType == AmountType.Debt || x.PaymentType == AmountType.OverdueDebt).Sum(x => x.Cost);
                    if (cost > 0)
                    {
                        ContractActionRow creditLineActionRow = new ContractActionRow
                        {
                            PaymentType = AmountType.CreditLineLimit,
                            Cost = cost
                        };
                        var creditLineActionRows = new List<ContractActionRow>();
                        creditLineActionRows.Add(creditLineActionRow);
                        ContractAction creditLineAction = new ContractAction()
                        {
                            ActionType = ContractActionType.Payment,
                            Date = action.Date,
                            Reason = $"Увеличение лимита КЛ при оплате договора займа {contract.ContractNumber} от {contract.ContractDate.ToString("dd.MM.yyyy")}",
                            TotalCost = cost,
                            Cost = cost,
                            ContractId = creditLineContract.Id,
                            ParentActionId = action.Id,
                            ParentAction = action,
                            PayTypeId = action.PayTypeId ?? null,
                            RequisiteId = action.RequisiteId ?? null,
                            RequisiteCost = action.PayTypeId.HasValue ? (int)action.Cost : 0,
                            Checks = action.Checks,
                            Files = action.Files
                        };
                        creditLineAction.Rows = creditLineActionRows.ToArray();
                        _contractActionOperationService.Register(creditLineContract, creditLineAction, authorId, branchId, forceExpensePrepaymentReturn: forceExpensePrepaymentReturn, orderStatus: orderStatus);
                        action.ChildActionId = creditLineAction.Id;
                        _contractActionService.Save(action);
                    }
                }


                if (autoApprove && contract.ContractClass != ContractClass.CreditLine)
                    ExecuteOnApprove(action, branchId, authorId, forceExpensePrepaymentReturn);
                transaction.Commit();
            }

            return action;
        }

        public void ExecuteOnApprove(ContractAction action, int branchId, int authorId, bool forceExpensePrepaymentReturn)
        {
            var contract = _contractRepository.Get(action.ContractId);

            List<ContractActionRow> contractActionRows = action.Rows.ToList();
            List<int> payableExtraContractExpensesIds = action.ExtraExpensesIds;
            decimal payableExtraContractExpensesCost = action.ExtraExpensesCost ?? 0;
            decimal actionRowsCost = 0;
            if (contractActionRows.Count > 0)
            {
                var amountTypeAndCostDict = new Dictionary<AmountType, decimal>();
                // Валидируем 
                foreach (ContractActionRow row in contractActionRows)
                {
                    decimal cost;
                    if (amountTypeAndCostDict.TryGetValue(row.PaymentType, out cost))
                    {
                        if (row.Cost != cost)
                            throw new PawnshopApplicationException(
                                $"Суммы в {nameof(contractActionRows)} по типу {row.PaymentType} не одинаковые");
                    }

                    amountTypeAndCostDict[row.PaymentType] = row.Cost;
                }

                actionRowsCost = amountTypeAndCostDict.Values.Sum();
            }

            using (IDbTransaction transaction = _contractRepository.BeginTransaction())
            {
                decimal prepaymentBalance = _contractService.GetPrepaymentBalance(action.ContractId);
                if (payableExtraContractExpensesIds != null && payableExtraContractExpensesIds.Count > 0)
                {
                    var contractExtraExpenses = new List<ContractExpense>();
                    HashSet<int> payableExtraExpensesIdsSet = payableExtraContractExpensesIds.ToHashSet();
                    if (payableExtraExpensesIdsSet.Count != payableExtraContractExpensesIds.Count)
                        throw new PawnshopApplicationException("Обнаружены дубли доп расходов");

                    List<Expense> expenseTypes = _expenseService.GetList(new ListQuery { Page = null });
                    HashSet<int> extraExpenseTypesIds = expenseTypes.Where(e => e.ExtraExpense).Select(e => e.Id).ToHashSet();
                    var contractExtraExpensesListModelQuery = new ListQueryModel<ContractExpenseFilter>
                    {
                        Page = null,
                        Model = new ContractExpenseFilter { ContractId = action.ContractId, IsPayed = false }
                    };

                    List<ContractExpense> contractExpenses = _contractExpenseService.List(contractExtraExpensesListModelQuery).List;
                    contractExtraExpenses = contractExpenses.Where(e => extraExpenseTypesIds.Contains(e.ExpenseId)).ToList();
                    if (!payableExtraExpensesIdsSet.IsSubsetOf(contractExtraExpenses.Select(e => e.Id)))
                        throw new PawnshopApplicationException("Список доп расходов не сходится со списком доп расходов базы");

                    List<ContractExpense> payableContractExtraExpensesFromDB =
                        contractExtraExpenses.Where(e => payableExtraExpensesIdsSet.Contains(e.Id)).ToList();
                    foreach (ContractExpense contractExpense in payableContractExtraExpensesFromDB)
                    {
                        ContractExpense contractExpenseWithRowsAndOrders = _contractExpenseService.GetAsync(contractExpense.Id).Result;
                        if (contractExpenseWithRowsAndOrders == null)
                            throw new PawnshopApplicationException($"Ожидалось {nameof(contractExpenseWithRowsAndOrders)} не будет null");

                        // берем те расходы у которых все ордера апрувнуты
                        if (contractExpenseWithRowsAndOrders.ContractExpenseRows.Any(r => r.ContractExpenseRowOrders.Any(r => r.Order.ApproveStatus != OrderStatus.Approved)))
                            throw new PawnshopApplicationException($"У доп. расхода {contractExpense.Reason} не все кассовые ордера подтверждены");
                    }

                    decimal payableContractExtraExpensesCost = payableContractExtraExpensesFromDB.Sum(e => e.TotalCost);
                    if (payableContractExtraExpensesCost != payableExtraContractExpensesCost)
                        throw new PawnshopApplicationException($"Сумма доп. расходов не сходится, она должна быть {payableContractExtraExpensesCost}");
                }

                decimal totalCost = action.TotalCost;
                totalCost += payableExtraContractExpensesCost;

                decimal surplus = action.Cost - totalCost;
                decimal prepaymentCost = 0;
                if (Math.Round(surplus, 2) < 0)
                    throw new PawnshopApplicationException($"{nameof(action)}.{nameof(action.Cost)} меньше чем сумма {nameof(action.TotalCost)}");

                decimal prepaymentAndTotalCostDifference = prepaymentBalance - totalCost;
                if (prepaymentAndTotalCostDifference < 0)
                    prepaymentCost = Math.Abs(prepaymentAndTotalCostDifference);

                prepaymentCost += surplus;

                if (totalCost > 0)
                {
                    _crmPaymentService.Enqueue(contract);

                    DateTime coreMigrationDate = Constants.CORE_MIGRATION_DATE;

                    var payedProfitCashOrders = GetPayedInterest(contract.Id, action.Date);

                    if (contract.PercentPaymentType != PercentPaymentType.EndPeriod)
                    {
                        decimal payedProfitCashOrdersSum = 0;
                        if (payedProfitCashOrders.Any())
                            payedProfitCashOrdersSum = payedProfitCashOrders.Sum(co => co.StornoId.HasValue ? -co.OrderCost : co.OrderCost);

                        decimal overPaidDifference = 0;

                        var scheduleBeforeMigration = new List<ContractPaymentSchedule>();
                        scheduleBeforeMigration = contract.PaymentSchedule.Where(x => !x.Canceled.HasValue && !x.DeleteDate.HasValue && x.ActualDate < coreMigrationDate).ToList();

                        if (scheduleBeforeMigration.Any())
                        {
                            var payedProfitCashOrdersBeforeMigration = new List<CashOrder>();
                            payedProfitCashOrdersBeforeMigration = payedProfitCashOrders.Where(co => co.OrderDate < coreMigrationDate).ToList();

                            if (payedProfitCashOrdersBeforeMigration.Any())
                            {
                                overPaidDifference = scheduleBeforeMigration.Sum(x => x.PercentCost);
                                overPaidDifference -= payedProfitCashOrdersBeforeMigration.Sum(co => co.OrderCost);
                                overPaidDifference = overPaidDifference < 0 ? overPaidDifference : 0;
                            }
                        }

                        foreach (ContractPaymentSchedule schedule in contract.PaymentSchedule)
                        {
                            if (schedule.Canceled.HasValue || schedule.DeleteDate.HasValue)
                                continue;

                            decimal schedulePercentCost = Math.Round(schedule.PercentCost, 2);

                            if (contract.IsContractRestructured)
                            {
                                decimal restructuredScheduleCost = 0;
                                var restructuredSchedule = contract.RestructedPaymentSchedule.FirstOrDefault(x => x.Id == schedule.Id);
                                if (restructuredSchedule != null) 
                                {
                                    restructuredScheduleCost += Math.Round(restructuredSchedule.PaymentBalanceOfDefferedPercent.Value, 2);
                                    restructuredScheduleCost += Math.Round(restructuredSchedule.PaymentBalanceOfOverduePercent.Value, 2);
                                    restructuredScheduleCost += Math.Round(restructuredSchedule.PaymentPenaltyOfOverdueDebt.Value, 2);
                                    restructuredScheduleCost += Math.Round(restructuredSchedule.PaymentPenaltyOfOverduePercent.Value, 2);
                                }

                                if (restructuredScheduleCost > 0)
                                {
                                    schedulePercentCost += restructuredScheduleCost;
                                }
                            }

                            if (schedule.ActualDate.HasValue && schedule.ActualDate.Value < coreMigrationDate)
                            {
                                if (payedProfitCashOrders.Any(co => co.ContractActionId == schedule.ActionId.Value))
                                {
                                    schedulePercentCost = payedProfitCashOrders.Where(co => co.ContractActionId == schedule.ActionId.Value).Sum(co => co.OrderCost);
                                }
                            }

                            payedProfitCashOrdersSum -= schedulePercentCost;
                            if (Math.Round(payedProfitCashOrdersSum - overPaidDifference) < 0)
                                break;

                            if (schedule.Date > action.Date)
                                throw new PawnshopApplicationException("Проценты по графику переплачены, обратитесь в поддержку");

                            if (!schedule.ActionId.HasValue)
                            {
                                schedule.ActionId = action.Id;
                                schedule.ActualDate = action.Date;
                            }
                        }

                        List<ContractPaymentSchedule> unpayedSchedules = contract.PaymentSchedule.Where(s => !s.ActionId.HasValue
                            && !s.Canceled.HasValue).ToList();

                        if (unpayedSchedules.Count == 0)
                        {
                            decimal unpaiddPenalty = _contractService.GetPenyAccountBalance(contract.Id, action.Date) +
                                                     _contractService.GetPenyProfitBalance(contract.Id, action.Date);

                            decimal unpaidDebt = _contractService.GetOverdueAccountBalance(contract.Id, action.Date) +
                                                 _contractService.GetAccountBalance(contract.Id, action.Date);

                            if (unpaiddPenalty + unpaidDebt == 0 && contract.ContractClass != ContractClass.CreditLine)
                            {
                                contract.BuyoutDate = action.Date;
                                contract.Status = ContractStatus.BoughtOut;

                                if (authorId == Constants.ADMINISTRATOR_IDENTITY)
                                    _contractCloseService.Exec(contract, action.Date, authorId, action);
                            }
                        }
                        else
                        {
                            DateTime minUnpayedScheduleDate = unpayedSchedules.Min(s => s.Date);
                            contract.NextPaymentDate = minUnpayedScheduleDate;
                        }

                        _contractRepository.Update(contract);
                        _contractPaymentScheduleService.Save(contract.PaymentSchedule, contract.Id, authorId);
                    }
                    _penaltyRateService.IncreaseRates(contract, action.Date, authorId, action);
                }

                transaction.Commit();
            }
        }

        public List<CashOrder> GetPayedInterest(int contractId, DateTime endDate, DateTime? beginDate = null)
        {
            var requiredProfitBoSettingCode = new List<string>
                    {
                        Constants.BO_SETTING_PAYMENT_OVERDUE_PROFIT,
                        Constants.BO_SETTING_PAYMENT_PROFIT,
                        Constants.BO_DISCOUNT_OVERDUE_PROFIT,
                        Constants.BO_DISCOUNT_PROFIT,
                        Constants.INTEREST_PAID_OFFBALANCE
                    };

            var payedProfitCashOrders = new List<CashOrder>();


            foreach (string boCode in requiredProfitBoSettingCode)
            {
                List<BusinessOperationSetting> businessOperationSettings = _businessOperationSettingService.List(new ListQueryModel<BusinessOperationSettingFilter>
                {
                    Page = null,
                    Model = new BusinessOperationSettingFilter
                    {
                        Code = boCode,
                        IsActive = true,

                    }
                }).List;

                if (!businessOperationSettings.Any())
                    throw new PawnshopApplicationException($"Настройки бизнес операции '{boCode}' не найдены");

                List<int> ids = businessOperationSettings.Select(e => e.Id).ToList();

                foreach (BusinessOperationSetting businessOperationSetting in businessOperationSettings)
                {
                    List<CashOrder> cashOrders = _cashOrderService.List(new Services.Models.List.ListQueryModel<CashOrderFilter>
                    {
                        Page = null,
                        Model = new CashOrderFilter
                        {
                            ContractId = contractId,
                            ApproveStatus = OrderStatus.Approved,
                            BusinessOperationSettingId = businessOperationSetting.Id,
                            EndDate = endDate.AddDays(1).AddTicks(-1),
                            BeginDate = beginDate != null ? beginDate.Value : beginDate
                        }
                    }).List;

                    payedProfitCashOrders.AddRange(cashOrders);
                }
            }
            return payedProfitCashOrders;
        }
    }
}
