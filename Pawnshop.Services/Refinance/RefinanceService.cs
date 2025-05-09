using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pawnshop.Core.Queries;

namespace Pawnshop.Services.Refinance
{
    public sealed class RefinanceService : IRefinanceService
    {
        private readonly OnlineApplicationRepository _onlineApplicationService;
        private readonly ContractRepository _contractRepository;
        private readonly GroupRepository _groupRepository;
        private readonly IBusinessOperationService _businessOperationService;
        private readonly ICashOrderService _cashOrderService;
        private readonly ContractService _contractService;
        private readonly AccountRepository _accountRepository;
        private readonly CashOrderRepository _cashOrderRepository;
        private readonly ILogger<RefinanceService> _logger;
        public RefinanceService(OnlineApplicationRepository onlineApplicationService,
            ContractRepository contractRepository,
            GroupRepository groupRepository,
            IBusinessOperationService businessOperationService,
            ICashOrderService cashOrderService,
            ContractService contractService,
            AccountRepository accountRepository, 
            CashOrderRepository cashOrderRepository,
            ILogger<RefinanceService> logger)
        {
            _onlineApplicationService = onlineApplicationService;
            _contractRepository = contractRepository;
            _groupRepository = groupRepository;
            _businessOperationService = businessOperationService;
            _contractService = contractService;
            _cashOrderService = cashOrderService;
            _accountRepository = accountRepository;
            _cashOrderRepository = cashOrderRepository;
            _logger = logger;
        }

        /// <summary>
        /// Рефинансировать все займы за счет контракта 
        /// </summary>
        /// <param name="contractId">Идентификатор контракта за счет которого будут рефинансированы займы</param>
        /// <returns></returns>
        [Obsolete]
        public async Task<bool> RefinanceAllAssociatedContracts(int contractId)
        {
            try
            {
                var contract = _contractRepository.Get(contractId);
                var branch = _groupRepository.Get(contract.BranchId);
                var application = await _onlineApplicationService.FindByContractIdAsync(new { ContractId = contractId.ToString() });

                if (application == null)
                    return true;

                for (int i = 0; i < application.OnlineApplicationRefinances.Count; i++)
                {
                    var refinancedContract = _contractRepository.Get(application.OnlineApplicationRefinances[i].RefinancedContractId.Value);
                    branch = _groupRepository.Get(contract.BranchId);
                    var operation = _businessOperationService.FindBusinessOperation(contract.ContractTypeId,
                        "REFINANCE",
                        refinancedContract.BranchId,
                        branch.OrganizationId);
                    var cashOrders = _cashOrderRepository.List(new ListQuery(), 
                        new { ContractId = refinancedContract.Id, BusinessOperationId = operation.Id, ApproveStatus = OrderStatus.Approved });
                    if (cashOrders.Count > 0)
                    {
                        return false;// Договор уже был рефинансирован
                    }

                    var amount = await CalculateRefinanceAmountForRefinancedContract(application.OnlineApplicationRefinances[i].RefinancedContractId.Value);
                    var amountDict = new Dictionary<AmountType, decimal> { { AmountType.Refinance, amount } };
                    var cashOrder = _businessOperationService.Register(refinancedContract, DateTime.Now, 
                        Constants.BO_REFINANCE, branch, 1, amountDict);
                    cashOrder[0].Item1.ApproveStatus = OrderStatus.Approved;
                    cashOrder[0].Item1.OrderDate = DateTime.Now;
                    _cashOrderService.Register(cashOrder[0].Item1, branch);
                }
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception, $"Не удалось рефинансировать займ с ошибкой {exception.Message}");
            }
            return true;
        }

        //Сумма рефинансирования =
        //    + Account
        //    + Overdue_Account
        //    + Profit
        //    + Overdue_Profit
        //    + Peny_Account
        //    + Peny_Profit
        //    - Depo ????

        /// <summary>
        /// Вычисляет сумму необходимую для рефинансирования всех займов для обозначенного контракта
        /// </summary>
        /// <param name="contractId">Идентификатор займа за счет которого будут рефинансированы договора</param>
        /// <returns></returns>
        [Obsolete]
        public async Task<decimal> CalculateRefinanceAmountForContract(int contractId)
        {
            try
            {
                var contract = _contractRepository.Get(contractId);
                var onlineApplication = _onlineApplicationService
                    .FindByContractIdAsync(new { ContractId = contract.Id.ToString() }).Result;
                decimal amount = 0;
                foreach (var refinance in onlineApplication.OnlineApplicationRefinances)
                {
                    amount += await CalculateRefinanceAmountForRefinancedContract(refinance.RefinancedContractId.Value);
                }

                return amount;
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception, $"Не удалось рефинансировать займ с ошибкой {exception.Message}");
                throw;
            }
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

