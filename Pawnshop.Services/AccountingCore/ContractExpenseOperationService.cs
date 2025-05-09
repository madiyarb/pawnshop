using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Access.AccountingCore;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Crm;
using Pawnshop.Services.Exceptions;
using Pawnshop.Services.Expenses;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using Pawnshop.Services.Remittances;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Services.AccountingCore
{
    public class ContractExpenseOperationService : IContractExpenseOperationService
    {
        private readonly IExpenseService _expenseService;
        private readonly ISessionContext _sessionContext;
        private readonly ICashOrderService _cashOrderService;
        private readonly ICrmPaymentService _crmPaymentService;
        private readonly IRemittanceService _remittanceService;
        private readonly IBusinessOperationService _businessOperationService;
        private readonly IDictionaryWithSearchService<Group, BranchFilter> _branchService;
        private readonly IContractService _contractService;
        private readonly IContractExpenseService _contractExpenseService;
        private readonly IContractActionService _contractActionService;
        private readonly IBusinessOperationSettingService _businessOperationSettingService;

        private readonly GroupRepository _groupRepository;
        private readonly ContractExpenseRowRepository _contractExpenseRowRepository;
        private readonly ContractExpenseRowOrderRepository _contractExpenseRowOrderRepository;
        private readonly UserRepository _userRepository;
        private readonly AccountSettingRepository _accountSettingRepository;

        public ContractExpenseOperationService(IExpenseService expenseService, ISessionContext sessionContext,
            ContractExpenseRowRepository contractExpenseRowRepository,
            IContractService contractService, ICashOrderService cashOrderService,
            IDictionaryWithSearchService<Group, BranchFilter> branchService,
            ICrmPaymentService crmPaymentService, IRemittanceService remittanceService,
            IBusinessOperationService businessOperationService, IContractExpenseService contractExpenseService,
            IContractActionService contractActionService,
            ContractExpenseRowOrderRepository contractExpenseRowOrderRepository,
            UserRepository userRepository, GroupRepository groupRepository,
            IBusinessOperationSettingService businessOperationSettingService,
            AccountSettingRepository accountSettingRepository)
        {
            _expenseService = expenseService;
            _sessionContext = sessionContext;
            _contractExpenseRowRepository = contractExpenseRowRepository;
            _contractService = contractService;
            _cashOrderService = cashOrderService;
            _branchService = branchService;
            _crmPaymentService = crmPaymentService;
            _remittanceService = remittanceService;
            _businessOperationService = businessOperationService;
            _contractExpenseService = contractExpenseService;
            _contractActionService = contractActionService;
            _contractExpenseRowOrderRepository = contractExpenseRowOrderRepository;
            _userRepository = userRepository;
            _groupRepository = groupRepository;
            _businessOperationSettingService = businessOperationSettingService;
            _accountSettingRepository = accountSettingRepository;
        }

        public async Task PayExtraExpensesAsync(
            int contractExpenseId,
            int authorId,
            int branchId,
            int? actionId = null,
            bool forcePrepaymentReturn = true,
            int? prepaymentContractId = null
            )
        {
            ContractExpense contractExpense = await GetContractExpenseAsync(contractExpenseId);
            Contract contract = GetContract(contractExpense.ContractId);
            ContractAction contractAction = null;
            if (actionId.HasValue)
            {
                contractAction = await GetContractActionAsync(actionId.Value);
                if (contractAction.ContractId != contractExpense.ContractId && contract.ContractClass != ContractClass.CreditLine)
                    throw new PawnshopApplicationException($"У расхода {contractExpense.Id} и действия {contractAction.Id} должны совпадать договоры");
            }

            GetUser(authorId);
            Expense expenseType = GetExpenseType(contractExpense.ExpenseId);
            if (!expenseType.ExtraExpense)
                throw new PawnshopApplicationException($"Расход {contractExpense.Id} {contractExpense.Name} не является дополнительным расходом");

            if (contractExpense.IsPayed)
                throw new PawnshopApplicationException($"Расход {contractExpense.Id} {contractExpense.Name} уже оплачен");

            Group branch = await GetBranchAsync(branchId);
            ContractExpenseRow onCreateExpenseRow = contractExpense.ContractExpenseRows.SingleOrDefault();
            if (onCreateExpenseRow == null)
                throw new PawnshopApplicationException("Отсутствует факт создания расхода");

            List<CashOrder> onCreateExpenseRowOrders =
                onCreateExpenseRow.ContractExpenseRowOrders.Select(o => o.Order).ToList();

            if (onCreateExpenseRowOrders.Any(o => o.ApproveStatus != OrderStatus.Approved))
                throw new PawnshopApplicationException($"Расход {contractExpense.Name} имеет неподтвержденные кассовые ордера");

            using (IDbTransaction transaction = _contractExpenseService.BeginTransaction())
            {
                contractExpense.IsPayed = true;
                _contractExpenseService.Save(contractExpense);
                (ContractExpenseRow paymentOrder, ContractExpenseRow prepaymentRow) = RegisterPayedAndPrepaymentContractExpenseRows(contractExpense, contract, branch, expenseType, authorId, contractAction, forcePrepaymentReturn, prepaymentContractId);
                if (paymentOrder == null)
                    contractExpense.ContractExpenseRows.Add(paymentOrder);

                if (prepaymentRow == null)
                    contractExpense.ContractExpenseRows.Add(prepaymentRow);

                transaction.Commit();
            }
        }

        public async Task RegisterAsync(ContractExpense contractExpense, int authorId, int branchId, int? actionId = null, bool forcePrepaymentReturn = true, OrderStatus orderStatus = OrderStatus.WaitingForApprove)
        {
            if (contractExpense == null)
                throw new ArgumentNullException(nameof(contractExpense));

            if (contractExpense.Id != 0)
                throw new ArgumentException($"Поле {nameof(contractExpense.Id)} должно быть равно нулю", nameof(contractExpense));

            Expense expenseType = GetExpenseType(contractExpense.ExpenseId);
            if (contractExpense.TotalCost < 0)
                throw new PawnshopApplicationException("Расход должен иметь положительную сумму");

            if (expenseType.ExtraExpense && contractExpense.TotalCost == 0)
                throw new PawnshopApplicationException("Доп. расход должен сумму больше нуля");

            contractExpense.IsPayed = false;
            contractExpense.ContractExpenseRows = new List<ContractExpenseRow>();
            GetUser(authorId);
            Contract contract = GetContract(contractExpense.ContractId);
            Group branch = await GetBranchAsync(branchId);
            ContractAction contractAction = null;
            if (actionId.HasValue)
            {
                contractAction = await GetContractActionAsync(actionId.Value);
                if (contractAction.ContractId != contractExpense.ContractId)
                    throw new PawnshopApplicationException($"У расхода {contractExpense.Id} и действия {contractAction.Id} должны совпадать договоры");
            }

            if (expenseType.UserId.HasValue)
                contractExpense.UserId = expenseType.UserId.Value;

            if (contractExpense.TotalCost > 0)
                using (IDbTransaction transaction = _contractExpenseService.BeginTransaction())
                {
                    contractExpense.CreateDate = DateTime.Now;
                    contractExpense.AuthorId = authorId;
                    _contractExpenseService.Save(contractExpense);
                    ContractExpenseRow onCreateRow = RegisterOnCreateContractExpenseRow(contractExpense, contract, branch, expenseType, authorId, contractAction, orderStatus);
                    contractExpense.ContractExpenseRows.Add(onCreateRow);
                    if (!expenseType.ExtraExpense)
                    {
                        (ContractExpenseRow paymentOrder, ContractExpenseRow prepaymentRow) = RegisterPayedAndPrepaymentContractExpenseRows(contractExpense, contract, branch, expenseType, authorId, contractAction, forcePrepaymentReturn);
                        if (paymentOrder != null)
                            contractExpense.ContractExpenseRows.Add(paymentOrder);

                        if (prepaymentRow != null)
                            contractExpense.ContractExpenseRows.Add(prepaymentRow);
                    }

                    transaction.Commit();
                }
        }

        public async Task CancelAsync(int contractExpenseId, int authorId, int branchId, int? actionId = null)
        {
            ContractExpense contractExpense = await GetContractExpenseAsync(contractExpenseId);
            User author = GetUser(authorId);
            if (contractExpense.Date.Date != DateTime.Today && !author.ForSupport)
                throw new PawnshopApplicationException("Данный расход отмене не подлежит.");

            HashSet<OrderStatus> uniqueOrderStatuses =
                contractExpense.ContractExpenseRows.SelectMany(r => r.ContractExpenseRowOrders.Select(ro => ro.Order.ApproveStatus)).ToHashSet();
            if (uniqueOrderStatuses.Count == 0)
                throw new PawnshopApplicationException($"Расход не содержит ни одного кассового ордера");

            if (uniqueOrderStatuses.Count > 1)
                throw new PawnshopApplicationException($"Статусы кассовых ордеров текущего расхода({contractExpenseId}) должны быть одинаковыми");

            Expense expenseType = GetExpenseType(contractExpense.ExpenseId);
            if (!actionId.HasValue)
            {
                if (contractExpense.ContractExpenseRows.Any(r => r.ActionId.HasValue))
                    throw new PawnshopApplicationException("Нельзя отменить данный расход, так как он привязан к определенному действию к договору");
            }
            else if (contractExpense.ContractExpenseRows.Any(r => r.ActionId.HasValue && r.ActionId.Value != actionId.Value))
                throw new PawnshopApplicationException("Нельзя отменить данный расход, так как он привязан к другому действию к договору");

            Contract contract = _contractService.Get(contractExpense.ContractId);
            if (contract == null)
                throw new PawnshopApplicationException($"Договор по расходу {contractExpense} не найден");

            Group branch = await GetBranchAsync(branchId);
            ContractExpenseRow contractExpenseRowPayed = contractExpense.ContractExpenseRows
                .SingleOrDefault(r => r.ExpensePaymentType == ExpensePaymentType.Payed);

            // Если расход был оплчен
            if (contractExpenseRowPayed != null)
            {
                List<CashOrder> contractExpenseRowPayedOrders =
                    contractExpenseRowPayed.ContractExpenseRowOrders.Select(o => o.Order).ToList();
                if (contractExpenseRowPayedOrders.Count == 0)
                    throw new PawnshopApplicationException("Кассовый ордер должен быть заполнен у текущего расхода");

                if (expenseType.ExtraExpense
                    && contractExpenseRowPayedOrders.Any(o => o.ApproveStatus != OrderStatus.Approved)
                    && !author.ForSupport)
                    throw new PawnshopApplicationException("Данный расход подтвержден и отмене не подлежит.");
            }

            using (IDbTransaction transaction = _contractExpenseService.BeginTransaction())
            {
                List<ContractExpenseRow> prepaymentRows = contractExpense.ContractExpenseRows.Where(r => r.ExpensePaymentType == ExpensePaymentType.Prepayment).ToList();
                decimal prepaymentSum = prepaymentRows.Count > 0 ? prepaymentRows.Sum(r => r.Cost) : 0;
                foreach (ContractExpenseRow contractExpenseRow in contractExpense.ContractExpenseRows)
                {
                    _contractExpenseRowRepository.Delete(contractExpenseRow.Id);
                    contractExpenseRow.ContractExpenseRowOrders.Reverse();
                    foreach (ContractExpenseRowOrder contractExpenseRowOrder in contractExpenseRow.ContractExpenseRowOrders)
                    {
                        _contractExpenseRowOrderRepository.Delete(contractExpenseRowOrder.Id);
                        CashOrder order = contractExpenseRowOrder.Order;
                        if (order.ApproveStatus == OrderStatus.Approved)
                        {
                            if (contractExpenseRowPayed != null)
                                _cashOrderService.Cancel(order, authorId, branch);
                        }
                        else
                            _cashOrderService.Delete(order.Id);
                    }
                }

                var amountsDict = new Dictionary<AmountType, decimal>
                {
                    { AmountType.Prepayment, prepaymentSum },
                    { AmountType.Expense, contractExpense.TotalCost }
                };

                if (contractExpenseRowPayed == null && uniqueOrderStatuses.Single() == OrderStatus.Approved)
                    _businessOperationService.Register(contract, DateTime.Now, Constants.BUSINESS_OPERATION_EXPENSE_CANCEL,
                        branch, authorId, amountsDict, typeId: expenseType.TypeId);

                _contractExpenseService.Save(contractExpense);
                _contractExpenseService.Delete(contractExpense.Id);
                _crmPaymentService.Enqueue(contract);
                transaction.Commit();
            }
        }

        public async Task<IDictionary<int, (int, DateTime)>> CancelExpensesByActionIdAsync(int actionId, int authorId, int branchId, bool isStorn)
        {
            var recalculateAccountDict = new Dictionary<int, (int, DateTime)>();
            User author = GetUser(authorId);
            ContractAction contractAction = await GetContractActionAsync(actionId);
            Contract contract = GetContract(contractAction.ContractId);
            Group branch = await GetBranchAsync(branchId);
            var listQueryModel = new ListQueryModel<ContractExpenseFilter>
            {
                Page = null,
                Model = new ContractExpenseFilter { ContractId = contractAction.ContractId }
            };

            ListModel<ContractExpense> contractExpensesListModel = _contractExpenseService.List(listQueryModel);
            if (contractExpensesListModel == null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(contractExpensesListModel)} не будет null");

            if (contractExpensesListModel.List == null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(contractExpensesListModel)}.{nameof(contractExpensesListModel.List)} не будет null");

            List<Expense> expenseTypes = _expenseService.GetList(new Core.Queries.ListQuery { Page = null });
            Dictionary<int, Expense> expenseTypesDict = expenseTypes.ToDictionary(e => e.Id, e => e);
            List<ContractExpense> contractExpenses = contractExpensesListModel.List;
            using (IDbTransaction transaction = _contractExpenseService.BeginTransaction())
            {
                var expensesWithRowsAndOrders = new List<ContractExpense>();
                foreach (ContractExpense contractExpense in contractExpenses)
                    expensesWithRowsAndOrders.Add(await _contractExpenseService.GetAsync(contractExpense.Id));

                foreach (ContractExpense expense in expensesWithRowsAndOrders)
                {
                    // пропускаем если это расход действия
                    if (expense.Id == contractAction.ExpenseId)
                        continue;

                    Expense expenseType = null;
                    if (!expenseTypesDict.TryGetValue(expense.ExpenseId, out expenseType))
                        throw new PawnshopApplicationException($"Не найден тип расхода {expense.ExpenseId}");

                    if (!expenseType.ExtraExpense)
                        continue;

                    List<ContractExpenseRow> contractExpenseRowsByActionId = expense.ContractExpenseRows.Where(r => r.ActionId == actionId).ToList();
                    if (contractExpenseRowsByActionId.Count == 0)
                        continue;

                    foreach (ContractExpenseRow contractExpenseRow in contractExpenseRowsByActionId)
                    {
                        if (contractExpenseRow.ExpensePaymentType == ExpensePaymentType.OnCreate)
                            throw new PawnshopApplicationException("Нельзя сторнировать факт создания расхода");

                        _contractExpenseRowRepository.Delete(contractExpenseRow.Id);
                        foreach (ContractExpenseRowOrder contractExpenseRowOrder in contractExpenseRow.ContractExpenseRowOrders)
                        {
                            _contractExpenseRowOrderRepository.Delete(contractExpenseRowOrder.Id);
                            CashOrder order = contractExpenseRowOrder.Order;
                            if (isStorn)
                                _cashOrderService.Cancel(order, authorId, branch);
                            else
                            {
                                IDictionary<int, (int, DateTime)> actionsWithDatesDict = _cashOrderService.Delete(order.Id, authorId, branchId);
                                if (actionsWithDatesDict == null)
                                    throw new PawnshopApplicationException(
                                        $"Ожидалось что {nameof(_cashOrderService)}.{nameof(_cashOrderService.Delete)} не вернет null");

                                foreach ((int accountId, (int accountRecordId, DateTime accountDate)) in actionsWithDatesDict)
                                {
                                    if (recalculateAccountDict.ContainsKey(accountId))
                                    {
                                        (int _, DateTime date) = recalculateAccountDict[accountId];
                                        if (date < accountDate)
                                            continue;
                                    }

                                    recalculateAccountDict[accountId] = (accountRecordId, accountDate);
                                }
                            }
                        }
                    }

                    expense.IsPayed = false;
                    expense.TotalLeft = 0;
                    _contractExpenseService.Save(expense);
                }

                transaction.Commit();
                return recalculateAccountDict;
            }
        }

        public List<ContractExpense> GetIncomingExtraExpensesByContractId(int contractId)
        {
            Contract contract = _contractService.Get(contractId);
            if (contract == null)
                throw new PawnshopApplicationException($"Договор {contractId} не найден");

            var contractExpenseListQuery = new ListQueryModel<ContractExpenseFilter>
            {
                Page = null,
                Model = new ContractExpenseFilter
                {
                    ContractId = contract.Id,
                    IsPayed = false,
                }
            };

            ListModel<ContractExpense> contractExpensesListModel = _contractExpenseService.List(contractExpenseListQuery);
            if (contractExpensesListModel == null)
                throw new PawnshopApplicationException($"Ожидалось что '{nameof(contractExpensesListModel)}' не будет null");

            if (contractExpensesListModel.List == null)
                throw new PawnshopApplicationException($"Ожидалось что '{nameof(contractExpensesListModel)}.{nameof(contractExpensesListModel.List)}' не будет null");
            List<ContractExpense> contractExtraExpenses = contractExpensesListModel.List;
            List<Expense> expenseTypes = _expenseService.GetList(new ListQuery { Page = null });
            if (expenseTypes == null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(_expenseService)}.{nameof(_expenseService.GetList)} не вернет null");

            Dictionary<int, Expense> expenseTypesDict = expenseTypes.ToDictionary(e => e.Id, e => e);
            foreach (ContractExpense contractExpense in contractExtraExpenses)
            {
                ContractExpense tempContractExpense = _contractExpenseService.GetAsync(contractExpense.Id).Result;
                contractExpense.ContractExpenseRows = tempContractExpense.ContractExpenseRows;
                Expense expenseType = null;
                if (!expenseTypesDict.TryGetValue(contractExpense.ExpenseId, out expenseType))
                    throw new PawnshopApplicationException($"Тип расхода(ExpenseId) {contractExpense.ExpenseId} не найден");

                contractExpense.Expense = expenseType;
            }

            contractExtraExpenses = contractExtraExpenses
                .Where(e => e.Expense.ExtraExpense
                 && e.ContractExpenseRows
                    .Any(r => !r.ContractExpenseRowOrders
                        .Any(ro => ro.Order.ApproveStatus != OrderStatus.Approved)))
                .ToList();

            return contractExtraExpenses;
        }

        public List<ContractExpense> GetNeededExpensesForPrepayment(int contractId, int branchId, IEnumerable<int> extraExpensesIds = null)
        {
            Group branch = _groupRepository.Get(branchId);
            if (branch == null)
                throw new PawnshopApplicationException($"Филиал {branchId} не найден");

            List<ContractExpense> extraExpenses = GetIncomingExtraExpensesByContractId(contractId);
            if (extraExpenses == null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(GetIncomingExtraExpensesByContractId)} не вернет null");

            var contract = _contractService.GetOnlyContract(contractId);
            if (contract.ContractClass == ContractClass.Tranche)
            {
                extraExpenses.AddRange(GetIncomingExtraExpensesByContractId(contract.CreditLineId.Value));
            }

            if (extraExpensesIds != null)
            {
                HashSet<int> extraExpensesIdsSet = extraExpensesIds.ToHashSet();
                if (!extraExpensesIdsSet.IsSubsetOf(extraExpenses.Select(e => e.Id)))
                    throw new PawnshopApplicationException("Список доп расходов не сходится со списком доп расходов базы");

                extraExpenses = extraExpenses.Where(e => extraExpensesIdsSet.Contains(e.Id)).ToList();
            }

            var result = new List<ContractExpense>();
            foreach (ContractExpense contractExpense in extraExpenses)
            {
                Expense expenseType = contractExpense.Expense;
                BusinessOperation paymentBusinessOperation = _businessOperationService.FindBusinessOperation(expenseType.TypeId.Value, Constants.BUSINESS_OPERATION_EXPENSE_PAYMENT, branch.Id, branch.OrganizationId);
                if (paymentBusinessOperation == null)
                    throw new PawnshopApplicationException($"Бизнес операция по коду {Constants.BUSINESS_OPERATION_EXPENSE_PAYMENT} не найдена");

                ListQueryModel<BusinessOperationSettingFilter> listQueryModel = new ListQueryModel<BusinessOperationSettingFilter>
                {
                    Page = null,
                    Model = new BusinessOperationSettingFilter
                    {
                        BusinessOperationId = paymentBusinessOperation.Id,
                        IsActive = true
                    }
                };

                ListModel<BusinessOperationSetting> businessOperationsListModel = _businessOperationSettingService.List(listQueryModel);
                foreach (BusinessOperationSetting businessOperationSetting in businessOperationsListModel.List)
                {
                    if (businessOperationSetting.DebitSettingId.HasValue)
                    {
                        AccountSetting debitAccountSetting = _accountSettingRepository.Get(businessOperationSetting.DebitSettingId.Value);
                        if (debitAccountSetting == null)
                            throw new PawnshopApplicationException($"Настройки счетов {businessOperationSetting.DebitSettingId.Value} не найдены");

                        bool isDepoSetting = debitAccountSetting.Code == Constants.ACCOUNT_SETTING_DEPO;
                        if (isDepoSetting)
                        {
                            result.Add(contractExpense);
                            break;
                        }
                    }
                }
            }

            return result;
        }

        public IDictionary<int, (int, DateTime)> DeleteWithOrders(int id, int authorId, int branchId, int? contractActionId = null)
        {
            User author = _userRepository.Get(authorId);
            if (author == null)
                throw new PawnshopApplicationException($"Пользователь {authorId} не найден");

            Group branch = _branchService.GetAsync(branchId).Result;
            if (branch == null)
                throw new PawnshopApplicationException($"Филиал {branchId} не найден");

            var recalculateAccountDict = new Dictionary<int, (int, DateTime)>();
            ContractExpense contractExpense = _contractExpenseService.GetAsync(id).Result;
            if (contractExpense.ContractExpenseRows == null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(contractExpense)}.{contractExpense.ContractExpenseRows} не будет null");

            foreach (ContractExpenseRow contractExpenseRow in contractExpense.ContractExpenseRows)
            {
                if (contractExpenseRow.ActionId.HasValue)
                {
                    if (!contractActionId.HasValue)
                        throw new PawnshopApplicationException($"Расход {contractExpense.Id} привязан к определенному действию, нельзя удалить данный расход");

                    int contractExpenseRowActionId = contractExpenseRow.ActionId.Value;
                    if (contractExpenseRowActionId != contractActionId.Value)
                        throw new PawnshopApplicationException($"Расход {contractExpense.Id} привязан к другому действию, нельзя удалить данный расход");
                }

                if (contractExpenseRow.ContractExpenseRowOrders == null)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(contractExpenseRow)}.{contractExpenseRow.ContractExpenseRowOrders} не будет null");

                contractExpenseRow.ContractExpenseRowOrders.Reverse();
                foreach (ContractExpenseRowOrder contractExpenseRowOrder in contractExpenseRow.ContractExpenseRowOrders)
                {
                    if (contractExpenseRowOrder.Order == null)
                        throw new PawnshopApplicationException($"Ожидалось что {nameof(contractExpenseRowOrder)}.{contractExpenseRowOrder.Order} не будет null");
                }
            }

            using (IDbTransaction transaction = _contractExpenseRowRepository.BeginTransaction())
            {
                foreach (ContractExpenseRow contractExpenseRow in contractExpense.ContractExpenseRows)
                {
                    foreach (ContractExpenseRowOrder contractExpenseRowOrder in contractExpenseRow.ContractExpenseRowOrders)
                    {
                        _contractExpenseRowOrderRepository.Delete(contractExpenseRowOrder.Id);
                        IDictionary<int, (int, DateTime)> actionsWithDatesDict =
                            _cashOrderService.Delete(contractExpenseRowOrder.Order.Id, author.Id, branch.Id);
                        if (actionsWithDatesDict == null)
                            throw new PawnshopApplicationException(
                                $"Ожидалось что {nameof(_cashOrderService)}.{nameof(_cashOrderService.Delete)} не вернет null");

                        foreach ((int accountId, (int accountRecordId, DateTime accountDate)) in actionsWithDatesDict)
                        {
                            if (recalculateAccountDict.ContainsKey(accountId))
                            {
                                (int _, DateTime date) = recalculateAccountDict[accountId];
                                if (date < accountDate)
                                    continue;
                            }

                            recalculateAccountDict[accountId] = (accountRecordId, accountDate);
                        }
                    }

                    _contractExpenseRowRepository.Delete(contractExpenseRow.Id);
                }

                _contractExpenseService.Delete(id);
                transaction.Commit();
                return recalculateAccountDict;
            }
        }

        private User GetUser(int userId)
        {
            User user = _userRepository.Get(userId);
            if (user == null)
                throw new PawnshopApplicationException($"Пользотватель(автор) {userId}  не найден");

            return user;
        }

        public Expense GetExpenseType(int expenseId)
        {
            Expense expenseType = _expenseService.Get(expenseId);
            if (expenseType == null)
                throw new PawnshopApplicationException("Тип расхода не найден у текущего расхода");

            return expenseType;
        }

        private async Task<Group> GetBranchAsync(int branchId)
        {
            Group branch = await _branchService.GetAsync(branchId);
            if (branch == null)
                throw new PawnshopApplicationException($"Филиал {branchId} не найден");

            return branch;
        }

        private Contract GetContract(int contractId)
        {
            Contract contract = _contractService.Get(contractId);
            if (contract == null)
                throw new PawnshopApplicationException($"Договор по расходу {contractId} не найден");

            return contract;
        }

        private async Task<ContractAction> GetContractActionAsync(int contractActionId)
        {
            ContractAction contractAction = await _contractActionService.GetAsync(contractActionId);
            if (contractAction == null)
                throw new PawnshopApplicationException($"Действие {contractActionId} не найдено");

            return contractAction;
        }

        private async Task<ContractExpense> GetContractExpenseAsync(int contractExpenseId)
        {
            ContractExpense contractExpense = await _contractExpenseService.GetAsync(contractExpenseId);
            if (contractExpense == null)
                throw new PawnshopApplicationException($"Расход {contractExpenseId}");

            string contractExpenseIdString = $"{nameof(contractExpense)}.{nameof(contractExpense.Id)}={contractExpense.Id}";
            string contractExpenseRowsPropertyName = $"{nameof(contractExpense)}.{nameof(contractExpense.ContractExpenseRows)}";
            if (contractExpense.ContractExpenseRows == null)
                throw new PawnshopApplicationException($"Ожидалось что {contractExpenseRowsPropertyName} {contractExpenseIdString}");

            foreach (ContractExpenseRow contractExpenseRow in contractExpense.ContractExpenseRows)
            {
                if (contractExpenseRow == null)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(contractExpenseRow)} не будет null, {contractExpenseIdString}");

                string contractExpenseRowOrdersPropertyName = $"{nameof(contractExpenseRow)}.{nameof(contractExpenseRow.ContractExpenseRowOrders)}";
                string contractExpenseRowIdString = $"{nameof(contractExpenseRow)}.{nameof(contractExpenseRow.Id)}={contractExpenseRow.Id}";
                if (contractExpenseRow.ContractExpenseRowOrders == null)
                    throw new PawnshopApplicationException($"Ожидалось что {contractExpenseRowOrdersPropertyName} не будет null",
                    contractExpenseIdString,
                    contractExpenseRowIdString);

                foreach (ContractExpenseRowOrder contractExpenseRowOrder in contractExpenseRow.ContractExpenseRowOrders)
                {
                    if (contractExpenseRowOrder == null)
                        throw new PawnshopApplicationException(
                            $"Ожидалось что {nameof(contractExpenseRowOrder)} не будет null",
                            contractExpenseIdString,
                            contractExpenseRowIdString);

                    string contractExpenseRowOrderIdString = $"{nameof(contractExpenseRowOrder)}.{nameof(contractExpenseRowOrder.Id)}={contractExpenseRowOrder.Id}";
                    string orderPropertyName = $"{nameof(contractExpenseRowOrder)}.{nameof(contractExpenseRowOrder.Order)}";
                    if (contractExpenseRowOrder.Order == null)
                        throw new PawnshopApplicationException(
                            $"Ожидалось что {orderPropertyName} не будет null",
                            contractExpenseIdString,
                            contractExpenseRowIdString,
                            contractExpenseRowOrderIdString);
                }
            }

            return contractExpense;
        }

        private ContractExpenseRow RegisterOnCreateContractExpenseRow(ContractExpense expense, Contract contract, Group branch, Expense expenseType, int authorId, ContractAction contractAction, OrderStatus orderStatus = OrderStatus.WaitingForApprove)
        {
            if (expense == null)
                throw new ArgumentNullException(nameof(expense));

            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            if (branch == null)
                throw new ArgumentNullException(nameof(contract));

            if (expenseType == null)
                throw new ArgumentNullException(nameof(expenseType));

            var onCreateRow = new ContractExpenseRow
            {
                Cost = expense.TotalCost,
                ActionId = contractAction?.Id,
                AuthorId = authorId,
                ContractExpenseId = expense.Id,
                CreateDate = DateTime.Now,
                ExpensePaymentType = ExpensePaymentType.OnCreate,
                ContractExpenseRowOrders = new List<ContractExpenseRowOrder>()
            };

            _contractExpenseRowRepository.Insert(onCreateRow);
            if (expenseType.ExtraExpense)
            {
                var onCreateAmountDict = new Dictionary<AmountType, decimal>
                {
                    { AmountType.Expense, expense.TotalCost }
                };

                int? orderUserId = !expenseType.NotFillUserid ? expense.UserId : default(int?);
                List<(CashOrder, List<AccountRecord>)> onCreateOperationResult =
                    _businessOperationService.Register(contract, DateTime.Now,
                        Constants.BUSINESS_OPERATION_EXPENSE_CREATION, branch, authorId, onCreateAmountDict,
                        typeId: expenseType.TypeId, orderStatus: orderStatus, orderUserId: orderUserId, action: contractAction);

                if (onCreateOperationResult.Count == 0)
                    throw new PawnshopApplicationException($"Ожидалось что бизнес операция {Constants.BUSINESS_OPERATION_EXPENSE_CREATION} не вернет пустую коллекцию");

                List<ContractExpenseRowOrder> payedContractExpenseRowOrders =
                    CreateContractExpenseRowOrdersFromCashOrders(onCreateRow.Id, onCreateOperationResult.Select(c => c.Item1));
                onCreateRow.ContractExpenseRowOrders.AddRange(payedContractExpenseRowOrders);
            }

            return onCreateRow;
        }

        private (ContractExpenseRow, ContractExpenseRow) RegisterPayedAndPrepaymentContractExpenseRows(ContractExpense expense, Contract contract, Group branch, Expense expenseType,
            int authorId, ContractAction contractAction, bool forcePrepaymentReturn, int? prepaymentContractId = null)
        {
            if (expense == null)
                throw new ArgumentNullException(nameof(expense));

            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            if (branch == null)
                throw new ArgumentNullException(nameof(contract));

            if (expenseType == null)
                throw new ArgumentNullException(nameof(expenseType));

            if (!expenseType.TypeId.HasValue)
                throw new ArgumentException($"Поле {nameof(expenseType.TypeId)} не должен быть null", nameof(expenseType));

            string paymentBusinessOperationCode = expenseType.ExtraExpense ? Constants.BUSINESS_OPERATION_EXPENSE_PAYMENT : Constants.BUSINESS_OPERATION_EXPENSE_CREATION;

            //if (!expense.IsPayed)
            //    return (null, null);

            var paymentRow = new ContractExpenseRow
            {
                Cost = expense.TotalCost,
                ActionId = contractAction?.Id,
                AuthorId = authorId,
                ContractExpenseId = expense.Id,
                CreateDate = DateTime.Now,
                ExpensePaymentType = ExpensePaymentType.Payed,
                ContractExpenseRowOrders = new List<ContractExpenseRowOrder>()
            };

            _contractExpenseRowRepository.Insert(paymentRow);
            var paymentAmountDict = new Dictionary<AmountType, decimal>
            {
                { AmountType.Expense, expense.TotalCost }
            };

            Contract prepaymentContract = contract;
            Contract creditLineContract = null;

            if (prepaymentContractId.HasValue)
            {
                prepaymentContract = _contractService.Get(prepaymentContractId.Value);
                creditLineContract = contract;
            }

            int? orderUserId = !expenseType.NotFillUserid ? expense.UserId : default(int?);
            List<(CashOrder, List<AccountRecord>)> paymentOperationResult = _businessOperationService.Register(prepaymentContract, DateTime.Now,
                paymentBusinessOperationCode, branch, authorId, paymentAmountDict,
                typeId: expenseType.TypeId, orderUserId: orderUserId, action: contractAction, creditLine: creditLineContract);

            if (paymentOperationResult.Count == 0)
                throw new PawnshopApplicationException($"Ожидалось что бизнес операция {paymentBusinessOperationCode} вернет непустую коллекцию");

            List<ContractExpenseRowOrder> payedContractExpenseRowOrders =
            CreateContractExpenseRowOrdersFromCashOrders(paymentRow.Id, paymentOperationResult.Select(c => c.Item1));
            paymentRow.ContractExpenseRowOrders.AddRange(payedContractExpenseRowOrders);
            if (contractAction?.ActionType != ContractActionType.Sign)
                try
                {
                    decimal prepaymentSum = expense.TotalCost;
                    if (!forcePrepaymentReturn)
                    {
                        decimal prepaymentBalance = _contractService.GetPrepaymentBalance(prepaymentContractId ?? contract.Id, contractAction?.Date);
                        if (contractAction != null)
                        {
                            _contractActionService.Save(contractAction);
                            prepaymentBalance += CalculatePrepaymentTurns(contractAction.Id);
                        }
                        prepaymentSum = prepaymentBalance >= Math.Ceiling(expense.TotalCost) ? Math.Ceiling(expense.TotalCost) : Math.Floor(prepaymentBalance);
                    }

                    if (prepaymentSum > 0)
                    {
                        var prepaymentAmountDict = new Dictionary<AmountType, decimal>
                        {
                            { AmountType.Expense, prepaymentSum }
                        };

                        List<(CashOrder, List<AccountRecord>)> prepaymentOperationResult = _businessOperationService.Register(
                            prepaymentContract,
                            DateTime.Now,
                            Constants.BUSINESS_OPERATION_EXPENSE_PREPAYMENT_RETURN,
                            branch,
                            authorId,
                            prepaymentAmountDict,
                            typeId: expenseType.TypeId,
                            action: contractAction);

                        if (prepaymentOperationResult.Count == 0)
                            throw new PawnshopApplicationException(
                                $"Ожидалось что бизнес операция {Constants.BUSINESS_OPERATION_EXPENSE_PREPAYMENT_RETURN} не вернет пустой список");

                        var prepaymentRow = new ContractExpenseRow
                        {
                            Cost = prepaymentSum,
                            ActionId = contractAction?.Id,
                            AuthorId = authorId,
                            ContractExpenseId = expense.Id,
                            CreateDate = DateTime.Now,
                            ExpensePaymentType = ExpensePaymentType.Prepayment,
                            ContractExpenseRowOrders = new List<ContractExpenseRowOrder>()
                        };

                        _contractExpenseRowRepository.Insert(prepaymentRow);
                        List<ContractExpenseRowOrder> prepaymentContractExpenseRowOrders =
                            CreateContractExpenseRowOrdersFromCashOrders(prepaymentRow.Id, prepaymentOperationResult.Select(c => c.Item1));

                        prepaymentRow.ContractExpenseRowOrders.AddRange(prepaymentContractExpenseRowOrders);
                        return (paymentRow, prepaymentRow);
                    }
                }
                catch (Exception ex)
                {
                    if (!(ex is BusinessOperationNotFoundException))
                        throw;
                }

            return (paymentRow, null);
        }

        private List<ContractExpenseRowOrder> CreateContractExpenseRowOrdersFromCashOrders(int contractExpenseRowId, IEnumerable<CashOrder> orders)
        {
            if (orders == null)
                throw new ArgumentNullException(nameof(orders));

            if (!orders.Any())
                throw new ArgumentException("Коллекция должна содержать хоть один элемент", nameof(orders));

            int authorId = _sessionContext.IsInitialized ? _sessionContext.UserId : Constants.ADMINISTRATOR_IDENTITY;
            var contractExpenseRowOrders = new List<ContractExpenseRowOrder>();
            foreach (CashOrder order in orders)
            {
                if (order == null)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(order)} не будет null");

                var contractExpenseRowOrder = new ContractExpenseRowOrder
                {
                    AuthorId = authorId,
                    ContractExpenseRowId = contractExpenseRowId,
                    CreateDate = DateTime.Now,
                    OrderId = order.Id,
                    Order = order
                };

                _contractExpenseRowOrderRepository.Insert(contractExpenseRowOrder);
                contractExpenseRowOrders.Add(contractExpenseRowOrder);
            }

            return contractExpenseRowOrders;
        }

        private decimal CalculatePrepaymentTurns(int actionId)
        {
            var relatedActions = _contractActionService.GetRelatedContractActionsByActionId(actionId).Result;

            if (relatedActions is null || !relatedActions.Any())
                return 0;

            decimal debitPrepaymentTurns = _cashOrderService.GetAccountSettingDebitTurnsByActionIds(relatedActions, Constants.ACCOUNT_SETTING_DEPO);
            decimal creditPrepaymentTurns = _cashOrderService.GetAccountSettingCreditTurnsByActionIds(relatedActions, Constants.ACCOUNT_SETTING_DEPO);

            return creditPrepaymentTurns - debitPrepaymentTurns;
        }
    }
}
