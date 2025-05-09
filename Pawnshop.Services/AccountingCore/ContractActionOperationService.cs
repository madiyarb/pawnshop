using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Access.AccountingCore;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Discounts;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Files;
using Pawnshop.Data.Models.InnerNotifications;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Mintos.UploadModels;
using Pawnshop.Data.Models.PayOperations;
using Pawnshop.Data.Models.Sellings;
using Pawnshop.Services.Applications;
using Pawnshop.Services.Calculation;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Crm;
using Pawnshop.Services.Discounts;
using Pawnshop.Services.Expenses;
using Pawnshop.Services.Insurance;
using Pawnshop.Services.Models.Calculation;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.Insurance;
using Pawnshop.Services.Models.List;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using Pawnshop.Services.Cars;
using Pawnshop.Services.Collection;
using Pawnshop.Services.ClientDeferments.Interfaces;
using Pawnshop.Core.Impl;

namespace Pawnshop.Services.AccountingCore
{
    public class ContractActionOperationService : IContractActionOperationService
    {
        private readonly ICashOrderService _cashOrderService;
        private readonly IBusinessOperationService _businessOperationService;
        private readonly IDictionaryWithSearchService<Group, BranchFilter> _branchService;
        private readonly IContractExpenseOperationService _contractExpenseOperationService;
        private readonly IContractExpenseService _contractExpenseService;
        private readonly IContractActionService _contractActionService;
        private readonly ContractActionRowRepository _contractActionRowRepository;
        private readonly IContractService _contractService;
        private readonly IExpenseService _expenseService;
        private readonly ClientRepository _clientRepository;
        private readonly DiscountRepository _discountRepository;
        private readonly DiscountRowRepository _discountRowRepository;
        private readonly ContractDiscountRepository _contractDiscountRepository;
        private readonly ContractFileRowRepository _contractFileRowRepository;
        private readonly ContractActionCheckValueRepository _contractActionCheckValueRepository;
        private readonly PayTypeRepository _payTypeRepository;
        private readonly PayOperationRepository _payOperationRepository;
        private readonly PayOperationNumberCounterRepository _payOperationNumberCounterRepository;
        private readonly ContractRepository _contractRepository;
        private readonly UserRepository _userRepository;
        private readonly ContractDiscountRepository contractDiscountRepository;
        private readonly ContractActionRepository _contractActionRepository;
        private readonly IContractActionOperationPermisisonService _contractActionOperationPermisisonService;
        private readonly IContractDutyService _contractDutyService;
        private readonly MintosContractRepository _mintosContractRepository;
        private readonly MintosContractActionRepository _mintosContractActionRepository;
        private readonly CategoryRepository _categoryRepository;
        private readonly ParkingHistoryRepository _parkingHistoryRepository;
        private readonly ParkingActionService _parkingActionService;
        private readonly InsuranceRepository _insuranceRepository;
        private readonly CashOrderRepository _orderRepository;
        private readonly InsuranceActionRepository _insuranceActionRepository;
        private readonly CarRepository _carRepository;
        private readonly MachineryRepository _machineryRepository;
        private readonly SellingRepository _sellingRepository;
        private readonly GroupRepository _groupRepository;
        private readonly IEventLog _eventLog;
        private readonly InnerNotificationRepository _innerNotificationRepository;
        private readonly OnlinePaymentRepository _onlinePaymentRepository;
        private readonly AccountRecordRepository _accountRecordRepository;
        private readonly IDiscountService _discountService;
        private readonly ICrmPaymentService _crmPaymentService;
        private readonly IAccountRecordService _accountRecordService;
        private readonly IContractPaymentScheduleService _contractPaymentScheduleService;
        private readonly IInsurancePolicyService _insurancePolicyService;
        private readonly ISessionContext _sessionContext;
        private readonly IInsurancePoliceRequestService _insurancePoliceRequestService;
        private readonly IContractRateService _contractRateService;
        private readonly IApplicationService _applicationService;
        private readonly ICollectionService _collectionService;
        private readonly ContractAdditionalInfoRepository _сontractAdditionalInfoRepository;
        private readonly ContractPaymentScheduleRepository _contractPaymentScheduleRepository;
        private readonly IInsuranceService _insuranceService;
        private readonly IClientDefermentService _clientDefermentService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAccountService _accountService;

        public ContractActionOperationService(ICashOrderService cashOrderService, IBusinessOperationService businessOperationService,
            IDictionaryWithSearchService<Group, BranchFilter> branchService,
            IContractExpenseOperationService contractExpenseOperationService,
            IContractActionService contractActionService,
            IContractExpenseService contractExpenseService,
            ContractActionRowRepository contractActionRowRepository,
            DiscountRepository discountRepository,
            DiscountRowRepository discountRowRepository,
            ContractDiscountRepository contractDiscountRepository,
            ContractFileRowRepository contractFileRowRepository,
            ContractActionCheckValueRepository contractActionCheckValueRepository,
            IContractService contractService, PayTypeRepository payTypeRepository,
            ClientRepository clientRepository, PayOperationRepository payOperationRepository,
            PayOperationNumberCounterRepository payOperationNumberCounterRepository,
            IExpenseService expenseService, ContractRepository contractRepository,
            UserRepository userRepository, IContractDutyService contractDutyService,
            IContractActionOperationPermisisonService contractActionOperationPermisisonService,
            ContractActionRepository contractActionRepository,
            MintosContractRepository mintosContractRepository,
            MintosContractActionRepository mintosContractActionRepository,
            CategoryRepository categoryRepository, InsuranceRepository insuranceRepository,
            ParkingHistoryRepository parkingHistoryRepository, CashOrderRepository orderRepository,
            ParkingActionService parkingActionService,
            IEventLog eventLog, InsuranceActionRepository insuranceActionRepository,
            CarRepository carRepository, MachineryRepository machineryRepository,
            SellingRepository sellingRepository, GroupRepository groupRepository,
            InnerNotificationRepository innerNotificationRepository,
            OnlinePaymentRepository onlinePaymentRepository,
            IDiscountService discountService, ICrmPaymentService crmPaymentService,
            AccountRecordRepository accountRecordRepository,
            IAccountRecordService accountRecordService,
            IContractPaymentScheduleService contractPaymentScheduleService,
            IInsurancePolicyService insurancePolicyService,
            ISessionContext sessionContext,
            IContractRateService contractRateService,
            IInsurancePoliceRequestService insurancePoliceRequestService,
            IApplicationService applicationService,
            ICollectionService collectionService,
            ContractAdditionalInfoRepository сontractAdditionalInfoRepository,
            ContractPaymentScheduleRepository contractPaymentScheduleRepository,
            IUnitOfWork unitOfWork,
            IInsuranceService insuranceService,
            IClientDefermentService clientDefermentService,
            IAccountService accountService)
        {
            _cashOrderService = cashOrderService;
            _businessOperationService = businessOperationService;
            _branchService = branchService;
            _contractExpenseOperationService = contractExpenseOperationService;
            _contractActionService = contractActionService;
            _contractExpenseService = contractExpenseService;
            _contractActionRowRepository = contractActionRowRepository;
            _discountRepository = discountRepository;
            _discountRowRepository = discountRowRepository;
            _contractDiscountRepository = contractDiscountRepository;
            _contractFileRowRepository = contractFileRowRepository;
            _contractActionCheckValueRepository = contractActionCheckValueRepository;
            _contractService = contractService;
            _payTypeRepository = payTypeRepository;
            _clientRepository = clientRepository;
            _payOperationRepository = payOperationRepository;
            _payOperationNumberCounterRepository = payOperationNumberCounterRepository;
            _expenseService = expenseService;
            _contractRepository = contractRepository;
            _userRepository = userRepository;
            _contractActionOperationPermisisonService = contractActionOperationPermisisonService;
            _contractDutyService = contractDutyService;
            _contractActionRepository = contractActionRepository;
            _mintosContractRepository = mintosContractRepository;
            _mintosContractActionRepository = mintosContractActionRepository;
            _categoryRepository = categoryRepository;
            _parkingHistoryRepository = parkingHistoryRepository;
            _parkingActionService = parkingActionService;
            _insuranceRepository = insuranceRepository;
            _orderRepository = orderRepository;
            _eventLog = eventLog;
            _insuranceActionRepository = insuranceActionRepository;
            _carRepository = carRepository;
            _machineryRepository = machineryRepository;
            _sellingRepository = sellingRepository;
            _groupRepository = groupRepository;
            _innerNotificationRepository = innerNotificationRepository;
            _onlinePaymentRepository = onlinePaymentRepository;
            _discountService = discountService;
            _crmPaymentService = crmPaymentService;
            _accountRecordRepository = accountRecordRepository;
            _accountRecordService = accountRecordService;
            _contractPaymentScheduleService = contractPaymentScheduleService;
            _insurancePolicyService = insurancePolicyService;
            _sessionContext = sessionContext;
            _insurancePoliceRequestService = insurancePoliceRequestService;
            _contractRateService = contractRateService;
            _applicationService = applicationService;
            _collectionService = collectionService;
            _сontractAdditionalInfoRepository = сontractAdditionalInfoRepository;
            _contractPaymentScheduleRepository = contractPaymentScheduleRepository;
            _insuranceService = insuranceService;
            _clientDefermentService = clientDefermentService;
            _unitOfWork = unitOfWork;
            _accountService = accountService;
        }

        public ContractAction Register(IContract contract, ContractAction action, int authorId, int? branchId = null,
            bool callActionRowBusinessOperation = true, OrderStatus? orderStatus = null, bool forceExpensePrepaymentReturn = true)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            ValidateContractAction(action);
            User author = _userRepository.Get(authorId);
            if (author == null)
                throw new PawnshopApplicationException($"Пользователь {authorId} не найден");

            action.ContractId = contract.Id;
            action.AuthorId = authorId;
            action.CreateDate = DateTime.Now;
            Group branch = _branchService.GetAsync(branchId ?? contract.BranchId).Result;
            if (branch == null)
                throw new PawnshopApplicationException($"Филиал {branchId} не найден");

            if (action.Data == null)
                action.Data = new ContractActionData();

            action.Data.Branch = branch;
            using (IDbTransaction transaction = _cashOrderService.BeginCashOrderTransaction())
            {
                action.ExpenseId = null;
                _contractActionService.Save(action);
                RegisterDiscounts(action, contract, branch, author);
                RegisterContractActionRows(action, contract, branch,
                    callActionRowBusinessOperation, author.Id, orderStatus: orderStatus);
                RegisterContractActionCheckValues(action);
                RegisterFileRows(action);
                PayExtraExpenses(action, branch, authorId, forceExpensePrepaymentReturn);
                RegisterExpense(action, branch, authorId, forceExpensePrepaymentReturn);
                _contractActionService.Save(action);
                transaction.Commit();
            }

            return action;
        }

        public void CancelDelete(int contractActionId, int authorId, int branchId)
        {
            Group branch = _groupRepository.Get(branchId);
            if (branch == null)
                throw new PawnshopApplicationException($"Филиал {branchId} не найден");

            User author = _userRepository.Get(authorId);
            if (author == null)
                throw new PawnshopApplicationException($"Пользователь(автор) {authorId} не найден");

            ContractAction contractAction = _contractActionRepository.Get(contractActionId);
            if (contractAction == null)
                throw new PawnshopApplicationException($"Действие по договору {contractActionId} не найдено");

            var contract = _contractService.Get(contractAction.ContractId);
            if (contract == null)
                throw new PawnshopApplicationException($"Договор {contractAction.ContractId} не найден");

            var relatedContractActions = _contractActionService.GetRelatedContractActionsByActionId(contractAction.Id).Result;
            using (IDbTransaction transaction = _cashOrderService.BeginCashOrderTransaction())
            {
                foreach (var id in relatedContractActions)
                {
                    var relatedContractAction = _contractActionRepository.Get(id);
                    if (relatedContractAction == null)
                        throw new PawnshopApplicationException($"Действие по договору {id} не найдено");

                    relatedContractAction.Status = ContractActionStatus.Await;
                    _contractActionService.Save(relatedContractAction);

                }
                transaction.Commit();
            }
        }

        public void UndoCancel(int contractActionId, int authorId, int branchId)
        {
            Group branch = _groupRepository.Get(branchId);
            if (branch == null)
                throw new PawnshopApplicationException($"Филиал {branchId} не найден");

            User author = _userRepository.Get(authorId);
            if (author == null)
                throw new PawnshopApplicationException($"Пользователь(автор) {authorId} не найден");

            ContractAction contractAction = _contractActionRepository.Get(contractActionId);
            if (contractAction == null)
                throw new PawnshopApplicationException($"Действие по договору {contractActionId} не найдено");

            var contract = _contractService.Get(contractAction.ContractId);
            if (contract == null)
                throw new PawnshopApplicationException($"Договор {contractAction.ContractId} не найден");

            var relatedContractActions = _contractActionService.GetRelatedContractActionsByActionId(contractAction.Id).Result;
            using (IDbTransaction transaction = _cashOrderService.BeginCashOrderTransaction())
            {
                foreach (var id in relatedContractActions.OrderByDescending(x => x))
                {
                    var relatedContractAction = _contractActionRepository.Get(id);
                    if (relatedContractAction == null)
                        throw new PawnshopApplicationException($"Действие по договору {id} не найдено");

                    relatedContractAction.Status = ContractActionStatus.Approved;
                    _contractActionService.Save(relatedContractAction);
                }
                transaction.Commit();
            }
        }

        public async Task Cancel(int contractActionId, int authorId, int branchId, bool isStorn, bool autoApprove)
        {
            Group branch = _groupRepository.Get(branchId);
            if (branch == null)
                throw new PawnshopApplicationException($"Филиал {branchId} не найден");

            User author = _userRepository.Get(authorId);
            if (author == null)
                throw new PawnshopApplicationException($"Пользователь(автор) {authorId} не найден");

            ContractAction contractAction = _contractActionRepository.Get(contractActionId);
            if (contractAction == null)
                throw new PawnshopApplicationException($"Действие по договору {contractActionId} не найдено");

            var contract = _contractService.Get(contractAction.ContractId);
            if (contract == null)
                throw new PawnshopApplicationException($"Договор {contractAction.ContractId} не найден");

            var savedAction = _contractActionService.GetAsync(contractAction.Id).Result;
            if (savedAction.DeleteDate != null)
                throw new PawnshopApplicationException("Невозможно отменить действие второй раз, обновите страницу.");

            if (contractAction.PayOperationId.HasValue)
            {
                var payOperation = _payOperationRepository.Get(contractAction.PayOperationId.Value);
                if (payOperation.Status == PayOperationStatus.Checked ||
                    payOperation.Status == PayOperationStatus.Executed)
                {
                    throw new PawnshopApplicationException(
                        "Невозможно отменить действие, платежная операция проверена или находится в обработке");
                }
            }

            Category category = null;
            if (contract.ContractClass != ContractClass.Tranche && (contract.CollateralType == CollateralType.Car || contract.CollateralType == CollateralType.Machinery))
            {
                category = _categoryRepository.Get(contract.Positions.FirstOrDefault().CategoryId);
            }

            bool foundCashOrders = false;
            var relatedContractActions = _contractActionService.GetRelatedContractActionsByActionId(contractAction.Id).Result;
            foreach (var id in relatedContractActions)
            {
                foundCashOrders = _cashOrderService.CashOrdersExists(id).Result;
                if (foundCashOrders)
                    break;
            }
            if (!foundCashOrders)
                autoApprove = true;

            using (IDbTransaction transaction = _cashOrderService.BeginCashOrderTransaction())
            {
                if (contract.ContractClass != ContractClass.Tranche && !autoApprove &&
                    (contractAction.ActionType == ContractActionType.Buyout || contractAction.ActionType == ContractActionType.BuyoutRestructuringCred))
                {
                    _parkingActionService.CancelParkingHistory(contract);
                }

                if (contractAction.ActionType == ContractActionType.Sign || contractAction.ActionType == ContractActionType.PartialPayment)
                {
                    if (_contractService.CancelChangeCategory(contract, contractAction))
                    {
                        _contractService.Save(contract);
                        _contractActionService.Save(contractAction);
                    }
                }

                foreach (var id in relatedContractActions.OrderByDescending(x => x))
                {
                    var relatedContractAction = _contractActionRepository.Get(id);
                    if (relatedContractAction == null)
                        throw new PawnshopApplicationException($"Действие по договору {id} не найдено");

                    if (autoApprove)
                    {
                        contract = _contractService.Get(relatedContractAction.ContractId);
                        if (contract == null)
                            throw new PawnshopApplicationException($"Договор {contractAction.ContractId} не найден");
                        ExecOnCancelApprove(relatedContractAction, ref contract, category, author, isStorn, branch);
                        relatedContractAction.Status = ContractActionStatus.Canceled;

                        //откат графика
                        if (relatedContractAction.ActionType == ContractActionType.PartialPayment)
                        {
                            var debtCost = relatedContractAction.Cost;

                            var rows = _contractActionRowRepository.GetByContractActionId(relatedContractAction.Id);
                            if (rows != null)
                            {
                                debtCost = rows.Where(x => x.PaymentType == AmountType.Debt).Sum(x => x.Cost);
                            }

                            _contractPaymentScheduleService.RollbackScheduleToPreviousPartialPayment(contract.Id, id, debtCost).Wait();
                            var next = await _contractPaymentScheduleService.GetNextPaymentSchedule(contract.Id);
                            if (next != null)
                            {
                                contract.NextPaymentDate = next.Date;
                            }
                            _contractService.Save(contract);

                            if (relatedContractAction.Data != null && relatedContractAction.Data.CategoryChanged && contract.ContractClass == ContractClass.Tranche)
                            {
                                var creditLine = _contractRepository.GetContractPositions(contract.CreditLineId.Value);

                                if (creditLine.ContractData != null && _contractService.GetContractSettings(creditLine.Id).IsInsuranceAdditionalLimitOn
                                    && _contractService.CancelChangeCategory(creditLine, relatedContractAction))
                                {
                                    _contractService.Save(creditLine);
                                }
                            }
                        }
                    }
                    else
                    {
                        relatedContractAction.Status = ContractActionStatus.AwaitForCancelApprove;
                    }

                    if (contract.ContractClass != ContractClass.Tranche && !autoApprove && (contractAction.ActionType == ContractActionType.Buyout || contractAction.ActionType == ContractActionType.BuyoutRestructuringCred))
                    {
                        _parkingActionService.CancelParkingHistory(contract);
                    }

                    if ((contractAction.ActionType == ContractActionType.Sign && relatedContractAction.ActionType == ContractActionType.Addition) || relatedContractAction.ActionType == ContractActionType.PartialPayment)
                    {
                        if (_contractService.CancelChangeCategory(contract, relatedContractAction))
                        {
                            _contractService.Save(contract);
                        }
                    }

                    _contractActionService.Save(relatedContractAction);
                }
                transaction.Commit();
            }
        }

        public void ExecOnCancelApprove(ContractAction contractAction, ref Contract contract, Category category, User author, bool isStorn, Group branch)
        {
            var recalculateBalanceAccountDict = new Dictionary<int, (int, DateTime)>();
            var parkingHistoryForCancel = _parkingHistoryRepository.GetActiveLastByContractId(contract.Id);
            var insurance = _insuranceRepository.Find(new InsuranceQueryModel { ContractId = contractAction.ContractId });

            using (var transaction = _insurancePolicyService.BeginTransaction())
            {
                if (contractAction.ActionType == ContractActionType.Sign && contractAction.ParentActionId.HasValue)
                {
                    var policeRequest = _insurancePoliceRequestService.GetApprovedPoliceRequest(contract.Id);

                    if (policeRequest != null)
                    {
                        var policy = _insurancePolicyService.GetInsurancePolicy(policeRequest.Id);

                        if (policy != null)
                        {
                            CancelPoliciesForContract(new List<InsurancePolicy>() { policy }, contract);
                            transaction.Commit();
                        }
                    }
                }
            }

            using (IDbTransaction transaction = _carRepository.BeginTransaction())
            {

                if (parkingHistoryForCancel != null && parkingHistoryForCancel.ActionId == contractAction.Id)
                    _parkingHistoryRepository.Delete(parkingHistoryForCancel.Id);

                if (contractAction.FollowedId.HasValue)
                {
                    var followedContract = _contractService.Get(contractAction.FollowedId.Value);
                    if (followedContract == null)
                        throw new PawnshopApplicationException($"Договор(порожденный) {contractAction.FollowedId.Value} не найден");

                    if (!followedContract.DeleteDate.HasValue)
                    {
                        if (followedContract.Status > ContractStatus.Draft)
                            throw new PawnshopApplicationException(
                                "Нельзя отменить действие, если порожденный договор подписан.");

                        _contractRepository.Delete(followedContract.Id);
                    }
                }

                HandleCancelForActionTypes(contractAction, ref contract, insurance, category, author.ForSupport, author.Id);

                if (contractAction.PayOperationId.HasValue)
                {
                    var payOperation = _payOperationRepository.Get(contractAction.PayOperationId.Value);
                    payOperation.Status = PayOperationStatus.Canceled;
                    _payOperationRepository.Update(payOperation);
                    _payOperationRepository.Delete(contractAction.PayOperationId.Value);
                }

                var orders = _orderRepository.GetAllOrdersByContractActionId(contractAction.Id).OrderBy(x => x.OrderType);
                foreach (var order in orders.Where(x => x.DeleteDate == null).OrderByDescending(x => x.Id))
                {
                    if (isStorn)
                        _cashOrderService.Cancel(_cashOrderService.GetAsync(order.Id).Result, author.Id, branch);
                    else
                    {
                        IDictionary<int, (int, DateTime)> actionsWithDatesDict = _cashOrderService.Delete(order.Id, author.Id, branch.Id);
                        if (actionsWithDatesDict == null)
                            throw new PawnshopApplicationException(
                                $"Ожидалось что {nameof(_cashOrderService)}.{nameof(_cashOrderService.Delete)} не вернет null");

                        foreach ((int accountId, (int accountRecordId, DateTime accountDate)) in actionsWithDatesDict)
                        {
                            if (recalculateBalanceAccountDict.ContainsKey(accountId))
                            {
                                (int _, DateTime date) = recalculateBalanceAccountDict[accountId];
                                if (date < accountDate)
                                    continue;
                            }

                            recalculateBalanceAccountDict[accountId] = (accountRecordId, accountDate);
                        }
                    }

                    _eventLog.Log(EventCode.CashOrderСanceled, EventStatus.Success, EntityType.CashOrder,
                        order.Id, null, null, userId: author.Id);
                }

                if (contractAction.Files != null && contractAction.Files.Count > 0)
                {
                    foreach (var file in contractAction.Files)
                    {
                        _contractFileRowRepository.DeleteFileRow(file.Id);
                    }
                }

                if (contractAction.Data != null && contractAction.Data.Notification != null)
                {
                    _innerNotificationRepository.Delete((int)contractAction.Data.Notification.Id);
                    _innerNotificationRepository.Insert(new InnerNotification
                    {
                        CreateDate = DateTime.Now,
                        CreatedBy = author.Id,
                        EntityType = EntityType.Contract,
                        EntityId = contract.Id,
                        Message =
                            $"Действие \"{contractAction.ActionType.GetDisplayName()}\" на сумму {contractAction.TotalCost} для договора {contract.ContractNumber} было отменено в филиале {branch.DisplayName}. Примите соответствующие меры.",
                        ReceiveBranchId = contract.BranchId,
                        Status = InnerNotificationStatus.Sent
                    });

                }

                if (contractAction.OnlinePaymentId.HasValue)
                {
                    if (contract.Actions.Where(x => x.OnlinePaymentId == contractAction.OnlinePaymentId).Count() == 1)
                    {
                        _onlinePaymentRepository.Delete((int)contractAction.OnlinePaymentId);
                    }
                }

                if (contractAction.ExpenseId.HasValue)
                {
                    int contractExpenseId = contractAction.ExpenseId.Value;
                    if (isStorn)
                        _contractExpenseOperationService.CancelAsync(contractExpenseId, author.Id, branch.Id, actionId: contractAction.Id).Wait();
                    else
                    {
                        IDictionary<int, (int, DateTime)> actionsWithDatesDict =
                            _contractExpenseOperationService.DeleteWithOrders(contractExpenseId, author.Id, branch.Id, contractActionId: contractAction.Id);
                        if (actionsWithDatesDict == null)
                            throw new PawnshopApplicationException(
                                $"Ожидалось что {nameof(_contractExpenseOperationService)}.{nameof(_contractExpenseOperationService.DeleteWithOrders)} не вернет null");

                        foreach ((int accountId, (int accountRecordId, DateTime accountDate)) in actionsWithDatesDict)
                        {
                            if (recalculateBalanceAccountDict.ContainsKey(accountId))
                            {
                                (int _, DateTime date) = recalculateBalanceAccountDict[accountId];
                                if (date < accountDate)
                                    continue;
                            }

                            recalculateBalanceAccountDict[accountId] = (accountRecordId, accountDate);
                        }
                    }
                }

                var mintosAction = _mintosContractActionRepository.GetByContractActionId(contractAction.Id);
                if (mintosAction != null && mintosAction.Status == MintosUploadStatus.Success)
                {
                    throw new PawnshopApplicationException(
                        "Данное действие отмене не подлежит, так как уже учитывается в другой системе.");
                }

                if (mintosAction != null)
                {
                    mintosAction.Status = MintosUploadStatus.Canceled;
                    _mintosContractActionRepository.Update(mintosAction);
                    _mintosContractActionRepository.Delete(mintosAction.Id);
                }

                if (contractAction.ExtraExpensesCost.HasValue && contractAction.ExtraExpensesCost.Value > 0)
                {
                    IDictionary<int, (int, DateTime)> actionsWithDatesDict =
                        _contractExpenseOperationService.CancelExpensesByActionIdAsync(contractAction.Id, author.Id, branch.Id, isStorn).Result;
                    if (actionsWithDatesDict == null)
                        throw new PawnshopApplicationException(
                            $"Ожидалось что {nameof(_contractExpenseOperationService)}.{nameof(_contractExpenseOperationService.CancelExpensesByActionIdAsync)} не вернет null");

                    foreach ((int accountId, (int accountRecordId, DateTime accountDate)) in actionsWithDatesDict)
                    {
                        if (recalculateBalanceAccountDict.ContainsKey(accountId))
                        {
                            (int _, DateTime date) = recalculateBalanceAccountDict[accountId];
                            if (date < accountDate)
                                continue;
                        }

                        recalculateBalanceAccountDict[accountId] = (accountRecordId, accountDate);
                    }
                }

                _contractRepository.Update(contract);
                List<ContractDiscount> contractDiscounts = _contractDiscountRepository.GetByContractActionId(contractAction.Id);
                if (contractDiscounts == null)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(contractDiscounts)} не будет null");

                var contractDiscountIds = new HashSet<int>();
                foreach (ContractDiscount contractDiscount in contractDiscounts)
                {
                    if (contractDiscount == null)
                        throw new PawnshopApplicationException($"Ожидалось что {nameof(contractDiscount)} не будет null");

                    contractDiscountIds.Add(contractDiscount.Id);
                }

                List<Discount> discounts = _discountService.GetByContractActionId(contractAction.Id);
                if (discounts == null)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(discounts)} не будет null");

                foreach (Discount discount in discounts)
                {
                    if (discount == null)
                        throw new PawnshopApplicationException($"Ожидалось что {nameof(discount)} не будет null");

                    if (discount.ContractDiscountId.HasValue)
                    {
                        contractDiscountIds.Remove(discount.ContractDiscountId.Value);
                        if (discount.ContractDiscount == null)
                            throw new PawnshopApplicationException($"Ожидалось что {nameof(discount)}.{nameof(discount.ContractDiscount)}");

                        ContractDiscount contractDiscount = discount.ContractDiscount;
                        if (contractDiscount.ContractActionId != discount.ActionId)
                            throw new PawnshopApplicationException($"Не сходятся идентификаторы действия в сущностях {nameof(ContractDiscount)} и {nameof(Discount)}");

                        contractDiscount.ContractActionId = null;
                        contractDiscount.DeleteDate = DateTime.Now;
                        _contractDiscountRepository.Update(contractDiscount);
                    }

                    List<DiscountRow> discountRows = discount.Rows;
                    if (discountRows == null)
                        throw new PawnshopApplicationException($"Ожидалось что {nameof(discountRows)} не будет null");

                    foreach (DiscountRow discountRow in discountRows)
                    {
                        if (discountRow == null)
                            throw new PawnshopApplicationException($"Ожидалось что {nameof(discountRow)} не будет null");

                        if (!discountRow.OrderId.HasValue)
                            throw new PawnshopApplicationException($"Ожидалось что {nameof(discountRow.OrderId)} не будет null");

                        if (discountRow.Order == null)
                            throw new PawnshopApplicationException($"Ожидалось что {nameof(discountRow.Order)} не будет null, " +
                                $"id = {discountRow.OrderId.Value}");

                        if (isStorn)
                            _cashOrderService.Cancel(discountRow.Order, author.Id, branch);
                        else
                        {
                            IDictionary<int, (int, DateTime)> actionsWithDatesDict =
                                _cashOrderService.Delete(discountRow.Order.Id, author.Id, branch.Id);

                            if (actionsWithDatesDict == null)
                                throw new PawnshopApplicationException(
                                    $"Ожидалось что {nameof(_cashOrderService)}.{nameof(_cashOrderService.Delete)} не вернет null");

                            foreach ((int accountId, (int accountRecordId, DateTime accountDate)) in actionsWithDatesDict)
                            {
                                if (recalculateBalanceAccountDict.ContainsKey(accountId))
                                {
                                    (int _, DateTime date) = recalculateBalanceAccountDict[accountId];
                                    if (date < accountDate)
                                        continue;
                                }

                                recalculateBalanceAccountDict[accountId] = (accountRecordId, accountDate);
                            }
                        }
                    }
                }

                if (contractDiscountIds.Count > 0)
                    throw new PawnshopApplicationException($"{nameof(contractDiscounts)} и {nameof(discounts)} не сходятся по скидкам договора");

                foreach ((int accountId, (int accountRecordId, DateTime date)) in recalculateBalanceAccountDict)
                {
                    DateTime? recalculateDate = null;
                    AccountRecord accountRecordBeforeDate = _accountRecordRepository.GetLastRecordByAccountIdAndEndDate(accountId, accountRecordId, date);
                    if (accountRecordBeforeDate != null)
                        recalculateDate = accountRecordBeforeDate.Date;

                    ListModel<AccountRecord> records = _accountRecordService.List(new ListQueryModel<AccountRecordFilter> { Page = null, Model = new AccountRecordFilter { AccountId = accountId } });
                    _accountRecordService.RecalculateBalanceOnAccount(accountId, beginDate: recalculateDate);
                }

                _contractActionRepository.Delete(contractAction.Id);

                _crmPaymentService.Enqueue(contract);

                if (contractAction.ActionType == ContractActionType.Sign)
                {
                    InsurancePolicyFilter filter = new InsurancePolicyFilter()
                    {
                        StartDate = contract.SignDate
                    };

                    if (contractAction.ParentActionId.HasValue)
                        filter.ContractId = contract.Id;
                    else
                        filter.RootContractId = contract.Id;

                    var policies = _insurancePolicyService.List(new ListQuery(), filter).List?.OrderByDescending(t => t.CreateDate).ToList();

                    CancelPoliciesForContract(policies, contract);
                }
                _collectionService.CancelCloseCollection(contract.Id, contractAction.Id);
                transaction.Commit();
            }
        }

        public async Task CancelActions(ContractActionAutoStorno autoStornoModel)
        {
            var contract = await _contractService.GetAsync(autoStornoModel.ContractId);
            if(contract is null)
                throw new PawnshopApplicationException("Договор не найден");

            if (contract.Status != ContractStatus.Signed)
                throw new PawnshopApplicationException("Договор не подписан");

            var actionList = await _contractActionService.GetByContractIdAndDates(autoStornoModel.ContractId, autoStornoModel.StartDate, autoStornoModel.EndDate);

            var hasAnotherAction = actionList.Any(x => x.ActionType != ContractActionType.InterestAccrual &&
                                                        x.ActionType != ContractActionType.PenaltyAccrual &&
                                                        x.ActionType != ContractActionType.MoveToOverdue && 
                                                        x.ActionType != ContractActionType.Payment &&
                                                        x.ActionType != ContractActionType.PrepaymentFromTransit &&
                                                        x.ActionType != ContractActionType.PrepaymentToTransit &&
                                                        x.ActionType != ContractActionType.PenaltyLimitAccrual && 
                                                        x.ActionType != ContractActionType.ControlDateChange &&
                                                        x.ActionType != ContractActionType.MonthlyPayment &&
                                                        x.ActionType != ContractActionType.ChangeCreditLineCategory &&
                                                        x.ActionType != ContractActionType.InterestAccrualOnOverdueDebt &&
                                                        x.ActionType != ContractActionType.MoveScheduleToNextDate &&
                                                        x.ActionType != ContractActionType.PenaltyRateDecrease &&
                                                        x.ActionType != ContractActionType.PenaltyRateIncrease &&
                                                        x.ActionType != ContractActionType.InstantDiscount &&
                                                        x.ActionType != ContractActionType.Prepayment);

            if (hasAnotherAction)
                throw new PawnshopApplicationException("Среди действ по договору найден список действ которых надо отменить вручную");

            using (var transaction = _unitOfWork.BeginTransaction())
            {
                foreach (var action in actionList.OrderByDescending(x=>x.Id).ToList())
                {
                    var relatedActions = await _contractActionService.GetRelatedContractActionsByActionId(action.Id);
                    foreach (var relatedAction in relatedActions.OrderByDescending(x=>x).ToList())
                    {
                        var relatedActionObject = await _contractActionService.GetAsync(relatedAction);
                        if (relatedActionObject.ActionType == ContractActionType.Prepayment)
                            continue;

                        if (relatedActionObject != null && 
                                (relatedActionObject.ActionType == ContractActionType.PrepaymentFromTransit 
                                || relatedActionObject.ActionType == ContractActionType.PrepaymentToTransit
                                || relatedActionObject.ActionType == ContractActionType.Payment)
                            )
                        {
                            if (relatedActionObject.ChildActionId.HasValue)
                            {
                                var childAction = await _contractActionService.GetAsync(relatedActionObject.ChildActionId.Value);
                                if (childAction.ParentActionId.HasValue)
                                {
                                    childAction.ParentAction = null;
                                    childAction.ParentActionId = null;
                                    _contractActionService.Save(childAction);

                                    relatedActionObject.ChildActionId = null;
                                    relatedActionObject.ChildAction = null;
                                    _contractActionService.Save(relatedActionObject);
                                }
                            }

                            if (relatedActionObject.ParentActionId.HasValue)
                            {
                                var parentAction = await _contractActionService.GetAsync(relatedActionObject.ParentActionId.Value);
                                if (parentAction.ChildActionId.HasValue)
                                {
                                    parentAction.ChildAction = null;
                                    parentAction.ChildActionId = null;
                                    _contractActionService.Save(parentAction);

                                    relatedActionObject.ParentActionId = null;
                                    relatedActionObject.ParentAction = null;
                                    _contractActionService.Save(relatedActionObject);
                                }
                            }
                        }
                    }

                    foreach(var relatedAction in relatedActions.OrderByDescending(x => x).ToList())
                    {
                        var relatedActionObject = await _contractActionService.GetAsync(relatedAction);
                        if (relatedActionObject.ActionType == ContractActionType.Prepayment)
                            continue;

                        if (relatedActionObject.DeleteDate != null)
                            continue;

                        if (relatedActionObject.PayOperationId.HasValue)
                        {
                            var payOperation = _payOperationRepository.Get(relatedActionObject.PayOperationId.Value);
                            if (payOperation.Status == PayOperationStatus.Checked ||
                                payOperation.Status == PayOperationStatus.Executed)
                            {
                                throw new PawnshopApplicationException(
                                    $"Невозможно отменить действие Id = {relatedActionObject.Id}, платежная операция проверена или находится в обработке");
                            }
                        }

                        if (relatedActionObject.ActionType == ContractActionType.Payment)
                        {
                            RevertScheduleBeforeAction(autoStornoModel.ContractId, relatedActionObject.Id, autoStornoModel.AuthorId);
                            contract = await _contractService.GetAsync(contract.Id);
                            contract.BuyoutDate = null;
                            contract.BuyoutReasonId = null;

                            _contractService.Save(contract);
                            _contractPaymentScheduleService.Save(contract.PaymentSchedule, contract.Id, autoStornoModel.AuthorId);
                        }

                        var orders = _orderRepository.GetAllOrdersByContractActionId(relatedActionObject.Id);

                        foreach (var order in orders.Where(x => x.DeleteDate == null).OrderByDescending(x => x.Id))
                        {
                            await _cashOrderService.CancelWithoutRecalculateAsync(order, autoStornoModel.AuthorId, null);
                            relatedActionObject.Status = ContractActionStatus.Canceled;

                            _eventLog.Log(EventCode.CashOrderСanceled, EventStatus.Success, EntityType.CashOrder,
                                order.Id, null, null, userId: autoStornoModel.AuthorId);
                        }

                        if (relatedActionObject.OnlinePaymentId.HasValue)
                        {
                            if (contract.Actions.Count(x => x.OnlinePaymentId == relatedActionObject.OnlinePaymentId) == 1)
                            {
                                _onlinePaymentRepository.Delete((int)relatedActionObject.OnlinePaymentId);
                            }
                        }

                        var mintosAction = _mintosContractActionRepository.GetByContractActionId(action.Id);
                        if (mintosAction != null && mintosAction.Status == MintosUploadStatus.Success)
                        {
                            throw new PawnshopApplicationException(
                                "Данное действие отмене не подлежит, так как уже учитывается в другой системе.");
                        }

                        if (mintosAction != null)
                        {
                            mintosAction.Status = MintosUploadStatus.Canceled;
                            _mintosContractActionRepository.Update(mintosAction);
                            _mintosContractActionRepository.Delete(mintosAction.Id);
                        }
                        _contractActionService.Delete(relatedActionObject.Id);
                        _crmPaymentService.Enqueue(contract);
                        _contractActionService.Save(relatedActionObject);
                        _collectionService.CancelCloseCollection(contract.Id, relatedActionObject.Id);
                    }
                }

                transaction.Commit();
            }
        }

        private void CancelPoliciesForContract(List<InsurancePolicy> policies, Contract contract)
        {
            foreach (var policy in policies)
            {
                if (policy.StartDate.Date != DateTime.Now.Date)
                    throw new PawnshopApplicationException(
                        $"Действие можно было отменить только {policy.StartDate:dd.MM.yyyy}");

                var actualPoliceRequest = _insurancePoliceRequestService.GetApprovedPoliceRequest(contract.Id) ?? _insurancePoliceRequestService.GetErrorPoliceRequest(contract.Id);
                _insuranceService.BPMCancelPolicy(actualPoliceRequest);
            }
        }

        public void RevertScheduleBeforeAction(int contractId, int contractActionId, int authorId)
        {
            using (IDbTransaction transaction = _contractRepository.BeginTransaction())
            {
                Contract contract = _contractService.Get(contractId);
                if (contract == null)
                    throw new PawnshopApplicationException($"Договор {contractId} не найден");

                if (contract.PaymentSchedule == null)
                    throw new PawnshopApplicationException($"Отсутствует график платежей у договора {contract.ContractNumber}");

                var allowedContractStatuses = new HashSet<ContractStatus>
                {
                    ContractStatus.Signed,
                    ContractStatus.BoughtOut,
                    ContractStatus.SoldOut,
                    ContractStatus.Disposed
                };

                if (!allowedContractStatuses.Contains(contract.Status))
                    throw new PawnshopApplicationException($"Откат графика недоступен для данного договора {contract.ContractNumber}");

                ContractAction contractAction = _contractActionService.GetAsync(contractActionId).Result;
                if (contractAction == null)
                    throw new PawnshopApplicationException($"Действие {contractActionId} по договору {contract.ContractNumber} не найдено");

                List<ContractPaymentSchedule> paymentSchedule = contract.PaymentSchedule;
                foreach (ContractPaymentSchedule schedule in paymentSchedule)
                {
                    //&& contractAction.ActionType != ContractActionType.PartialPayment) -> для ЧДП не создаем запись в ревизиях графика
                    if (schedule.ActionId == contractAction.Id && contractAction.ActionType != ContractActionType.PartialPayment)
                    {
                        schedule.ActionId = null;
                        schedule.ActualDate = null;
                    }
                }

                List<ContractPaymentSchedule> unpayedSchedules = paymentSchedule.Where(s => !s.ActionId.HasValue
                    && !s.Canceled.HasValue).ToList();

                if (unpayedSchedules.Count == 0)
                    throw new PawnshopApplicationException(
                        $"Не найдены пункты графика для отката для действия {contractActionId} " +
                        $"по договору {contract.ContractNumber}");

                DateTime minUnpayedScheduleDate = unpayedSchedules.Min(s => s.Date);
                contract.NextPaymentDate = minUnpayedScheduleDate;
                _contractRepository.Update(contract);
                _contractPaymentScheduleService.Save(contract.PaymentSchedule, contract.Id, authorId);
                transaction.Commit();
            }
        }

        private void PayExtraExpenses(ContractAction action, Group branch, int authorId, bool forcePrepaymentReturn)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (branch == null)
                throw new ArgumentNullException(nameof(branch));

            if (action.ExtraExpensesIds != null)
            {
                bool canPayCertainExtraExpenses = _contractActionOperationPermisisonService.CanPayCertainExtraExpenses(action.ActionType);
                if (!canPayCertainExtraExpenses)
                    throw new PawnshopApplicationException($"Тип действия {action.ActionType} не может совершать оплату отдельных доп расходов");
            }
            else
            {
                bool canPayExtraExpenses = _contractActionOperationPermisisonService.CanPayExtraExpenses(action.ActionType);
                if (!canPayExtraExpenses)
                {
                    if (action.ExtraExpensesCost > 0)
                        throw new PawnshopApplicationException($"Тип действия {action.ActionType} не может совершать оплату доп расходов\nсумма доп расходов должна быть равна 0");

                    return;
                }
            }

            List<ContractExpense> contractExtraExpenses = _contractExpenseOperationService.GetIncomingExtraExpensesByContractId(action.ContractId);
            if (contractExtraExpenses == null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(_contractExpenseOperationService)}.{nameof(_contractExpenseOperationService.GetIncomingExtraExpensesByContractId)} не вернет null");

            var contract = _contractService.GetOnlyContract(action.ContractId);
            if (contract.ContractClass == ContractClass.Tranche)
            {
                contractExtraExpenses.AddRange(_contractExpenseOperationService.GetIncomingExtraExpensesByContractId(contract.CreditLineId.Value));
            }

            if (action.ExtraExpensesIds == null)
            {
                decimal calculatedExtraExpensesCost = 0;

                if (contract.ContractClass == ContractClass.Credit)
                {
                    calculatedExtraExpensesCost = _contractService.GetExtraExpensesCost(action.ContractId, action.Date);
                }
                else if (contract.ContractClass == ContractClass.Tranche)
                {
                    calculatedExtraExpensesCost = _contractService.GetExtraExpensesCost(contract.CreditLineId.Value, action.Date);
                }
                else if (contract.ContractClass == ContractClass.CreditLine)
                {
                    var activeTranches = _contractService.GetAllSignedTranches(contract.Id).Result;

                    if (!activeTranches.Any())
                        calculatedExtraExpensesCost = _contractService.GetExtraExpensesCost(contract.Id, action.Date);
                }

                decimal extraExpensesCost = action.ExtraExpensesCost ?? 0;
                if (extraExpensesCost != calculatedExtraExpensesCost)
                    throw new PawnshopApplicationException($"Присланная сумма доп. расходов не сходится с реальной суммой доп. расходов в базе\nДолжно быть:  {calculatedExtraExpensesCost} тенге");
            }
            else
            {
                HashSet<int> extraExpensesIdsSet = action.ExtraExpensesIds.ToHashSet();
                if (!extraExpensesIdsSet.IsSubsetOf(contractExtraExpenses.Select(e => e.Id)))
                    throw new PawnshopApplicationException("Список доп расходов не сходится со списком доп расходов базы");

                contractExtraExpenses = contractExtraExpenses.Where(e => extraExpensesIdsSet.Contains(e.Id)).ToList();
            }

            IEnumerable<int> contractExtraExpensesIds = contractExtraExpenses.Select(e => e.Id);
            List<ContractExpense> contractExpenseWithDepoInDebitInPayment = _contractExpenseOperationService.GetNeededExpensesForPrepayment(action.ContractId, branch.Id, contractExtraExpensesIds);
            var processedExpenses = new HashSet<int>();
            foreach (ContractExpense contractExpense in contractExpenseWithDepoInDebitInPayment)
            {
                if (contractExpense == null)
                    throw new PawnshopApplicationException($"Ожидалось что '{nameof(contractExpense)}' не будет null");

                _contractExpenseOperationService.PayExtraExpensesAsync(
                    contractExpense.Id,
                    authorId, branch.Id,
                    actionId: action.Id,
                    forcePrepaymentReturn: forcePrepaymentReturn,
                    prepaymentContractId: contract.ContractClass == ContractClass.Tranche ? contract.Id : (int?)null)
                    .Wait();
                processedExpenses.Add(contractExpense.Id);
            }

            foreach (ContractExpense contractExpense in contractExtraExpenses)
            {
                if (contractExpense == null)
                    throw new PawnshopApplicationException($"Ожидалось что '{nameof(contractExpense)}' не будет null");

                if (processedExpenses.Contains(contractExpense.Id))
                    continue;

                _contractExpenseOperationService.PayExtraExpensesAsync(
                    contractExpense.Id,
                    authorId,
                    branch.Id,
                    actionId: action.Id,
                    forcePrepaymentReturn: forcePrepaymentReturn,
                    prepaymentContractId: contract.ContractClass == ContractClass.Tranche ? contract.Id : (int?)null)
                    .Wait();
            }
        }

        private void RegisterExpense(ContractAction action, Group branch, int authorId, bool forcePrepaymentReturn)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (branch == null)
                throw new ArgumentNullException(nameof(branch));

            if (action.Expense == null)
                return;

            bool canPayEncumbrance = _contractActionOperationPermisisonService.CanPayEncumbrance(action.ActionType);
            if (!canPayEncumbrance)
                throw new PawnshopApplicationException($"Тип действия {action.ActionType} не может совершать оплату основных расходов");

            ContractExpense contractExpense = action.Expense;
            Expense expenseType = _expenseService.Get(contractExpense.ExpenseId);
            if (expenseType == null)
                throw new PawnshopApplicationException($"Тип расхода {contractExpense.ExpenseId} не найден");

            if (expenseType.ExtraExpense)
                throw new PawnshopApplicationException("Расход действия должен быть основным");

            contractExpense.ContractId = action.ContractId;
            contractExpense.AuthorId = action.AuthorId;
            _contractExpenseOperationService.RegisterAsync(contractExpense, authorId, branch.Id, actionId: action.Id, forcePrepaymentReturn).Wait();
            if (action.Expense.Id != 0)
            {
                action.ExpenseId = action.Expense.Id;
                _contractActionService.Save(action);
            }
        }


        private void RegisterContractActionRows(ContractAction action, IContract contract, Group branch, bool callActionRowBusinessOperation, int authorId,
                                                 OrderStatus? orderStatus = null)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (action.Rows == null)
                throw new ArgumentException($"Поле {nameof(action.Rows)} не должен быть null", nameof(action));

            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            if (branch == null)
                throw new ArgumentNullException(nameof(branch));

            var amountTypeDict = new Dictionary<AmountType, decimal>();
            foreach (var row in action.Rows)
            {
                decimal amount;
                if (amountTypeDict.TryGetValue(row.PaymentType, out amount))
                {
                    if (amount != row.Cost)
                        throw new PawnshopApplicationException("Суммы проводок по типам платежей не сходятся");
                }

                amountTypeDict[row.PaymentType] = row.Cost;
            }

            if (callActionRowBusinessOperation)
            {
                PayOperation payOperation = null;
                bool canMakePayOperations = _contractActionOperationPermisisonService.CanMakePayOperations(action);
                if (canMakePayOperations)
                    payOperation = RegisterPayOperation(action, contract, branch, authorId);

                if (payOperation != null && payOperation.PayType == null)
                    throw new PawnshopApplicationException(
                        $"{nameof(payOperation)}.{nameof(payOperation.PayTypeId)} {payOperation.PayTypeId} не должен быть null");

                action.PayOperationId = payOperation?.Id;
                _contractActionService.Save(action);
                List<(CashOrder, List<AccountRecord>)> orders = _businessOperationService.Register(
                    contract,
                    action.Date.Date == action.CreateDate.Date ? action.CreateDate : action.Date,
                    _businessOperationService.GetOperationCode(
                        action.ActionType,
                        contract.CollateralType,
                        action.EmployeeId.HasValue,
                        action.IsReceivable.HasValue && action.IsReceivable.Value,
                        action?.IsInitialFee ?? false
                        ),
                    branch,
                    authorId,
                    amountTypeDict,
                    action.PayTypeId,
                    payOperation: payOperation,
                    action: action,
                    orderStatus: payOperation != null ? OrderStatus.WaitingForApprove : orderStatus);

                var ordersSet = new HashSet<CashOrder>();
                foreach (var order in orders)
                    ordersSet.Add(order.Item1);

                foreach (var row in action.Rows)
                {
                    List<CashOrder> foundOrders = ordersSet.Where(x => x.CreditAccountId == row.CreditAccountId
                        && x.DebitAccountId == row.DebitAccountId
                        && x.BusinessOperationSetting.AmountType == row.PaymentType).ToList();

                    if (foundOrders.Count == 0)
                        throw new PawnshopApplicationException($"Ошибка, не найден кассовый ордер для {row.PaymentType}");

                    if (foundOrders.Count > 1)
                        throw new PawnshopApplicationException($"Количество кассовых ордеров для {row.PaymentType} больше одного");

                    CashOrder order = foundOrders.Single();
                    ordersSet.Remove(order);
                    row.CreditAccountId = order.CreditAccountId;
                    row.DebitAccountId = order.DebitAccountId;
                    row.OrderId = order.Id;
                }
            }

            foreach (var row in action.Rows.Where(t => t.Cost > 0))
            {
                row.ActionId = action.Id;
                _contractActionRowRepository.Insert(row);
            }
        }

        private void RegisterDiscounts(ContractAction action, IContract contract, Group branch, User author)
        {

            if (action == null)
                throw new ArgumentNullException(nameof(action.Discount));

            if (action.Discount == null)
                return;

            if (branch == null)
                throw new ArgumentNullException(nameof(branch));

            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            if (author == null)
                throw new ArgumentNullException(nameof(author));

            ContractDutyDiscount dutyDiscount = action.Discount;
            if (dutyDiscount.Discounts == null)
                return;

            List<Discount> discounts = dutyDiscount.Discounts;
            foreach (Discount discount in discounts)
            {
                discount.ActionId = action.Id;
                _discountRepository.Insert(discount);
                var dictAmountDiscountRows = new Dictionary<AmountType, decimal>();
                foreach (DiscountRow discountRow in discount.Rows)
                {
                    decimal amount;
                    if (dictAmountDiscountRows.TryGetValue(discountRow.PaymentType, out amount))
                    {
                        if (discountRow.SubtractedCost != amount)
                            throw new PawnshopApplicationException("Стоимости проводок по типам сумм не сходится");
                    }

                    dictAmountDiscountRows[discountRow.PaymentType] = discountRow.SubtractedCost;
                }

                string discountOperationCode = Constants.BO_DISCOUNT;
                // 286 - Айдын Сапашев, 482 - Заманбек Канижан
                // !!! ЭТО ВРЕМЕННОЕ РЕШЕНИЕ, УБРАТЬ ПОСЛЕ КОРРЕКТИРОВКИ !!!
                var correctionUserIdSet = new HashSet<int> { 276, 482 };
                if (correctionUserIdSet.Contains(author.Id))
                    discountOperationCode = Constants.BO_DISCOUNT_CORRECTION;

                List<(CashOrder, List<AccountRecord>)> result = _businessOperationService.Register(contract, action.Date,
                    discountOperationCode, branch, author.Id, dictAmountDiscountRows, orderUserId: author.Id);

                var ordersSet = new HashSet<CashOrder>();
                foreach (var order in result)
                {
                    ordersSet.Add(order.Item1);

                    if (!order.Item1.ContractActionId.HasValue)
                    {
                        order.Item1.ContractActionId = action.Id;
                        _orderRepository.Update(order.Item1);
                    }
                }

                foreach (DiscountRow discountRow in discount.Rows)
                {
                    discountRow.DiscountId = discount.Id;
                    List<CashOrder> foundOrders = ordersSet.Where(x => x.BusinessOperationSetting.AmountType == discountRow.PaymentType).ToList();
                    if (foundOrders.Count == 0)
                        throw new PawnshopApplicationException($"Ошибка, не найден кассовый ордер для {discountRow.PaymentType}");

                    if (foundOrders.Count > 1)
                        throw new PawnshopApplicationException($"Количество кассовых ордеров для {discountRow.PaymentType} больше одного");

                    CashOrder order = foundOrders.Single();
                    ordersSet.Remove(order);
                    discountRow.OrderId = order.Id;
                    _discountRowRepository.Insert(discountRow);
                }

                if (discount.ContractDiscountId.HasValue)
                {
                    int contractDiscountId = discount.ContractDiscountId.Value;
                    ContractDiscount contractDiscount = _contractDiscountRepository.Get(contractDiscountId);
                    if (contractDiscount == null)
                        throw new PawnshopApplicationException($"Скидка договора {contractDiscountId} не найдена");

                    contractDiscount.ContractActionId = action.Id;
                    _contractDiscountRepository.Update(contractDiscount);
                }
            }
        }

        private void RegisterFileRows(ContractAction contractAction)
        {
            if (contractAction == null)
                throw new ArgumentNullException(nameof(contractAction));

            if (contractAction.Files == null)
                return;

            List<FileRow> files = contractAction.Files;
            foreach (FileRow fileRow in files)
            {
                var contractFilerow = new ContractFileRow
                {
                    ContractId = contractAction.ContractId,
                    ActionId = contractAction.Id,
                    FileRowId = fileRow.Id
                };

                _contractFileRowRepository.Insert(contractFilerow);
            }
        }

        private void RegisterContractActionCheckValues(ContractAction contractAction)
        {
            if (contractAction == null)
                throw new ArgumentNullException(nameof(contractAction));

            if (contractAction.Checks == null)
                return;

            List<ContractActionCheckValue> contractActionCheckValues = contractAction.Checks;
            foreach (ContractActionCheckValue contractActionCheckValue in contractActionCheckValues)
            {
                contractActionCheckValue.ActionId = contractAction.Id;
                _contractActionCheckValueRepository.Insert(contractActionCheckValue);
            }
        }

        private PayOperation RegisterPayOperation(ContractAction contractAction, IContract contract, Group branch, int authorId)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            if (branch == null)
                throw new ArgumentNullException(nameof(branch));

            string numberCode = branch.Configuration?.ContractSettings?.NumberCode;
            if (numberCode == null)
                throw new ArgumentException("Поле Configuration.ContractSettings.NumberCode не должно быть null", nameof(branch));

            if (contractAction == null)
                throw new ArgumentNullException(nameof(contractAction));

            if (string.IsNullOrWhiteSpace(numberCode))
                throw new ArgumentException("Поле не должно быть пустым или содержать одни пробелы", nameof(numberCode));

            if (!contractAction.PayTypeId.HasValue)
                return null;

            if (contractAction.RequisiteCost <= 0)
                return null;

            PayType payType = _payTypeRepository.Get(contractAction.PayTypeId.Value);
            if (payType == null)
                throw new PawnshopApplicationException("Тип оплаты не выбран");

            if (!contractAction.IsFromOnlinePaymentJob)
                if (!payType.Rules.Any(x => x.ActionType == contractAction.ActionType))
                    throw new PawnshopApplicationException("Выбранный вид оплаты не возможен по данному действию");

            if (!payType.AccountantUploadRequired)
                return null;

            var contractIsOnline = _contractRepository.IsOnline(contract.Id);

            if (!contractIsOnline && (contractAction.Files == null || contractAction.Files.Count == 0))
                throw new PawnshopApplicationException(
                    "Не найдены обязательные файлы для создания платежной операции/поручения");

            Contract contractFromDB = _contractService.Get(contract.Id);
            if (contractFromDB == null)
                throw new PawnshopApplicationException($"Не найден договор {contract.Id}");

            Client client = _clientRepository.Get(contractFromDB.ClientId);
            if (client == null)
                throw new PawnshopApplicationException($"Клиент {contract.ClientId} не найден");

            if (payType.RequisiteIsRequired && contractFromDB.ProductTypeId.HasValue &&
                contractFromDB.ProductType.Code == "BUYCAR")
            {

                var merchant = contractFromDB.Subjects?.FirstOrDefault(x =>
                    x.Subject.Code == "MERCHANT" && x.Client.Requisites.Any(x => x.Id == contractAction.RequisiteId));
                if (merchant == null)
                    throw new PawnshopApplicationException(
                        $"Не найден субъект кредитования \"Мерчант\"(MERCHANT) с выбранным номером счета");

                client = merchant.Client;
            }

            ClientRequisite requisite = null;

            if (payType.RequisiteIsRequired)
            {
                if (!contractAction.RequisiteId.HasValue)
                    throw new PawnshopApplicationException("Не выбран обязательный реквизит");

                if (client.Requisites.Where(x => x.Id == contractAction.RequisiteId.Value).FirstOrDefault()
                    .RequisiteTypeId != payType.RequiredRequisiteTypeId.Value)
                {
                    throw new PawnshopApplicationException(
                        "Выбранный реквизит не подходит для выбранного вида оплаты!");
                }
                else
                {
                    requisite = client.Requisites.Where(x => x.Id == contractAction.RequisiteId.Value).FirstOrDefault();
                }

                if (requisite == null)
                    throw new PawnshopApplicationException(
                        $"Выбранный реквизит не найден в списке доступных реквизитов клиента {client.FullName} ({client.IdentityNumber})");
            }

            //удаление старых операций
            var canceledOperations = _payOperationRepository.List(new ListQuery() { Page = null },
                new { Status = PayOperationStatus.Canceled, ContractId = contract.Id });
            var returnedOperations = _payOperationRepository.List(new ListQuery() { Page = null },
                new { Status = PayOperationStatus.ReturnToRepair, ContractId = contract.Id });
            var operationsToDelete = new List<PayOperation>();
            operationsToDelete.AddRange(canceledOperations);
            operationsToDelete.AddRange(returnedOperations);
            if (operationsToDelete.Count > 0)
                operationsToDelete.ForEach(o => { _payOperationRepository.Delete(o.Id); });

            var operation = new PayOperation()
            {
                Date = contractAction.Date,
                PayTypeId = contractAction.PayTypeId.Value,
                Name = contractAction.Reason,
                Note = contractAction.Note,
                ClientId = client.Id,
                ContractId = contractAction.ContractId,
                ActionId = contractAction.Id,
                BranchId = branch.Id,
                AuthorId = authorId,
                CreateDate = DateTime.Now,
                Status = PayOperationStatus.AwaitForCheck,
                Number = _payOperationNumberCounterRepository.Next(contractAction.PayTypeId.Value,
                        contractAction.Date.Year, branch.Id, payType.OperationCode, numberCode),
                PayType = payType
            };

            if (payType.RequisiteIsRequired)
            {
                operation.RequisiteTypeId = requisite.RequisiteTypeId;
                operation.RequisiteId = requisite.Id;
            }

            operation.Files = contractAction.Files;
            _payOperationRepository.Insert(operation);
            contractAction.PayOperationId = operation.Id;
            if (contractAction.ActionType == ContractActionType.Sign)
            {
                contract.Status = ContractStatus.AwaitForMoneySend;
                contractFromDB.Status = ContractStatus.AwaitForMoneySend;
                _contractService.Save(contractFromDB);
            }

            return operation;
        }


        private void ValidateContractAction(ContractAction contractAction)
        {
            if (contractAction == null)
                throw new ArgumentNullException(nameof(contractAction));

            if (contractAction.ProcessingType.HasValue && !contractAction.ProcessingId.HasValue)
                throw new ArgumentException($"При заполненном {nameof(contractAction.ProcessingType)} этот свойство должно быть заполнено", nameof(contractAction.ProcessingId));

            if (!contractAction.ProcessingType.HasValue && contractAction.ProcessingId.HasValue)
                throw new ArgumentException($"При заполненном {nameof(contractAction.ProcessingId)} этот свойство должно быть заполнено", nameof(contractAction.ProcessingType));

            if (contractAction.Rows == null)
                throw new ArgumentException($"Поле {nameof(contractAction.Rows)} не должен быть null", nameof(contractAction));

            bool doNotValidateActionRowsAndDiscounts =
                _contractActionOperationPermisisonService.DoNotValidateActionsRowsAndDiscounts(contractAction.ActionType);
            if (doNotValidateActionRowsAndDiscounts)
                return;

            var contractAcitonRows = contractAction.Rows != null ? contractAction.Rows.ToList() : new List<ContractActionRow>();
            decimal extraExpensesCost = contractAction.ExtraExpensesCost ?? 0;
            decimal cost = extraExpensesCost + contractAction.TotalCost;
            ContractDutyCheckModel checkModel = new ContractDutyCheckModel
            {
                ActionType = contractAction.ActionType,
                ContractId = contractAction.ContractId,
                Cost = cost,
                Date = contractAction.Date,
                PayTypeId = contractAction.PayTypeId
            };
            ContractDuty contractDuty = _contractDutyService.GetContractDuty(checkModel);
            if (contractDuty.ExtraExpensesCost != extraExpensesCost)
                throw new PawnshopApplicationException("Суммы присланной суммы оплачиваемых доп расходов не сходятся с вычисленной суммой доп расходов");

            bool doNotValidateContractActionRowsIntegrity =
                _contractActionOperationPermisisonService.DoNotValidateActionsRowsIntegrity(contractAction.ActionType);

            if (!doNotValidateContractActionRowsIntegrity)
                ValidateContractActionRows(contractAcitonRows, contractDuty.Rows);

            if ((contractDuty.Discount == null && contractAction.Discount != null) ||
                (contractAction.Discount == null && contractDuty.Discount != null))
                throw new PawnshopApplicationException("Присланные скидки не совпадает со высчитанной скидкой");

            if (contractDuty.Discount != null)
            {
                List<Discount> dutyDiscounts = contractDuty.Discount.Discounts;
                List<Discount> contractActionDiscounts = contractAction.Discount.Discounts;
                if (dutyDiscounts.Count != contractActionDiscounts.Count)
                    throw new PawnshopApplicationException("Присланные скидки не совпадает со высчитанной скидкой");

                for (int i = 0; i < dutyDiscounts.Count; i++)
                {
                    Discount dutyDiscount = dutyDiscounts[i];
                    Discount contractActionDiscount = contractActionDiscounts[i];
                    ValidateContractDiscount(dutyDiscount, contractActionDiscount);
                }
            }
        }

        private void ValidateContractDiscount(Discount dutyDiscount, Discount contractActionDiscount)
        {
            if (dutyDiscount == null)
                throw new ArgumentNullException(nameof(dutyDiscount));

            if (contractActionDiscount == null)
                throw new ArgumentNullException(nameof(contractActionDiscount));

            List<DiscountRow> dutyDiscountRows = dutyDiscount.Rows;
            List<DiscountRow> contractActionDiscountRows = contractActionDiscount.Rows;
            var dutyDiscountRowsSet = dutyDiscountRows.ToHashSet();
            var contractActionDiscountRowsSet = contractActionDiscountRows.ToHashSet();
            while (dutyDiscountRowsSet.Count > 0 && contractActionDiscountRowsSet.Count > 0)
            {
                if (dutyDiscountRowsSet.Count == 0 || contractActionDiscountRowsSet.Count == 0)
                    throw new PawnshopApplicationException("Присланные проводки по скидкам сходятся с расчитанными проводками по скидкам");

                DiscountRow dutyDiscountRowToDelete = null;
                DiscountRow contractActionDiscountRowToDelete = null;
                foreach (DiscountRow contractActionDiscountRow in contractActionDiscountRowsSet)
                {
                    foreach (DiscountRow dutyDiscountRow in contractActionDiscountRowsSet)
                    {
                        bool isEqual = dutyDiscountRow.AddedCost == contractActionDiscountRow.AddedCost
                            && dutyDiscountRow.AddedDays == contractActionDiscountRow.AddedDays
                            && dutyDiscountRow.DiscountId == contractActionDiscountRow.DiscountId
                            && dutyDiscountRow.Id == contractActionDiscountRow.Id
                            && dutyDiscountRow.OrderId == contractActionDiscountRow.OrderId
                            && dutyDiscountRow.OriginalCost == contractActionDiscountRow.OriginalCost
                            && dutyDiscountRow.OriginalDays == contractActionDiscountRow.OriginalDays
                            && dutyDiscountRow.PaymentType == contractActionDiscountRow.PaymentType
                            && dutyDiscountRow.PercentAdjustment == contractActionDiscountRow.PercentAdjustment
                            && dutyDiscountRow.SubtractedCost == contractActionDiscountRow.SubtractedCost
                            && dutyDiscountRow.SubtractedDays == contractActionDiscountRow.SubtractedDays;

                        if (isEqual)
                        {
                            dutyDiscountRowToDelete = dutyDiscountRow;
                            contractActionDiscountRowToDelete = contractActionDiscountRow;
                            break;
                        }
                    }

                    if (dutyDiscountRowToDelete != null)
                        break;
                }

                if (contractActionDiscountRowToDelete == null && dutyDiscountRowToDelete == null)
                    throw new PawnshopApplicationException("Присланные проводки по скидкам не сходятся с расчитанными проводками по скидкам");

                contractActionDiscountRowsSet.Remove(contractActionDiscountRowToDelete);
                dutyDiscountRowsSet.Remove(dutyDiscountRowToDelete);
            }
        }

        private void ValidateContractActionRows(List<ContractActionRow> contractActionRows, List<ContractActionRow> dutyRows)
        {
            if (contractActionRows == null)
                throw new ArgumentNullException(nameof(contractActionRows));

            if (dutyRows == null)
                throw new ArgumentNullException(nameof(dutyRows));

            var contractActionRowsSet = contractActionRows.Where(r => r.Cost > 0).ToHashSet();
            var dutyRowsSet = dutyRows.Where(r => r.Cost > 0).ToHashSet();
            while (dutyRowsSet.Count > 0 && contractActionRowsSet.Count > 0)
            {
                if (dutyRowsSet.Count == 0 || contractActionRowsSet.Count == 0)
                    throw new PawnshopApplicationException("Присланные проводки не сходятся с расчитанными проводками");

                ContractActionRow dutyRowToDelete = null;
                ContractActionRow contractActionRowToDelete = null;
                foreach (ContractActionRow contractActionRow in contractActionRowsSet)
                {
                    foreach (ContractActionRow dutyRow in dutyRowsSet)
                    {
                        bool isEqual = dutyRow.ActionId == contractActionRow.ActionId
                            && dutyRow.BusinessOperationSettingId == contractActionRow.BusinessOperationSettingId
                            && dutyRow.Cost == contractActionRow.Cost
                            && dutyRow.CreditAccountId == contractActionRow.CreditAccountId
                            && dutyRow.DebitAccountId == contractActionRow.DebitAccountId
                            && dutyRow.Id == contractActionRow.Id
                            && dutyRow.LoanSubjectId == contractActionRow.LoanSubjectId
                            && dutyRow.OrderId == contractActionRow.OrderId
                            && dutyRow.OriginalPercent == contractActionRow.OriginalPercent
                            && dutyRow.PaymentType == contractActionRow.PaymentType
                            && dutyRow.Percent == contractActionRow.Percent
                            && dutyRow.Period == contractActionRow.Period;

                        if (isEqual)
                        {
                            dutyRowToDelete = dutyRow;
                            contractActionRowToDelete = contractActionRow;
                            break;
                        }
                    }

                    if (contractActionRowToDelete != null)
                        break;
                }

                if (contractActionRowToDelete == null && dutyRowToDelete == null)
                    throw new PawnshopApplicationException("Присланные проводки не сходятся с расчитанными проводками");

                contractActionRowsSet.Remove(contractActionRowToDelete);
                dutyRowsSet.Remove(dutyRowToDelete);
            }
        }

        private void HandleCancelForActionTypes(ContractAction contractAction, ref Contract contract, Data.Models.Insurances.Insurance insurance, Category category, bool isSupport, int authorId)
        {
            if (contractAction == null)
                throw new ArgumentNullException(nameof(contractAction));

            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            switch (contractAction.ActionType)
            {
                case ContractActionType.Prolong:
                    var lastProlong = contract.Actions
                        .Where(a => a.ActionType == ContractActionType.Prolong && a.Id != contractAction.Id)
                        .OrderBy(a => a.Date)
                        .LastOrDefault();
                    if (lastProlong != null)
                    {
                        contract.ProlongDate = lastProlong.Date;
                        contract.MaturityDate = lastProlong.Date.AddDays(lastProlong.Data.ProlongPeriod);
                    }
                    else
                    {
                        contract.ProlongDate = null;
                        contract.MaturityDate = contract.OriginalMaturityDate;
                    }

                    if (contract.PaymentSchedule.Count > 1)
                        contract.PaymentSchedule.RemoveAll(x => x.ActionId == null && x.Date > contractAction.Date);

                    foreach (var schedule in contract.PaymentSchedule.Where(x => x.ActionId == contractAction.Id))
                    {
                        if (!schedule.Prolongated.HasValue)
                            throw new PawnshopApplicationException("Ошибка не заполнена дата пролонгации в графике!");

                        ContractPaymentScheduleRevision previousRevision = null;
                        if (schedule.Revision <= 0)
                            throw new PawnshopApplicationException($"Ревизия графика {schedule.Id} не может быть отрицательным числом или нулю");

                        if (schedule.Revision > 1)
                        {
                            int previousRevisionNumber = schedule.Revision - 1;
                            previousRevision = _contractPaymentScheduleService.GetRevision(schedule.Id, previousRevisionNumber);
                            if (previousRevision == null)
                                throw new PawnshopApplicationException($"Ревизия графика {schedule.Id}(номер ревизии {schedule.Revision})");
                        }

                        if (previousRevision != null)
                        {
                            schedule.ContractId = previousRevision.ContractId;
                            schedule.Date = previousRevision.Date;
                            schedule.ActualDate = previousRevision.ActualDate;
                            schedule.DebtLeft = previousRevision.DebtLeft;
                            schedule.DebtCost = previousRevision.DebtCost;
                            schedule.PercentCost = previousRevision.PercentCost;
                            schedule.PenaltyCost = previousRevision.PenaltyCost;
                            schedule.CreateDate = previousRevision.CreateDate;
                            schedule.DeleteDate = previousRevision.DeleteDate;
                            schedule.ActionId = previousRevision.ActionId;
                            schedule.Canceled = previousRevision.Canceled;
                            schedule.Period = previousRevision.Period;
                            schedule.Prolongated = previousRevision.Prolongated;
                        }
                        else
                        {
                            schedule.Date = schedule.Prolongated.Value;
                            schedule.PenaltyCost = schedule.ActionId = null;
                            schedule.ActualDate = schedule.Prolongated = null;
                            schedule.DebtCost = Math.Round(contract.LoanCost, 2);
                            schedule.DebtLeft = 0;
                            schedule.PercentCost = Math.Round(contract.LoanPercentCost * contract.LoanPeriod, 2);
                            schedule.Period = contract.LoanPeriod;
                        }
                    }

                    contract.NextPaymentDate = contract.MaturityDate;
                    break;
                case ContractActionType.Buyout:
                case ContractActionType.BuyoutRestructuringCred:
                case ContractActionType.WithdrawByAuction:
                case ContractActionType.PartialBuyout:
                case ContractActionType.PartialPayment:
                case ContractActionType.Addition:
                case ContractActionType.Refinance:
                case ContractActionType.CreditLineClose:
                    if (contract.ContractClass != ContractClass.CreditLine)
                    {
                        foreach (var position in contract.Positions)
                        {
                            position.Status = ContractPositionStatus.Active;
                        }

                        if (insurance != null)
                        {
                            if (insurance.Status != InsuranceStatus.Closed)
                                throw new InvalidOperationException();
                            insurance.Status = InsuranceStatus.Signed;
                            _insuranceRepository.Update(insurance);
                        }

                        List<ContractPaymentSchedule> schedules = contract.PaymentSchedule;
                        foreach (ContractPaymentSchedule schedule in schedules)
                        {
                            schedule.Canceled = null;
                        }

                        _contractPaymentScheduleService.Save(schedules, contract.Id, authorId);
                        RevertScheduleBeforeAction(contract.Id, contractAction.Id, authorId);
                    }
                    contract = _contractService.Get(contract.Id);
                    contract.Status = ContractStatus.Signed;
                    contract.BuyoutDate = null;
                    contract.BuyoutReasonId = null;
                    break;
                case ContractActionType.Sign:
                    //if (!isSupport)
                    //    throw new PawnshopApplicationException("Данное действие отмене не подлежит.");

                    _insurancePoliceRequestService.DeleteInsurancePoliceRequestsByContractId(contract.Id);

                    contract.Status = ContractStatus.Draft;
                    contract.SignDate = null;
                    contract.NextPaymentDate = null;
                    if (insurance != null)
                    {
                        if (insurance.Status != InsuranceStatus.Signed)
                            throw new InvalidOperationException();
                        insurance.Status = InsuranceStatus.Draft;
                        _insuranceRepository.Update(insurance);

                        var insuranceAction =
                            insurance.Actions.FirstOrDefault(a => a.ActionType == InsuranceActionType.Sign);
                        if (insuranceAction != null)
                        {
                            if (!insurance.PrevInsuranceId.HasValue)
                            {
                                _orderRepository.Delete(insuranceAction.OrderId);
                                _eventLog.Log(EventCode.CashOrderDeleted, EventStatus.Success, EntityType.CashOrder,
                                    insuranceAction.OrderId, null, null);
                            }

                            _insuranceActionRepository.Delete(insuranceAction.Id);
                        }
                    }

                    if (category != null && (contract.CollateralType == CollateralType.Car ||
                                             contract.CollateralType == CollateralType.Machinery))
                    {
                        if (contract.CollateralType == CollateralType.Car)
                        {
                            var position = contract.Positions.FirstOrDefault().Position as Car;
                            position.ParkingStatusId = (int)category.DefaultParkingStatusId;

                            _carRepository.Update(position);
                        }

                        if (contract.CollateralType == CollateralType.Machinery)
                        {
                            var position = contract.Positions.FirstOrDefault().Position as Machinery;
                            position.ParkingStatusId = (int)category.DefaultParkingStatusId;

                            _machineryRepository.Update(position);
                        }
                    }

                    _applicationService.CancelApplicationByContractId(contract.Id);

                    break;
                case ContractActionType.Selling:
                    if (contractAction.SellingId <= 0)
                        throw new ArgumentNullException($"Не заполнен SellingId для действия {contractAction.Id}");
                    var _selling = _sellingRepository.Get((int)contractAction.SellingId);

                    foreach (var row in _selling.SellingRows)
                    {
                        _sellingRepository.DeleteRow(row.Id);
                    }

                    _selling.SellingCost = null;
                    _selling.SellingDate = null;
                    _selling.CashOrderId = null;
                    _selling.Status = SellingStatus.InStock;
                    _selling.AuthorId = authorId;

                    _sellingRepository.Update(_selling);

                    if (_selling.ContractId != contract.Id)
                        throw new PawnshopApplicationException($@"Id контракта {contract.Id} не совпадает с Id контракта из Реализации {_selling.ContractId}");

                    var _position = contract.Positions.SingleOrDefault(p => p.Id == _selling.ContractPositionId.Value);

                    if (_selling.ContractId > 0 && _selling.ContractPositionId.HasValue)
                    {
                        if (_position != null)
                        {
                            _position.Status = ContractPositionStatus.SoldOut;
                        }
                        if (contract.Positions.All(p => p.Status == ContractPositionStatus.SoldOut))
                        {
                            contract.Status = ContractStatus.SoldOut;
                        }
                    }

                    break;
                case ContractActionType.PrepareSelling:
                    foreach (var position in contract.Positions)
                    {
                        position.Status = ContractPositionStatus.Active;

                        var selling = _sellingRepository.Find(new { ContractPositionId = position.Id });

                        if (selling.Status == SellingStatus.Sold)
                        {
                            var error =
                                $@"Данное действие отмене не подлежит - залог был реализован {selling.SellingDate.Value.ToString("dd.MM.yyyy HH:mm")}";
                            throw new PawnshopApplicationException(error);
                        }

                        if (selling != null)
                        {
                            _sellingRepository.Delete(selling.Id);
                            _eventLog.Log(EventCode.SellingDeleted, EventStatus.Success, EntityType.Selling,
                                selling.Id, null, null);
                        }
                    }
                    contract.Status = ContractStatus.Signed;

                    break;
                case ContractActionType.Transfer:
                    throw new PawnshopApplicationException($@"Данное действие отмене не подлежит");
                //contract.TransferDate = null;
                //contract.PoolNumber = null;
                //var contractTransferDate = _contractTransferRepository.Get(action.ContractId);
                //contractTransferDate.TransferDate = null;
                //_contractTransferRepository.Update(contractTransferDate);
                case ContractActionType.MonthlyPayment:
                    if (contract.Status == ContractStatus.BoughtOut)
                    {
                        foreach (var position in contract.Positions)
                        {
                            position.Status = ContractPositionStatus.Active;
                        }

                        if (insurance != null)
                        {
                            if (insurance.Status != InsuranceStatus.Closed)
                                throw new InvalidOperationException();
                            insurance.Status = InsuranceStatus.Signed;
                            _insuranceRepository.Update(insurance);
                        }
                    }

                    RevertScheduleBeforeAction(contract.Id, contractAction.Id, authorId);
                    contract = _contractService.Get(contract.Id);
                    contract.Status = ContractStatus.Signed;
                    contract.BuyoutDate = null;
                    contract.BuyoutReasonId = null;
                    break;
                case ContractActionType.Prepayment:
                    if (contractAction.ProcessingId.HasValue && !isSupport)
                        throw new PawnshopApplicationException("Данное действие отмене не подлежит.");

                    if (contractAction.IsInitialFee == true)
                    {
                        decimal depoMerchantBalance = _contractService.GetDepoMerchantBalance(contract.Id, contractAction.Date);
                        if (contract.RequiredInitialFee <= depoMerchantBalance)
                            contract.Status = ContractStatus.AwaitForInitialFee;
                    }

                    break;
                case ContractActionType.PrepaymentReturn:
                    if (contract.Status == ContractStatus.Canceled)
                        contract.Status = ContractStatus.Draft;

                    break;
                case ContractActionType.Payment:
                    if (contract.ContractClass != ContractClass.CreditLine)
                    {
                        RevertScheduleBeforeAction(contract.Id, contractAction.Id, authorId);
                    }
                    contract = _contractService.Get(contract.Id);
                    contract.Status = ContractStatus.Signed;
                    contract.BuyoutDate = null;
                    contract.BuyoutReasonId = null;
                    break;

                case ContractActionType.InterestAccrualOnOverdueDebt:
                    if (contract.PercentPaymentType != PercentPaymentType.EndPeriod)
                        throw new PawnshopApplicationException("Нельзя отменить данное действие так как договор не является дискретом");

                    if (contract.Status == ContractStatus.BoughtOut)
                        throw new PawnshopApplicationException("Нельзя отменить данное дейстие так как договор уже выкуплен");

                    ContractPaymentSchedule currentPayment = contract.PaymentSchedule.Where(
                        x => !x.ActualDate.HasValue
                        && !x.ActionId.HasValue
                        && !x.Canceled.HasValue)
                    .OrderBy(x => x.Date).FirstOrDefault();

                    if (currentPayment == null)
                        throw new PawnshopApplicationException("График текущего платежа не найден(был оплачен или удален)");

                    if (currentPayment.Date >= contractAction.Date)
                        throw new PawnshopApplicationException("Нельзя отменить это действие так как график был оплачен");

                    currentPayment.PercentCost -= contractAction.TotalCost;
                    if (currentPayment.PercentCost <= 0)
                        throw new PawnshopApplicationException("Сумма вознаграждения в текушем графике не может меньше нуля");

                    break;

                case ContractActionType.InterestAccrual:
                case ContractActionType.MoveToOverdue:
                case ContractActionType.PenaltyAccrual:
                case ContractActionType.PrepaymentToTransit:
                case ContractActionType.PrepaymentFromTransit:
                case ContractActionType.PenaltyLimitAccrual:
                case ContractActionType.PenaltyLimitWriteOff:
                case ContractActionType.ChangeCreditLineCategory:
                case ContractActionType.BuyoutRestructuringTranches:
                    break;
                case ContractActionType.PenaltyRateDecrease:
                case ContractActionType.PenaltyRateIncrease:
                    _contractRateService.DeleteContractRateForCancelAction(contractAction.Id);
                    contract.ContractRates.RemoveAll(x => x.ActionId == contractAction.Id);
                    break;
                case ContractActionType.ControlDateChange:

                    ContractAdditionalInfo contractAdditionalInfo = null;
                    if (contract.ContractClass != ContractClass.CreditLine)
                    {
                        contractAdditionalInfo = _сontractAdditionalInfoRepository.Get(contract.Id);
                        if (contractAdditionalInfo != null)
                        {
                            contractAdditionalInfo.DateOfChangeControlDate = null;
                            contractAdditionalInfo.ChangedControlDate = null;
                            _сontractAdditionalInfoRepository.Update(contractAdditionalInfo);
                        }
                    }

                    var creditline = contract.CreditLineId;
                    if ((creditline.HasValue && contract.ContractClass == ContractClass.Tranche) || contract.ContractClass == ContractClass.CreditLine)
                    {
                        var list = new List<int>();
                        if (contract.ContractClass == ContractClass.CreditLine)
                        {
                            list = _contractRepository.GetTrancheIdsByCreditLine(contract.Id).Result;
                        }
                        else
                        {
                            list = _contractRepository.GetTrancheIdsByCreditLine(contract.CreditLineId.Value).Result;
                        }
                        foreach (var id in list)
                        {
                            var tranche = _contractRepository.Get(id);
                            if (tranche.DeleteDate != null)
                            {
                                continue;
                            }
                            List<ContractPaymentSchedule> schedules = tranche.PaymentSchedule;
                            foreach (ContractPaymentSchedule schedule in schedules)
                            {
                                schedule.Revision++;
                                schedule.DeleteDate = DateTime.Now;
                                _contractPaymentScheduleRepository.Update(schedule);
                            }
                            var actions = _contractActionRepository.GetByContractId(id).Result;
                            var action = actions.LastOrDefault(x => x.ActionType == ContractActionType.ControlDateChange && x.Date == contractAction.Date && x.Status != ContractActionStatus.Canceled);
                            if (action != null)
                            {
                                schedules = _contractPaymentScheduleService.GetScheduleByActionForChangeDate(action.Id).Result;
                                foreach (var schedule in schedules)
                                {
                                    var item = _contractPaymentScheduleRepository.GetWithDeleted(schedule.Id);
                                    if (item != null)
                                    {
                                        item.ContractId = id;
                                        item.Revision = 1;
                                        item.DeleteDate = null;
                                        _contractPaymentScheduleRepository.Update(item);
                                    }
                                }
                                _contractPaymentScheduleService.UpdateContractPaymentScheduleHistoryStatus(action.ContractId, action.Id, 20);
                                _contractPaymentScheduleService.DeleteContractPaymentScheduleHistory(id, action.Id);
                                action.Status = ContractActionStatus.Canceled;
                                _contractActionRepository.Update(action);
                                _contractActionRepository.Delete(action.Id);
                                var trancheAdditionalInfo = _сontractAdditionalInfoRepository.Get(id);
                                if (trancheAdditionalInfo != null)
                                {
                                    trancheAdditionalInfo.DateOfChangeControlDate = null;
                                    trancheAdditionalInfo.ChangedControlDate = null;
                                    _сontractAdditionalInfoRepository.Update(trancheAdditionalInfo);
                                }
                            }

                            _eventLog.Log(EventCode.CancelContractControlDateChanged, EventStatus.Success, EntityType.Contract, id, userId: authorId, branchId: tranche.BranchId);
                        }

                        if (contract.ContractClass == ContractClass.Tranche && contract.CreditLineId.HasValue)
                        {
                            var creditLineId = contract.CreditLineId.Value;
                            var creditLine = _contractRepository.Get(creditLineId);
                            var actions = _contractActionRepository.GetByContractId(creditLineId).Result;
                            var action = actions.LastOrDefault(x => x.ActionType == ContractActionType.ControlDateChange && x.Date == contractAction.Date && x.Status != ContractActionStatus.Canceled);
                            if (action != null)
                            {
                                action.Status = ContractActionStatus.Canceled;
                                _contractActionRepository.Update(action);
                                _contractActionRepository.Delete(action.Id);
                            }
                            _eventLog.Log(EventCode.CancelContractControlDateChanged, EventStatus.Success, EntityType.Contract, creditLineId, userId: authorId, branchId: creditLine.BranchId);
                        }

                        if (contract.ContractClass == ContractClass.CreditLine)
                        {
                            _eventLog.Log(EventCode.CancelContractControlDateChanged, EventStatus.Success, EntityType.Contract, contract.Id, userId: authorId, branchId: contract.BranchId);
                        }
                    }
                    else if (contract.ContractClass == ContractClass.Credit)
                    {
                        List<ContractPaymentSchedule> schedules = contract.PaymentSchedule;
                        foreach (ContractPaymentSchedule schedule in schedules)
                        {
                            schedule.Revision++;
                            schedule.DeleteDate = DateTime.Now;
                            _contractPaymentScheduleRepository.Update(schedule);
                        }

                        schedules = _contractPaymentScheduleService.GetScheduleByActionForChangeDate(contractAction.Id).Result;

                        foreach (var schedule in schedules)
                        {
                            var item = _contractPaymentScheduleRepository.GetWithDeleted(schedule.Id);
                            if (item != null)
                            {
                                item.ContractId = contract.Id;
                                item.Revision = 1;
                                item.DeleteDate = null;
                                _contractPaymentScheduleRepository.Update(item);
                            }
                        }

                        _contractPaymentScheduleService.DeleteContractPaymentScheduleHistory(contract.Id, contractAction.Id);
                        _eventLog.Log(EventCode.CancelContractControlDateChanged, EventStatus.Success, EntityType.Contract, contract.Id, userId: authorId, branchId: contract.BranchId);
                    }
                    contract = _contractService.Get(contract.Id);
                    contract.Status = ContractStatus.Signed;
                    contract.BuyoutDate = null;
                    contract.BuyoutReasonId = null;
                    break;
                case ContractActionType.MoveScheduleToNextDate:

                    DateTime scheduleCheckDate = contractAction.Date.Date;

                    if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
                    {
                        scheduleCheckDate = scheduleCheckDate.AddDays(1);
                    }

                    int paidScheduleCount = contract.PaymentSchedule.Where(p => p.NextWorkingDate.HasValue && p.Date == scheduleCheckDate && p.ActualDate.HasValue).Count();
                    if (paidScheduleCount > 0)
                    {
                        throw new PawnshopApplicationException("Невозможно отменить перенос КД для уже оплаченного пункта графика");
                    }

                    foreach (ContractPaymentSchedule schedule in contract.PaymentSchedule.Where(p => p.NextWorkingDate.HasValue && p.Date == scheduleCheckDate))
                    {
                        schedule.NextWorkingDate = null;
                        if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
                        {
                            schedule.PercentCost -= contract.LoanPercentCost;
                            schedule.Date = schedule.Date.AddDays(-1);
                            contract.NextPaymentDate = contract.NextPaymentDate?.AddDays(-1);
                            contract.MaturityDate = contract.MaturityDate.AddDays(-1);
                        }
                    }
                    break;
                case ContractActionType.RestructuringTranches:
                case ContractActionType.RestructuringCred:
                    #region откат графика

                    var clientDeferment = _clientDefermentService.GetActiveDeferment(contract.Id);
                    var oldSchedule = _contractPaymentScheduleService.GetScheduleByActionForChangeDate(contractAction.Id).Result;

                    if (oldSchedule == null || oldSchedule.Count == 0)
                        throw new PawnshopApplicationException("Невозможно отменить реструктуризацию, не найден график до изменений");

                    foreach (ContractPaymentSchedule schedule in contract.PaymentSchedule)
                    {
                        schedule.Revision++;
                        schedule.DeleteDate = DateTime.Now;
                        _contractPaymentScheduleRepository.Update(schedule);
                    }

                    foreach (ContractPaymentSchedule schedule in oldSchedule)
                    {
                        var item = _contractPaymentScheduleRepository.GetWithDeleted(schedule.Id);
                        if (item != null)
                        {
                            item.ContractId = contract.Id;
                            item.Revision = 1;
                            item.DeleteDate = null;
                            _contractPaymentScheduleRepository.Update(item);
                        }
                    }

                    _contractPaymentScheduleService.UpdateContractPaymentScheduleHistoryStatus(contractAction.ContractId, contractAction.Id, 20);
                    _contractPaymentScheduleService.DeleteContractPaymentScheduleHistory(contractAction.ContractId, contractAction.Id);

                    contractAction.Status = ContractActionStatus.Canceled;
                    _contractActionRepository.Update(contractAction);
                    _contractActionRepository.Delete(contractAction.Id);

                    _clientDefermentService.CancelClientDeferment(clientDeferment);

                    #endregion

                    #region откат MaturityDate и NextPaymentDate

                    contract.MaturityDate = contractAction.Data.MaturityDate.HasValue ? contractAction.Data.MaturityDate.Value : contract.MaturityDate;
                    contract.NextPaymentDate = contractAction.Data.NextPaymentDate.HasValue ? contractAction.Data.NextPaymentDate.Value : contract.NextPaymentDate;
                    _contractService.Save(contract);

                    if (contract.ContractClass == ContractClass.Tranche)
                    {
                        var creditLine = _contractService.GetOnlyContract(contract.CreditLineId.Value);
                        creditLine.MaturityDate = contractAction.Data.CreditLineMaturityDate.HasValue ? contractAction.Data.CreditLineMaturityDate.Value : creditLine.MaturityDate;
                        _contractRepository.UpdateMaturityDate(creditLine);
                    }

                    #endregion
                    break;
                case ContractActionType.RestructuringTransferToAccountCred:
                case ContractActionType.RestructuringTransferToAccountTranches:
                    _accountService.DeleteAccount(contractAction.Data.OpenedAccountId.Value).Wait();
                    break;
                case ContractActionType.RestructuringTransferToTransitCred:
                case ContractActionType.RestructuringTransferToTransitTranches:
                    _accountService.UndoCloseAccount(contractAction.Data.ClosedAccountId.Value).Wait();
                    _contractService.UpdatePeriodType(Constants.SHORT_TERM_PERIOD_TYPE_ID, contract.Id).Wait();
                    _contractService.Save(contract);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _contractService.Save(contract);
            if (contractAction.ActionType != ContractActionType.ControlDateChange && contractAction.ActionType != ContractActionType.RestructuringCred && contractAction.ActionType != ContractActionType.RestructuringTranches)
            {
                _contractPaymentScheduleService.Save(contract.PaymentSchedule, contract.Id, authorId);
            }
        }
    }
}
