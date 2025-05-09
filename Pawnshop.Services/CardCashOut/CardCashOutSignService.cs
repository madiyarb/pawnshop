using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.PayOperations;
using Pawnshop.Services.Refinance;
using System;
using System.Threading.Tasks;
using Pawnshop.Core;
using Pawnshop.Services.AbsOnline;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.ApplicationOnlineRefinances;
using Pawnshop.Services.Contracts;
using System.Data;
using Serilog;

namespace Pawnshop.Services.CardCashOut
{
    public sealed class CardCashOutSignService : ICardCashOutSignService
    {
        private readonly ContractRepository _contractRepository;
        private readonly GroupRepository _groupRepository;
        private readonly IRefinanceService _oldRefinanceService;
        private readonly IApplicationOnlineRefinancesService _refinancesService;
        private readonly PayOperationRepository _payOperationRepository;
        private readonly IAbsOnlineService _absOnlineService;
        private readonly IContractActionService _contractActionService;
        private readonly IContractPaymentScheduleService _contractPaymentScheduleService;
        private readonly ICashOrderService _cashOrderService;
        private readonly PayOperationActionRepository _payOperationActionRepository;
        private readonly IRefinanceBuyOutService _refinanceBuyOutService;
        private readonly ILogger _logger;

        public CardCashOutSignService(ContractRepository contractRepository,
            GroupRepository groupRepository,
            IRefinanceService oldRefinanceService,
            PayOperationRepository payOperationRepository,
            IAbsOnlineService absOnlineService,
            IContractActionService contractActionService,
            ICashOrderService cashOrderService,
            PayOperationActionRepository payOperationActionRepository,
            IRefinanceBuyOutService refinanceBuyOutService,
            IContractPaymentScheduleService contractPaymentScheduleService,
            IApplicationOnlineRefinancesService refinanceService,
            ILogger logger
        )
        {
            _contractRepository = contractRepository;
            _groupRepository = groupRepository;
            _cashOrderService = cashOrderService;
            _oldRefinanceService = oldRefinanceService;
            _payOperationRepository = payOperationRepository;
            _contractActionService = contractActionService;
            _absOnlineService = absOnlineService;
            _contractPaymentScheduleService = contractPaymentScheduleService;
            _payOperationActionRepository = payOperationActionRepository;
            _refinanceBuyOutService = refinanceBuyOutService;
            _refinancesService = refinanceService;
            _logger = logger;

        }

        public async Task Sign(int contractId, bool oldRefinance = false)
        {

            var contract = _contractRepository.Get(contractId);
            var branch = _groupRepository.Get(contract.BranchId);

            try 
            { 
                _logger.Information($"FInd and try refinance for contract : {contractId}");//Рефинанс выполняется до операций подписания из за особенностей БО
                if (oldRefinance) //TODO эта вилка должна уйти вместе после успешного релиза оставив только else
                    await _oldRefinanceService.RefinanceAllAssociatedContracts(contract.Id);
                else
                    await _refinancesService.RefinanceAllAssociatedContracts(contractId);
                _logger.Information($"Refinance passed try to sign :{contractId}");
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }
            using (IDbTransaction transaction = _payOperationRepository.BeginTransaction())
            {
                try
                {
                    var payOperationId = _payOperationRepository.GetCreditCardCashOutPayOperationByContractId(contractId);
                    var payOperation = _payOperationRepository.Get(payOperationId.Id);
                    PayOperationAction action = new PayOperationAction()
                    {
                        ActionType = PayOperationActionType.Execute,
                        AuthorId = Constants.ADMINISTRATOR_IDENTITY,
                        CreateDate = DateTime.Now,
                        Date = DateTime.Now,
                        OperationId = payOperation.Id
                    };

                    var policyResult = _absOnlineService.RegisterPolicy(contract.Id, contract);

                    if (!string.IsNullOrEmpty(policyResult))
                        _absOnlineService.SaveRetrySendInsurance(contract.Id);

                    ContractAction operactionAction = payOperation.Action;
                    if (payOperation.Action.ActionType == ContractActionType.Sign)
                    {
                        contract.SignDate = DateTime.Now;
                        contract.Status = ContractStatus.Signed;
                        payOperation.Status = PayOperationStatus.Executed;
                        operactionAction.Status = ContractActionStatus.Approved;
                        _contractActionService.Save(operactionAction);
                        _contractPaymentScheduleService.UpdateFirstPaymentInfo(contract.Id, contract);
                    }

                    foreach (CashOrder order in _cashOrderService.GetCashOrdersForApprove(payOperation.Orders))
                    {
                        if (order.OrderDate.Date != DateTime.Now.Date)
                            order.OrderDate = DateTime.Now;

                        order.ApproveStatus = OrderStatus.Approved;
                        _cashOrderService.Register(order, branch);
                    }

                    _payOperationRepository.Update(payOperation);
                    _contractRepository.Update(contract);
                    _payOperationActionRepository.Insert(action);
                    _logger.Information($"Create entitys for operations{contractId}");
                    transaction.Commit();
                }
                catch (Exception exception)
                {
                    transaction.Rollback();
                    _logger.Error(exception, exception.Message);
                    throw;
                }

            }

            try 
            { //Выкуп осущствляется после подписания
                if (oldRefinance) //TODO эта вилка должна уйти вместе после успешного релиза оставив только else
                    await _refinanceBuyOutService.BuyOutAllRefinancedContracts(contract.Id);
                else
                    await _refinanceBuyOutService.BuyOutAllRefinancedContractsForApplicationsOnline(contract.Id);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }
        }
    }
}
