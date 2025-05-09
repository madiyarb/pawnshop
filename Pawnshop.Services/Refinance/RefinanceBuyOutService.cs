using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Calculation;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Expenses;
using Pawnshop.Services.Integrations.UKassa;
using Pawnshop.Services.Models.Calculation;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System;
using Serilog;

namespace Pawnshop.Services.Refinance
{
    public sealed class RefinanceBuyOutService : IRefinanceBuyOutService
    {
        private readonly OnlineApplicationRepository _onlineApplicationService;
        private readonly ContractRepository _contractRepository;
        private readonly ICashOrderService _cashOrderService;
        private readonly IContractActionService _contractActionService;
        private readonly ContractService _contractService;
        private readonly IInscriptionService _inscriptionService;
        private readonly IContractActionRowBuilder _contractActionRowBuilder;
        private readonly IContractActionBuyoutService _contractActionBuyoutService;
        private readonly IContractActionSellingService _contractSellingService;
        private readonly IContractExpenseService _contractExpenseService;
        private readonly IExpenseService _expenseService;
        private readonly IUKassaService _uKassaService;
        private readonly IContractDutyService _contractDutyService;
        private readonly IContractExpenseOperationService _contractExpenseOperationService;
        private readonly IContractActionPrepaymentService _contractActionPrepaymentService;
        private readonly PayTypeRepository _payTypeRepository;
        private readonly ILogger _logger;
        private readonly ApplicationOnlineRefinancesRepository _appOnlineRefinancesRepository;
        public RefinanceBuyOutService(OnlineApplicationRepository onlineApplicationService,
            ContractRepository contractRepository,
            ICashOrderService cashOrderService,
            IContractActionService contractActionService,
            ContractService contractService,
            IInscriptionService inscriptionService,
            IContractActionRowBuilder contractActionRowBuilder,
            IContractActionBuyoutService contractActionBuyoutService,
            IContractActionSellingService contractSellingService,
            IContractExpenseService contractExpenseService,
            IExpenseService expenseService,
            IUKassaService uKassaService,
            IContractDutyService contractDutyService,
            IContractExpenseOperationService contractExpenseOperationService,
            IContractActionPrepaymentService contractActionPrepaymentService,
            PayTypeRepository payTypeRepository,
            ILogger logger,
            ApplicationOnlineRefinancesRepository appOnlineRefinancesRepository)
        {
            _onlineApplicationService = onlineApplicationService;
            _contractRepository = contractRepository;
            _contractActionService = contractActionService;
            _contractService = contractService;
            _expenseService = expenseService;
            _cashOrderService = cashOrderService;
            _inscriptionService = inscriptionService;
            _contractActionBuyoutService = contractActionBuyoutService;
            _contractActionRowBuilder = contractActionRowBuilder;
            _contractSellingService = contractSellingService;
            _contractExpenseService = contractExpenseService;
            _uKassaService = uKassaService;
            _contractDutyService = contractDutyService;
            _contractExpenseOperationService = contractExpenseOperationService;
            _contractActionPrepaymentService = contractActionPrepaymentService;
            _payTypeRepository = payTypeRepository; ;
            _logger = logger;
            _appOnlineRefinancesRepository = appOnlineRefinancesRepository;

        }

        /// <summary>
        /// Выкупить все займы которые были рефинансированы за счет нового контракта 
        /// </summary>
        /// <param name="contractId">Идентификатор контракта за счет которого были рефинансированы займы</param>
        /// <returns></returns>
        [Obsolete]
        public async Task<bool> BuyOutAllRefinancedContracts(int contractId)
        {
            var application = await _onlineApplicationService.FindByContractIdAsync(new { ContractId = contractId.ToString() });

            if (application == null)
                return true;

            try
            {
                for (int i = 0; i < application.OnlineApplicationRefinances.Count; i++)
                {
                    await Buyout(application.OnlineApplicationRefinances[i].RefinancedContractId.Value);
                    await ApproveBuyout(application.OnlineApplicationRefinances[i].RefinancedContractId.Value);
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, $"Не удалось рефинансировать займ с ошибкой {exception.Message}");
                return false;
            }

            return true;
        }

        public async Task<bool> BuyOutAllRefinancedContractsForApplicationsOnline(int contractId)
        {
            var refinances = await _appOnlineRefinancesRepository.GetApplicationOnlineRefinancesByContractId(contractId);

            if (refinances == null)
                return true;

            try
            {
                for (int i = 0; i < refinances.Count; i++)
                {
                    await Buyout(refinances[i].RefinancedContractId);
                    await ApproveBuyout(refinances[i].RefinancedContractId);
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, $"Не удалось рефинансировать займ с ошибкой {exception.Message}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Выкупить контракт
        /// </summary>
        /// <param name="contractId">Идентификатор выкупаемого контракта</param>
        /// <returns></returns>
        /// <exception cref="PawnshopApplicationException"></exception>
        public async Task Buyout(int contractId)
        {
            var payType = _payTypeRepository.Find(new { Code = "CASH" });
            var contract = _contractRepository.Get(contractId);
            ContractDutyCheckModel checkModel = new ContractDutyCheckModel
            {
                ActionType = contract.ContractClass == ContractClass.Credit && contract.IsContractRestructured ? ContractActionType.BuyoutRestructuringCred : ContractActionType.Buyout,
                ContractId = contract.Id,
                Cost = 0,
                Date = DateTime.Today,
                PayTypeId = payType.Id
            };
            var contractDuty = _contractDutyService.GetContractDuty(checkModel);

            var action = new ContractAction
            {
                ProcessingId = null,
                ProcessingType = null,
                ActionType = contract.ContractClass == ContractClass.Credit && contract.IsContractRestructured ? ContractActionType.BuyoutRestructuringCred : ContractActionType.Buyout,
                Checks = contractDuty.Checks.Select((x, i) => new ContractActionCheckValue { Check = x, CheckId = x.Id, Value = true }).ToList(),
                ContractId = contract.Id,
                Date = contractDuty.Date,
                Discount = contractDuty.Discount,
                Expense = new ContractExpense
                {
                    Id = 0,
                    Date = DateTime.Today,
                    ExpenseId = 6,
                    Expense = null,
                    ContractId = contract.Id,
                    TotalCost = 0,
                    TotalLeft = 0,
                    Reason = $"Без снятия обременение по договору займа № {contract.ContractNumber} от {DateTime.Today}",
                    Name = "Без снятия обременение",
                    UserId = 1,
                    IsPayed = false,
                },
                ExtraExpensesCost = contractDuty.ExtraExpensesCost,
                PayTypeId = payType.Id,
                Reason = contractDuty.Reason,
                RequisiteCost = null,
                RequisiteId = null,
                Rows = contractDuty.Rows.ToArray(),
                TotalCost = contractDuty.Cost,
                BuyoutReasonId = 109,
                BuyoutCreditLine = true
            };
            action.ProcessingId = null;
            action.ProcessingType = null;
            action.ActionType = contract.ContractClass == ContractClass.Credit && contract.IsContractRestructured ? ContractActionType.BuyoutRestructuringCred : ContractActionType.Buyout;
            action.AuthorId = 1;
            action.CreateDate = DateTime.Now;

            int branchId = contract.BranchId;
            int authorId = 1;
            ContractAction prepaymentAction = null;
            using (IDbTransaction transaction = _contractRepository.BeginTransaction())
            {
                decimal depoBalance = _contractService.GetPrepaymentBalance(contract.Id, action.Date);
                decimal totalCost = action.TotalCost;
                decimal prepaymentCost = 0;
                List<ContractExpense> extraExpensesCostForPayment = _contractExpenseOperationService.GetNeededExpensesForPrepayment(contract.Id, branchId);
                decimal extraExpensesSum = extraExpensesCostForPayment.Count > 0 ? extraExpensesCostForPayment.Sum(e => e.TotalCost) : 0;
                decimal diff = depoBalance - totalCost - extraExpensesSum;
                if (diff < 0)
                    prepaymentCost = Math.Ceiling(Math.Abs(diff));

                if (prepaymentCost > 0)
                {
                    prepaymentAction = _contractActionPrepaymentService.Exec(contract.Id, prepaymentCost, action.PayTypeId.Value, branchId, authorId, date: action.Date);
                    if (prepaymentAction == null)
                        throw new PawnshopApplicationException($"Ожидалось что {nameof(_contractActionPrepaymentService)}.{nameof(_contractActionPrepaymentService.Exec)} не вернет null");

                    action.ParentActionId = prepaymentAction.Id;
                }
                await _contractActionBuyoutService.Execute(action, authorId, branchId, forceExpensePrepaymentReturn: false, false, prepaymentAction);
                contract.BuyoutReasonId = action.BuyoutReasonId;

                if (prepaymentAction != null)
                {
                    prepaymentAction.ChildActionId = action.Id;
                    _contractActionService.Save(prepaymentAction);
                    action.ParentActionId = prepaymentAction.Id;
                    action.ParentAction = prepaymentAction;
                }

                _contractActionService.Save(action);
                _contractService.Save(contract);

                transaction.Commit();
            }
        }

        /// <summary>
        /// Подтвердить выкуп контракта
        /// </summary>
        /// <param name="contractId">Идентификатор выкупаемого контрактка</param>
        /// <returns></returns>
        /// <exception cref="PawnshopApplicationException"></exception>
        public async Task ApproveBuyout(int contractId)
        {
            var contractActions = await _contractActionService.GetContractActionsByContractId(contractId);

            var contractAction = contractActions.FirstOrDefault(contractAction => contractAction.Status == ContractActionStatus.Await);
            var action = _contractActionService.GetAsync(contractAction.Id).Result;

            if (!action.Status.HasValue || action.Status == ContractActionStatus.Approved)
                throw new PawnshopApplicationException("Действие не требует подтверждения или уже подтверждено");

            var orders = await _cashOrderService.CheckOrdersForConfirmation(contractAction.Id);
            if (orders.Item1)
                throw new PawnshopApplicationException("Подтверждение будет доступно после согласования через кассовые ордера");

            using var transaction = _cashOrderService.BeginCashOrderTransaction();
            {
                var relatedActions = orders.Item2;

                if (action.ActionType == ContractActionType.Buyout || action.ActionType == ContractActionType.BuyoutRestructuringCred)
                {
                    //если это выкуп и есть исполнительная надпись, то вначале проводим все действия по ней, иначе выходит ошибка о нулевом балансе на авансовом счете
                    var contract = _contractService.Get(action.ContractId);
                    if (contract.InscriptionId != null && contract.Inscription.Status == InscriptionStatus.Executed)
                    {
                        _inscriptionService.WriteOffOnBuyout(contract, action);
                        _contractActionRowBuilder.Init(contract, action.CreateDate);
                        foreach (var actionRow in action.Rows)
                            actionRow.Cost = _contractActionRowBuilder.CalculateAmountByAmountType(action.Date, actionRow.PaymentType);

                        _inscriptionService.RestoreOnBalanceOnBuyout(contract, action.CreateDate);
                    }
                }
                var contract2 = _contractService.Get(action.ContractId);
                await _cashOrderService.ChangeLanguageForOrders(relatedActions, 2);
                await _cashOrderService.ChangeStatusForOrders(relatedActions, OrderStatus.Approved, 1, contract2.Branch, false);

                foreach (var contractActionId in relatedActions.OrderBy(x => x))
                {
                    var relatedAction = _contractActionService.GetAsync(contractActionId).Result;
                    if (relatedAction != null && relatedAction.Status.HasValue && relatedAction.Status != ContractActionStatus.Await)
                        continue;

                    relatedAction.Status = ContractActionStatus.Approved;
                    _contractActionService.Save(relatedAction);

                    switch (relatedAction.ActionType)
                    {
                        case ContractActionType.CreditLineClose:
                        case ContractActionType.Buyout:
                        case ContractActionType.BuyoutRestructuringCred:
                            {
                                if (relatedAction.SellingId.HasValue)
                                {
                                    relatedAction.isFromSelling = true;
                                }
                                await _contractActionBuyoutService.ExecuteOnApprove(relatedAction, 1, contract2.BranchId, null);

                                if (relatedAction.SellingId.HasValue)
                                {
                                    var contract = _contractService.Get(relatedAction.ContractId);
                                    contract.Status = ContractStatus.Disposed;
                                    _contractService.Save(contract);
                                    var selling = _contractSellingService.GetSelling(relatedAction.SellingId.Value);
                                    selling.Status = Data.Models.Sellings.SellingStatus.Sold;
                                    _contractSellingService.Save(selling);
                                }

                                if (relatedAction.ExpenseId.HasValue)
                                {
                                    var expense = await _contractExpenseService.GetAsync(relatedAction.ExpenseId.Value);
                                    var expenseType = _expenseService.Get(expense.ExpenseId);
                                    if (expense != null && !expenseType.ExtraExpense)
                                    {
                                        expense.IsPayed = true;
                                        _contractExpenseService.Save(expense);
                                    }
                                }

                                break;
                            }
                    }
                }
                transaction.Commit();
            }
            var orderIds = await _cashOrderService.GetAllRelatedOrdersByContractActionId(contractAction.Id);
            _uKassaService.FinishRequests(orderIds);
        }
    }

}
