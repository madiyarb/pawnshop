using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationOnlineRefinances;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.PayOperations;
using Pawnshop.Services.AbsOnline;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.ApplicationsOnline;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Insurance;
using Pawnshop.Services.Refinance;
using Serilog;

namespace Pawnshop.Services.ApplicationOnlineRefinances
{
    public sealed class ApplicationOnlineRefinancesService : IApplicationOnlineRefinancesService
    {

        private readonly ContractRepository _contractRepository;
        private readonly ApplicationOnlineRefinancesRepository _applicationOnlineRefinancesRepository;
        private readonly CreditLineRepository _creditLineRepository;
        private readonly ApplicationOnlineRepository _applicationOnlineRepository;
        private readonly IContractService _contractService;
        private readonly IContractActionService _contractActionService;
        private readonly IContractActionPrepaymentService _contractActionPrepaymentService;
        private readonly GroupRepository _groupRepository;
        private readonly AccountRepository _accountRepository;
        private readonly IBusinessOperationService _businessOperationService;
        private readonly CashOrderRepository _cashOrderRepository;
        private readonly ICashOrderService _cashOrderService;
        private readonly PayOperationRepository _payOperationRepository;
        private readonly IContractPaymentScheduleService _contractPaymentScheduleService;
        private readonly PayOperationActionRepository _payOperationActionRepository;
        private readonly IRefinanceBuyOutService _refinanceBuyOutService;
        private readonly InsurancePremiumCalculator _insurancePremiumCalculator;
        private readonly IAbsOnlineService _absOnlineService;
        private readonly ILogger _logger;
        private readonly ApplicationOnlineInsuranceRepository _applicationOnlineInsuranceRepository;
        private readonly ApplicationOnlineService _applicationOnlineService;

        public ApplicationOnlineRefinancesService(ContractRepository contractRepository,
            ApplicationOnlineRefinancesRepository applicationOnlineRefinancesRepository,
            CreditLineRepository creditLineRepository,
            ApplicationOnlineRepository applicationOnlineRepository,
            IContractService contractService,
            IContractActionService contractActionService,
            IContractActionPrepaymentService contractActionPrepaymentService,
            GroupRepository groupRepository,
            AccountRepository accountRepository,
            IBusinessOperationService businessOperationService,
            CashOrderRepository cashOrderRepository,
            ICashOrderService cashOrderService,
            PayOperationRepository payOperationRepository,
            IContractPaymentScheduleService contractPaymentScheduleService,
            PayOperationActionRepository payOperationActionRepository,
            IRefinanceBuyOutService refinanceBuyOutService,
            InsurancePremiumCalculator insurancePremiumCalculator,
            IAbsOnlineService absOnlineService,
            ILogger logger,
            ApplicationOnlineInsuranceRepository applicationOnlineInsuranceRepository,
            ApplicationOnlineService applicationOnlineService)
        {
            _contractRepository = contractRepository;
            _applicationOnlineRefinancesRepository = applicationOnlineRefinancesRepository;
            _creditLineRepository = creditLineRepository;
            _applicationOnlineRepository = applicationOnlineRepository;
            _contractService = contractService;
            _contractActionService = contractActionService;
            _contractActionPrepaymentService = contractActionPrepaymentService;
            _groupRepository = groupRepository;
            _accountRepository = accountRepository;
            _businessOperationService = businessOperationService;
            _cashOrderRepository = cashOrderRepository;
            _cashOrderService = cashOrderService;
            _payOperationRepository = payOperationRepository;
            _contractPaymentScheduleService = contractPaymentScheduleService;
            _payOperationActionRepository = payOperationActionRepository;
            _refinanceBuyOutService = refinanceBuyOutService;
            _insurancePremiumCalculator = insurancePremiumCalculator;
            _absOnlineService = absOnlineService;
            _logger = logger;
            _applicationOnlineInsuranceRepository = applicationOnlineInsuranceRepository;
            _applicationOnlineService = applicationOnlineService;

        }


        public async Task<bool> CreateRequiredRefinances(string vinCode, Guid applicationOnlineId)
        {
            try
            {
                var contracts = (await _contractRepository.GetTranchesByVinCode(vinCode))
                    .Where(contract => contract.Status == ContractStatus.Signed);

                if (contracts != null && contracts.Any())
                {
                    foreach (var contract in contracts)
                    {
                        await _applicationOnlineRefinancesRepository.Insert(
                            new ApplicationOnlineRefinance(applicationOnlineId, contract.Id, contract.ContractNumber, true));
                    }

                    return true;
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }

            return false;
        }

        public async Task<List<ApplicationOnlineRefinancedContractInfo>> GetRefinancedContractInfo(Guid applicationOnlineId)
        {
            try
            {
                var application = _applicationOnlineRepository.Get(applicationOnlineId);
                var contracts = _contractRepository.GetContractsByClientIdAndContractClases(application.ClientId,
                    new List<int>
                    {
                        (int)ContractClass.CreditLine,
                        (int)ContractClass.Tranche
                    }).Where(contract => contract.Status == ContractStatus.Signed);

                if (contracts == null)
                    return null;
                Dictionary<int, int> creditLinesTranchesDictionary = new Dictionary<int, int>();
                foreach (var contract in contracts)
                {
                    int creditLineId = await _contractRepository.GetCreditLineByTrancheId(contract.Id);
                    if (creditLineId != 0)
                    {
                        creditLinesTranchesDictionary.Add(contract.Id,
                            await _contractRepository.GetCreditLineByTrancheId(contract.Id));
                    }
                    else
                    {
                        creditLinesTranchesDictionary.Add(contract.Id, contract.Id);
                    }
                }

                var contractBalances =
                    await _creditLineRepository.GetBalancesByContractIdsAsync(contracts.Select(contract => contract.Id)
                        .ToList());
                var applicationOnlineRefinances = await
                    _applicationOnlineRefinancesRepository.GetApplicationOnlineRefinancesByApplicationId(
                        applicationOnlineId);
                return contractBalances.Join(contracts,
                        contracts => contracts.ContractId,
                        contractBalances => contractBalances.Id,
                        (contractBalance, contract) => new ApplicationOnlineRefinancedContractInfo
                        {
                            AccountAmount = contractBalance.AccountAmount,
                            ContractId = contractBalance.ContractId,
                            ContractNumber = contract.ContractNumber,
                            CurrentDebt = contractBalance.CurrentDebt,
                            ExpiredDays = ExpiredDayCalculation(contract.NextPaymentDate),
                            IsCreditLine = contract.ContractClass == ContractClass.CreditLine ? true : false,
                            PenyAmount = contractBalance.PenyAmount,
                            PercentAmount = contractBalance.ProfitAmount,
                            PrepaymentBalance = contractBalance.PrepaymentBalance,
                            TotalRedemptionAmount = contractBalance.TotalRedemptionAmount,
                            RefinanceRequired = applicationOnlineRefinances.Exists(refinanceData =>
                                refinanceData.RefinancedContractId == contract.Id && refinanceData.RefinanceRequired),
                            CheckedForRefinance = applicationOnlineRefinances.Exists(refinanceData =>
                                refinanceData.RefinancedContractId == contract.Id),
                            CreditLineId = creditLinesTranchesDictionary[contract.Id]
                        })
                    .OrderBy(contractBalance => contractBalance.CreditLineId)
                    .ThenByDescending(contractBalance => contractBalance.IsCreditLine)
                    .ThenByDescending(contractBalance => contractBalance.ContractId)
                    .ToList();
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }
        }

        public async Task UpdateApplicationOnlineRefinancesList(Guid applicationId, List<int> refinancedContractsIds)
        {
            try
            {
                var currentRefinances = await
                    _applicationOnlineRefinancesRepository.GetApplicationOnlineRefinancesByApplicationId(applicationId);
                var refinancesForDelete = currentRefinances.Where(currentRefinance =>
                    !refinancedContractsIds.Contains(currentRefinance.RefinancedContractId) &&
                    currentRefinance.RefinanceRequired == false).ToList();
                var refinancesForInsert = refinancedContractsIds.Where(refinancesContractId =>
                    !currentRefinances.Exists(curref => curref.RefinancedContractId == refinancesContractId)).ToList();
                for (int i = 0; i < refinancesForDelete.Count; i++)
                {
                    refinancesForDelete[i].Delete();
                    await _applicationOnlineRefinancesRepository.Update(refinancesForDelete[i]);
                }

                for (int i = 0; i < refinancesForInsert.Count; i++)
                {
                    var contract = _contractRepository.GetOnlyContract(refinancesForInsert[i]);
                    await _applicationOnlineRefinancesRepository.Insert(
                        new ApplicationOnlineRefinance(applicationId, contract.Id, contract.ContractNumber, false));
                }

                var refinancesAfterOperation = await
                    _applicationOnlineRefinancesRepository.GetApplicationOnlineRefinancesByApplicationId(applicationId);

                var application = _applicationOnlineRepository.Get(applicationId);
                if (refinancesAfterOperation.Count > 0)
                {
                    application.Type = ApplicationOnlineType.Refinance.ToString();
                }
                else
                {
                    application.Type = ApplicationOnlineType.Refinance.ToString();
                }
                await _applicationOnlineRepository.Update(application);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }

        }

        public async Task SetContractIdForRefinancedItems(ApplicationOnline application)
        {
            try
            {
                var currentRefinances = await
                    _applicationOnlineRefinancesRepository
                        .GetApplicationOnlineRefinancesByApplicationId(application.Id);

                for (int i = 0; i < currentRefinances.Count; i++)
                {
                    currentRefinances[i].SetContractId(application.ContractId.Value);
                    await _applicationOnlineRefinancesRepository.Update(currentRefinances[i]);
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }
        }

        public async Task<bool> IsRefinance(int contractId)
        {
            var refinances = await _applicationOnlineRefinancesRepository
                .GetApplicationOnlineRefinancesByContractId(contractId);
            if (refinances != null && refinances.Any())
            {
                return true;
            }

            return false;
        }

        public async Task<string> EnoughMoneyForRefinancing(int contractId)
        {
            var refinances =
                await _applicationOnlineRefinancesRepository.GetApplicationOnlineRefinancesByContractId(contractId);

            var creditlinesIds =
                _contractRepository.GetCreditLinesFromTranchesIds(refinances
                    .Select(refinance => refinance.RefinancedContractId).ToList());

            var creditlinesBalances =
                await _creditLineRepository.GetBalancesByContractIdsAsync(creditlinesIds
                    .ToList());
            var contractBalances =
                await _creditLineRepository.GetBalancesByContractIdsAsync(refinances
                    .Select(refinance => refinance.RefinancedContractId).ToList());

            var prepaymentBalance = creditlinesBalances.Sum(balance => balance.PrepaymentBalance);

            var needToPay = contractBalances.Sum(balance => balance.ProfitAmount);
            if (needToPay == 0)
                return null;

            if (needToPay > prepaymentBalance)
                return $"Клиенту необходимо погасить проценты и задолжности в сумме {needToPay}. Денег на балансе всех кредитных линиях участвующих в рефинансировании : {prepaymentBalance}";
            return null;
        }

        public async Task<bool> MovePrepaymentForRefinance(int contractId, int branchId)
        {
            try
            {
                var refinances =
                    await _applicationOnlineRefinancesRepository.GetApplicationOnlineRefinancesByContractId(contractId);

                var creditlinesIds =
                    _contractRepository.GetCreditLinesFromTranchesIds(refinances
                        .Select(refinance => refinance.RefinancedContractId).ToList());

                var creditlinesBalances =
                    await _creditLineRepository.GetBalancesByContractIdsAsync(creditlinesIds
                        .ToList());
                var contractBalances =
                    await _creditLineRepository.GetBalancesByContractIdsAsync(refinances
                        .Select(refinance => refinance.RefinancedContractId).ToList());

                var prepaymentBalance = creditlinesBalances.Sum(balance => balance.PrepaymentBalance);

                var needToPay = contractBalances.Sum(balance => balance.ProfitAmount);
                if (needToPay == 0)
                    return true;

                if (needToPay > prepaymentBalance)
                    return false;
                if (creditlinesBalances.Count > 1)
                {
                    //Все деньги перечисляем на первую КЛ
                    for (int i = 1; i < creditlinesBalances.Count; i++)
                    {
                        await MovePrepayment(creditlinesBalances[i].ContractId, creditlinesBalances[0].ContractId,
                            creditlinesBalances[i].PrepaymentBalance, branchId);
                    }
                }

                for (int i = 0; i < contractBalances.Count; i++)
                {
                    if (contractBalances[i].ProfitAmount > 0)
                    {
                        //Закидываем на транши стока скока надо для погашения пени
                        await MovePrepayment(creditlinesBalances[0].ContractId, contractBalances[i].ContractId,
                            contractBalances[i].ProfitAmount, branchId);
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }

            return true;
        }

        public async Task<decimal> CalculateRefinanceAmountForContract(int contractId)
        {
            try
            {
                var applicationOnlineRefinances = await
                    _applicationOnlineRefinancesRepository.GetApplicationOnlineRefinancesByContractId(
                        contractId);
                decimal amount = 0;
                foreach (var refinance in applicationOnlineRefinances)
                {
                    amount += await CalculateRefinanceAmountForRefinancedContract(refinance.RefinancedContractId);
                }

                return amount;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }
        }

        public async Task CorrectApplicationAmountSumForRefinancedContracts(Guid applicationId, int userId)
        {
            try
            {
                var application = _applicationOnlineRepository.Get(applicationId);
                var refinances = await
                    _applicationOnlineRefinancesRepository.GetApplicationOnlineRefinancesByApplicationId(applicationId);
                var contractBalances =
                    await _creditLineRepository.GetBalancesByContractIdsAsync(refinances
                        .Select(refinance => refinance.RefinancedContractId)
                        .ToList());

                var needToRefinanceSum = contractBalances.Sum(contract => contract.AccountAmount);
                var insurance = await _applicationOnlineInsuranceRepository.GetByApplicationId(application.Id);
                if (insurance != null)
                {
                    await _applicationOnlineService.ChangeDetailForInsurance(application,
                        Constants.ADMINISTRATOR_IDENTITY, null, needToRefinanceSum);
                }
                else
                {
                    application.ChangeApplicationAmount(contractBalances.Sum(contract => contract.AccountAmount),
                        userId);
                }

                await _applicationOnlineRepository.Update(application);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }
        }

        public async Task<bool> IsApplicationAmountMoreThenRefinancedSum(ApplicationOnline application)
        {
            var refinances = await
                _applicationOnlineRefinancesRepository.GetApplicationOnlineRefinancesByApplicationId(application.Id);
            var contractBalances =
                await _creditLineRepository.GetBalancesByContractIdsAsync(refinances
                    .Select(refinance => refinance.RefinancedContractId)
                    .ToList());
            var needToRefinanceSum = contractBalances.Sum(contract => contract.AccountAmount);
            var insurance = await _applicationOnlineInsuranceRepository.GetByApplicationId(application.Id);
            if (insurance != null)
            {
                if (insurance.AmountForCustomer >= needToRefinanceSum)
                {
                    return true;
                }
            }
            else
            {
                if (application.ApplicationAmount >= needToRefinanceSum)
                {
                    return true;
                }
            }

            return false;
        }

        public async Task InternalRefinance(int contractId)
        {
            bool insurance = false;
            try
            {
                var application = _applicationOnlineRepository.GetByContractId(contractId);
                if (application != null)
                {
                    var insurane = await _applicationOnlineInsuranceRepository.GetByApplicationId(application.Id);
                    if (insurane != null)
                    {
                        if (insurane.Status == "IsProvided")
                        {
                            insurance = true;
                        }
                    }
                }
                decimal amount = await CalculateRefinanceAmountForContract(contractId);

                var contract = _contractService.Get(contractId);
                var branch = _groupRepository.Get(contract.BranchId);
                decimal refSum;
                if (insurance)
                    refSum = _insurancePremiumCalculator.GetLoanCostWithoutInsurancePremium(contract.LoanCost);
                else
                    refSum = contract.LoanCost;

                if (refSum - amount < (decimal)0.01)
                {
                    await RefinanceAllAssociatedContracts(contract.Id);

                    #region ChangeStatusAndAddOnlinePayment

                    var payOperationId =
                        _payOperationRepository.GetPayOperationByContractIdWithoutCashOrders(contract.Id);
                    var payOperation = _payOperationRepository.Get(payOperationId.Id);
                    PayOperationAction action = new PayOperationAction()
                    {
                        ActionType = PayOperationActionType.Execute,
                        AuthorId = 1,
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

                    var cashOrders = await _cashOrderService.GetAllRelatedOrdersByContractActionId(operactionAction.Id);

                    for (int i = 0; i < cashOrders.Count; i++)
                    {
                        CashOrder order = await _cashOrderService.GetAsync(cashOrders[i]);
                        if (order.OrderDate.Date != DateTime.Now.Date)
                            order.OrderDate = DateTime.Now;

                        order.ApproveStatus = OrderStatus.Approved;
                        _cashOrderService.Register(order, branch);
                    }

                    _payOperationRepository.Update(payOperation);
                    _contractRepository.Update(contract);
                    _payOperationActionRepository.Insert(action);

                    #endregion

                    await _refinanceBuyOutService.BuyOutAllRefinancedContractsForApplicationsOnline(contract.Id);
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }
        }

        /// <summary>
        /// Рефинансировать все займы за счет контракта 
        /// </summary>
        /// <param name="contractId">Идентификатор контракта за счет которого будут рефинансированы займы</param>
        /// <returns></returns>
        public async Task<bool> RefinanceAllAssociatedContracts(int contractId)
        {
            try
            {
                var contract = _contractRepository.Get(contractId);
                var branch = _groupRepository.Get(contract.BranchId);
                var refinances = await _applicationOnlineRefinancesRepository.GetApplicationOnlineRefinancesByContractId(contractId);

                if (refinances == null)
                    return true;

                for (int i = 0; i < refinances.Count; i++)
                {
                    var refinancedContract = _contractRepository.Get(refinances[i].RefinancedContractId);
                    branch = _groupRepository.Get(contract.BranchId);
                    var operation = _businessOperationService.FindBusinessOperation(contract.ContractTypeId,
                        Constants.BO_REFINANCE,
                        refinancedContract.BranchId,
                        branch.OrganizationId);
                    var cashOrders = _cashOrderRepository.List(new ListQuery(),
                        new { ContractId = refinancedContract.Id, BusinessOperationId = operation.Id, ApproveStatus = OrderStatus.Approved });
                    if (cashOrders.Count > 0)
                    {
                        return false;// Договор уже был рефинансирован
                    }

                    var amount = await CalculateRefinanceAmountForRefinancedContract(refinances[i].RefinancedContractId);
                    var amountDict = new Dictionary<AmountType, decimal> { { AmountType.Refinance, amount } };
                    var cashOrder = _businessOperationService.Register(refinancedContract, DateTime.Now,
                        Constants.BO_REFINANCE, branch, Constants.ADMINISTRATOR_IDENTITY, amountDict);
                    cashOrder[0].Item1.ApproveStatus = OrderStatus.Approved;
                    cashOrder[0].Item1.OrderDate = DateTime.Now;
                    _cashOrderService.Register(cashOrder[0].Item1, branch);
                }
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
            }
            return true;
        }


        private async Task MovePrepayment(int sourceContractId, int recepientContractId, decimal amount, int branchId)
        {
            try
            {
                var branch = _groupRepository.Get(branchId);

                var prepaymentModel = new MovePrepayment
                {
                    SourceContractId = sourceContractId,
                    Date = DateTime.Now.Date,
                    Amount = amount,
                    RecipientContractId = recepientContractId,
                    Note = "Перевод денег для рефинансирования"
                };

                var incompleteExists = await _contractActionService.IncopleteActionExists(sourceContractId);
                if (incompleteExists)
                    throw new ContractsContainIncompleteActionsException(
                        $"Договор {sourceContractId} имеет невыполненые действия.");

                _contractActionPrepaymentService.MovePrepayment(prepaymentModel, Constants.ADMINISTRATOR_IDENTITY,
                    branch);
            }
            catch (ContractsContainIncompleteActionsException exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }
        }

        private int ExpiredDayCalculation(DateTime? nextPaymentDate)
        {
            if (nextPaymentDate == null)
                return 0;
            var days = (DateTime.Now - nextPaymentDate).Value.Days;
            if (days < 0)
                return 0;
            return days;
        }

        /// <summary>
        /// Считает сумму которая необходима для рефинансирования контракта
        /// Основной долг (balance.AccountAmount) + Просроченный основной долг (balance.OverdueAccountAmount)
        ///  + проценты начисленные (balance.ProfitAmount) + проценты просроченные (balance.OverdueProfitAmount)
        ///  + Пеня на долг просроченные = Пеня на долг просроченный И Пеня на проценты просроченные  (balance.PenyAmount) - аванс balance.PrepaymentBalance
        /// </summary>
        /// <param name="contractId">Идентификатор договора который будет рефинансирован</param>
        /// <returns></returns>
        private async Task<decimal> CalculateRefinanceAmountForRefinancedContract(int contractId)
        {
            var refinanceContract = _contractService.Get(contractId);
            var balance = await _accountRepository.GetBalanceByContractIdAsync(refinanceContract.Id);
            decimal amount = balance.AccountAmount + balance.OverdueAccountAmount + balance.ProfitAmount +
                balance.OverdueProfitAmount + balance.PenyAmount - balance.PrepaymentBalance;
            // Основной долг (balance.AccountAmount) + Просроченный основной долг (balance.OverdueAccountAmount)
            // + проценты начисленные (balance.ProfitAmount) + проценты просроченные (balance.OverdueProfitAmount)
            // + Пеня на долг просроченные = Пеня на долг просроченный И Пеня на проценты просроченные  (balance.PenyAmount) - аванс balance.PrepaymentBalance
            return amount;
        }
    }
}
