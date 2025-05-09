using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Services.CreditLines;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Services.Contracts.ContractActionOnlineExecutionCheckerService
{
    public sealed class ContractActionOnlineExecutionCheckerService : IContractActionOnlineExecutionCheckerService
    {
        private readonly AccountRepository _accountRepository;
        private readonly IContractService _contractService;
        private readonly ICreditLineService _creditLineService;
        private readonly FunctionSettingRepository _functionSettingRepository;

        public ContractActionOnlineExecutionCheckerService(
            AccountRepository accountRepository,
            IContractService contractService,
            ICreditLineService creditLineService,
            FunctionSettingRepository functionSettingRepository)
        {
            _accountRepository = accountRepository;
            _contractService = contractService;
            _creditLineService = creditLineService;
            _functionSettingRepository = functionSettingRepository;
        }


        public async Task<ContractActionOnlineExecutionCheckResult> Check(int contractId)
        {
            try
            {
                // технические неполадки (запрет на освоение)
                var depoMasteringSetting = _functionSettingRepository.GetByCode(Constants.FUNCTION_SETTING__DEPO_MASTERING);

                if (depoMasteringSetting != null && depoMasteringSetting.BooleanValue.HasValue && depoMasteringSetting.BooleanValue.Value)
                {
                    return new ContractActionOnlineExecutionCheckResult(HttpStatusCode.InternalServerError,
                        "Technical problems (prohibition of development).",
                        ContractActionOnlineExecutionErrorType.TechnicalIssues, true, true);
                }

                // Время (10.00 до 22.30)
                DateTime now = DateTime.Now;

                if (now.TimeOfDay >= Constants.STOP_ONLINE_PAYMENTS || now.TimeOfDay < Constants.START_ONLINE_PAYMENTS)
                {
                    return new ContractActionOnlineExecutionCheckResult(HttpStatusCode.InternalServerError,
                        "The time for mastering online payments should be between 10:00 and 22:30.",
                        ContractActionOnlineExecutionErrorType.BadTimeToRequest, true, true);
                }


                var contract = _contractService.GetOnlyContract(contractId);

                if (contract == null)
                {
                    return new ContractActionOnlineExecutionCheckResult(HttpStatusCode.NotFound,
                        $"The contract {contractId} not found!",
                        ContractActionOnlineExecutionErrorType.NotFound, true);
                }

                if (contract.Status != ContractStatus.Signed)
                {
                    return new ContractActionOnlineExecutionCheckResult(HttpStatusCode.UnprocessableEntity,
                        $"The contract {contractId} has incorrect status!",
                        ContractActionOnlineExecutionErrorType.BadEntity, true);
                }

                if (contract.ContractClass == ContractClass.CreditLine)
                {
                    return new ContractActionOnlineExecutionCheckResult(HttpStatusCode.UnprocessableEntity,
                        $"The contract {contractId} is credit line, not supported!",
                        ContractActionOnlineExecutionErrorType.BadEntity, true);
                }

                if (contract.ContractClass == ContractClass.Credit)
                {
                    // не должно быть исполнительной надписи
                    if (contract.IsOffBalance)
                    {
                        return new ContractActionOnlineExecutionCheckResult(HttpStatusCode.UnprocessableEntity,
                            $"The contract {contractId} is off balance.",
                            ContractActionOnlineExecutionErrorType.BadEntity, true);
                    }

                    // парковка(статус должен быть у клиента)
                    if (contract.CollateralType == CollateralType.Car)
                    {
                        var parkingStatus = await _contractService.CarHasClientAsync(contractId);

                        if (!parkingStatus)
                        {
                            return new ContractActionOnlineExecutionCheckResult(HttpStatusCode.UnprocessableEntity,
                                $"The contract {contractId} car is parking.",
                                ContractActionOnlineExecutionErrorType.BadEntity, true);
                        }
                    }

                    // не должно быть не подтвержденных действий
                    var hasIncompleteActions = await _contractService.IncompleteActionExistsAsync(contractId);

                    if (hasIncompleteActions)
                    {
                        return new ContractActionOnlineExecutionCheckResult(HttpStatusCode.UnprocessableEntity,
                            $"The contract {contractId} has incomplete actions.",
                            ContractActionOnlineExecutionErrorType.BadEntity, true);
                    }

                    // не должно быть доп расходов
                    var hasExpenses = await _contractService.HasExpenses(contractId);

                    if (hasExpenses)
                    {
                        return new ContractActionOnlineExecutionCheckResult(HttpStatusCode.UnprocessableEntity,
                            $"The contract {contractId} has expenses.",
                            ContractActionOnlineExecutionErrorType.BadEntity, true);
                    }

                    // не должно быть просрочки или не оплаченных платежей по договору
                    var balance = await _accountRepository.GetBalanceByContractIdAsync(contractId);

                    if (contract.NextPaymentDate <= DateTime.Now || balance.CurrentDebt > 0)
                    {
                        return new ContractActionOnlineExecutionCheckResult(HttpStatusCode.UnprocessableEntity,
                            $"The contract {contractId} has overdue.",
                            ContractActionOnlineExecutionErrorType.BadEntity, true);
                    }
                }
                else
                {
                    var creditLineId = contract.CreditLineId.Value;
                    var creditLine = await _contractService.GetOnlyContractAsync(creditLineId);

                    var tranches = await _contractService.GetAllSignedTranches(creditLineId);

                    if (!tranches.Any())
                    {
                        return new ContractActionOnlineExecutionCheckResult(HttpStatusCode.UnprocessableEntity,
                            $"The credit line {creditLineId} has not active tranches.",
                            ContractActionOnlineExecutionErrorType.BadEntity, true);
                    }

                    var tranchesIds = tranches.Select(x => x.Id).ToList();

                    if (tranches.Any(x => x.IsOffBalance))
                    {
                        return new ContractActionOnlineExecutionCheckResult(HttpStatusCode.UnprocessableEntity,
                            $"The credit line {creditLineId} has tranches is off balance.",
                            ContractActionOnlineExecutionErrorType.BadEntity, true);
                    }

                    if (creditLine.CollateralType == CollateralType.Car)
                    {
                        var parkingStatus = await _contractService.CarHasClientAsync(creditLineId);

                        if (!parkingStatus)
                        {
                            return new ContractActionOnlineExecutionCheckResult(HttpStatusCode.UnprocessableEntity,
                                $"The credit line {creditLineId} car is parking.",
                                ContractActionOnlineExecutionErrorType.BadEntity, true);
                        }
                    }

                    var hasIncompleteActions = await _creditLineService.IncompleteActionExistsAsync(tranchesIds);

                    if (hasIncompleteActions)
                    {
                        return new ContractActionOnlineExecutionCheckResult(HttpStatusCode.UnprocessableEntity,
                            $"The credit line {creditLineId} has tranches incomplete actions.",
                            ContractActionOnlineExecutionErrorType.BadEntity, true);
                    }

                    var hasExpenses = await _contractService.HasExpenses(creditLineId);
                    var hasTranchesExpenses = await _creditLineService.HasExpenses(tranchesIds);

                    if (hasExpenses || hasTranchesExpenses)
                    {
                        return new ContractActionOnlineExecutionCheckResult(HttpStatusCode.UnprocessableEntity,
                            $"The credit line {creditLineId} has expenses.",
                            ContractActionOnlineExecutionErrorType.BadEntity, true);
                    }

                    var creditLineBalance = await _creditLineService.GetCurrentlyDebtForCreditLine(creditLineId, tranchesIds);

                    if (creditLineBalance.SummaryCurrentDebt > 0 || tranches.Any(x => x.NextPaymentDate <= DateTime.Now))
                    {
                        return new ContractActionOnlineExecutionCheckResult(HttpStatusCode.UnprocessableEntity,
                            $"The credit line {creditLineId} has overdue.",
                            ContractActionOnlineExecutionErrorType.BadEntity, true);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ContractActionOnlineExecutionCheckResult(HttpStatusCode.InternalServerError,
                    ex.Message,
                    ContractActionOnlineExecutionErrorType.TechnicalIssues, true);
            }

            return new ContractActionOnlineExecutionCheckResult(HttpStatusCode.OK);
        }
    }
}
