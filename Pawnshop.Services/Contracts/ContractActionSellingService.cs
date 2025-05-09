using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Discounts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.InnerNotifications;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Sellings;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Calculation;
using Pawnshop.Services.Domains;
using Pawnshop.Services.Integrations.UKassa;
using Pawnshop.Services.Models.Calculation;
using Pawnshop.Services.Models.List;
using Pawnshop.Services.Models.Sellings;


namespace Pawnshop.Services.Contracts
{
    public class ContractActionSellingService : IContractActionSellingService
    {
        private readonly SellingRepository _repository;
        private readonly ContractActionRepository _contractActionRepository;
        private readonly IContractService _contractService;
        private ISellingRowBuilder _sellingRowBuilder;
        private readonly ISessionContext _sessionContext;
        private readonly string _mainBranchCode = Constants.BKS;
        private readonly InnerNotificationRepository _innerNotificationRepository;
        private readonly IContractActionOperationService _contractActionOperationService;
        private readonly IContractActionPrepaymentService _contractActionPrepaymentService;
        private readonly IContractActionService _contractActionService;
        private readonly IContractActionBuyoutService _contractActionBuyoutService;
        private readonly UserRepository _userRepository;
        private readonly ContractDiscountRepository _contractDiscountRepository;
        private readonly int _payTypeId;
        private readonly PayTypeRepository _payTypeRepository;
        private readonly IUKassaService _uKassaService;
        private readonly ICashOrderService _cashOrderService;
        private readonly GroupRepository _groupRepository;
        private readonly IDomainService _domainService;

        public ContractActionSellingService(SellingRepository repository, ContractActionRepository contractActionRepository,
                                      IContractService contractService, ISellingRowBuilder sellingRowBuilder,
                                      ISessionContext sessionContext,
                                      InnerNotificationRepository innerNotificationRepository,
                                      IContractActionOperationService contractActionOperationService,
                                      IContractActionPrepaymentService contractActionPrepaymentService,
                                      IContractActionService contractActionService,
                                      IContractActionBuyoutService contractActionBuyoutService,
                                      UserRepository userRepository,
                                      ContractDiscountRepository contractDiscountRepository,
                                      PayTypeRepository payTypeRepository,
                                        IUKassaService uKassaService,
            ICashOrderService cashOrderService,
             GroupRepository groupRepository, IDomainService domainService
            )
        {
            _repository = repository;
            _contractActionRepository = contractActionRepository;
            _contractService = contractService;
            _sellingRowBuilder = sellingRowBuilder;
            _sessionContext = sessionContext;
            _innerNotificationRepository = innerNotificationRepository;
            _contractActionOperationService = contractActionOperationService;
            _contractActionPrepaymentService = contractActionPrepaymentService;
            _contractActionService = contractActionService;
            _contractActionBuyoutService = contractActionBuyoutService;
            _userRepository = userRepository;
            _contractDiscountRepository = contractDiscountRepository;
            _payTypeRepository = payTypeRepository;
            _payTypeId = payTypeRepository.Find(new { Code = "CASH" }).Id;
            _uKassaService = uKassaService;
            _cashOrderService = cashOrderService;
            _groupRepository = groupRepository;
            _domainService = domainService;
        }

        public ListModel<Selling> List(ListQueryModel<SellingListQueryModel> listQuery)
        {
            return new ListModel<Selling>
            {
                List = _repository.List(listQuery, listQuery.Model),
                Count = _repository.Count(listQuery, listQuery.Model)
            };
        }

        public ListQueryModel<SellingListQueryModel> enrichQuery(ListQueryModel<SellingListQueryModel> listQuery)
        {
            switch (listQuery.Model.CurrentBranchName)
            {
                case Constants.BKS:
                    {
                        if (listQuery.Model.OwnerId > 0)
                        {
                            listQuery.Model.OwnerId = listQuery.Model.OwnerId;
                            listQuery.Model.IsAllBranchesList = false;
                        }
                        else
                        {
                            listQuery.Model.IsAllBranchesList = true;
                        }

                        break;
                    }
                default:
                    {
                        listQuery.Model.OwnerId = listQuery.Model.CurrentBranchId;
                        listQuery.Model.IsAllBranchesList = false;
                        break;
                    }
            }

            return listQuery;
        }

        public void RegisterSellings(Contract contract, ContractAction action, int userId, int branchId)
        {
            foreach (var position in action.Data.Positions)
            {
                if (position.PositionCount > 1)
                    throw new PawnshopApplicationException(
                        "Реализация товаров в количестве больше 1 не поддерживается.");
                var monthlyPaymentTotalCost = contract.Actions
                    .Where(a => a.ActionType == ContractActionType.MonthlyPayment).SelectMany(a => a.Rows)
                    .Where(r => r.PaymentType == AmountType.Debt).Sum(r => r.Cost);
                var selling = new Selling
                {
                    AuthorId = userId,
                    BranchId = branchId,
                    OwnerId = branchId,
                    CollateralType = contract.CollateralType,
                    ContractId = contract.Id,
                    ContractPositionId = position.Id > 0 ? position.Id : (int?)null,
                    PositionId = position.PositionId,
                    PositionSpecific = position.PositionSpecific,
                    PriceCost = contract.PercentPaymentType == PercentPaymentType.EndPeriod
                        ? position.LoanCost
                        : position.LoanCost - (int)monthlyPaymentTotalCost,
                    Note = action.Note,
                    CreateDate = action.Date.Date,
                    Status = SellingStatus.InStock,
                };
                _repository.Insert(selling);
            }
        }

        public Selling GetSelling(int id)
        {
            var selling = _repository.Get(id);
            if (selling == null)
                throw new NullReferenceException($"Реализация с Id {id} не найдена");

            if (!selling.SellingDate.HasValue)
            {
                selling.SellingDate = DateTime.Now;
            }

            if (selling.SellingRows.Any())
            {
                var actions = _contractActionRepository.GetActions(selling.Id).OrderByDescending(a=>a.Id);

                selling.ActionRows = actions.Where(a => a.ActionType == ContractActionType.Buyout || a.ActionType == ContractActionType.BuyoutRestructuringCred).First().Rows.ToList();
                selling.Discount = actions.Where(a => a.ActionType == ContractActionType.Buyout || a.ActionType == ContractActionType.BuyoutRestructuringCred).First().Discount;

                var sellingAction = actions.Where(a => a.ActionType == ContractActionType.Selling).First();
                selling.SellingActionRows = sellingAction.Rows.ToList();

                selling.ExtraExpensesCost = sellingAction.ExtraExpensesCost??0;
            }
            else
            {
                if (selling.ContractId <= 0)
                    throw new PawnshopApplicationException($"Ссылка на Договор для Реализации - {selling.Id} не найдена");                
                var contract = _contractService.Get(selling.ContractId);
                if (contract == null)
                    throw new NullReferenceException($"Не найден Договор c Id {selling.ContractId}");

                selling = _sellingRowBuilder.GetSellingDuty(contract, selling, false);
            }

            return selling;
        }

        private void SaveSellingActionRowToSellingRows(Selling selling, ContractAction sellingAction)
        {
            sellingAction.Rows.ToList().ForEach(s=> {
                var sellingRow = new SellingRow();
                sellingRow.SellingId = selling.Id;
                sellingRow.Cost = s.Cost;
                sellingRow.SellingPaymentType = selling.GetSellingPaymentType();
                sellingRow.CreateDate = sellingAction.CreateDate;
                sellingRow.DebitAccountId = (int)s.DebitAccountId;
                sellingRow.CreditAccountId = (int)s.CreditAccountId;
                sellingRow.OrderId = s.OrderId;
                sellingRow.ActionId = s.ActionId;
                sellingRow.ContractDiscountId = (bool)selling.Discount?.Discounts.Any()?selling.Discount.Discounts.First().ContractDiscountId:null;
                sellingRow.ExtraExpensesCost = selling.ExtraExpensesCost;

                SaveSellingRow(sellingRow);

                selling.CashOrderId = s.OrderId;
                selling.SellingCost = selling.SellingCost;
                selling.Status = SellingStatus.Sold;
            });
            Save(selling);
            _contractService.ContractStatusUpdate(selling.ContractId, ContractStatus.Disposed);
        }

        public Selling GetSellingDuty(SellingDuty sellingDuty)
        {
            var selling = _repository.Get(sellingDuty.Id);
            if (selling == null)
                throw new NullReferenceException($"Не найдена запись Selling для Id {sellingDuty.Id}");

            if (selling.ContractId <= 0)
                throw new PawnshopApplicationException($"Ссылка на Договор для Реализации - {selling.Id} не найдена");
            var contract = _contractService.Get(selling.ContractId);
            if (contract == null)
                throw new NullReferenceException($"Не найден Договор c Id {selling.ContractId}");

            selling.SellingCost = sellingDuty.SellingCost;
            return Save(_sellingRowBuilder.GetSellingDuty(contract, selling, false));
        }

        public Selling Sell(Selling selling, int branchId)
        {
            if (selling.Id == 0)
                throw new PawnshopApplicationException("Сохраните реализацию для регистрации продажи");
            if (selling.CashOrderId.HasValue)
                throw new PawnshopApplicationException("Продажа уже зарегистрирована");

            var sellingDB = _repository.Get(selling.Id);
            if (sellingDB == null)
                throw new NullReferenceException($"Не найдена запись Selling для Id {selling.Id}");

            if (selling.SellingCost == null)
                throw new PawnshopApplicationException($"Введите сумму продажи по Договору с Id {selling.ContractId}");
            if (selling.SellingCost!= sellingDB.SellingCost)
                throw new PawnshopApplicationException($"Суммы продаж по Реализации не совпадают по Договору с Id {selling.ContractId}");

            if (selling.ContractId <= 0)
                throw new PawnshopApplicationException($"Ссылка на Договор для Реализации - {selling.Id} не найдена");
            
            var contract = _contractService.Get(selling.ContractId);
            if (contract == null)
                throw new NullReferenceException($"Не найден Договор c Id {selling.ContractId}");

            if (selling.PriceCost != sellingDB.PriceCost && contract.PercentPaymentType != PercentPaymentType.EndPeriod)
                throw new PawnshopApplicationException($"Суммы себестоимости по Реализации не совпадают по Договору с Id {selling.ContractId}");

            int authorId = _sessionContext.UserId;

            using (IDbTransaction transaction = _repository.BeginTransaction())
            {
                //Создаем Дисконт в БД
                _sellingRowBuilder.GetSellingDuty(contract, selling, true);

                //Предоплата
                var prepaymentCost = selling.GetDiffPrepaymentBalanceAndPriceCost();

                ContractAction prepaymentAction = _contractActionPrepaymentService.Exec(contract.Id, prepaymentCost, _payTypeId, branchId, authorId, date: DateTime.Now, orderStatus: OrderStatus.Approved);

                if (prepaymentAction == null)
                    throw new PawnshopApplicationException($"Проводка по предоплате не создана для Договора с Id {contract.Id}");

                //Реализация
                var sellingAmount = Math.Abs(selling.GetDiffSellingCostAndPriceCost());

                var sellingAction = new ContractAction
                {
                    ActionType = ContractActionType.Selling,
                    AuthorId = authorId,
                    ContractId = contract.Id,
                    Cost = sellingAmount,
                    TotalCost = sellingAmount,
                    Rows = selling.SellingActionRows.ToArray(),
                    Discount = null,
                    Reason = "",
                    Date = DateTime.Now,
                    IsInitialFee = false,
                    Data = new ContractActionData(),
                    EmployeeId = null,
                    SellingId = selling.Id,
                    PayTypeId = _payTypeId
                };

                _contractActionOperationService.Register(contract, sellingAction, authorId, branchId: branchId, orderStatus: OrderStatus.Approved);

                if (prepaymentAction != null)
                {
                    prepaymentAction.SellingId = selling.Id;
                    prepaymentAction.ChildActionId = sellingAction.Id;
                    _contractActionService.Save(prepaymentAction);

                    sellingAction.ParentActionId = prepaymentAction.Id;
                    sellingAction.ParentAction = prepaymentAction;
                    _contractActionService.Save(sellingAction);
                }

                //Выкуп
                string buyoutReason = string.Format(Constants.REASON_AUTO_BUYOUT, contract.ContractNumber, sellingAction.Date.ToString("dd.MM.yyyy"));
                int buyoutReasonId = _domainService.GetDomainValue(Constants.BUYOUT_REASON_CODE, Constants.BUYOUT_AUTOMATIC_BUYOUT).Id;
                decimal calculateCost = selling.ActionRows.Sum(a=>a.Cost);
                var buyoutAction = new ContractAction
                {
                    ActionType = contract.IsContractRestructured && contract.ContractClass == ContractClass.Credit ? ContractActionType.BuyoutRestructuringCred: ContractActionType.Buyout,
                    isFromSelling = true,
                    AuthorId = authorId,
                    ContractId = contract.Id,
                    Cost = calculateCost,
                    TotalCost = calculateCost,
                    Rows = selling.ActionRows.ToArray(),
                    Discount = selling.Discount,
                    Reason = buyoutReason,
                    Date = DateTime.Now,
                    PayTypeId = _payTypeId,
                    IsInitialFee = false,
                    Data = new ContractActionData(),
                    EmployeeId = null,
                    ExtraExpensesCost = selling.ExtraExpensesCost != 0 ? selling.ExtraExpensesCost : 0,
                    SellingId = selling.Id
                };

                _contractActionOperationService.Register(contract, buyoutAction, authorId, branchId: branchId, orderStatus: OrderStatus.Approved);

                var orders = _cashOrderService.CheckOrdersForConfirmation(buyoutAction.Id).Result;
                var relatedActions = orders.Item2;
                var branch = _groupRepository.Get(branchId);
                _cashOrderService.ChangeStatusForOrders(relatedActions, OrderStatus.Approved, _sessionContext.UserId, branch, _sessionContext.ForSupport);

                _contractActionBuyoutService.ExecuteOnApprove(buyoutAction, authorId, branchId, sellingAction).Wait();

                buyoutAction.ParentActionId = sellingAction.Id;
                buyoutAction.ParentAction = sellingAction;
                _contractActionService.Save(buyoutAction);

                sellingAction.ChildActionId = buyoutAction.Id;
                _contractActionService.Save(sellingAction);

                 SaveSellingActionRowToSellingRows(selling, sellingAction);

                transaction.Commit();
                var orderIds = _cashOrderService.GetAllRelatedOrdersByContractActionId(sellingAction.Id).Result;
                _uKassaService.FinishRequests(orderIds);
            }

            return selling;
        }

        private void CancelSellingAction(ContractAction sellingAction, Selling selling, int authorId)
        {
            User author = _userRepository.Get(authorId);
            if (author == null)
                throw new NullReferenceException($"Пользователь(автор) {author} не найден");

            sellingAction.isFromSelling = true;
            foreach (var row in selling.SellingRows)
            {
                _repository.DeleteRow(row.Id);
            }

            selling.SellingCost = null;
            selling.SellingDate = null;
            selling.CashOrderId = null;
            selling.Status = SellingStatus.InStock;
            selling.AuthorId = authorId;

            _repository.Update(selling);

            var contract = _contractService.Get(selling.ContractId);
            var position = contract.Positions.SingleOrDefault(p => p.Id == selling.ContractPositionId.Value);

            if (selling.ContractId > 0 && selling.ContractPositionId.HasValue)
            {
                if (position != null)
                {
                    position.Status = ContractPositionStatus.SoldOut;
                }
                if (contract.Positions.All(p => p.Status == ContractPositionStatus.SoldOut))
                {
                    contract.Status = ContractStatus.SoldOut;
                }
                _contractService.Save(contract);
            }
        }

        public void Cancel(int id, int branchId)
        {
            var selling = GetSelling(id);
            if (selling.Status != SellingStatus.Sold)
                throw new PawnshopApplicationException("Не возможно отменить продажу у непроданного изделия");
            if (!selling.CashOrderId.HasValue)
                throw new PawnshopApplicationException("Не найден приходный кассовый ордер, отмена не возможна");
            int authorId = _sessionContext.UserId;
            
            using (var transaction = _repository.BeginTransaction())
            {
                if (IsCancelSelling(selling))
                {
                    var lastAction = _contractActionRepository.GetActions(selling.Id).OrderByDescending(a => a.Id).First();
                    _contractActionOperationService.Cancel(lastAction.Id, authorId, branchId, true, false).Wait();
                }

                transaction.Commit();
            }
        }

        private bool IsCancelSelling(Selling selling)
        {
            if (selling.SellingRows.Any() && selling.SellingRows.First().OrderId != 0) return true;
            return false;
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

        public IDbTransaction BeginContractActionSellingTransaction()
        {
            return _repository.BeginTransaction();
        }

        public Selling Save(Selling model, int? branchId = null)
        {
            using (var transaction = BeginContractActionSellingTransaction())
            {
                if (model.Id == 0)
                {
                    model.OwnerId = (int)branchId;
                    model.BranchId = (int)branchId;
                    model.AuthorId = _sessionContext.UserId;
                }

                if (model.Id > 0)
                    _repository.Update(model);
                else
                    _repository.Insert(model);

                transaction.Commit();
            }
            return model;
        }

        public SellingRow SaveSellingRow(SellingRow model)
        {
            using (var transaction = BeginContractActionSellingTransaction())
            {
                if (model.Id > 0)
                    _repository.UpdateRow(model);
                else
                    _repository.InsertRow(model);

                transaction.Commit();
            }

            return model;
        }
    }
}
