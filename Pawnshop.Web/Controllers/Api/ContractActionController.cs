using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Data.Models.InnerNotifications;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Mintos;
using Pawnshop.Web.Models.Contract.Refinance;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Contracts;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Services.Crm;
using Pawnshop.Services.Expenses;
using System.Data;
using Pawnshop.Services.Calculation;
using Pawnshop.Services.Discounts;
using Pawnshop.Data.Access.AccountingCore;
using Pawnshop.Services.Insurance;
using Pawnshop.Services.PenaltyLimit;
using Pawnshop.Services.Models.Contracts;
using Pawnshop.Services.Applications;
using Pawnshop.Services.Integrations.UKassa;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Services.Models.Contracts.Kdn;
using Pawnshop.Data.Models.Parking;
using Pawnshop.Services.Cars;
using Pawnshop.Services.Contracts.PartialPayment;
using Pawnshop.Services.Positions;
using Pawnshop.Services.Models.Calculation;
using Pawnshop.Data.Models.Collection;
using Pawnshop.Services.Collection;
using Pawnshop.Web.Models.Inscription;
using Pawnshop.Services.Clients;
using Pawnshop.Services.Collection.http;
using MediatR;
using Pawnshop.Data.Models;
using Pawnshop.Data.Models.Auction.HttpRequestModels;
using Pawnshop.Data.Models.MobileApp.HardCollection.Commands;
using Pawnshop.Services.PaymentSchedules;
using Pawnshop.Services.LegalCollection.Inerfaces;
using Pawnshop.Services.ApplicationsOnline;
using Pawnshop.Services.ApplicationOnlineRefinances;
using Pawnshop.Services.Auction.HttpServices.Interfaces;
using Pawnshop.Services.Auction.Interfaces;
using Pawnshop.Services.Refinance;
using Pawnshop.Services.Models.Clients;
using Pawnshop.Data.Models.MobileApp;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.ContractManage)]
    public class ContractActionController : Controller
    {
        private readonly ISessionContext _sessionContext;
        private readonly BranchContext _branchContext;
        private readonly ContractRepository _contractRepository;
        private readonly IContractActionSellingService _contractSellingService;
        private readonly EventLog _eventLog;
        private readonly InnerNotificationRepository _innerNotificationRepository;
        private readonly ContractExpenseRepository _contractExpenseRepository;
        private readonly MintosContractRepository _mintosContractRepository;
        private readonly MintosContractActionRepository _mintosContractActionRepository;
        private readonly CarRepository _carRepository;
        private readonly CashOrderRepository _cashOrderRepository;
        private readonly IContractActionRowBuilder _rowBuilder;
        private readonly ContractExpenseRowRepository _contractExpenseRowRepository;
        private readonly ContractExpenseRowOrderRepository _contractExpenseRowOrderRepository;
        private readonly AccountRecordRepository _accountRecordRepository;
        private readonly ContractActionRepository _contractActionRepository;
        private readonly LoanPercentRepository _loanPercentRepository;
        private readonly ICashOrderService _cashOrderService;
        private readonly IContractActionService _contractActionService;
        private readonly ICrmPaymentService _crmPaymentService;
        private readonly IContractActionCheckService _contractActionCheckService;
        private readonly IExpenseService _expenseService;
        private readonly IContractExpenseService _contractExpenseService;
        private readonly IContractExpenseOperationService _contractExpenseOperationService;
        private readonly IContractActionOperationService _contractActionOperationService;
        private readonly IInscriptionService _inscriptionService;
        private readonly IContractService _contractService;
        private readonly IContractRateService _contractRateService;
        private readonly IContractPaymentService _contractPaymentService;
        private readonly IContractActionPrepaymentService _contractActionPrepaymentService;
        private readonly IContractActionProlongService _contractActionProlongService;
        private readonly IContractActionSignService _contractActionSignService;
        private readonly IContractActionBuyoutService _contractActionBuyoutService;
        private readonly IContractActionAdditionService _contractActionAdditionService;
        private readonly IAccountRecordService _accountRecordService;
        private readonly IContractActionPartialPaymentService _contractActionPartialPaymentService;
        private readonly IContractPaymentScheduleService _contractPaymentScheduleService;
        private readonly IParkingActionService _parkingActionService;
        private readonly IContractVerificationService _contractVerificationService;
        private readonly IContractStatusHistoryService _contractStatusHistoryService;
        private readonly ICollectionService _collectionService;

        private readonly IAccountService _accountService;
        private readonly IInterestAccrual _interestAccrualService;
        private readonly IPenaltyAccrual _penaltyAccrualService;
        private readonly ITakeAwayToDelay _takeAwayToDelayService;
        private readonly IInsurancePoliceRequestService _insurancePoliceRequestService;
        private readonly IPenaltyLimitAccrualService _penaltyLimitAccrualService;
        private readonly IPenaltyRateService _penaltyRateService;
        private readonly IContractCloseService _contractCloseService;
        private readonly IApplicationService _applicationService;
        private readonly IUKassaService _uKassaService;
        private readonly UserRepository _userRepository;
        private readonly IPositionEstimateHistoryService _positionEstimateHistoryService;

        private readonly IContractActionRowBuilder _contractActionRowBuilder;

        private readonly IContractKdnService _contractKdnService;
        private readonly IContractDutyService _contractDutyService;
        private readonly IPositionSubjectService _positionSubjectService;

        private readonly IInscriptionOffBalanceAdditionService _inscriptionOffBalanceAdditionService;
        private readonly ApplicationRepository _applicationRepository;
        private readonly IRequestMobileAppService _requestMobileAppService;
        private readonly IPaymentScheduleService _paymentScheduleService;
        private readonly ILegalCollectionCloseService _closeLegalCaseService;
        private readonly IClientIncomeService _clientIncomeService;
        private readonly IClientAdditionalIncomeService _clientAdditionalIncomeService;
        private readonly ILegalCollectionNotificationService _legalCollectionNotificationService;
        private readonly IClientExpenseService _clientExpenseService;

        private readonly FunctionSettingRepository _functionSettingRepository;
        private readonly IAuctionOperationHttpService _auctionOperationHttpService;
        private readonly IContractBuyOutByAuctionService _contractBuyOutByAuctionService;
        private readonly ICarAuctionService _auctionService;
        private readonly IDeleteExpenseService _deleteExpenseService;

        /// <summary>
        /// Максимальное значение APR/ГЭСВ
        /// </summary>
        private decimal _maxAPR = 56;

        private string _notEnoughPermissionError = "У вас недостаточно прав для проведения действий другим числом. Обратитесь к администратору.";
        private string _badDateError = "Дата не может быть меньше даты последнего действия по договору";

        public ContractActionController(
            ISessionContext sessionContext, BranchContext branchContext,
            ContractRepository contractRepository, IContractActionService contractActionService,
            IContractActionSellingService contractSellingService,
            EventLog eventLog,
            InnerNotificationRepository innerNotificationRepository,
            ContractExpenseRepository contractExpenseRepository,
            MintosContractRepository mintosContractRepository,
            MintosContractActionRepository mintosContractActionRepository, CarRepository carRepository,
            CashOrderRepository cashOrderRepository, IContractActionRowBuilder rowBuilder,
            ContractExpenseRowRepository contractExpenseRowRepository,
            ContractActionRepository contractActionRepository,
            LoanPercentRepository loanPercentRepository,
            CategoryRepository categoryRepository,
            ICashOrderService cashOrderService,
            ICrmPaymentService crmPaymentService, IContractActionCheckService contractActionCheckService,
            IExpenseService expenseService,
            IContractExpenseService contractExpenseService, IContractExpenseOperationService contractExpenseOperationService,
            IContractActionOperationService contractActionOperationService, IContractService contractService,
            IContractRateService contractRateService,
            ContractExpenseRowOrderRepository contractExpenseRowOrderRepository,
            IInscriptionService inscriptionService,
            IContractActionPrepaymentService contractActionPrepaymentService,
            IContractPaymentService contractPaymentService,
            IParkingActionService parkingActionService,
            IContractActionProlongService contractActionProlongService,
            IContractActionSignService contractActionSignService,
            IContractActionAdditionService contractActionAdditionService,
            IContractActionBuyoutService contractActionBuyoutService,
            IAccountRecordService accountRecordService, AccountRecordRepository accountRecordRepository,
            IAccountService accountService,
            IContractActionPartialPaymentService contractActionPartialPaymentService,
            IInterestAccrual interestAccrualService, IPenaltyAccrual penaltyAccrualService,
            ITakeAwayToDelay takeAwayToDelayService,
            IInsurancePoliceRequestService insurancePoliceRequestService,
            IPenaltyLimitAccrualService penaltyLimitAccrualService,
            IPenaltyRateService penaltyRateService,
            IContractCloseService contractCloseService,
            IApplicationService applicationService,
            IUKassaService uKassaService,
            UserRepository userRepository,
            IContractActionRowBuilder contractActionRowBuilder,
            IContractPaymentScheduleService contractPaymentScheduleService,
            IContractKdnService contractKdnService,
            IContractScheduleBuilder contractScheduleBuilder,
            IPositionEstimateHistoryService positionEstimateHistoryService,
            IContractDutyService contractDutyService,
            IContractVerificationService contractVerificationService,
            IPositionSubjectService positionSubjectService,
            IContractStatusHistoryService contractStatusHistoryService,
            ICollectionService collectionService,
            IInscriptionOffBalanceAdditionService inscriptionOffBalanceAdditionService,
            ApplicationRepository applicationRepository,
            IRequestMobileAppService requestMobileAppService,
            IPaymentScheduleService paymentScheduleService,
            ILegalCollectionCloseService closeLegalCaseService,
            IClientIncomeService clientIncomeService,
            IClientAdditionalIncomeService clientAdditionalIncomeService,
            ILegalCollectionNotificationService legalCollectionNotificationService,
            IClientExpenseService clientExpenseService,
            FunctionSettingRepository functionSettingRepository,
            IAuctionOperationHttpService auctionOperationHttpService,
            IContractBuyOutByAuctionService contractBuyOutByAuctionService,
            ICarAuctionService auctionService,
            IDeleteExpenseService deleteExpenseService)
        {
            _sessionContext = sessionContext;
            _branchContext = branchContext;
            _contractRepository = contractRepository;
            _contractSellingService = contractSellingService;
            _eventLog = eventLog;
            _innerNotificationRepository = innerNotificationRepository;
            _contractExpenseRepository = contractExpenseRepository;
            _expenseService = expenseService;
            _mintosContractRepository = mintosContractRepository;
            _mintosContractActionRepository = mintosContractActionRepository;
            _carRepository = carRepository;
            _cashOrderRepository = cashOrderRepository;
            _contractActionRepository = contractActionRepository;
            _parkingActionService = parkingActionService;
            _loanPercentRepository = loanPercentRepository;
            _rowBuilder = rowBuilder;
            _contractDutyService = contractDutyService;
            _contractExpenseRowRepository = contractExpenseRowRepository;
            _contractExpenseRowOrderRepository = contractExpenseRowOrderRepository;
            _cashOrderService = cashOrderService;
            _contractActionService = contractActionService;
            _crmPaymentService = crmPaymentService;
            _contractActionCheckService = contractActionCheckService;
            _contractExpenseService = contractExpenseService;
            _contractExpenseOperationService = contractExpenseOperationService;
            _contractActionOperationService = contractActionOperationService;
            _contractExpenseService = contractExpenseService;
            _contractService = contractService;
            _contractRateService = contractRateService;
            _inscriptionService = inscriptionService;
            _contractActionPrepaymentService = contractActionPrepaymentService;
            _contractPaymentService = contractPaymentService;
            _contractActionProlongService = contractActionProlongService;
            _contractActionSignService = contractActionSignService;
            _contractActionBuyoutService = contractActionBuyoutService;
            _contractActionAdditionService = contractActionAdditionService;
            _accountRecordService = accountRecordService;
            _accountRecordRepository = accountRecordRepository;
            _accountService = accountService;
            _contractActionPartialPaymentService = contractActionPartialPaymentService;
            _interestAccrualService = interestAccrualService;
            _penaltyAccrualService = penaltyAccrualService;
            _takeAwayToDelayService = takeAwayToDelayService;
            _insurancePoliceRequestService = insurancePoliceRequestService;
            _penaltyLimitAccrualService = penaltyLimitAccrualService;
            _penaltyRateService = penaltyRateService;
            _contractCloseService = contractCloseService;
            _applicationService = applicationService;
            _uKassaService = uKassaService;
            _userRepository = userRepository;
            _contractActionRowBuilder = contractActionRowBuilder;
            _contractPaymentScheduleService = contractPaymentScheduleService;
            _contractKdnService = contractKdnService;
            _positionEstimateHistoryService = positionEstimateHistoryService;
            _contractVerificationService = contractVerificationService;
            _positionSubjectService = positionSubjectService;
            _contractStatusHistoryService = contractStatusHistoryService;
            _collectionService = collectionService;
            _inscriptionOffBalanceAdditionService = inscriptionOffBalanceAdditionService;
            _applicationRepository = applicationRepository;
            _requestMobileAppService = requestMobileAppService;
            _paymentScheduleService = paymentScheduleService;
            _closeLegalCaseService = closeLegalCaseService;
            _clientIncomeService = clientIncomeService;
            _clientAdditionalIncomeService = clientAdditionalIncomeService;
            _legalCollectionNotificationService = legalCollectionNotificationService;
            _clientExpenseService = clientExpenseService;
            _functionSettingRepository = functionSettingRepository;
            _auctionOperationHttpService = auctionOperationHttpService;
            _contractBuyOutByAuctionService = contractBuyOutByAuctionService;
            _auctionService = auctionService;
            _deleteExpenseService = deleteExpenseService;
        }

        //Отменить уже совершенное и заапрувленное действие
        [HttpPost("/api/contractAction/delete/{contractActionId:int}")]
        [Event(EventCode.ContractActionCancel, EventMode = EventMode.All, EntityType = EntityType.Contract, IncludeFails = true)]
        public async Task<IActionResult> Delete([FromRoute] int contractActionId)
        {
            int authorId = _sessionContext.UserId;
            int branchId = _branchContext.Branch.Id;
            await _contractActionOperationService.Cancel(contractActionId, authorId, branchId, false, false);
            return Ok();
        }

        //Подтвердить отмену (то что было вызвано на строке 198-Delete или 254-Reverse)
        [HttpPost("/api/contractAction/ApproveDelete/{contractActionId:int}")]
        [Event(EventCode.ContractActionCancel, EventMode = EventMode.All, EntityType = EntityType.Contract, IncludeFails = true)]
        public async Task<IActionResult> ApproveDelete([FromRoute] int contractActionId)
        {
            int authorId = _sessionContext.UserId;
            int branchId = _branchContext.Branch.Id;
            var relatedActions = await _contractActionService.GetRelatedContractActionsByActionId(contractActionId);
            var contractActionIds = string.Join(", ", relatedActions);
            var maxDate = _cashOrderRepository.GetLastOrderDate(contractActionIds);
            using var transaction = _cashOrderService.BeginCashOrderTransaction();
            {
                var contractAction = await _contractActionService.GetAsync(contractActionId);
                var contract = await _contractService.GetAsync(contractAction.ContractId);
                await _contractActionOperationService.Cancel(contractActionId, authorId, branchId, false, true);
                await _cashOrderService.DeleteCashOrderPrintLanguageForContractActions(relatedActions);
                await _parkingActionService.CancelParkingHistory(contract);
                transaction.Commit();
            }
            var orderIds = await _cashOrderService.GetAllRelatedOrdersByContractActionId(contractActionId);
            _uKassaService.FinishRequests(orderIds);

            var orders = new List<int>();
            orders.AddRange(_cashOrderRepository.GetNewOrderIdsForFiscalCheck(contractActionIds, maxDate));

            return Ok(orders);
        }


        //Отклонить отмену (то что было вызвано на строке 198-Delete или 254-Reverse)
        [HttpPost("/api/contractAction/NotApproveDelete/{contractActionId:int}")]
        [Event(EventCode.ContractActionCancel, EventMode = EventMode.All, EntityType = EntityType.Contract, IncludeFails = true)]
        public async Task<IActionResult> NotApproveDelete([FromRoute] int contractActionId)
        {
            int authorId = _sessionContext.UserId;
            int branchId = _branchContext.Branch.Id;
            _contractActionOperationService.UndoCancel(contractActionId, authorId, branchId);
            return Ok();
        }

        [HttpPost("/api/contractAction/CancelDelete/{contractActionId:int}")]
        [Event(EventCode.ContractActionCancel, EventMode = EventMode.All, EntityType = EntityType.Contract, IncludeFails = true)]
        public async Task<IActionResult> CancelDelete([FromRoute] int contractActionId)
        {
            int authorId = _sessionContext.UserId;
            int branchId = _branchContext.Branch.Id;
            _contractActionOperationService.CancelDelete(contractActionId, authorId, branchId);
            return Ok();
        }

        //Сторнирование
        [HttpPost("/api/contractAction/reverse/{contractActionId:int}")]
        [Event(EventCode.ContractActionCancel, EventMode = EventMode.All, EntityType = EntityType.Contract, IncludeFails = true)]
        public async Task<IActionResult> Reverse([FromRoute] int contractActionId)
        {
            int authorId = _sessionContext.UserId;
            int branchId = _branchContext.Branch.Id;
            await _contractActionOperationService.Cancel(contractActionId, authorId, branchId, true, false);
            return Ok();
        }

        [HttpPost("/api/contractAction/expenseCancel/{contractExpenseId:int}")]
        [Event(EventCode.ContractExpenseCancel, EventMode = EventMode.All, EntityType = EntityType.Contract,
            IncludeFails = true)]
        public async Task<IActionResult> ExpenseReverse([FromRoute] int contractExpenseId)
        {
            if (_sessionContext.ForSupport)
            {
                int authorId = _sessionContext.UserId;
                int branchId = _branchContext.Branch.Id;
                await _contractExpenseOperationService.CancelAsync(contractExpenseId, authorId, branchId);
            }
            return Ok();
        }

        [HttpPost("/api/contractAction/expenseRemove/{contractExpenseId:int}")]
        [Event(EventCode.ContractExpenseCancel, EventMode = EventMode.All, EntityType = EntityType.Contract, IncludeFails = true)]
        public async Task<IActionResult> ExpenseDelete([FromRoute] int contractExpenseId)
        {
            int branchId = _branchContext.Branch.Id;
            await _deleteExpenseService.DeleteExpenseWithRecalculation(contractExpenseId, branchId);

            return Ok();
        }


        //Отклонить незаапрувленное действие
        [HttpPost("/api/contractAction/notapprove")]
        [Event(EventCode.ContractActionApprove, EventMode = EventMode.All, EntityType = EntityType.Contract,
            IncludeFails = true)]
        public async Task NotApprove([FromBody] int actionId)
        {
            var action = _contractActionService.GetAsync(actionId).Result;
            if (!action.Status.HasValue || action.Status == ContractActionStatus.Approved)
                throw new PawnshopApplicationException("Действие не требует подтверждения или уже подтверждено");

            using var transaction = _cashOrderService.BeginCashOrderTransaction();
            {
                await _contractActionOperationService.Cancel(actionId, _sessionContext.UserId, _branchContext.Branch.Id, false, true);
                transaction.Commit();
            }
            var orderIds = await _cashOrderService.GetAllRelatedOrdersByContractActionId(actionId);
            _uKassaService.FinishRequests(orderIds);
        }

        [HttpPost("/api/contractAction/cancelActionList")]
        [Event(EventCode.ContractActionListCancel, EventMode = EventMode.All, EntityType = EntityType.ContractActionAutoStorno,
            IncludeFails = true)]
        public async Task CancelActionList([FromBody] ContractActionAutoStorno storno)
        {
            storno.AuthorId = _sessionContext.UserId;
            await _contractActionOperationService.CancelActions(storno);
        }


        //Подвтердить незаапрувленное действия
        [HttpPost("/api/contractAction/approve")]
        [Event(EventCode.ContractActionApprove, EventMode = EventMode.All, EntityType = EntityType.Contract,
            IncludeFails = true)]
        public async Task Approve(
            [FromBody] ContractActionApproveInputStructure approveStructure,
            [FromServices] IMediator mediator,
            [FromServices] IApplicationOnlineService applicationOnlineService,
            [FromServices] IApplicationOnlineRefinancesService applicationOnlineRefinancesService,
            [FromServices] IRefinanceBuyOutService refinanceBuyOutService)
        {
            ContractAction action = await _contractActionService.GetAsync(approveStructure.ActionId);
            if (!action.Status.HasValue || action.Status == ContractActionStatus.Approved)
                throw new PawnshopApplicationException("Действие не требует подтверждения или уже подтверждено");

            var orders = await _cashOrderService.CheckOrdersForConfirmation(approveStructure.ActionId);
            if (orders.Item1)
                throw new PawnshopApplicationException("Подтверждение будет доступно после согласования через кассовые ордера");
            
            var contractIdsForCloseLegalCase = new List<int>();
            Guid? auctionRequestId = null;
            using var transaction = _cashOrderService.BeginCashOrderTransaction();
            {
                var relatedActions = orders.Item2;

                if (action.ActionType == ContractActionType.Buyout || action.ActionType == ContractActionType.BuyoutRestructuringCred)
                {
                    var contract = _contractService.Get(action.ContractId);
                    auctionRequestId = await SetOrderRequestId(auctionRequestId, contract);
                    bool auctionExists = auctionRequestId != null;
                    //если это выкуп и есть исполнительная надпись, то вначале проводим все действия по ней, иначе выходит ошибка о нулевом балансе на авансовом счете
                    if (contract.InscriptionId != null && contract.Inscription.Status == InscriptionStatus.Executed && !auctionExists)
                    {
                        _inscriptionService.WriteOffOnBuyout(contract, action);
                        _contractActionRowBuilder.Init(contract, action.CreateDate);
                        foreach (var actionRow in action.Rows)
                            actionRow.Cost = _contractActionRowBuilder.CalculateAmountByAmountType(action.Date, actionRow.PaymentType);
                        
                        _inscriptionService.RestoreOnBalanceOnBuyout(contract, action.CreateDate);
                    }
                }

                var signAction = await _contractActionService.GetSignAction(relatedActions);

                if (signAction != null && signAction.Status == ContractActionStatus.Await)
                {
                    var refinanceResult = await applicationOnlineRefinancesService.RefinanceAllAssociatedContracts(signAction.ContractId);

                    if (!refinanceResult)
                    {
                        transaction.Rollback();

                        throw new PawnshopApplicationException("Ошибка рефинансирования займов: Не удалось рефинансировать займы!");
                    }
                }

                await _cashOrderService.ChangeLanguageForOrders(relatedActions, approveStructure.LanguageId);
                await _cashOrderService.ChangeStatusForOrders(relatedActions, OrderStatus.Approved, _sessionContext.UserId, _branchContext.Branch, _sessionContext.ForSupport);

                foreach (var contractActionId in relatedActions.OrderBy(x => x))
                {
                    var relatedAction = await _contractActionService.GetAsync(contractActionId);
                    if (relatedAction != null && relatedAction.Status.HasValue &&
                        relatedAction.Status != ContractActionStatus.Await)
                        continue;

                    relatedAction.Status = ContractActionStatus.Approved;
                    _contractActionService.Save(relatedAction);

                    switch (relatedAction.ActionType)
                    {
                        case ContractActionType.Selling:
                        {
                            var close = new CollectionClose()
                            {
                                ContractId = relatedAction.ContractId,
                                ActionId = relatedAction.Id,
                            };
                            contractIdsForCloseLegalCase.Add(close.ContractId);
                            _collectionService.CloseContractCollection(close);

                            break;
                        }
                        case ContractActionType.Prolong:
                        {
                            _contractActionProlongService.ExecOnApprove(relatedAction, _sessionContext.UserId);

                            var close = new CollectionClose()
                            {
                                ContractId = relatedAction.ContractId,
                                ActionId = relatedAction.Id,
                            };
                            contractIdsForCloseLegalCase.Add(close.ContractId);
                            var isClosed = _collectionService.CloseContractCollection(close);
                            if (isClosed)
                                await mediator.Send(new SendClosedContractCommand()
                                    { ContractId = relatedAction.ContractId });
                            else
                                await mediator.Send(new SendContractOnlyCommand()
                                    { ContractId = relatedAction.ContractId });

                            break;
                        }
                        case ContractActionType.Sign:
                        {
                            if (!_contractService.IsKDNPassedForOffline(action.ContractId))
                            {
                                throw new PawnshopApplicationException("КДН не пройден!");
                            }

                            var contract = _contractService.Get(relatedAction.ContractId);
                            if (contract != null && contract.Status != ContractStatus.AwaitForOrderApprove)
                                throw new PawnshopApplicationException($"Договор {contract.ContractNumber} не находится в состоянии 'Ожидает подтверждения кассового ордера' для подтверждения проводок");

                            await applicationOnlineService.CheckEncumbranceRegisteredForCashIssue(contract.Id);

                            if (contract.ContractClass == ContractClass.Tranche)
                            {
                                var creditLineId = await _contractService.GetCreditLineId(contract.Id);
                                var creditLine = _contractService.Get(creditLineId);
                                _contractService.CheckBlackListOnActionType(creditLine, relatedAction.ActionType);
                                contract.LoanPeriod =
                                    Math.Abs((contract.MaturityDate.Date - contract.ContractDate.Date).Days);
                            }
                            else
                            {
                                _contractService.CheckBlackListOnActionType(contract, relatedAction.ActionType);
                            }

                            if (contract != null && contract.Status == ContractStatus.AwaitForOrderApprove)
                            {
                                if (contract.IsBuyCar)
                                {
                                    contract.SignDate = DateTime.Now.Date;
                                    _paymentScheduleService.BuildWithContract(contract);
                                    if (contract.FirstPaymentDate != contract.PaymentSchedule.FirstOrDefault().Date)
                                    {
                                        contract.FirstPaymentDate = contract.PaymentSchedule.FirstOrDefault().Date;
                                    }

                                    contract.MaturityDate = contract.PaymentSchedule.Max(x => x.Date);

                                    _contractService.CheckSchedule(contract);

                                    contract.NextPaymentDate =
                                        contract.PercentPaymentType == PercentPaymentType.EndPeriod
                                            ? contract.MaturityDate
                                            : contract.PaymentSchedule
                                                .Where(x => x.ActionId == null && x.Canceled == null)
                                                .Min(x => x.Date);

                                    using (var tran = _contractRepository.BeginTransaction())
                                    {
                                        _contractRepository.Update(contract);
                                        _contractPaymentScheduleService.Save(contract.PaymentSchedule, contract.Id,
                                            _sessionContext.UserId);
                                        await _contractService.CalculateAPR(contract);
                                        _contractRepository.Update(contract);
                                        if (contract.SignDate < Constants.NEW_MAX_APR_DATE)
                                        {
                                            if (contract.APR > Constants.MAX_APR_OLD)
                                                throw new PawnshopApplicationException(
                                                    $"Cтавка ГЭСВ ({contract.APR}) превышает допустимое значение, попробуйте выбрать другую дату!");
                                        }
                                        else
                                        {
                                            if (contract.APR > Constants.MAX_APR_V2)
                                                throw new PawnshopApplicationException(
                                                    $"Cтавка ГЭСВ ({contract.APR}) превышает допустимое значение, попробуйте выбрать другую дату!");
                                        }

                                        tran.Commit();
                                    }
                                }

                                _clientIncomeService.RemoveIncomesAfterSign(contract.Id, contract.ClientId);
                                _clientAdditionalIncomeService.RemoveAdditionalIncomesAfterSign(contract.Id,
                                    contract.ClientId);

                                _contractService.ContractStatusUpdate(contract.Id, ContractStatus.Signed);
                                if (!contract.PartialPaymentParentId.HasValue)
                                {
                                    var setting = _contractService.GetSettingForContract(contract);
                                    if (setting.IsInsuranceAvailable)
                                    {
                                        InsurancePoliceRequest latestPoliceRequest = null;
                                        int contractId = contract.Id;

                                        if (relatedAction.ParentActionId != null)
                                        {
                                            var parentAction = await _contractActionService
                                                .GetAsync(relatedAction.ParentActionId.Value);
                                            
                                            var parentActionContract = await _contractService
                                                .GetOnlyContractAsync(parentAction.ContractId);

                                            if (parentActionContract.ContractClass == ContractClass.Credit
                                                || parentActionContract.ParentId.HasValue
                                                || parentActionContract.ClosedParentId.HasValue)
                                                contractId = parentAction.ContractId;
                                        }
                                    }
                                }

                                if (relatedAction.ParentActionId.HasValue && relatedAction.ParentActionId != null)
                                {
                                    var parentAction = await _contractActionService.GetAsync(relatedAction.ParentActionId.Value);
                                    if (parentAction != null && parentAction.ActionType == ContractActionType.Addition)
                                    {
                                        if (parentAction.Data != null && parentAction.Data.CategoryChanged)
                                        {
                                            _contractService.ChangeCategory(parentAction, contract, contract.LoanCost);
                                        }
                                        else
                                        {
                                            _contractService.CheckCategoryLimitSum(contract, contract.LoanCost, false);
                                        }

                                        var close = new CollectionClose()
                                        {
                                            ContractId = parentAction.ContractId,
                                            ActionId = parentAction.Id
                                        };
                                        contractIdsForCloseLegalCase.Add(close.ContractId);
                                        _collectionService.CloseContractCollection(close);
                                    }
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

                                await _positionEstimateHistoryService
                                    .SavePositionEstimateToHistoryForContract(contract);
                                await _positionSubjectService.SavePositionSubjectsToHistoryForContract(contract);
                                await applicationOnlineService.SendInsurance(contract.Id);

                                var refinanceBuyOutResult =
                                    await refinanceBuyOutService.BuyOutAllRefinancedContractsForApplicationsOnline(
                                        contract.Id);

                                if (!refinanceBuyOutResult)
                                {
                                    transaction.Rollback();

                                    throw new PawnshopApplicationException(
                                        "Ошибка выкупа займов: Не удалось выкупить займы!");
                                }

                                var expence = _clientExpenseService.Get(contract.ClientId);
                                if (expence != null)
                                {
                                    expence.AllLoan = null;
                                    var expenseDto = new ClientExpenseDto()
                                    {
                                        AvgPaymentToday = expence.AvgPaymentToday,
                                        Family = expence.Family,
                                        Housing = expence.Housing,
                                        Loan = expence.Loan,
                                        Other = expence.Other,
                                        Vehicle = expence.Vehicle,
                                        AllLoan = expence.AllLoan,
                                    };

                                    _clientExpenseService.Save(contract.ClientId, expenseDto);
                                }
                            }

                            break;
                        }
                        case ContractActionType.CreditLineClose:
                        case ContractActionType.Buyout:
                        case ContractActionType.BuyoutRestructuringCred:
                        {
                            int authorId = _sessionContext.UserId;
                            var contract = _contractService.Get(action.ContractId);
                            auctionRequestId = await SetOrderRequestId(auctionRequestId, contract);
                            if (relatedAction.SellingId.HasValue)
                            {
                                relatedAction.isFromSelling = true;
                            }

                            await _contractActionBuyoutService.ExecuteOnApprove(relatedAction,
                                _sessionContext.UserId, _branchContext.Branch.Id, null);

                            if (relatedAction.SellingId.HasValue)
                            {
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

                            var close = new CollectionClose()
                            {
                                ContractId = relatedAction.ContractId,
                                ActionId = relatedAction.Id
                            };
                            contractIdsForCloseLegalCase.Add(close.ContractId);
                            var isClosed = _collectionService.CloseContractCollection(close);
                            if (isClosed)
                                await mediator.Send(new SendClosedContractCommand()
                                    { ContractId = relatedAction.ContractId });
                            else
                                await mediator.Send(new SendContractOnlyCommand()
                                    { ContractId = relatedAction.ContractId });
                            _parkingActionService.UpdateParkingHistory(authorId, action.BuyoutCreditLine, action.Id,
                                contract);
                            break;
                        }
                        case ContractActionType.Payment:
                        {
                            _contractPaymentService.ExecuteOnApprove(relatedAction, _branchContext.Branch.Id,
                                _sessionContext.UserId, forceExpensePrepaymentReturn: false);

                            var close = new CollectionClose()
                            {
                                ContractId = relatedAction.ContractId,
                                ActionId = relatedAction.Id
                            };
                            contractIdsForCloseLegalCase.Add(close.ContractId);
                            var isClosed = _collectionService.CloseContractCollection(close);
                            if (isClosed)
                                mediator.Send(new SendClosedContractCommand()
                                    { ContractId = relatedAction.ContractId }).Wait();
                            else
                                mediator.Send(new SendContractOnlyCommand()
                                    { ContractId = relatedAction.ContractId }).Wait();

                            break;
                        }
                        case ContractActionType.Prepayment:
                        {
                            if (relatedAction.IsInitialFee.HasValue && relatedAction.IsInitialFee.Value)
                            {
                                Contract contract = _contractService.Get(action.ContractId);

                                decimal depoMerchantBalance =
                                    _contractService.GetDepoMerchantBalance(contract.Id, relatedAction.Date);

                                if (contract.RequiredInitialFee <= depoMerchantBalance)
                                {
                                    // перезагружаем договор
                                    contract = _contractRepository.Get(contract.Id);
                                    if (contract == null)
                                        throw new PawnshopApplicationException($"Договор {contract.Id} не найден");

                                    contract.Status = ContractStatus.PositionRegistration;
                                    contract.PayedInitialFee = depoMerchantBalance;
                                    _contractRepository.Update(contract);
                                }

                                try
                                {
                                    var application = _applicationRepository.FindByContractId(action.ContractId);

                                    if (application != null && application.IsAutocredit == 1)
                                    {
                                        var appId = new UpdateOnStatusPositionRegistration()
                                        {
                                            Apply_id = application.AppId
                                        };
                                        await _requestMobileAppService.UpdateStatusInMobileApp(appId);
                                    }
                                }
                                catch (Exception e)
                                {
                                    await _eventLog.LogAsync(EventCode.CallRequestMobileAppService, EventStatus.Failed,
                                        EntityType.Contract, action.ContractId, null, e.Message);
                                }

                                await mediator.Send(new SendContractOnlyCommand()
                                    { ContractId = relatedAction.ContractId });
                            }

                            await _legalCollectionNotificationService
                                .SendPrepaymentReceivedToLegalCase(relatedAction.ContractId,
                                    _sessionContext.UserId);
                            break;
                        }
                        case ContractActionType.PrepaymentReturn:
                        {
                            Contract contract = _contractService.Get(action.ContractId);
                            if (contract.Status < ContractStatus.Signed)
                                contract.Status = ContractStatus.Canceled;
                            _contractRepository.Update(contract);
                            break;
                        }
                        case ContractActionType.PartialPayment:
                        {
                            var contract = _contractService.Get(relatedAction.ContractId);
                            _contractPaymentScheduleService.Save(contract.PaymentSchedule, contract.Id,
                                _sessionContext.UserId);

                            await _contractPaymentScheduleService.UpdateContractPaymentScheduleHistoryStatus(
                                relatedAction.ContractId, relatedAction.Id, 20);
                            await _contractPaymentScheduleService.UpdateActionIdForPartialPayment(relatedAction.Id,
                                relatedAction.Date, relatedAction.ContractId);
                            var penaltyCost = action.Rows.Where(x =>
                                    (x.PaymentType == AmountType.DebtPenalty ||
                                     x.PaymentType == AmountType.LoanPenalty ||
                                     x.PaymentType == AmountType.AmortizedDebtPenalty ||
                                     x.PaymentType == AmountType.AmortizedLoanPenalty) && x.DebitAccountId != null)
                                .Sum(r => r.Cost);
                            await _contractPaymentScheduleService.UpdateActionIdForPartialPaymentUnpaid(
                                relatedAction.Id, relatedAction.Date, relatedAction.ContractId, penaltyCost,
                                isEndPeriod: contract.PercentPaymentType == PercentPaymentType.EndPeriod);

                            var nextPaymentDate =
                                await _contractPaymentScheduleService.GetNextPaymentSchedule(contract.Id);
                            if (nextPaymentDate != null)
                            {
                                contract.NextPaymentDate = nextPaymentDate.Date;
                            }

                            if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
                            {
                                var unpaid = await _contractPaymentScheduleService.GetUnpaidSchedule(contract.Id);
                                if (unpaid != null)
                                {
                                    contract.MaturityDate = unpaid.Date;
                                    contract.NextPaymentDate = unpaid.Date;
                                }
                            }

                            _contractRepository.Update(contract);

                            var close = new CollectionClose()
                            {
                                ContractId = relatedAction.ContractId,
                                ActionId = relatedAction.Id
                            };
                            contractIdsForCloseLegalCase.Add(close.ContractId);
                            var isClosed = _collectionService.CloseContractCollection(close);
                            if (isClosed)
                                await mediator.Send(new SendClosedContractCommand()
                                    { ContractId = relatedAction.ContractId });
                            else
                                await mediator.Send(new SendContractOnlyCommand()
                                    { ContractId = relatedAction.ContractId });

                            _crmPaymentService.Enqueue(contract);
                            break;
                        }
                    }
                }

                transaction.Commit();

                if (contractIdsForCloseLegalCase.Any())
                {
                    foreach (var contractId in contractIdsForCloseLegalCase)
                    {
                        await _closeLegalCaseService.CloseAsync(contractId);
                    }
                }

                if (auctionRequestId.HasValue)
                {
                    await _auctionOperationHttpService.Approve(new ApproveAuctionCommand
                        {
                            RequestId = (Guid)auctionRequestId,
                            AuthorName = _sessionContext.UserName
                        }
                    );
                }
            }

            var orderIds = await _cashOrderService.GetAllRelatedOrdersByContractActionId(approveStructure.ActionId);
            _uKassaService.FinishRequests(orderIds);
        }

        [HttpPost("/api/contractAction/selling"), ProducesResponseType(typeof(ContractAction), 200)]
        [Event(EventCode.ContractSelling, EventMode = EventMode.All, EntityType = EntityType.Contract, IncludeFails = true)]
        public async Task<IActionResult> Selling([FromBody] ContractAction action)
        {
            ModelState.Validate();

            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (action.ContractId <= 0)
                throw new ArgumentException();
            action.ActionType = ContractActionType.PrepareSelling;
            _contractActionCheckService.ContractActionCheck(action);

            var contract = _contractService.Get(action.ContractId);

            if (contract.Status != ContractStatus.Signed)
                throw new InvalidOperationException();
            if (contract.Actions.Any() && action.Date.Date < contract.Actions.Max(x => x.Date).Date)
                throw new PawnshopApplicationException(_badDateError);

            var incompleteExists = await _contractActionService.IncopleteActionExists(contract.Id);
            if (incompleteExists)
                throw new PawnshopApplicationException("В договоре имеется неподтвержденное действие");

            using (var transaction = _contractRepository.BeginTransaction())
            {

                _contractRepository.UpdatePositions(action.ContractId, action.Data.Positions);
                contract.Positions = action.Data.Positions.ToList();

                _contractSellingService.RegisterSellings(contract, action, _sessionContext.UserId, _branchContext.Branch.Id);
                _eventLog.Log(EventCode.SellingSaved, EventStatus.Success, EntityType.Selling, contract.Id, null, null);

                foreach (var contractPosition in contract.Positions)
                {
                    contractPosition.Status = ContractPositionStatus.SoldOut;
                }

                if (contract.ContractClass == ContractClass.CreditLine)
                {
                    var tranches = await _contractService.GetAllSignedTranches(contract.Id);
                    foreach (var tranche in tranches)
                    {
                        tranche.Status = ContractStatus.SoldOut;
                        _contractRepository.Update(tranche);
                    }
                }

                contract.Status = ContractStatus.SoldOut;
                _contractRepository.Update(contract);

                action.AuthorId = _sessionContext.UserId;
                action.CreateDate = DateTime.Now;
                _contractActionService.Save(action);

                transaction.Commit();
            }

            ScheduleMintosPaymentUpload(_contractService.Get(action.ContractId), action);

            return Ok(action);
        }

        [HttpPost("/api/contractAction/sign")]
        [Event(EventCode.ContractSign, EventMode = EventMode.All, EntityType = EntityType.Contract,
            IncludeFails = true)]
        public async Task<IActionResult> Sign([FromBody] ContractAction action)
        {
            ModelState.Validate();

            action.ProcessingId = null;
            action.ProcessingType = null;

            if (action.Date.Date != DateTime.Now.Date
                && !_sessionContext.Permissions.Any(x => x == Permissions.ContractDiscount)
                && !_sessionContext.ForSupport)
                throw new PawnshopApplicationException(_notEnoughPermissionError);

            var contract = _contractRepository.Get(action.ContractId);

            if (contract.ContractClass == ContractClass.CreditLine)
                throw new PawnshopApplicationException("Нельзя подписать кредитную линию.");

            await _contractVerificationService.CheckIfRealtyContractConfirmed(contract);
            await _contractVerificationService.CheckClientsAge(contract);
            await _paymentScheduleService.CheckPayDayFromContract(contract);

            //ChecksOnChangeCategory(action);

            using (var transaction = _contractRepository.BeginTransaction())
            {
                if (contract.ContractClass == ContractClass.Tranche)
                {
                    var creditLine = _contractRepository.Get(contract.CreditLineId.Value);

                    if (creditLine == null)
                        throw new PawnshopApplicationException("Не найдена кредитная линия.");

                    if (_contractService.IsContractBusinessPurpose(contract))
                        _contractService.SaveContractExpertOpinionData(contract.Id);

                    if (creditLine.Status != ContractStatus.Signed)
                    {
                        ContractDutyCheckModel checkModel = new ContractDutyCheckModel
                        {
                            ActionType = ContractActionType.Sign,
                            ContractId = creditLine.Id,
                            Cost = creditLine.LoanCost,
                            Date = creditLine.ContractDate,
                            PayTypeId = action.PayTypeId
                        };

                        var contractDuty = _contractDutyService.GetContractDuty(checkModel);

                        var creditLineAction = new ContractAction
                        {
                            ActionType = ContractActionType.Sign,
                            Checks = contractDuty.Checks.Select((x, i) => new ContractActionCheckValue { Check = x, CheckId = x.Id, Value = true }).ToList(),
                            ContractId = creditLine.Id,
                            Date = contractDuty.Date,
                            Discount = contractDuty.Discount,
                            Expense = action.Expense,
                            ExtraExpensesCost = contractDuty.ExtraExpensesCost,
                            PayTypeId = action.PayTypeId,
                            Reason = contractDuty.Reason,
                            RequisiteCost = 0,
                            RequisiteId = action.RequisiteId,
                            Rows = contractDuty.Rows.ToArray(),
                            TotalCost = contractDuty.Cost,
                        };

                        InternalSign(creditLineAction, creditLine);

                        creditLine.SignDate = DateTime.Now;
                        creditLine.Status = ContractStatus.AwaitForOrderApprove;
                        _contractService.Save(creditLine);

                        action.Expense = null;
                        action.ParentAction = creditLineAction;
                        action.ParentActionId = creditLineAction.Id;

                        action = InternalSign(action, contract);

                        creditLineAction.ChildActionId = action.Id;
                        _contractActionService.Save(creditLineAction);

                        transaction.Commit();

                        return Ok(action);
                    }

                    else if (creditLine.Status == ContractStatus.Signed && creditLine.ContractData != null && _contractService.GetContractSettings(creditLine.Id).IsInsuranceAdditionalLimitOn)
                    {
                        // сохранение сумм "с правом вождения" и "без права вождения" из заявки в позицию
                        var trancheApp = _applicationRepository.FindByContractId(contract.Id);
                        var position = creditLine.Positions.FirstOrDefault();
                        int index = creditLine.Positions.IndexOf(position);

                        if (index != -1)
                        {
                            position.MotorCost = trancheApp.MotorCost;
                            position.TurboCost = trancheApp.TurboCost;
                            creditLine.Positions[index] = position;
                            _contractRepository.UpdatePositions(creditLine.Id, creditLine.Positions.ToArray());
                        }

                        // смена категории из contractAction (contractAction для смены категории при созании транша создается в ApplicationController.InitTrancheFromAppV2)
                        var changeCategoryAction = _contractActionService.GetContractActionsByContractId(contract.Id).Result.
                            Where(action => action.ActionType == ContractActionType.ChangeCreditLineCategory).LastOrDefault();
                        if (changeCategoryAction != null && changeCategoryAction.Data != null)
                        {
                            if (_contractService.ChangeCategory(changeCategoryAction, creditLine, changeCategoryAction.Cost))
                            {
                                changeCategoryAction.Data.CategoryChanged = true;
                                changeCategoryAction.Status = ContractActionStatus.Approved;
                                action.Data = new ContractActionData() { CategoryChanged = false };
                                _contractActionService.Save(changeCategoryAction);

                            }
                        }
                    }
                }

                action = InternalSign(action, contract);

                transaction.Commit();

                return Ok(action);
            }
        }

        private ContractAction InternalSign(ContractAction action, Contract contract)
        {
            var isKdnRequired = _contractKdnService.IsKdnRequired(contract);
            var isKdnPassed = _contractKdnService.IsKDNPassed(action.ContractId, false).Result;

            if (isKdnRequired && !isKdnPassed)
                throw new PawnshopApplicationException("КДН не пройден. Подписание недоступно");

            action.ActionType = ContractActionType.Sign;
            int authorId = _sessionContext.UserId;
            int branchId = _branchContext.Branch.Id;
            bool unsecuredContractSignNotallowed = _sessionContext
                    .Permissions.Where(x => x.Equals(Permissions.UnsecuredContractSign))
                    .FirstOrDefault() != Permissions.UnsecuredContractSign;

            _contractActionSignService.Exec(action, authorId, branchId, unsecuredContractSignNotallowed);

            return action;
        }

        [HttpPost("/api/contractAction/prolong"), ProducesResponseType(typeof(ContractAction), 200)]
        [Event(EventCode.ContractProlong, EventMode = EventMode.All, EntityType = EntityType.Contract,
            IncludeFails = true)]
        public async Task<IActionResult> Prolong([FromBody] ContractAction action, [FromServices] FunctionSettingRepository functionSettingRepository)
        {
            var depoMasteringSetting = functionSettingRepository.GetByCode(Constants.FUNCTION_SETTING__DEPO_MASTERING);

            if (depoMasteringSetting != null && depoMasteringSetting.BooleanValue.HasValue && depoMasteringSetting.BooleanValue.Value)
            {
                throw new PawnshopApplicationException("Функционал временно отключен. Внесите деньги на аванс договора.");
            }

            ModelState.Validate();
            action.ProcessingId = null;
            action.ProcessingType = null;

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (action.ContractId <= 0)
                throw new ArgumentException();

            if (!action.PayTypeId.HasValue)
                throw new PawnshopApplicationException("Тип платежа обязателен");

            action.ActionType = ContractActionType.Prolong;
            _contractActionCheckService.ContractActionCheck(action);
            var contract = _contractService.Get(action.ContractId);
            if (contract == null)
                throw new PawnshopApplicationException($"Договор {action.ContractId} не найден");

            var incompleteExists = await _contractActionService.IncopleteActionExists(contract.Id);
            if (incompleteExists)
                throw new PawnshopApplicationException("В договоре имеется неподтвержденное действие");

            if (contract.Status != ContractStatus.Signed)
                throw new InvalidOperationException();

            if (contract.PercentPaymentType != PercentPaymentType.EndPeriod)
                throw new PawnshopApplicationException($"Для данного договора({contract.Id}) невозможна пролонгация");

            if (contract.LoanPeriod <= 0)
                throw new PawnshopApplicationException("Период займа договора меньше или равно 0, ожидалось получить значение больше 0");

            if (action.Date.Date < contract.Actions.Max(x => x.Date))
                throw new PawnshopApplicationException(_badDateError);

            if (action.Date.Date != DateTime.Now.Date && _sessionContext.Permissions.All(x => x != Permissions.ContractDiscount) && !_sessionContext.ForSupport)
                throw new PawnshopApplicationException(_notEnoughPermissionError);

            var loanAmountTypes = new HashSet<AmountType>
            {
                AmountType.Loan,
                AmountType.OverdueLoan,
                AmountType.DebtPenalty,
                AmountType.LoanPenalty,
                AmountType.Receivable
            };

            var loanAmountDict = new Dictionary<AmountType, decimal>();
            var loanRows = new List<ContractActionRow>();
            foreach (ContractActionRow row in action.Rows)
            {
                if (!loanAmountTypes.Contains(row.PaymentType))
                    continue;

                decimal amount;
                if (loanAmountDict.TryGetValue(row.PaymentType, out amount))
                {
                    if (amount != row.Cost)
                        throw new PawnshopApplicationException($"Стоимость проводок по типу {row.PaymentType} не сходятся");
                }

                loanRows.Add(row);
                loanAmountDict[row.PaymentType] = row.Cost;
            }

            decimal loanAmount = 0;
            if (loanAmountDict.Count > 0)
                loanAmount = loanAmountDict.Values.Sum();

            action.TotalCost = loanAmount;
            decimal extraExpensesCost = action.ExtraExpensesCost ?? 0;
            ContractAction prepaymentAction = null;
            using (var transaction = _contractRepository.BeginTransaction())
            {
                _crmPaymentService.Enqueue(contract);
                int authorId = _sessionContext.UserId;
                int branchId = _branchContext.Branch.Id;
                decimal depoBalance = _contractService.GetPrepaymentBalance(contract.Id, action.Date);
                List<ContractExpense> extraExpensesCostForPayment = _contractExpenseOperationService.GetNeededExpensesForPrepayment(contract.Id, branchId);
                decimal extraExpensesSum = extraExpensesCostForPayment.Count > 0 ? extraExpensesCostForPayment.Sum(e => e.TotalCost) : 0;
                decimal diff = depoBalance - loanAmount - extraExpensesSum;
                if (diff < 0)
                {
                    decimal prepaymentCost = Math.Ceiling(Math.Abs(diff));
                    prepaymentAction = _contractActionPrepaymentService.Exec(contract.Id, prepaymentCost, action.PayTypeId.Value, branchId, authorId, action.Date);
                    if (prepaymentAction == null)
                        throw new PawnshopApplicationException($"Ожидалось что {nameof(_contractActionPrepaymentService)}.{nameof(_contractActionPrepaymentService.Exec)} не вернет null");

                    action.ParentActionId = prepaymentAction.Id;
                }

                _contractActionProlongService.Exec(action, authorId, branchId, forceExpensePrepaymentReturn: false, autoApprove: false);
                if (prepaymentAction != null)
                {
                    prepaymentAction.ChildActionId = action.Id;
                    _contractActionService.Save(prepaymentAction);
                    action.ParentAction = prepaymentAction;
                }

                transaction.Commit();
            }

            if (prepaymentAction != null)
                action = prepaymentAction;

            return Ok(action);
        }

        [HttpPost("/api/contractAction/buyout"), ProducesResponseType(typeof(ContractAction), 200)]
        [Event(EventCode.ContractBuyout, EventMode = EventMode.All, EntityType = EntityType.Contract,
            IncludeFails = true)]
        public async Task<IActionResult> Buyout([FromBody] ContractAction action, [FromServices] FunctionSettingRepository functionSettingRepository)
        {
            var depoMasteringSetting = functionSettingRepository.GetByCode(Constants.FUNCTION_SETTING__DEPO_MASTERING);

            if (depoMasteringSetting != null && depoMasteringSetting.BooleanValue.HasValue && depoMasteringSetting.BooleanValue.Value)
            {
                throw new PawnshopApplicationException("Функционал временно отключен. Внесите деньги на аванс договора.");
            }

            ModelState.Validate();
            action.ProcessingId = null;
            action.ProcessingType = null;
            action.ActionType = ContractActionType.Buyout;
            action.AuthorId = _sessionContext.UserId;
            action.CreateDate = DateTime.Now;
            if (action.Date.Date != DateTime.Now.Date && !_sessionContext.Permissions.Any(x => x == Permissions.ContractDiscount) && !_sessionContext.ForSupport)
                throw new PawnshopApplicationException(_notEnoughPermissionError);

            var contract = _contractRepository.Get(action.ContractId);
            if (contract.IsContractRestructured && contract.ContractClass == ContractClass.Credit)
                action.ActionType = ContractActionType.BuyoutRestructuringCred;

            if (contract == null)
                throw new PawnshopApplicationException($"Договор {action.ContractId} не найден");

            if (contract.ContractClass == ContractClass.CreditLine)
                action.BuyoutCreditLine = true;//При выкупе кредитной линии с фронта всегда приходит данный параметр как false однако должен быть true

            if (contract.Status != ContractStatus.Signed && !action.isFromSelling)
                throw new PawnshopApplicationException("Выкуп невозможен, так как данный договор не является действующим");

            if (contract.Actions.Any() && action.Date.Date < contract.Actions.Max(x => x.Date).Date)
                throw new PawnshopApplicationException(_badDateError);

            var incompleteExists = await _contractActionService.IncopleteActionExists(contract.Id);
            if (incompleteExists)
                throw new PawnshopApplicationException("В договоре имеется неподтвержденное действие");

            int branchId = _branchContext.Branch.Id;
            int authorId = _sessionContext.UserId;
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
                return Ok(action);
            }
        }
        
        [HttpPost("/api/contractAction/buyoutAuction"), ProducesResponseType(typeof(ContractAction), 200)]
        [Event(EventCode.ContractBuyout, EventMode = EventMode.All, EntityType = EntityType.Contract, IncludeFails = true)]
        public async Task<IActionResult> BuyoutByAuction([FromBody] ContractBuyOutByAuctionCommand command)
        {
            var depoMasteringSetting = await _functionSettingRepository.GetByCodeAsync(Constants.FUNCTION_SETTING__DEPO_MASTERING);
            if (depoMasteringSetting != null && depoMasteringSetting.BooleanValue.HasValue &&
                depoMasteringSetting.BooleanValue.Value)
            {
                throw new PawnshopApplicationException(
                    "Функционал временно отключен. Внесите деньги на аванс договора.");
            }

            ModelState.Validate();
            if (_sessionContext.Permissions.All(x => x != Permissions.AuctionManage))
            {
                throw new PawnshopApplicationException(Constants.NotEnoughRights);
            }

            var action = await _contractBuyOutByAuctionService.BuyOut(command);
            return Ok(action);
        }

        [HttpPost("/api/contractAction/partialBuyout"), ProducesResponseType(typeof(ContractAction), 200)]
        [Event(EventCode.ContractPartialBuyout, EventMode = EventMode.All, EntityType = EntityType.Contract, IncludeFails = true)]
        public async Task<IActionResult> PartialBuyout([FromBody] ContractAction action)
        {
            throw new PawnshopApplicationException("Функционал недоступен");
        }

        [HttpPost("/api/contractAction/partialPayment"), ProducesResponseType(typeof(ContractAction), 200)]
        [Event(EventCode.ContractPartialPayment, EventMode = EventMode.All, EntityType = EntityType.Contract, IncludeFails = true)]
        public async Task<IActionResult> PartialPayment([FromBody] ContractAction action, [FromServices] FunctionSettingRepository functionSettingRepository)
        {
            var depoMasteringSetting = functionSettingRepository.GetByCode(Constants.FUNCTION_SETTING__DEPO_MASTERING);

            if (depoMasteringSetting != null && depoMasteringSetting.BooleanValue.HasValue && depoMasteringSetting.BooleanValue.Value)
            {
                throw new PawnshopApplicationException("Функционал временно отключен. Внесите деньги на аванс договора.");
            }

            ModelState.Validate();
            if (action.ContractId <= 0)
                throw new PawnshopApplicationException("Идентификатор договора должен быть больше нуля");

            if (action.Date.Date > DateTime.Now.Date)
                throw new PawnshopApplicationException("Нельзя делать ЧДП будущей датой");

            action.ProcessingId = null;
            action.ProcessingType = null;
            action.ActionType = ContractActionType.PartialPayment;
            _contractActionCheckService.ContractActionCheck(action);


            if (action.Date.Date != DateTime.Now.Date && !_sessionContext.Permissions.Any(x => x == Permissions.ContractDiscount) && !_sessionContext.ForSupport)
                throw new PawnshopApplicationException(_notEnoughPermissionError);

            if (action.Cost <= 0 && !action.HasPercentAdjustment)
                throw new PawnshopApplicationException("Укажите сумму частичного гашения");

            var parentContract = _contractService.Get(action.ContractId);
            if (parentContract.Status != ContractStatus.Signed)
                throw new PawnshopApplicationException("Договор должен быть подписан");

            if (parentContract.PaymentSchedule.Where(x => parentContract.PercentPaymentType != PercentPaymentType.EndPeriod && x.Date == DateTime.Now.Date && x.ActionId == null && x.Canceled == null).Any())
            {
                throw new PawnshopApplicationException("Не возможно сделать ЧДП, т.к. по договору имеется не оплаченный платеж с сегодняшней датой оплаты. Вначале сделайте погашение имеющейся задолженности по договору через функционал оплаты(кнопка 'Оплата').");
            }

            var incompleteExists = await _contractActionService.IncopleteActionExists(action.ContractId);
            if (incompleteExists)
                throw new PawnshopApplicationException("В договоре имеется неподтвержденное действие");

            using (var transaction = _contractRepository.BeginTransaction())
            {
                int authorId = _sessionContext.UserId;
                int branchId = _branchContext.Branch.Id;
                bool unsecuredContractSignNotallowed = _sessionContext
                    .Permissions.Where(x => x.Equals(Permissions.UnsecuredContractSign))
                    .FirstOrDefault() != Permissions.UnsecuredContractSign;

                ContractAction prepaymentAction = null;
                decimal extraExpensesCost = action.ExtraExpensesCost ?? 0;

                decimal depoBalance = _contractService.GetPrepaymentBalance(action.ContractId, action.Date);
                List<ContractExpense> extraExpensesCostForPayment = _contractExpenseOperationService.GetNeededExpensesForPrepayment(action.ContractId, branchId);
                decimal extraExpensesSum = extraExpensesCostForPayment.Count > 0 ? extraExpensesCostForPayment.Sum(e => e.TotalCost) : 0;

                decimal diff = depoBalance - action.Cost - extraExpensesSum;
                if (diff < 0)
                {
                    decimal prepaymentCost = Math.Ceiling(Math.Abs(diff));
                    prepaymentAction = _contractActionPrepaymentService.Exec(action.ContractId, prepaymentCost, action.PayTypeId.Value, branchId, authorId, action.Date, orderStatus: OrderStatus.WaitingForApprove);
                }

                await _contractActionPartialPaymentService.Exec(action, authorId, branchId, unsecuredContractSignNotallowed, forceExpensePrepaymentReturn: false);
                if (prepaymentAction != null)
                {
                    prepaymentAction.ChildActionId = action.Id;
                    _contractActionService.Save(prepaymentAction);
                    action.ParentActionId = prepaymentAction.Id;
                    _contractActionService.Save(action);
                    action.ParentAction = prepaymentAction;
                }

                if (action.CategoryChanged.HasValue && action.CategoryChanged.Value)
                {
                    _contractService.ChangeCategory(action, parentContract, parentContract.LeftLoanCost - action.Cost);

                    if (action.Data != null)
                        action.Data.CategoryChanged = action.CategoryChanged.Value;
                    else
                    {
                        ContractActionData data = new ContractActionData();
                        data.CategoryChanged = action.CategoryChanged.Value;
                        action.Data = data;
                    }

                    _contractActionService.Save(action);
                }

                transaction.Commit();
                var orderIds = await _cashOrderService.GetAllRelatedOrdersByContractActionId(action.Id);
                _uKassaService.FinishRequests(orderIds);
            }

            return Ok(action);
        }

        [HttpPost("/api/contractAction/multiTransfer"), ProducesResponseType(typeof(List<ContractAction>), 200)]
        [Authorize(Permissions.ContractTransfer)]
        public async Task<IActionResult> MultiTransfer([FromBody] List<ContractAction> actions)
        {
            throw new PawnshopApplicationException("Функционал временно недоступен");
        }

        [HttpPost("/api/contractAction/transfer"), ProducesResponseType(typeof(ContractAction), 200)]
        [Authorize(Permissions.ContractTransfer)]
        [Event(EventCode.ContractTransferred, EventMode = EventMode.All, EntityType = EntityType.Contract, IncludeFails = true)]
        public async Task<IActionResult> Transfer([FromBody] ContractAction action)
        {
            throw new PawnshopApplicationException("Функционал временно недоступен");
        }

        [HttpPost("/api/contractAction/monthlyPayment"), ProducesResponseType(typeof(ContractAction), 200)]
        [Event(EventCode.ContractMonthlyPayment, EventMode = EventMode.All, EntityType = EntityType.Contract, IncludeFails = true)]
        public async Task<IActionResult> MonthlyPayment([FromBody] ContractAction action)
        {
            throw new PawnshopApplicationException("Функционал временно недоступен");
        }

        [HttpPost("/api/contractAction/checkKdn4Addition"), ProducesResponseType(typeof(ContractKdnModel), 200)]
        public async Task<IActionResult> CheckKdn4Addition([FromBody] ContractActionModel4Kdn contractActionModel4Kdn)
        {
            if (contractActionModel4Kdn == null || contractActionModel4Kdn.additionLoanPeriod == null || contractActionModel4Kdn.AdditionCost == 0)
                throw new PawnshopApplicationException("Не заполнены все нужные поля");


            int authorId = _sessionContext.UserId;
            int branchId = _branchContext.Branch.Id;

            var user = _userRepository.Get(authorId);
            decimal surchargeAmount = contractActionModel4Kdn.SurchargeAmount ?? 0;

            var model = _contractKdnService.CheckKdn4AdditionWithCreateChild(contractActionModel4Kdn.Action, branchId, user, surchargeAmount, contractActionModel4Kdn.AdditionCost, contractActionModel4Kdn.SettingId, contractActionModel4Kdn.additionLoanPeriod, contractActionModel4Kdn.subjectId, contractActionModel4Kdn.PositionEstimatedCost);

            return Ok(model);
        }

        [HttpPost("/api/contractAction/addition")]
        [Event(EventCode.ContractAddition, EventMode = EventMode.All, EntityType = EntityType.Contract, IncludeFails = true)]
        public async Task<ContractAction> Addition([FromBody] ContractAction action, [FromServices] FunctionSettingRepository functionSettingRepository)
        {
            var depoMasteringSetting = functionSettingRepository.GetByCode(Constants.FUNCTION_SETTING__DEPO_MASTERING);

            if (depoMasteringSetting != null && depoMasteringSetting.BooleanValue.HasValue && depoMasteringSetting.BooleanValue.Value)
            {
                throw new PawnshopApplicationException("Функционал временно отключен. Внесите деньги на аванс договора.");
            }

            ModelState.Validate();
            if (action == null)
                throw new PawnshopApplicationException("Идентификатор договора должен быть больше нуля");

            _applicationService.ValidateApplicationForAdditionCost(action.ContractId, action.Cost);

            action.ProcessingId = null;
            action.ProcessingType = null;
            if (action.Date.Date != DateTime.Now.Date
                   && !_sessionContext.Permissions.Any(x => x == Permissions.ContractDiscount)
                   && !_sessionContext.ForSupport)
                throw new PawnshopApplicationException(_notEnoughPermissionError);

            if (action.Cost <= 0)
                throw new PawnshopApplicationException("Укажите сумму добора");

            //проверка наличия результата расчета КДН на форме "Заявка на добор" на дату Добора. если нет, выкидывать ошибку
            var contractKdnCalculationLog = _contractKdnService.GetContractKdnCalculationLog4AdditionDate(action.ContractId, action.Date.Date);
            action.ChildSettingId = contractKdnCalculationLog.ChildSettingId;
            action.ChildLoanPeriod = contractKdnCalculationLog.ChildLoanPeriod;
            action.ChildSubjectId = contractKdnCalculationLog.ChildSubjectId;

            var isKdnPassed = await _contractKdnService.IsKDNPassed(action.ContractId, true);
            if (!isKdnPassed)
                throw new PawnshopApplicationException("КДН не пройден. Подписание недоступно");

            int authorId = _sessionContext.UserId;
            int branchId = _branchContext.Branch.Id;
            bool unsecuredContractSignNotallowed = _sessionContext
                .Permissions.Where(x => x.Equals(Permissions.UnsecuredContractSign))
                .FirstOrDefault() != Permissions.UnsecuredContractSign;

            var incompleteExists = await _contractActionService.IncopleteActionExists(action.ContractId);
            if (incompleteExists)
                throw new PawnshopApplicationException("В договоре имеется неподтвержденное действие");

            var loanAmountTypes = new HashSet<AmountType>
            {
                AmountType.Debt,
                AmountType.Loan,
                AmountType.OverdueLoan,
                AmountType.DebtPenalty,
                AmountType.LoanPenalty,
                AmountType.OverdueDebt,
                AmountType.Receivable
            };

            var loanAmountDict = _rowBuilder.GetDistinctRowAmounts(action.Rows.ToList(), loanAmountTypes);

            decimal loanAmount = 0;
            if (loanAmountDict != null && loanAmountDict.Count > 0)
                loanAmount = loanAmountDict.Values.Sum();

            using var transaction = _cashOrderService.BeginCashOrderTransaction();
            {
                ContractAction prepaymentAction = null;
                decimal extraExpensesCost = action.ExtraExpensesCost ?? 0;

                decimal depoBalance = _contractService.GetPrepaymentBalance(action.ContractId, action.Date);
                List<ContractExpense> extraExpensesCostForPayment = _contractExpenseOperationService.GetNeededExpensesForPrepayment(action.ContractId, branchId);
                decimal extraExpensesSum = extraExpensesCostForPayment.Count > 0 ? extraExpensesCostForPayment.Sum(e => e.TotalCost) : 0;
                decimal diff = depoBalance - loanAmount - extraExpensesSum;
                if (diff < 0)
                {
                    decimal prepaymentCost = Math.Ceiling(Math.Abs(diff));
                    prepaymentAction = _contractActionPrepaymentService.Exec(action.ContractId, prepaymentCost, action.PayTypeId.Value, branchId, authorId, action.Date, orderStatus: OrderStatus.Approved);
                }

                var contract = _contractService.Get(action.ContractId);

                _contractActionAdditionService.Exec(action, authorId, branchId, unsecuredContractSignNotallowed, forceExpensePrepaymentReturn: false);
                _contractCloseService.Exec(contract, action.Date, authorId, prepaymentAction ?? action, OrderStatus.Approved);

                if (prepaymentAction != null)
                {
                    prepaymentAction.ChildActionId = action.Id;
                    _contractActionService.Save(prepaymentAction);
                    action.ParentActionId = prepaymentAction.Id;
                    action.ParentAction = prepaymentAction;
                }

                _contractActionService.Save(action);

                transaction.Commit();
                var orderIds = await _cashOrderService.GetAllRelatedOrdersByContractActionId(action.Id);

                _uKassaService.FinishRequests(orderIds);
            }

            return action;
        }

        [HttpPost("/api/contractAction/prepaymentWithNotification"), ProducesResponseType(typeof(ContractAction), 200)]
        [Event(EventCode.Prepayment, EventMode = EventMode.All, EntityType = EntityType.Contract, IncludeFails = true)]
        public async Task<IActionResult> PrepaymentWithNotification([FromBody] ContractAction action)
        {
            ModelState.Validate();
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (action.ContractId <= 0)
                throw new ArgumentException();
            if (action.TotalCost <= 0)
                throw new PawnshopApplicationException("Укажите сумму авансового платежа");

            var incompleteExists = await _contractActionService.IncopleteActionExists(action.ContractId);
            if (incompleteExists)
                throw new PawnshopApplicationException("В договоре имеется неподтвержденное действие");

            action.ProcessingId = null;
            action.ProcessingType = null;
            action.ActionType = ContractActionType.Prepayment;
            _contractActionCheckService.ContractActionCheck(action);

            var prepaymentAction = await Prepayment(action);
            var contract = _contractService.Get(prepaymentAction.ContractId);

            if (!action.IsInitialFee.HasValue && contract.Actions.Any() && action.Date.Date < contract.Actions.Max(x => x.Date).Date)
                throw new PawnshopApplicationException(_badDateError);

            _innerNotificationRepository.Insert(new InnerNotification
            {
                CreateDate = DateTime.Now,
                CreatedBy = _sessionContext.UserId,
                EntityType = EntityType.Contract,
                EntityId = contract.Id,
                Message = $"На баланс договора {contract.ContractNumber} поступила сумма {action.TotalCost}. Уточните у Клиента для какого действия поступила сумма.",
                ReceiveBranchId = contract.BranchId,
                Status = InnerNotificationStatus.Sent
            });

            return Ok(action);
        }

        [HttpPost("/api/contractAction/movePrepayment")]
        [Authorize(Permissions.PrepaymentMoveManage)]
        public async Task<IActionResult> MovePrepayment([FromBody] MovePrepayment prepaymentModel)
        {
            int authorId = _sessionContext.UserId;
            var branch = _branchContext.Branch;
            var contract = _contractService.Get(prepaymentModel.SourceContractId);

            var incompleteExists = await _contractActionService.IncopleteActionExists(contract.Id);
            if (incompleteExists)
                throw new PawnshopApplicationException("В договоре имеется неподтвержденное действие");

            return Ok(_contractActionPrepaymentService.MovePrepayment(prepaymentModel, authorId, branch));
        }

        [HttpPost("/api/contractAction/prepayment"), ProducesResponseType(typeof(ContractAction), 200)]
        [Event(EventCode.Prepayment, EventMode = EventMode.All, EntityType = EntityType.Contract, IncludeFails = true)]
        public async Task<ContractAction> Prepayment([FromBody] ContractAction action)
        {
            ModelState.Validate();
            if (action.ContractId <= 0)
                throw new PawnshopApplicationException("Идентификатор договора должен быть больше нуля");

            if (action.TotalCost <= 0)
                throw new PawnshopApplicationException("Укажите сумму авансового платежа");

            if (action.Date.Date != DateTime.Now.Date
                && !_sessionContext.Permissions.Any(x => x == Permissions.ContractDiscount)
                && !_sessionContext.ForSupport)
                throw new PawnshopApplicationException(_notEnoughPermissionError);

            action.ProcessingId = null;
            action.ProcessingType = null;
            action.ActionType = ContractActionType.Prepayment;
            _contractActionCheckService.ContractActionCheck(action);
            var contract = _contractService.Get(action.ContractId);
            if (contract == null)
                throw new PawnshopApplicationException($"Договор {action.ContractId} не найден");

            if (!contract.SettingId.HasValue && contract.Status != ContractStatus.Signed)
                throw new PawnshopApplicationException($"Договор {contract.ContractNumber} должен быть подписан");

            var incompleteExists = await _contractActionService.IncopleteActionExists(action.ContractId);
            if (incompleteExists)
                throw new PawnshopApplicationException("В договоре имеется неподтвержденное действие");

            if (contract.SettingId.HasValue
                && contract.Setting.InitialFeeRequired.HasValue
                && contract.Setting.InitialFeeRequired.Value != 0
                && contract.Setting.InitialFeeRequired <= 0
                && contract.Status != ContractStatus.AwaitForInitialFee)
                throw new InvalidOperationException("Обнаружен невозможный случай, обратитесь в тех. поддержку");

            if (_branchContext?.Branch?.Name?.Contains("TSO", StringComparison.InvariantCultureIgnoreCase) ?? true)
                throw new PawnshopApplicationException("Некорректный филиал для поступления аванса");

            if (!action.IsInitialFee.HasValue && contract.Actions.Any() && action.Date.Date < contract.Actions.Max(x => x.Date).Date)
                throw new PawnshopApplicationException(_badDateError);

            if (action.Data == null)
                action.Data = new ContractActionData();

            action.EmployeeId = action.EmployeeId > 0 ? action.EmployeeId : null;
            using var transaction = _contractRepository.BeginTransaction();
            action.AuthorId = _sessionContext.UserId;
            action.CreateDate = DateTime.Now;
            int authorId = _sessionContext.UserId;
            int branchId = _branchContext.Branch.Id;
            ContractAction additionalPrepaymentAction = null;
            ContractActionRow[] contractActionRows = action.Rows;
            decimal calculatedAmount = action.TotalCost;
            decimal amount = action.TotalCost;
            if (contract.Status == ContractStatus.Signed)
            {
                if (contractActionRows.Length > 1)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(contractActionRows)} будет только один элемент");

                ContractActionRow contractActionRow = contractActionRows.Single();
                contractActionRow.Cost = action.TotalCost;
            }
            else
            {
                if (contractActionRows.Length > 2)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(contractActionRows)} будет только два элемента");

                Account depoAccount = _accountService.GetByAccountSettingCode(contract.Id, Constants.ACCOUNT_SETTING_DEPO);
                if (depoAccount == null)
                    throw new PawnshopApplicationException($"По договору {contract.Id} не найден счет по настройке {Constants.ACCOUNT_SETTING_DEPO}");

                Account depoMerchantAccount = _accountService.GetByAccountSettingCode(contract.Id, Constants.ACCOUNT_SETTING_DEPO_MERCHANT);
                if (depoMerchantAccount == null)
                    throw new PawnshopApplicationException($"По договору {contract.Id} не найден счет по настройке {Constants.ACCOUNT_SETTING_DEPO_MERCHANT}");

                ContractActionRow depoActionRow = contractActionRows.Where(r => r.CreditAccountId == depoAccount.Id).FirstOrDefault();
                if (depoActionRow == null)
                    throw new PawnshopApplicationException($"{nameof(contractActionRows)} должен был содержать row с кредитным счетом {depoAccount.Id}");

                ContractActionRow depoMerchantActionRow = contractActionRows.Where(r => r.CreditAccountId == depoMerchantAccount.Id).FirstOrDefault();
                if (depoMerchantActionRow == null)
                    throw new PawnshopApplicationException($"{nameof(contractActionRows)} должен был содержать row с кредитным счетом {depoMerchantAccount.Id}");

                decimal depoMerchantBalance = _contractService.GetDepoMerchantBalance(contract.Id, action.Date);
                if (!contract.RequiredInitialFee.HasValue)
                    throw new PawnshopApplicationException($"Договор {contract.Id} должен содержать {nameof(contract.RequiredInitialFee)}");

                decimal requiredInitialFee = contract.RequiredInitialFee.Value;
                if (depoMerchantBalance > requiredInitialFee)
                    throw new PawnshopApplicationException($"Договор {contract.Id} уже содержит необходимый первоначальный взнос, но его статус остался 'Ожидает первоначального взноса'");

                decimal depoMerchantBalanceAndAmount = depoMerchantBalance + amount;
                decimal calculatedDepoMerchantAmount = 0;
                if (depoMerchantBalanceAndAmount > requiredInitialFee)
                {
                    decimal initialFeeDifference = depoMerchantBalanceAndAmount - requiredInitialFee;
                    calculatedDepoMerchantAmount = amount - initialFeeDifference;
                }
                else
                {
                    calculatedDepoMerchantAmount = amount;
                }

                calculatedAmount = calculatedDepoMerchantAmount;
                depoMerchantActionRow.Cost = calculatedAmount;
                depoActionRow.Cost = calculatedAmount;
                if (calculatedDepoMerchantAmount < amount)
                {
                    decimal additionaPrepaymentSum = amount - calculatedDepoMerchantAmount;
                    ContractActionRow depoActionRowClone = new ContractActionRow
                    {
                        CreditAccountId = depoActionRow.CreditAccountId,
                        DebitAccountId = depoActionRow.DebitAccountId,
                        BusinessOperationSettingId = depoActionRow.BusinessOperationSettingId,
                        Cost = additionaPrepaymentSum,
                        LoanSubjectId = depoActionRow.LoanSubjectId,
                        OriginalPercent = depoActionRow.OriginalPercent,
                        PaymentType = depoActionRow.PaymentType,
                        Period = depoActionRow.Period,
                        Percent = depoActionRow.Percent,
                    };

                    var additionalPrepaymentActionRows = new ContractActionRow[] { depoActionRowClone };
                    additionalPrepaymentAction = new ContractAction
                    {
                        ActionType = ContractActionType.Prepayment,
                        AuthorId = authorId,
                        ContractId = contract.Id,
                        Cost = additionaPrepaymentSum,
                        TotalCost = additionaPrepaymentSum,
                        Rows = additionalPrepaymentActionRows,
                        Discount = action.Discount,
                        Reason = $"Поступление аванса по договору {contract.ContractNumber} от {action.Date:dd.MM.yyyy}",
                        Date = action.Date,
                        PayTypeId = action.PayTypeId,
                        IsInitialFee = false,
                        Data = new ContractActionData(),
                        EmployeeId = action.EmployeeId
                    };
                }
            }

            action.Cost = calculatedAmount;
            action.TotalCost = calculatedAmount;

            //если это Прием денег на расчетный счет то апрувим ордер автоматом.
            if (action.PayTypeId == 5)
                _contractActionOperationService.Register(contract, action, authorId, branchId: branchId, orderStatus: OrderStatus.Approved);
            else
                _contractActionOperationService.Register(contract, action, authorId, branchId: branchId);

            if (additionalPrepaymentAction != null)
            {
                additionalPrepaymentAction.ParentActionId = action.Id;
                _contractActionOperationService.Register(contract, additionalPrepaymentAction, authorId, branchId: branchId);
                additionalPrepaymentAction.ParentAction = action;
                action.ChildActionId = additionalPrepaymentAction.Id;
                _contractActionService.Save(action);
            }

            contract = _contractRepository.Get(action.ContractId);
            if (contract == null)
                throw new PawnshopApplicationException($"Договор {action.ContractId} не найден");

            if (action.IsInitialFee == true)
            {
                if (!contract.RequiredInitialFee.HasValue || contract.Status != ContractStatus.AwaitForInitialFee)
                    throw new PawnshopApplicationException("По договору не рассчитана сумма обязательного первоначального взноса, сделайте сначала операцию, которая рассчитывает и разрешает внесение первоначального взноса");

                decimal depoMerchantBalance = _contractService.GetDepoMerchantBalance(contract.Id, action.Date);
                if (contract.RequiredInitialFee <= depoMerchantBalance)
                {
                    contract.Status = ContractStatus.PositionRegistration;
                    contract.PayedInitialFee = depoMerchantBalance;
                }
            }

            _contractRepository.Update(contract);
            transaction.Commit();
            return additionalPrepaymentAction ?? action;
        }


        [HttpPost("/api/contractAction/prepaymentReturn"), ProducesResponseType(typeof(ContractAction), 200)]
        [Event(EventCode.PrepaymentReturn, EventMode = EventMode.All, EntityType = EntityType.Contract, IncludeFails = true)]
        public async Task<IActionResult> PrepaymentReturn([FromBody] ContractAction action)
        {
            ModelState.Validate();
            if (action.ContractId <= 0)
                throw new PawnshopApplicationException("Идентификатор договора должен быть больше нуля");

            if (action.TotalCost <= 0)
                throw new PawnshopApplicationException("Укажите сумму возвращаемого аванса");

            if ((action.Date.Date != DateTime.Now.Date)
                && (!(_sessionContext.Permissions.Where(x => x == Permissions.ContractDiscount).Count() > 0)
                && !(_sessionContext.Permissions.Where(x => x == Permissions.PrepaymentWithNotification).Count() > 0)
                && !_sessionContext.ForSupport))
                throw new PawnshopApplicationException("У вас недостаточно прав для проведения действий другим числом. Обратитесь к администратору.");

            if (action.Files == null || action.Files.Count <= 0)
                throw new PawnshopApplicationException("Не найдены обязательные файлы");

            if (action.Rows == null || action.Rows.Length <= 0)
                throw new PawnshopApplicationException("Не найдены обязательные проводки(ROWS)");

            action.ProcessingId = null;
            action.ProcessingType = null;
            action.ActionType = ContractActionType.PrepaymentReturn;
            action.Status = ContractActionStatus.Await;
            _contractActionCheckService.ContractActionCheck(action);

            var contract = _contractService.Get(action.ContractId);

            _contractService.AllowContractPrepaymentReturn(contract);

            action.Data ??= new ContractActionData();
            action.Data.PrepaymentUsed = action.TotalCost;

            using var transaction = _contractRepository.BeginTransaction();
            int branchId = _branchContext.Branch.Id;
            int authorId = _sessionContext.UserId;
            _contractActionOperationService.Register(contract, action, authorId, branchId: branchId, orderStatus: OrderStatus.WaitingForConfirmation);

            action.AuthorId = _sessionContext.UserId;
            action.CreateDate = DateTime.Now;
            _contractActionService.Save(action);
            transaction.Commit();

            return Ok(action);
        }

        [HttpPost("/api/contractAction/payment"), ProducesResponseType(typeof(ContractAction), 200)]
        [Event(EventCode.Payment, EventMode = EventMode.All, EntityType = EntityType.Contract, IncludeFails = true)]
        public async Task<IActionResult> Payment([FromBody] ContractAction action, [FromServices] FunctionSettingRepository functionSettingRepository)
        {
            var depoMasteringSetting = functionSettingRepository.GetByCode(Constants.FUNCTION_SETTING__DEPO_MASTERING);

            if (depoMasteringSetting != null && depoMasteringSetting.BooleanValue.HasValue && depoMasteringSetting.BooleanValue.Value)
            {
                throw new PawnshopApplicationException("Функционал временно отключен. Внесите деньги на аванс договора.");
            }

            ModelState.Validate();
            if (!action.PayTypeId.HasValue)
                throw new PawnshopApplicationException("Тип платежа обязателен");

            if (action.TotalCost < 0)
                throw new PawnshopApplicationException("Укажите сумму");

            action.ActionType = ContractActionType.Payment;
            if (action.ExtraExpensesIds == null)
                throw new PawnshopApplicationException($"{nameof(action)}.{nameof(action.ExtraExpensesIds)} не должен быть null");

            Contract contract = _contractService.Get(action.ContractId);
            if (contract == null)
                throw new PawnshopApplicationException("Договор {contract.Id} не найден");

            var incompleteExists = await _contractActionService.IncopleteActionExists(contract.Id);
            if (incompleteExists)
                throw new PawnshopApplicationException("В договоре имеется неподтвержденное действие");

            int authorId = _sessionContext.UserId;
            int branchId = _branchContext.Branch.Id;
            decimal prepaymentBalance = _contractService.GetPrepaymentBalance(action.ContractId, action.Date);
            decimal costByRowsAndExtraExpenses = action.TotalCost;
            if (action.ExtraExpensesCost.HasValue)
                costByRowsAndExtraExpenses += action.ExtraExpensesCost.Value;

            action.Cost = costByRowsAndExtraExpenses;
            decimal prepaymentCost = 0;
            List<ContractExpense> extraExpensesCostForPayment = _contractExpenseOperationService.GetNeededExpensesForPrepayment(contract.Id, branchId);
            decimal extraExpensesSum = extraExpensesCostForPayment.Count > 0 ? extraExpensesCostForPayment.Sum(e => e.TotalCost) : 0;
            decimal prepaymentAndTotalCostDifference = prepaymentBalance - action.TotalCost - extraExpensesSum;

            if (prepaymentAndTotalCostDifference < 0)
                prepaymentCost = Math.Ceiling(Math.Abs(prepaymentAndTotalCostDifference));
            ContractAction prepaymentAction = null;
            using (IDbTransaction transaction = _contractRepository.BeginTransaction())
            {
                if (prepaymentCost > 0)
                {
                    prepaymentAction = _contractActionPrepaymentService.Exec(contract.Id, prepaymentCost, action.PayTypeId.Value, branchId, authorId, date: action.Date, orderStatus: OrderStatus.WaitingForApprove);
                    if (prepaymentAction == null)
                        throw new PawnshopApplicationException($"Ожидалось что {nameof(_contractActionPrepaymentService)}.{nameof(_contractActionPrepaymentService.Exec)} не вернет null");
                }

                _contractPaymentService.Payment(action, branchId, authorId, forceExpensePrepaymentReturn: false, autoApprove: false);

                var contractStatus = _contractService.GetOnlyContract(action.ContractId).Status;

                if (contractStatus == ContractStatus.BoughtOut)
                    _contractCloseService.Exec(contract, action.Date, authorId, prepaymentAction ?? action);

                if (prepaymentAction != null)
                {
                    prepaymentAction.ChildActionId = action.Id;
                    _contractActionService.Save(prepaymentAction);
                    action.ParentActionId = prepaymentAction.Id;
                    action.ParentAction = prepaymentAction;
                }

                _contractActionService.Save(action);

                transaction.Commit();
                return Ok(action);
            }
        }

        [HttpPost("/api/contractAction/print"), ProducesResponseType(typeof(List<CashOrder>), 200)]
        [Event(EventCode.ContractActionPrint, EventMode = EventMode.All, EntityType = EntityType.Contract, IncludeFails = true)]
        public async Task<IActionResult> Print([FromBody] ContractAction action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            action.ProcessingId = null;
            action.ProcessingType = null;
            List<CashOrder> result = new List<CashOrder>();
            ContractAction savedAction = _contractActionService.GetAsync(action.Id).Result;
            if (savedAction.DeleteDate.HasValue)
                throw new PawnshopApplicationException("Нельзя распечатывать удаленное действие");

            result.AddRange(await CashAccountOrders(savedAction));

            if (savedAction.ChildActionId.HasValue)
            {
                ContractAction сhildAction = _contractActionService.GetAsync(savedAction.ChildActionId.Value).Result;
                result.AddRange(await CashAccountOrders(сhildAction));
            }

            if (savedAction.ParentActionId.HasValue)
            {
                ContractAction parentAction = _contractActionService.GetAsync(savedAction.ParentActionId.Value).Result;
                result.AddRange(await CashAccountOrders(parentAction));
            }

            return Ok(result);
        }

        private async Task<List<CashOrder>> CashAccountOrders(ContractAction action)
        {
            List<CashOrder> result = new List<CashOrder>();
            foreach (var row in action.Rows)
            {
                var order = await _cashOrderService.GetAsync(row.OrderId);
                if (order.OrderType == OrderType.CashIn || order.OrderType == OrderType.CashOut)
                    result.Add(_cashOrderRepository.Get(row.OrderId));
            }

            if (action.Expense != null)
            {
                result.AddRange(CashAccountOrdersExpense(action.Expense, action.Id));
            }

            if (action.ExtraExpensesCost > 0)
            {
                var contract = _contractService.Get(action.ContractId);
                foreach (var expense in contract.Expenses.Where(x => x.ContractExpenseRows.Select(r => r.ActionId).Contains(action.Id)))
                {
                    if (_expenseService.Get(expense.ExpenseId).ExtraExpense)
                        result.AddRange(CashAccountOrdersExpense(expense, action.Id));
                }
            }

            return result;
        }

        [HttpPost("/api/contractAction/printExpense"), ProducesResponseType(typeof(List<CashOrder>), 200)]
        [Event(EventCode.ContractActionPrint, EventMode = EventMode.All, EntityType = EntityType.Contract, IncludeFails = true)]
        public async Task<IActionResult> PrintExpense([FromBody] ContractExpense expense)
        {
            if (expense == null)
                throw new ArgumentNullException(nameof(expense));
            List<CashOrder> result = new List<CashOrder>();
            ContractExpense savedExpense = _contractExpenseRepository.Get(expense.Id);
            //if (savedExpense == null) return result.ToList(); 
            if (savedExpense.DeleteDate.HasValue)
                throw new PawnshopApplicationException("Нельзя распечатывать удаленное действие");

            result.AddRange(CashAccountOrdersExpense(savedExpense));

            return Ok(result);
        }

        private List<CashOrder> CashAccountOrdersExpense(ContractExpense expense, int? actionId = null)
        {
            if (expense == null)
                throw new ArgumentNullException(nameof(expense));

            if (expense.ContractExpenseRows == null)
                throw new ArgumentException($"Поле {nameof(expense.ContractExpenseRows)} не должно быть null", nameof(expense));

            expense.ContractExpenseRows = _contractExpenseRowRepository.GetByContractExpenseId(expense.Id).Where(t => t.ActionId == actionId).ToList();
            var requiredOrderTypes = new HashSet<OrderType> { OrderType.CashIn, OrderType.CashOut };
            List<CashOrder> result = new List<CashOrder>();
            foreach (ContractExpenseRow contractExpenseRow in expense.ContractExpenseRows)
            {
                List<ContractExpenseRowOrder> contractExpenseRowOrders =
                    _contractExpenseRowOrderRepository.GetByContractExpenseRowId(contractExpenseRow.Id);
                contractExpenseRow.ContractExpenseRowOrders = contractExpenseRowOrders;
                if (contractExpenseRow.ContractExpenseRowOrders == null)
                    throw new PawnshopApplicationException(
                        $"{nameof(contractExpenseRow)}.{nameof(contractExpenseRow.ContractExpenseRowOrders)} не должен быть null");

                if (contractExpenseRow.ContractExpenseRowOrders.Any(o => o.Order == null))
                    throw new PawnshopApplicationException(
                        $"{nameof(contractExpenseRow)}.{nameof(contractExpenseRow.ContractExpenseRowOrders)} содержит пустые кассовые ордера");

                IEnumerable<CashOrder> requiredOrders = contractExpenseRow.ContractExpenseRowOrders
                    .Where(o => requiredOrderTypes.Contains(o.Order.OrderType)).Select(o => o.Order);

                foreach (CashOrder requiredOrder in requiredOrders)
                {
                    result.Add(_cashOrderRepository.Get(requiredOrder.Id));
                }
            }

            return result;
        }

        [HttpPost("/api/contractAction/printForContract"), ProducesResponseType(typeof(CashOrder), 200)]
        //[Event(EventCode.ContractActionPrint, EventMode = EventMode.All, EntityType = EntityType.Contract, IncludeFails = true)]
        public async Task<IActionResult> PrintForContract([FromBody] int contractId)
        {
            if (contractId <= 0)
                throw new ArgumentNullException(nameof(contractId));
            var contract = _contractService.Get(contractId);

            var action = contract.Actions.Where(x => x.ActionType == ContractActionType.Sign).FirstOrDefault();
            if (action == null)
                throw new ArgumentNullException("Договор не подписан");
            ContractAction savedAction = _contractActionService.GetAsync(action.Id).Result;
            if (savedAction.DeleteDate.HasValue)
                throw new PawnshopApplicationException("Нельзя распечатывать удаленное действие");
            if (savedAction.ActionType != ContractActionType.Sign)
                throw new PawnshopApplicationException("Действие не является подписанием");

            var row = savedAction.Rows.Where(x =>
                                             x.PaymentType == AmountType.Debt &&
                                             (x.DebitAccountId == _branchContext.Configuration.CashOrderSettings.CashAccountId
                                             || x.CreditAccountId == _branchContext.Configuration.CashOrderSettings.CashAccountId))
                                .FirstOrDefault();
            if (row != null)
            {
                return Ok(_cashOrderRepository.Get(row.OrderId));
            }
            else
                return Ok();
        }

        [HttpPost("/api/contractAction/refinanceCheck"), ProducesResponseType(typeof(ContractAction), 200)]
        public async Task<IActionResult> RefinanceCheck([FromBody] ContractRefinanceCheck check)
        {
            //Блокировка функционала до лучших времен
            throw new PawnshopApplicationException("Функционал временно недоступен");
        }

        [HttpPost("/api/contractAction/refinance"), ProducesResponseType(typeof(ContractAction), 200)]
        [Event(EventCode.ContractRefinance, EventMode = EventMode.Response, EntityType = EntityType.Contract,
            IncludeFails = true)]
        public async Task<IActionResult> Refinance([FromBody] ContractAction action)
        {
            //Блокировка функционала до лучших времен
            throw new PawnshopApplicationException("Функционал временно недоступен");
        }

        [HttpPost("/api/contractAction/registerContractExpense"), ProducesResponseType(typeof(ContractExpense), 200)]
        [Event(EventCode.ContractExpenseSave, EventMode = EventMode.All, EntityType = EntityType.Contract,
            IncludeFails = true)]
        public async Task<IActionResult> RegisterContractExpense([FromBody] ContractExpense expense)
        {
            if (expense == null)
                throw new PawnshopApplicationException($"{nameof(expense)} не должен быть null");

            if (expense.ExpenseId <= 0)
                throw new PawnshopApplicationException("Не выбран вид расхода");
            if (expense.TotalCost < 0)
                throw new PawnshopApplicationException("Сумма не может быть отрицательной");

            int authorId = _sessionContext.UserId;
            int branchId = _branchContext.Branch.Id;
            await _contractExpenseOperationService.RegisterAsync(expense, authorId, branchId, forcePrepaymentReturn: false, orderStatus: OrderStatus.WaitingForConfirmation);
            return Ok(expense);
        }

        [HttpPost("/api/contractAction/openContractForInitialFee"), ProducesResponseType(typeof(Contract), 200)]
        [Event(EventCode.InitialFeeOpen, EventMode = EventMode.All, EntityType = EntityType.Contract,
                IncludeFails = true)]
        public async Task<IActionResult> OpenContractForInitialFee([FromBody] int id)
        {
            if (id <= 0)
                throw new ArgumentOutOfRangeException(nameof(id));

            var contract = _contractService.Get(id);
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            if (!contract.ProductTypeId.HasValue)
                throw new PawnshopApplicationException("Не выбран вид продукта кредитования");
            if (!contract.SettingId.HasValue)
                throw new PawnshopApplicationException("Не выбран продукт кредитования");

            if (contract.ProductTypeId.HasValue && contract.ProductType.Code != "BUYCAR")
                throw new PawnshopApplicationException(
                    "Выбранный вид продукта кредитования не предполагает предварительного взноса");
            if (!contract.Setting.InitialFeeRequired.HasValue || contract.Setting.InitialFeeRequired <= 0)
                throw new PawnshopApplicationException(
                    "По выбранному продукту кредитования первоначальный взнос не требуется!");

            if (contract.EstimatedCost <= 0)
                throw new PawnshopApplicationException("Оценка не может быть меньше или равной 0");
            if (contract.LoanCost <= 0)
                throw new PawnshopApplicationException("Ссуда не может быть равной 0");

            if (contract.Positions != null && contract.Positions.Count > 0)
            {
                if (contract.Positions.Any(x => x.EstimatedCost <= 0))
                    throw new PawnshopApplicationException("Оценка позиции не может быть меньше или равной 0");
                if (contract.Positions.Any(x => x.LoanCost <= 0))
                    throw new PawnshopApplicationException("Ссуда позиции не может быть меньше или равной 0");
            }

            contract.Status = ContractStatus.AwaitForInitialFee;
            contract.MinimalInitialFee =
                contract.EstimatedCost * ((decimal)contract.Setting.InitialFeeRequired / 100);
            contract.PayedInitialFee = 0;

            decimal depoMerchantBalance = _contractService.GetDepoMerchantBalance(contract.Id, DateTime.Now);
            if (depoMerchantBalance >= contract.RequiredInitialFee)
                contract.Status = ContractStatus.PositionRegistration;

            using (var transaction = _contractRepository.BeginTransaction())
            {
                _contractRepository.Update(contract);
                transaction.Commit();
            }

            return Ok(contract);
        }

        [HttpPost("/api/contractAction/afterPositionRegistration"), ProducesResponseType(typeof(Contract), 200)]
        [Event(EventCode.PositionRegistration, EventMode = EventMode.All, EntityType = EntityType.Contract,
            IncludeFails = true)]
        public async Task<IActionResult> AfterPositionRegistration([FromBody] ContractAction action)
        {
            if (action == null)
                throw new ArgumentOutOfRangeException(nameof(action));
            if (action.ContractId <= 0)
                throw new ArgumentOutOfRangeException(nameof(action.ContractId));

            var contract = _contractService.Get(action.ContractId);
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            if (contract.CollateralType != CollateralType.Car)
                throw new PawnshopApplicationException(
                    $"Только для договоров с видом залога \"{CollateralType.Car.GetDisplayName()}\"");

            if (!contract.ProductTypeId.HasValue)
                throw new PawnshopApplicationException("Не выбран вид продукта кредитования");
            if (!contract.SettingId.HasValue)
                throw new PawnshopApplicationException("Не выбран продукт кредитования");

            if (contract.ProductTypeId.HasValue && contract.ProductType.Code != "BUYCAR")
                throw new PawnshopApplicationException(
                    "Выбранный вид продукта кредитования не предполагает перерегистрации/переоформления залога");
            if (action.Cars == null)
                throw new PawnshopApplicationException("Не найдены данные о авто в действии");
            if (action.Cars.Count != contract.Positions.Count)
                throw new PawnshopApplicationException("Количество позиций в действии и в договоре отличаются");

            if (contract.Status != ContractStatus.Reissued)
            {
                contract.SignDate = action.Date;
                contract.FirstPaymentDate = contract.FirstPaymentDate?.AddDays((contract.SignDate.Value - contract.ContractDate).Days);

                _paymentScheduleService.BuildWithContract(contract);

                foreach (var position in contract.Positions)
                {
                    Car changedCar = action.Cars.FirstOrDefault(x => x.Id == position.PositionId);
                    if (changedCar == null)
                        throw new PawnshopApplicationException(
                            $"Позиция {position.Position.Name} не найдена в договоре ");

                    var car = (Car)position.Position;

                    car.TransportNumber = changedCar.TransportNumber;
                    car.BodyNumber = changedCar.BodyNumber;
                    car.Name = changedCar.Name;
                    car.TechPassportDate = changedCar.TechPassportDate;
                    car.TechPassportNumber = changedCar.TechPassportNumber;

                    position.Position = car;
                }

                contract.Status = ContractStatus.AwaitForSign;

                var setting = _contractService.GetSettingForContract(contract);
                if (setting.IsInsuranceAvailable)
                {
                    var latestRequest = _insurancePoliceRequestService.GetLatestPoliceRequest(contract.Id);

                    if (latestRequest != null && latestRequest.IsInsuranceRequired)
                        contract.Status = ContractStatus.AwaitForInsuranceSend;
                }

                using (var transaction = _contractRepository.BeginTransaction())
                {
                    contract.Positions.ForEach(position =>
                    {
                        try
                        {
                            _carRepository.Update((Car)position.Position);
                        }
                        catch (SqlException e)
                        {
                            if (e.Number == 2627)
                                throw new PawnshopApplicationException(
                                    "Позиция с такими данными (гос. номер, техпаспорт) уже существует");
                            throw new PawnshopApplicationException(e.Message);
                        }
                    });

                    _contractRepository.Update(contract);
                    transaction.Commit();
                }
            }
            else
            {
                using (var transaction = _contractRepository.BeginTransaction())
                {
                    contract.Status = ContractStatus.AwaitForSign;

                    var setting = _contractService.GetSettingForContract(contract);

                    if (setting.IsInsuranceAvailable)
                    {
                        var latestRequest = _insurancePoliceRequestService.GetLatestPoliceRequest(contract.Id);
                        if (latestRequest != null && latestRequest.IsInsuranceRequired)
                            contract.Status = ContractStatus.AwaitForInsuranceSend;
                    }

                    _contractRepository.Update(contract);
                    transaction.Commit();
                }
            }

            return Ok(contract);
        }

        [HttpPost("/api/contractAction/addRealtySignDate"), ProducesResponseType(typeof(Contract), 200)]
        [Event(EventCode.ContractAddSignDate, EventMode = EventMode.All, EntityType = EntityType.Contract,
    IncludeFails = true)]
        public async Task<IActionResult> AddRealtySignDate([FromBody] ContractAction action)
        {
            if (action == null)
                throw new ArgumentOutOfRangeException(nameof(action));
            if (action.ContractId <= 0)
                throw new ArgumentOutOfRangeException(nameof(action.ContractId));
            if (action.Date == null)
                throw new PawnshopApplicationException("Не указана дата подписания");
            if (action.Date.Date != DateTime.Today)
                throw new PawnshopApplicationException("Дата подписания не соответствует с сегодняшним днем");

            var contract = _contractService.Get(action.ContractId);
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            if (contract.CollateralType != CollateralType.Realty)
                throw new PawnshopApplicationException(
                    $"Только для договоров с видом залога \"{CollateralType.Realty.GetDisplayName()}\"");

            if (!contract.ProductTypeId.HasValue)
                throw new PawnshopApplicationException("Не выбран вид продукта кредитования");
            if (!contract.SettingId.HasValue)
                throw new PawnshopApplicationException("Не выбран продукт кредитования");

            contract.SignDate = action.Date;
            _paymentScheduleService.BuildWithContract(contract);
            contract.FirstPaymentDate = contract.PaymentSchedule.FirstOrDefault().Date;

            contract.MaturityDate = contract.PaymentSchedule.Max(x => x.Date);

            contract.NextPaymentDate = contract.PercentPaymentType == PercentPaymentType.EndPeriod
            ? contract.MaturityDate
            : contract.PaymentSchedule.Where(x => x.ActionId == null && x.Canceled == null).Min(x => x.Date);

            contract.Status = ContractStatus.AwaitForConfirmation;

            using (var transaction = _contractRepository.BeginTransaction())
            {
                _contractService.Save(contract);
                _contractPaymentScheduleService.Save(contract.PaymentSchedule, contract.Id, Constants.ADMINISTRATOR_IDENTITY);
                await _contractStatusHistoryService.SaveToStatusChangeHistory(contract.Id, contract.Status, action.Date, _sessionContext.UserId, true);
                transaction.Commit();
            }


            return Ok();
        }

        [HttpPost("/api/contractAction/confirmContract"), Authorize(Permissions.RealtyContractConfirm)]
        [Event(EventCode.ContractConfirmed, EventMode = EventMode.All, EntityType = EntityType.Contract,
    IncludeFails = true)]
        public async Task<IActionResult> ConfirmContract([FromBody] ContractAction action)
        {
            if (action == null)
                throw new ArgumentOutOfRangeException(nameof(action));
            if (action.ContractId <= 0)
                throw new ArgumentOutOfRangeException(nameof(action.ContractId));
            if (action.Date == null)
                throw new PawnshopApplicationException("Не указана дата согласования");
            if (action.Date.Date != DateTime.Today && !_sessionContext.ForSupport)
                throw new PawnshopApplicationException("Дата согласования не соответствует сегодняшним днем");

            var contract = _contractService.Get(action.ContractId);

            if (contract.CollateralType != CollateralType.Realty)
                throw new PawnshopApplicationException($"Только для договоров с видом залога \"{CollateralType.Realty.GetDisplayName()}\"");

            if (!contract.SignDate.HasValue)
                throw new PawnshopApplicationException("Не указана дата выдачи для договора недвижимости");

            if (!contract.ProductTypeId.HasValue)
                throw new PawnshopApplicationException("Не выбран вид продукта кредитования");
            if (!contract.SettingId.HasValue)
                throw new PawnshopApplicationException("Не выбран продукт кредитования");

            contract.Status = ContractStatus.Confirmed;

            using (var transaction = _contractRepository.BeginTransaction())
            {
                _contractService.Save(contract);
                await _contractStatusHistoryService.SaveToStatusChangeHistory(contract.Id, contract.Status, action.Date, _sessionContext.UserId, true);
                transaction.Commit();
            }

            return Ok();
        }


        public async Task<IActionResult> CreateInnerNotificationToOtherGroup(Contract contract, ContractAction action)
        {
            string message;
            switch (action.ActionType)
            {
                case ContractActionType.Prolong:
                    message = $@"Ваш договор {contract.ContractNumber} был продлен в филиале {_branchContext.Branch.DisplayName} на сумму {action.TotalCost}. Сделайте необходимые действия с кассовым аппаратом.";
                    break;
                case ContractActionType.Buyout:
                case ContractActionType.BuyoutRestructuringCred:
                    message = $@"Ваш договор {contract.ContractNumber} был выкуплен в филиале {_branchContext.Branch.DisplayName} на сумму {action.TotalCost}. Сделайте необходимые действия с кассовым аппаратом.";
                    break;
                case ContractActionType.MonthlyPayment:
                    message = $@"По договору {contract.ContractNumber} был принят ежемесячный платеж в филиале {_branchContext.Branch.DisplayName} на сумму {action.TotalCost}. Сделайте необходимые действия с кассовым аппаратом.";
                    break;
                case ContractActionType.Refinance:
                    message = $@"Ваш договор {contract.ContractNumber} был рефинансирован в филиале {_branchContext.Branch.DisplayName}. Сделайте необходимые действия с кассовым аппаратом.";
                    break;
                default:
                    message = $@"Договор {contract.ContractNumber} - действие {action.ActionType.GetDisplayName()} в филиале {_branchContext.Branch.DisplayName} на сумму {action.TotalCost} тг";
                    break;
            }

            var notification = new InnerNotification
            {
                Id = 0,
                CreateDate = DateTime.Now,
                CreatedBy = _sessionContext.UserId,
                EntityType = EntityType.Contract,
                EntityId = contract.Id,
                Message = message,
                ReceiveBranchId = contract.BranchId,
                Status = InnerNotificationStatus.Sent
            };
            _innerNotificationRepository.Insert(notification);
            return Ok(notification);
        }

        [HttpPost("/api/contractAction/interestAccrualOnOverdueDebt")]
        public async Task<IActionResult> InterestAccrualOnOverdueDebt([FromBody] ManualAccrualModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (model.Id <= 0)
                throw new ArgumentOutOfRangeException(nameof(model.Id));

            var contract = await Task.Run(() => _contractService.Get(model.Id));
            //Проверка на наличие начислений согласно правила начисления на конец месяца и контрольной даты 
            #region
            //var defaultDate = contract.ContractDate;

            //DateTime lastInterestAccrualOnOverdueDebtActionDate = contract.Actions.LastOrDefault(x => x.ActionType == ContractActionType.InterestAccrualOnOverdueDebt)?.Date ?? defaultDate;
            //DateTime lastInterestAccrualActionDate = contract.Actions.LastOrDefault(x => (x.ActionType == ContractActionType.InterestAccrual && x.DeleteDate == null))?.Date ?? defaultDate;
            //if (lastInterestAccrualActionDate> lastInterestAccrualOnOverdueDebtActionDate)
            //{
            //    lastInterestAccrualOnOverdueDebtActionDate = lastInterestAccrualActionDate;
            //}

            //DateTime targetDate = contract.NextPaymentDate ?? contract.PaymentSchedule.FirstOrDefault(x => x.ActualDate == null && x.Canceled == null)?.Date ?? defaultDate;
            //List<DateTime> lastDaysOfMonths = new List<DateTime>();
            //for (DateTime date = lastInterestAccrualOnOverdueDebtActionDate; date <= model.Date; date = date.AddMonths(1))
            //{
            //    lastDaysOfMonths.Add(new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month)));
            //}

            //foreach (var lastDayOfMonth in lastDaysOfMonths)
            //{
            //    DateTime minDate = DateTime.MinValue;
            //    DateTime maxDate = DateTime.MinValue;

            //    if (lastDayOfMonth > contract.NextPaymentDate)
            //    {
            //        maxDate = lastDayOfMonth;
            //        minDate = targetDate;
            //    }
            //    else
            //    {
            //        maxDate = targetDate;
            //        minDate = lastDayOfMonth;
            //    }

            //    if (minDate > lastInterestAccrualOnOverdueDebtActionDate)
            //    {
            //        if (minDate < model.Date)
            //            throw new PawnshopApplicationException($@"Необходимо сперва начислить на {minDate.ToString("dd/MM/yyyy")} дату согласно правила начисления на конец месяца и контрольной даты");
            //    }
            //    else if (maxDate > lastInterestAccrualOnOverdueDebtActionDate)
            //    {
            //        if (maxDate < model.Date)
            //            throw new PawnshopApplicationException($@"Необходимо сперва начислить на {maxDate.ToString("dd/MM/yyyy")} дату согласно правила начисления на конец месяца и контрольной даты");
            //    }
            //}
            #endregion

            _interestAccrualService.ManualInterestAccrualOnOverdueDebt(contract, _sessionContext.UserId, model.Date);

            return Ok();
        }

        [HttpPost("/api/contractAction/interestAccrual")]
        public async Task<IActionResult> InterestAccrual([FromBody] ManualAccrualModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (model.Id <= 0)
                throw new ArgumentOutOfRangeException(nameof(model.Id));

            var contract = await Task.Run(() => _contractService.Get(model.Id));
            if (DateTime.Now.Date == contract.SignDate.Value.Date)
                throw new PawnshopApplicationException("Нельзя начислять проценты в день подписания!");

            //Проверка на наличие начислений согласно правила начисления на конец месяца и контрольной даты 
            #region
            //var defaultDate = contract.ContractDate;

            //DateTime lastInterestAccrualActionDate = contract.Actions.LastOrDefault(x => (x.ActionType == ContractActionType.InterestAccrual && x.DeleteDate == null))?.Date ?? defaultDate;
            //DateTime lastInterestAccrualOnOverdueDebtActionDate = contract.Actions.LastOrDefault(x => x.ActionType == ContractActionType.InterestAccrualOnOverdueDebt)?.Date ?? defaultDate;
            //if (lastInterestAccrualOnOverdueDebtActionDate > lastInterestAccrualActionDate)
            //{
            //    lastInterestAccrualActionDate = lastInterestAccrualOnOverdueDebtActionDate;
            //}

            //if (lastInterestAccrualActionDate == model.Date)
            //    throw new PawnshopApplicationException("На эту дату было начисление процентов!");

            //DateTime targetDate = contract.NextPaymentDate ?? contract.PaymentSchedule.FirstOrDefault(x => x.ActualDate == null && x.Canceled == null)?.Date ?? defaultDate;
            //List<DateTime> lastDaysOfMonths = new List<DateTime>();
            //for (DateTime date = lastInterestAccrualActionDate; date <= model.Date; date = date.AddMonths(1))
            //{
            //    lastDaysOfMonths.Add(new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month)));
            //}

            //foreach (var lastDayOfMonth in lastDaysOfMonths)
            //{
            //    DateTime minDate = DateTime.MinValue;
            //    DateTime maxDate = DateTime.MinValue;

            //    if (lastDayOfMonth > contract.NextPaymentDate)
            //    {
            //        maxDate = lastDayOfMonth;
            //        minDate = targetDate;
            //    }
            //    else
            //    {
            //        maxDate = targetDate;
            //        minDate = lastDayOfMonth;
            //    }

            //    if (minDate > lastInterestAccrualActionDate 
            //        && minDate < model.Date)
            //    {
            //        throw new PawnshopApplicationException($@"Необходимо сперва начислить на {minDate.ToString("dd/MM/yyyy")} дату согласно правила начисления на конец месяца и контрольной даты");
            //    }
            //    else if (maxDate > lastInterestAccrualActionDate 
            //        && maxDate < model.Date)
            //    {
            //        throw new PawnshopApplicationException($@"Необходимо сперва начислить на {maxDate.ToString("dd/MM/yyyy")} дату согласно правила начисления на конец месяца и контрольной даты");
            //    }
            //}

            //if (lastDaysOfMonths.Count == 0)
            //    throw new PawnshopApplicationException("На эту дату есть начисление!");
            #endregion



            var contractRates = await Task.Run(() => _contractRateService.FindRateOnDateByFloatingContractAndRateSettingId(contract.Id));
            var isFloatingDiscrete = false;
            if (contract.SettingId.HasValue)
            {
                var product = contract.Setting != null ? contract.Setting
                    : await Task.Run(() => _loanPercentRepository.Get(contract.SettingId.Value));
                isFloatingDiscrete = product.IsFloatingDiscrete;
            }

            _interestAccrualService.OnAnyDateAccrual(contract, _sessionContext.UserId, model.Date, isFloatingDiscrete, contractRates, contract.LeftLoanCost);

            return Ok();
        }

        [HttpPost("/api/contractAction/takeAwayToDelay")]
        public async Task<IActionResult> TakeAwayToDelay([FromBody] ManualAccrualModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (model.Id <= 0)
                throw new ArgumentOutOfRangeException(nameof(model.Id));

            var contract = await Task.Run(() => _contractService.Get(model.Id));

            var takeAwayDate = contract.PaymentSchedule
                .Where(x => !x.ActualDate.HasValue && x.Date < model.Date)
                .OrderByDescending(x => x.Date)
                .FirstOrDefault();
            if (takeAwayDate == null)
            {
                throw new PawnshopApplicationException(
                    $"Не найдена оплата по графику платежей на {model.Date:dd.MM.yyyy} ");

            }
            _takeAwayToDelayService.TakeAwayToDelay(contract, takeAwayDate.Date, model.Date.AddDays(-1), _sessionContext.UserId);

            return Ok();
        }

        [HttpPost("/api/contractAction/penaltyAccrual")]
        public async Task<IActionResult> PenaltyAccrual([FromBody] ManualAccrualModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (model.Id <= 0)
                throw new ArgumentOutOfRangeException(nameof(model.Id));

            var contract = _contractService.Get(model.Id);
            _penaltyAccrualService.Execute(contract, model.Date, _sessionContext.UserId);
            return Ok();
        }


        [HttpPost("/api/contractAction/penaltyLimitAccrual")]
        public async Task<IActionResult> PenaltyLimitAccrual([FromBody] ManualAccrualModel model)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));
            if (model.Id <= 0)
                throw new ArgumentOutOfRangeException(nameof(model.Id));

            var contract = _contractService.Get(model.Id);

            _penaltyLimitAccrualService.ManualPenaltyLimitAccrual(contract, model.Date, _sessionContext.UserId);

            return Ok();
        }

        [HttpPost("/api/contractAction/penaltyRateDecrease")]
        public async Task<IActionResult> PenaltyRateDecrease([FromBody] ManualAccrualModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (model.Id <= 0)
                throw new ArgumentOutOfRangeException(nameof(model.Id));

            var contract = _contractService.Get(model.Id);

            _penaltyRateService.IncreaseOrDecreaseRateManualy(contract, model.Date, _sessionContext.UserId, false);

            return Ok();
        }

        [HttpPost("/api/contractAction/penaltyRateIncrease")]
        public async Task<IActionResult> PenaltyRateIncrease([FromBody] ManualAccrualModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (model.Id <= 0)
                throw new ArgumentOutOfRangeException(nameof(model.Id));

            var contract = _contractService.Get(model.Id);

            _penaltyRateService.IncreaseOrDecreaseRateManualy(contract, model.Date, _sessionContext.UserId);

            return Ok();
        }

        private InnerNotification CreateInnerNotificationExpenseToOtherGroup(Contract contract,
            ContractExpense expense)
        {
            string message =
                $@"На ваш договор {contract.ContractNumber} был создан расход {_branchContext.Branch.DisplayName} на сумму {expense.TotalLeft.ToString()}. Сделайте необходимые действия с кассовым аппаратом.";

            var notification = new InnerNotification
            {
                Id = 0,
                CreateDate = DateTime.Now,
                CreatedBy = _sessionContext.UserId,
                EntityType = EntityType.Contract,
                EntityId = contract.Id,
                Message = message,
                ReceiveBranchId = contract.BranchId,
                Status = InnerNotificationStatus.Sent
            };
            _innerNotificationRepository.Insert(notification);
            return notification;
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

            if (mintosContracts == null || mintosContract == null)
                return;
            if (!mintosContract.MintosStatus.Contains("active"))
                return;

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

        [HttpGet("/api/contractAction/get")]
        public async Task<ContractAction> Get(int id)
        {
            return await Task.Run(() => _contractActionService.GetAsync(id));
        }

        [HttpGet("/api/contractAction/getfiscalchecks")]
        public async Task<IActionResult> GetFiscalChecks(int contractActionId)
        {
            var list = new List<int>();
            var relatedContractActions = _contractActionService.GetRelatedContractActionsByActionId(contractActionId).Result;
            foreach (var id in relatedContractActions)
            {
                list.AddRange(_cashOrderRepository.GetOrderIdsForFiscalCheck(id));
            }
            return Ok(list);
        }

        [HttpGet("/api/contractAction/getorders")]
        public async Task<IActionResult> GetOrders(int contractActionId)
        {
            List<CashOrder> result = new List<CashOrder>();
            ContractAction savedAction = _contractActionService.GetAsync(contractActionId).Result;
            if (savedAction.DeleteDate.HasValue)
                throw new PawnshopApplicationException("Нельзя распечатывать удаленное действие");

            var relatedContractActions = _contractActionService.GetRelatedContractActionsByActionId(contractActionId).Result;
            foreach (var id in relatedContractActions)
            {
                var relatedContractAction = _contractActionRepository.Get(id);
                result.AddRange(_cashOrderRepository.GetAllCashOrdersByContractActionId(id));
            }
            return Ok(result);
        }

        [HttpPost("/api/contractAction/testOffBalanceAddition")]
        public async Task<IActionResult> TestOffBalanceAddition([FromBody] OffBalanceAdditionTestModel model)
        {
            await _inscriptionOffBalanceAdditionService.AddOffBalanceForSpecificContract(model.ContractId, model.EndDate);
            //var testContracts = _contractRepository.GetContractsForInscriptionOffBalanceAdditionService(Constants.INSCRIPTION_SERVICE_OFFBALANCE_ACCOUNT_ADDITION_START_DATE);
            return Ok();
        }

        [HttpPost("/api/contractAction/OffBalanceAdditionForBranch")]
        public async Task<IActionResult> OffBalanceAdditionForBranch([FromBody] OffBalanceAdditionForBranchModel model)
        {
            await _inscriptionOffBalanceAdditionService.AddOffbalancePaymentForContractsWithInscription(model.BranchId, model.EndDate);
            return Ok();
        }

        private async Task<CarAuction?> GetAuction(Contract contract)
        {
            if (contract.ContractClass == ContractClass.Tranche)
            {
                return await _auctionService.GetByContractIdAsync((int)contract.CreditLineId);
            }
            
            return await _auctionService.GetByContractIdAsync(contract.Id);
        }
        
        private async Task<Guid?> SetOrderRequestId(Guid? auctionRequestId, Contract contract)
        {
            if (auctionRequestId != null)
            {
                return auctionRequestId;
            }

            var auction = await GetAuction(contract);
            return auction?.OrderRequestId;
        }
    }
}