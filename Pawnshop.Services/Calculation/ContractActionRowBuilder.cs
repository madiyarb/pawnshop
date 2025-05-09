using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Models.Calculation;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using System;
using System.Collections.Generic;
using System.Linq;
using Pawnshop.Services.Insurance;
using Pawnshop.Data.Models.CashOrders;

namespace Pawnshop.Services.Calculation
{
    public class ContractActionRowBuilder : ContractAmount, IContractActionRowBuilder
    {
        protected readonly PayTypeRepository _payTypeRepository;
        protected readonly LoanProductTypeRepository _loanProductTypeRepository;
        protected List<Account> _accounts;
        protected Group _branch;
        protected ContractActionType _actionType;

        protected readonly IBusinessOperationService _businessOperationService;
        protected readonly IDictionaryWithSearchService<BusinessOperationSetting, BusinessOperationSettingFilter> _businessOperationSettingService;
        protected readonly IAccountService _accountService;
        protected readonly IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> _accountSettingService;
        protected readonly IDictionaryWithSearchService<PaymentOrder, PaymentOrderFilter> _paymentOrderService;
        protected readonly IDictionaryWithSearchService<Group, BranchFilter> _branchService;
        protected readonly IContractService _contractService;
        protected readonly IInsurancePolicyService _insurancePolicyService;
        protected readonly IInsurancePoliceRequestService _insurancePoliceRequestService;
        protected readonly ICashOrderService _cashOrderService;
        protected readonly ContractActionRepository _contractActionRepository;

        public ContractActionRowBuilder(BlackoutRepository blackoutRepository,
            ContractDiscountRepository contractDiscountRepository,
            PayTypeRepository payTypeRepository,
            LoanProductTypeRepository loanProductTypeRepository,
            IDictionaryWithSearchService<Holiday, HolidayFilter> holidayRepository,
            IDictionaryWithSearchService<Group, BranchFilter> branchService,
            IBusinessOperationService businessOperationService,
            IDictionaryWithSearchService<BusinessOperationSetting, BusinessOperationSettingFilter> businessOperationSettingService,
            IAccountService accountService,
            IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> accountSettingService,
            IAccountPlanSettingService accountPlanSettingService,
            IPostponementService postponementService,
            IContractService contractService,
            IDictionaryWithSearchService<PaymentOrder, PaymentOrderFilter> paymentOrderService,
            IContractActionOperationPermisisonService contractActionOperationPermisisonService,
            IInsurancePolicyService insurancePolicyService,
            IInsurancePoliceRequestService insurancePoliceRequestService, 
            ICashOrderService cashOrderService,
            ContractActionRepository contractActionRepository) :
            base(blackoutRepository, contractDiscountRepository, holidayRepository,
                accountService, accountSettingService, postponementService, contractService,
                contractActionOperationPermisisonService, cashOrderService)

        {
            _payTypeRepository = payTypeRepository;
            _loanProductTypeRepository = loanProductTypeRepository;
            _branchService = branchService;
            _businessOperationService = businessOperationService;
            _businessOperationSettingService = businessOperationSettingService;
            _accountService = accountService;
            _accountSettingService = accountSettingService;
            _paymentOrderService = paymentOrderService;
            _contractService = contractService;
            _insurancePolicyService = insurancePolicyService;
            _insurancePoliceRequestService = insurancePoliceRequestService;
            _cashOrderService = cashOrderService;
            _contractActionRepository = contractActionRepository;
        }

        private bool GetInitialFeeValue(ContractStatus contractStatus, ContractActionType contractActionType)
        {
            switch (contractStatus)
            {
                case ContractStatus.AwaitForInitialFee:   return true;
                case ContractStatus.PositionRegistration: return contractActionType == ContractActionType.PrepaymentReturn ? true : false;
                default: return false;
            }
        }

        private bool GetAllowZeroValue(ContractActionType contractActionType)
        {
            switch (contractActionType)
            {
                case ContractActionType.RestructuringCred:
                case ContractActionType.RestructuringTranches:
                case ContractActionType.RestructuringTransferToTransitCred:
                case ContractActionType.RestructuringTransferToTransitTranches:
                case ContractActionType.RestructuringTransferToAccountCred:
                case ContractActionType.RestructuringTransferToAccountTranches:
                case ContractActionType.PrepaymentReturn:
                case ContractActionType.Prepayment: return true;
                default: return false;
            }
        }

        public List<ContractActionRow> Build(Contract contract, ContractDutyCheckModel model, int? branchId = null, decimal refinance = 0)
        {
            _actionType = model.ActionType;
            Init(contract, model.Date, _actionType, refinance: refinance);
            if (!branchId.HasValue)
                branchId = contract.BranchId;

            _branch = _branchService.GetAsync(branchId.Value).Result;
            if (_branch == null)
                throw new PawnshopApplicationException($"Филиал {branchId} не найден");

            var operationCode = _businessOperationService.GetOperationCode(
                _actionType,
                contract.CollateralType,
                model.EmployeeId.HasValue,
                model.IsReceivable.HasValue && model.IsReceivable.Value,
                GetInitialFeeValue(contract.Status, _actionType));

            var operation = _businessOperationService.FindBusinessOperation(contract.ContractTypeId,
                operationCode,
                _branch.Id,
                _branch.OrganizationId);

            if (string.IsNullOrWhiteSpace(operationCode) || operation == null) throw new PawnshopApplicationException("Бизнес-операция не найдена!");

            Reason = $"{(operation?.Name ?? "Неизвестно")} по договору {contract?.ContractNumber} от {contract.ContractDate:dd.MM.yyyy}";

            // инициализация id счетов credit, debit, penalty и duty
            InitAccounts(contract);

            var rows = new List<ContractActionRow>();
            //var inscription = contract.Inscription != null && contract.Inscription.Status == InscriptionStatus.Executed ? contract.Inscription : null;
            PayType payType = null;
            if (model.PayTypeId.HasValue)
            {
                payType = _payTypeRepository.Get(model.PayTypeId.Value);
            }

            if (_actionType == ContractActionType.Buyout ||
               _actionType == ContractActionType.PartialBuyout ||
               _actionType == ContractActionType.PrepareSelling ||
               _actionType == ContractActionType.Selling ||
               _actionType == ContractActionType.Transfer ||
               _actionType == ContractActionType.Refinance ||
               _actionType == ContractActionType.Prolong ||
               _actionType == ContractActionType.MonthlyPayment ||
               _actionType == ContractActionType.Prepayment ||
               _actionType == ContractActionType.PrepaymentReturn ||
               _actionType == ContractActionType.PrepaymentFromTransit ||
               _actionType == ContractActionType.PrepaymentToTransit ||
               _actionType == ContractActionType.PenaltyLimitAccrual ||
               _actionType == ContractActionType.PenaltyLimitWriteOff ||
               _actionType == ContractActionType.Addition ||
               _actionType == ContractActionType.CreditLineClose ||
               _actionType == ContractActionType.RestructuringCred ||
               _actionType == ContractActionType.RestructuringTranches ||
               _actionType == ContractActionType.RestructuringTransferToTransitCred ||
               _actionType == ContractActionType.RestructuringTransferToTransitTranches ||
               _actionType == ContractActionType.RestructuringTransferToAccountCred ||
               _actionType == ContractActionType.RestructuringTransferToAccountTranches ||
               _actionType == ContractActionType.BuyoutRestructuringTranches ||
               _actionType == ContractActionType.BuyoutRestructuringCred)
            {
                BuildRowsForDefaultBusinessOperation(contract, rows, operation, model.Date, payType, allowZero: GetAllowZeroValue(_actionType), inscription: contract.Inscription);
            }
            else if (_actionType == ContractActionType.Sign)
            {
                BuildRowsForSign(contract, rows, operation, model.Date, payType, refinance);
            }
            else if (_actionType == ContractActionType.Payment)
            {
                BuildRowsForPaymentWithOrder(contract, rows, operation, model.Cost, model.Date, payType);
            }
            else if (_actionType == ContractActionType.PartialPayment)
            {
                BuildRowsForPartialPayment(contract, rows, operation, model.Cost, model.Date, payType);
                //BuildRowsForPartialPaymentWithOrder (contract, rows, operation, model.Cost, model.Date, payType);
                rows = rows.Where(x => x.Cost > 0).ToList();
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(_actionType));
            }
            return rows;

        }

        private AccountSettings GetAccountSettings(CollateralType collateralType, AmountType paymentType, bool isTransfered)
        {
            try
            {
                var setting = _branch.Configuration.CashOrderSettings.Get(collateralType);
                return paymentType switch
                {
                    AmountType.Debt => isTransfered
                        ? _branch.Configuration.CashOrderSettings.GetTransfered(collateralType).DebtSettings
                        : setting.DebtSettings,
                    AmountType.Loan => isTransfered
                        ? _branch.Configuration.CashOrderSettings.GetTransfered(collateralType).LoanSettings
                        : setting.LoanSettings,
                    AmountType.Penalty => isTransfered
                        ? _branch.Configuration.CashOrderSettings.GetTransfered(collateralType).PenaltySettings
                        : setting.PenaltySettings,
                    AmountType.Duty => isTransfered
                        ? _branch.Configuration.CashOrderSettings.GetTransfered(collateralType).DutySettings
                        : setting.DutySettings,
                    _ => throw new ArgumentOutOfRangeException(nameof(paymentType))
                };
            }
            catch (NullReferenceException)
            {
                throw new PawnshopApplicationException($"Не найдены настройки для {collateralType.GetDisplayName()} - {paymentType.GetDisplayName()}, {_branch.Configuration.LegalSettings.LegalName} - {_branch.DisplayName}");
            }
        }

        public void InitAccounts(Contract contract)
        {
            _accountService.OpenForContract(contract);

            _accounts = _accountService.List(new ListQueryModel<AccountFilter>
            {
                Page = null,
                Model = new AccountFilter
                {
                    ContractId = contract.Id
                }
            }).List;
        }

        private decimal CalculateAmount(BusinessOperationSetting setting, DateTime date, Inscription inscription = null, decimal cost = 0)
        {
            switch (_actionType)
            {
                case ContractActionType.Sign:
                    return setting.AmountType switch
                    {
                        AmountType.Debt => BuyoutRow.DebtAmount,
                        AmountType.Prepayment => PrepaymentCost,
                        AmountType.PartialPayment => 0,
                        AmountType.Addition => 0,
                        AmountType.InsurancePremium => 0,
                        AmountType.PenaltyLimit => 0,
                        AmountType.CreditLineLimit => CreditLineCost,
                        AmountType.Refinance => Refinance,
                        _ => AmountTypeNotFoundError(setting.AmountType)
                    };
                case ContractActionType.MonthlyPayment:
                    return setting.AmountType switch
                    {
                        AmountType.Debt => MonthlyRow.DebtAmount,
                        AmountType.Loan => MonthlyRow.PercentAmount,
                        _ => CalculateAmountByAmountType(date, setting.AmountType)
                    };
                case ContractActionType.Prolong:
                    if (setting.AmountType == AmountType.Prepayment)
                        return 0;

                    return CalculateAmountByAmountType(date, setting.AmountType);
                case ContractActionType.Buyout:
                case ContractActionType.BuyoutRestructuringCred:
                case ContractActionType.Payment:
                case ContractActionType.CreditLineClose:
                    return setting.AmountType switch
                    {
                        AmountType.Prepayment => cost,
                        _ => CalculateAmountByAmountType(date, setting.AmountType)
                    };
                case ContractActionType.Addition:
                    return setting.AmountType switch
                    {
                        AmountType.Addition => 0,
                        AmountType.Prepayment => 0,
                        AmountType.AdditionDebtPayment => 0,
                        AmountType.AdditionLoanPayment => 0,
                        _ => CalculateAmountByAmountType(date, setting.AmountType)
                    };
                case ContractActionType.PartialPayment:
                    return setting.AmountType switch
                    {
                        AmountType.PartialPayment => 0,
                        AmountType.PartialPaymentOverdueDebtReturn => 0,
                        AmountType.Debt => cost,
                        _ => CalculateAmountByAmountType(date, setting.AmountType)
                    };
                case ContractActionType.PrepaymentReturn:
                    return setting.AmountType switch
                    {
                        AmountType.Prepayment => Math.Floor(PrepaymentCost),
                        _ => AmountTypeNotFoundError(setting.AmountType)
                    };
                case ContractActionType.Transfer:
                    return setting.AmountType switch
                    {
                        _ => CalculateAmountByAmountType(date, setting.AmountType)
                    };
                case ContractActionType.RestructuringTranches:
                case ContractActionType.RestructuringCred:
                    return setting.AmountType switch
                    {
                        AmountType.AmortizedLoan => CalculateAmountByAmountType(date, true, AmountType.Loan),
                        AmountType.AmortizedOverdueLoan => CalculateAmountByAmountType(date, true, AmountType.OverdueLoan),
                        AmountType.AmortizedDebtPenalty => CalculateAmountByAmountType(date, true, AmountType.DebtPenalty),
                        AmountType.AmortizedLoanPenalty => CalculateAmountByAmountType(date, true, AmountType.LoanPenalty),
                        AmountType.Debt => CalculateAmountByAmountType(date, true, AmountType.OverdueDebt),
                        _ => CalculateAmountByAmountType(date, setting.AmountType)
                    };
                case ContractActionType.RestructuringTransferToTransitCred:
                case ContractActionType.RestructuringTransferToTransitTranches:
                    return setting.AmountType switch
                    {
                        AmountType.Debt => BuyoutRow.DebtAmount,
                        _ => AmountTypeNotFoundError(setting.AmountType)
                    };
                case ContractActionType.RestructuringTransferToAccountCred:
                case ContractActionType.RestructuringTransferToAccountTranches:
                    return setting.AmountType switch
                    {
                        _ => GetTransitAccountBalance()
                    };
                case ContractActionType.BuyoutRestructuringTranches:
                    return setting.AmountType switch
                    {
                        _ => CalculateAmountByAmountType(date, setting.AmountType)
                    };
                case ContractActionType.Prepayment:
                    return setting.AmountType switch
                    {
                        AmountType.Prepayment => 0,
                        _ => AmountTypeNotFoundError(setting.AmountType)
                    };
                case ContractActionType.Selling:
                case ContractActionType.PrepaymentToTransit:
                case ContractActionType.PrepaymentFromTransit:
                case ContractActionType.PenaltyLimitAccrual:
                case ContractActionType.PenaltyLimitWriteOff:
                    return setting.AmountType switch
                    {
                        _ => 0
                    };
                
                default:
                    throw new ArgumentOutOfRangeException(
                        $"Для выбранного действия {_actionType.GetDisplayName()}не найдены настройки");
            }
            /*Старое заполнение
            return setting.AmountType switch
            {
                AmountType.Debt => inscription?.Rows?.Where(x=>x.PaymentType == setting.AmountType)?.Sum(x=>x.Cost) ?? BuyoutRow.DebtAmount,
                AmountType.OverdueDebt => inscription?.Rows?.Where(x => x.PaymentType == setting.AmountType)?.Sum(x => x.Cost) ?? 0,
                AmountType.Loan => inscription?.Rows?.Where(x => x.PaymentType == setting.AmountType)?.Sum(x => x.Cost) ?? BuyoutRow.PercentAmount,
                AmountType.OverdueLoan => inscription?.Rows?.Where(x => x.PaymentType == setting.AmountType)?.Sum(x => x.Cost) ?? 0,
                AmountType.Penalty => inscription?.Rows?.Where(x => x.PaymentType == setting.AmountType)?.Sum(x => x.Cost) ?? BuyoutRow.PenaltyAmount,
                AmountType.DebtPenalty => inscription?.Rows?.Where(x => x.PaymentType == setting.AmountType)?.Sum(x => x.Cost) ?? 0,
                AmountType.LoanPenalty => inscription?.Rows?.Where(x => x.PaymentType == setting.AmountType)?.Sum(x => x.Cost) ?? 0,
                AmountType.Duty => inscription?.Rows?.Where(x => x.PaymentType == setting.AmountType)?.Sum(x => x.Cost) ?? BuyoutRow.PenaltyAmount,
                AmountType.Prepayment => inscription?.Rows?.Where(x => x.PaymentType == setting.AmountType)?.Sum(x => x.Cost) ?? 0,
                AmountType.PartialPayment => cost,
                AmountType.Addition => cost,
                _ => throw new ArgumentOutOfRangeException($"Для выбранного значения {setting.AmountType}({setting.AmountType.GetDisplayName()}) невозможно расчитать сумму!")
            };
            */
        }

        private decimal AmountTypeNotFoundError(AmountType amountType)
        {
            throw new ArgumentOutOfRangeException(
                $"Для выбранного действия({_actionType.GetDisplayName()}), для значения {amountType}({amountType.GetDisplayName()}) невозможно рассчитать сумму!");
        }

        public Dictionary<AmountType, decimal> GetDistinctRowAmounts (List<ContractActionRow> rows, HashSet<AmountType> loanAmountTypes)
        {
            if (!rows.Any()) return null;
            if (!loanAmountTypes.Any())
                throw new ArgumentNullException($"Не заполнен список сумм {nameof(loanAmountTypes)} для расчета");
            
            var loanAmountDict = new Dictionary<AmountType, decimal>();
            var loanRows = new List<ContractActionRow>();
            foreach (ContractActionRow row in rows)
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

            return loanAmountDict;

        }

        private void BuildRowsForDefaultBusinessOperation(IContract contract, List<ContractActionRow> rows, BusinessOperation operation, 
                                                          DateTime date, PayType payType = null, Inscription inscription = null, bool allowZero = false)
        {
            List<BusinessOperationSetting> settings = _businessOperationSettingService.List(new ListQueryModel<BusinessOperationSettingFilter>
            {
                Page = null,
                Model = new BusinessOperationSettingFilter
                {
                    BusinessOperationId = operation.Id,
                    IsActive = true,
                    PayTypeId = payType?.Id
                }
            }).List;

            if (!settings.Any()) throw new PawnshopApplicationException($"Настройки для выбранной операции(Код:{operation.Code}; Наименование:{operation.Name}; Идентификатор:{operation.Id}) не найдены!");
            foreach (var setting in settings.OrderBy(x => x.OrderBy))
            {
                ContractActionRow row = new ContractActionRow()
                {
                    PaymentType = setting.AmountType,
                    BusinessOperationSettingId = setting.Id,
                    BusinessOperationSetting = setting
                };

                RowBuildFromSetting(contract, row, setting, date, inscription);

                if (row.Cost != 0) rows.Add(row);
                if (row.Cost == 0 && allowZero) rows.Add(row);
            }

        }


        private void BuildRowsForSign(Contract contract, List<ContractActionRow> rows, BusinessOperation operation, DateTime date, PayType payType = null, decimal refinance = 0)
        {
            List<BusinessOperationSetting> settings = _businessOperationSettingService.List(new ListQueryModel<BusinessOperationSettingFilter>
            {
                Page = null,
                Model = new BusinessOperationSettingFilter
                {
                    BusinessOperationId = operation.Id,
                    IsActive = true,
                    PayTypeId = payType?.Id
                }
            }).List;

            bool isBuyCarProduct = default;

            if (contract.ProductTypeId.HasValue)
            {
                LoanProductType productType = _loanProductTypeRepository.Get(contract.ProductTypeId.Value);
                isBuyCarProduct = productType.Code == Constants.PRODUCT_BUYCAR;
            }

            if (!settings.Any()) throw new PawnshopApplicationException("Настройки для выбранной операции не найдены!");
            foreach (var setting in settings.OrderBy(x => x.OrderBy))
            {
                ContractActionRow row = new ContractActionRow()
                {
                    PaymentType = setting.AmountType,
                    BusinessOperationSettingId = setting.Id,
                    BusinessOperationSetting = setting
                };

                RowBuildFromSetting(contract, row, setting, date);

                if (row.PaymentType == AmountType.Debt)
                {
                    if (refinance == 0)
                        row.Cost = contract.LoanCost;
                    else
                        row.Cost = contract.LoanCost - refinance;
                }

                if (row.PaymentType == AmountType.Prepayment && isBuyCarProduct)
                    row.Cost = contract.LoanCost + PrepaymentCost;


                if (!contract.PartialPaymentParentId.HasValue)
                {
                    decimal insurancePremium = 0;
                    var loanPercentSetting = _contractService.GetSettingForContract(contract);

                    if (loanPercentSetting.IsInsuranceAvailable)
                    {
                        var insPolReq = _insurancePoliceRequestService.GetLatestPoliceRequestAllStatus(contract.Id);
                        if (insPolReq != null)
                            insurancePremium = insPolReq.RequestData.InsurancePremium;
                    }

                    if (row.PaymentType == AmountType.Debt ||
                        (row.PaymentType == AmountType.Prepayment && isBuyCarProduct))
                        row.Cost -= insurancePremium;

                    if (row.PaymentType == AmountType.InsurancePremium)
                        row.Cost = insurancePremium;
                }

                if (contract.UsePenaltyLimit)
                    if (row.PaymentType == AmountType.PenaltyLimit)
                        row.Cost = contract.LoanCost * 0.1M;

                if (row.Cost != 0) rows.Add(row);
            }
        }

        private void BuildRowsForPartialPayment(Contract contract, List<ContractActionRow> rows,
            BusinessOperation operation, decimal cost, DateTime operationDate, PayType payType = null)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            decimal costCounter = cost;
            if (costCounter == 0)
                return;

            BuildRowsForDefaultBusinessOperation(contract, rows, operation, operationDate, payType, allowZero: true);

            var loanAmountTypes = new HashSet<AmountType>
            {
                AmountType.OverdueDebt,
                AmountType.Loan,
                AmountType.OverdueLoan,
                AmountType.DebtPenalty,
                AmountType.LoanPenalty,
                AmountType.Receivable,
                AmountType.DefermentLoan,
                AmountType.AmortizedLoan,
                AmountType.AmortizedDebtPenalty,
                AmountType.AmortizedLoanPenalty
            };

            Dictionary<AmountType, decimal> loanAmountDict;

            loanAmountDict = GetDistinctRowAmounts(rows, loanAmountTypes);
            decimal loanAmount = 0;
            if (loanAmountDict != null && loanAmountDict.Count > 0)
                loanAmount = loanAmountDict.Values.Sum();


            decimal debtAmount = _contractService.GetAccountBalance(contract.Id, operationDate);
            decimal overdueDebtAmount = 0;
            if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
                overdueDebtAmount = _contractService.GetOverdueAccountBalance(contract.Id, operationDate);

            if (debtAmount + overdueDebtAmount <= 0)
                throw new PawnshopApplicationException("У договора нет остатка долга, для погашения других видов долга воспользуйтесь операцией ОПЛАТА");

            if (cost >= debtAmount + overdueDebtAmount)
                throw new PawnshopApplicationException($"Сумма ЧДП {cost} не может превышать общую сумму задолженности {debtAmount + overdueDebtAmount} по договору");

            decimal depoBalance = _contractService.GetPrepaymentBalance(contract.Id, operationDate);
            decimal expenseBalance = _contractService.GetExtraExpensesCost(contract.Id, operationDate);

            decimal prePaymentAmount = loanAmount + cost + expenseBalance - depoBalance;
            if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
                loanAmount -= overdueDebtAmount;

            var amountTypesForUpdate = new HashSet<AmountType>
            {
                AmountType.Debt,
                AmountType.PartialPayment,
                AmountType.PartialPaymentOverdueDebtReturn,
                AmountType.DefermentLoan,
                AmountType.AmortizedLoan,
                AmountType.AmortizedLoanPenalty,
                AmountType.AmortizedDebtPenalty
            };

            var amountTypesUpdated = new HashSet<AmountType>();
            foreach (ContractActionRow row in rows)
            {
                if (contract.ContractClass == ContractClass.Credit)
                {
                    if (!amountTypesForUpdate.Contains(row.PaymentType) && !amountTypesUpdated.Contains(row.PaymentType))
                    {
                        amountTypesUpdated.Add(row.PaymentType);
                        cost -= row.Cost;
                        continue;
                    }

                    if (amountTypesUpdated.Contains(row.PaymentType))
                    {
                        continue;
                    }

                    if (cost > 0 && row.Cost != 0)
                    {
                        if (cost >= row.Cost)
                        {
                            cost -= row.Cost;
                        }
                        else
                        {
                            row.Cost = cost;
                            cost = 0;
                        }
                    }
                    else
                    {
                        row.Cost = 0;
                    }

                    if (cost == 0)
                    {
                        continue;
                    }
                }

                if (row.PaymentType == AmountType.Debt)
                {
                    row.Cost = cost;
                }

                if (row.PaymentType == AmountType.PartialPayment && debtAmount + overdueDebtAmount > 0)
                    row.Cost = debtAmount + overdueDebtAmount - cost;

                if (row.PaymentType == AmountType.PartialPaymentOverdueDebtReturn && contract.PercentPaymentType == PercentPaymentType.EndPeriod)
                {
                    row.Cost = overdueDebtAmount;
                    foreach (ContractActionRow overdueDebtRow in rows.Where(x => x.PaymentType == AmountType.OverdueDebt))
                    {
                        overdueDebtRow.Cost = 0;
                    }
                }
            }
        }

        private void BuildRowsForPartialPaymentWithOrder(IContract contract, List<ContractActionRow> rows,
            BusinessOperation operation, decimal cost, DateTime operationDate, PayType payType = null)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));
            
            decimal costCounter = cost;
            if (costCounter == 0)
                return;

            BuildRowsForPaymentWithOrder(contract, rows, operation, cost, operationDate, payType);

            var loanAmountTypes = new HashSet<AmountType>
            {
                AmountType.Debt,
                AmountType.OverdueDebt,
                AmountType.Loan,
                AmountType.OverdueLoan,
                AmountType.DebtPenalty,
                AmountType.LoanPenalty
            };

            Dictionary<AmountType, decimal> loanAmountDict;

            loanAmountDict = GetDistinctRowAmounts(rows, loanAmountTypes);

            decimal totalPaymentBeforePartial = 0;
            decimal totalPaymentBeforePartialWithScheduleDebt = 0;
            
            if (loanAmountDict!= null && loanAmountDict.Count > 0)
            {
                totalPaymentBeforePartialWithScheduleDebt += loanAmountDict.Values.Sum();
                decimal paidDebt = 0;
                if (!loanAmountDict.TryGetValue(AmountType.Debt, out paidDebt))
                    paidDebt = 0;
                totalPaymentBeforePartial = totalPaymentBeforePartialWithScheduleDebt - paidDebt;
            }

            if (cost == totalPaymentBeforePartialWithScheduleDebt)
                throw new PawnshopApplicationException($"Суммы операции {cost} достаточно только на погашение текущей задолженности, воспользуйтесь операцией ОПЛАТА");

            decimal debtBalance = 0;
            
            debtBalance = _contractService.GetAccountBalance(contract.Id, operationDate);
            
            if (contract.PercentPaymentType == PercentPaymentType.EndPeriod && contract.MaturityDate.Date < operationDate.Date)
                debtBalance += _contractService.GetOverdueAccountBalance(contract.Id, operationDate);

            if (debtBalance == 0)
                throw new PawnshopApplicationException("У договора нет остатка долга, для погашения других видов долга воспользуйтесь операцией ОПЛАТА");

            decimal partialPaymentMoveAmount = debtBalance + totalPaymentBeforePartial - cost;

            if (partialPaymentMoveAmount <= 0)
                throw new PawnshopApplicationException($"Недостаточная сумма {partialPaymentMoveAmount} для переноса на новый договор");

            var settings = _businessOperationSettingService.List(new ListQueryModel<BusinessOperationSettingFilter>
            {
                Page = null,
                Model = new BusinessOperationSettingFilter
                {
                    BusinessOperationId = operation.Id,
                    IsActive = true,
                    PayTypeId = payType?.Id,
                }
            }).List;

            if (!settings.Any())
                throw new PawnshopApplicationException($"Не найдены настройки для операции {operation.Name} с типом {operation.TypeId}");

            var amountTypesForUpdate = new HashSet<AmountType>
            {
                AmountType.Debt,
                AmountType.PartialPayment,
                AmountType.PartialPaymentOverdueDebtReturn
            };

            foreach (BusinessOperationSetting setting in settings.OrderBy(x => x.OrderBy))
            {
                if (!amountTypesForUpdate.Contains(setting.AmountType))
                    continue;

                if (setting.AmountType == AmountType.Debt && cost > totalPaymentBeforePartial)
                {
                    ContractActionRow debtRow = rows.Where(x => x.PaymentType == AmountType.Debt).FirstOrDefault();
                    if (debtRow == null)
                    {
                        ContractActionRow debtRowAdd = new ContractActionRow
                        {
                            PaymentType = AmountType.Debt,
                            Cost = 0,

                        };
                        RowBuildFromSetting(contract, debtRowAdd, setting, operationDate);

                        debtRowAdd.Cost = cost - totalPaymentBeforePartial;
                        rows.Add(debtRowAdd);
                    }
                    else
                    {
                        debtRow.Cost = cost - totalPaymentBeforePartial;
                    }
                }

                if (setting.AmountType == AmountType.PartialPayment && partialPaymentMoveAmount > 0)
                {
                    ContractActionRow partialRowAdd = new ContractActionRow
                    {
                        PaymentType = AmountType.PartialPayment,
                        Cost = 0,

                    };
                    RowBuildFromSetting(contract, partialRowAdd, setting, operationDate);
                    partialRowAdd.Cost = partialPaymentMoveAmount;
                    rows.Add(partialRowAdd);
                }

                if (setting.AmountType == AmountType.PartialPaymentOverdueDebtReturn && contract.PercentPaymentType == PercentPaymentType.EndPeriod)
                {
                    ContractActionRow returnRowAdd = new ContractActionRow
                    {
                        PaymentType = AmountType.PartialPaymentOverdueDebtReturn,
                        Cost = 0,

                    };
                    RowBuildFromSetting(contract, returnRowAdd, setting, operationDate);
                    returnRowAdd.Cost = _contractService.GetOverdueAccountBalance(contract.Id, operationDate);
                    rows.Add(returnRowAdd);
                }
            }

        }

        private void BuildRowsForPaymentWithOrder(IContract contract, List<ContractActionRow> rows,
            BusinessOperation operation, decimal cost, DateTime operationDate, PayType payType = null)
            {
            decimal costCounter = cost;
            if (costCounter == 0)
                return;

            var paymentOrders = _paymentOrderService.List(new ListQuery { Page = null }).List;

            if (!paymentOrders.Any())
                throw new PawnshopApplicationException("Не найден порядок погашения в справочнике \"Очередность погашения\"");

            List<ContractPaymentSchedule> paymentSchedule;
            paymentSchedule = _contractService.GetOnlyPaymentSchedule(contract.Id);

            bool paymentOnScheduleDateAllowed = false;
            DateTime scheduleDateForNextWorkingDate = default;
            HashSet<DateTime> paymentScheduleDates = default;

            if (paymentSchedule != null)
            {
                paymentScheduleDates =
                    paymentSchedule.Where(x => !x.Canceled.HasValue && !x.ActualDate.HasValue && CheckOnRestucturedScheduleItem(x.ActionId)).Select(x => x.Date).ToHashSet();

                paymentOnScheduleDateAllowed = paymentScheduleDates.Contains(operationDate.Date);

                if (!paymentOnScheduleDateAllowed)
                {
                    ContractPaymentSchedule currentPaymentSchedule =
                        paymentSchedule.Where(x => !x.Canceled.HasValue 
                                                && !x.ActualDate.HasValue 
                                                && x.NextWorkingDate.HasValue
                                                && x.Date < operationDate.Date
                                                && x.NextWorkingDate >= operationDate.Date).FirstOrDefault();

                    if (currentPaymentSchedule != default)
                    {
                        scheduleDateForNextWorkingDate = currentPaymentSchedule.Date;
                    }
                    paymentOnScheduleDateAllowed = scheduleDateForNextWorkingDate != default;
                }
            }

            foreach (var paymentOrder in paymentOrders.Where(x => x.NotOnScheduleDateAllowed || paymentOnScheduleDateAllowed).OrderBy(x => x.SequenceNumber))
            {
                var accountToTakeMoneyFrom =
                    _accounts.FirstOrDefault(account => account.AccountSettingId == paymentOrder.AccountSettingId);

                if (accountToTakeMoneyFrom == null)
                    continue;

                var accountSetting = _accountSettingService.GetAsync(accountToTakeMoneyFrom.AccountSettingId).Result;
                if (accountSetting.DefaultAmountType == null)
                    throw new PawnshopApplicationException($"Не найдена сумма по умолчанию в настройках счета {accountSetting.Name}, код счета {accountSetting.Code}, тип счета {accountSetting.TypeId}");

                decimal? debtLeftAmount = 0;

                if (contract.PercentPaymentType == PercentPaymentType.EndPeriod
                    && (accountSetting.DefaultAmountType == AmountType.Debt
                    || accountSetting.DefaultAmountType == AmountType.OverdueDebt))
                    continue;

                DateTime checkBalanceOn = operationDate;
                decimal lateAccrualAmount = 0;
                if (accountSetting.DefaultAmountType == AmountType.Loan && scheduleDateForNextWorkingDate != default)
                {
                    lateAccrualAmount = GetLateAccrualAmount(contract.Id, paymentSchedule, operationDate, scheduleDateForNextWorkingDate);
                    
                    checkBalanceOn = scheduleDateForNextWorkingDate;
                    debtLeftAmount =  _cashOrderService.GetContractTotalOperationAmount(contract.Id, 
                                                                                        new List<string> { Constants.BO_SETTING_PAYMENT_PROFIT }, 
                                                                                        scheduleDateForNextWorkingDate.Date.AddDays(1), 
                                                                                        operationDate.Date.AddDays(1)).Result;
                }

                decimal accountToTakeMoneyBalance = _accountService.GetAccountBalance(accountToTakeMoneyFrom.Id, checkBalanceOn) + lateAccrualAmount;
                accountToTakeMoneyFrom.Balance = Math.Abs(accountToTakeMoneyBalance);
                if (accountToTakeMoneyFrom.Balance == 0)
                    continue;

                // для ОД сверка с графиком
                if (accountSetting.DefaultAmountType == AmountType.Debt && paymentScheduleDates != null)
                {
                    if (paymentScheduleDates.Contains(operationDate.Date) || paymentScheduleDates.Contains(scheduleDateForNextWorkingDate))
                    {
                        debtLeftAmount = paymentSchedule
                            .Where(x => !x.Canceled.HasValue && (x.Date == operationDate.Date || x.Date == scheduleDateForNextWorkingDate))
                            .Select(x => x.DebtLeft).Single();

                        debtLeftAmount = debtLeftAmount.HasValue ? debtLeftAmount : 0;
                        debtLeftAmount = Math.Max(Math.Round(debtLeftAmount.Value, 2, MidpointRounding.AwayFromZero), 0);
                    }
                }
                
                decimal currentCost = 0;
                if (accountToTakeMoneyFrom.Balance > 0)
                {
                    currentCost = costCounter > Math.Abs(accountToTakeMoneyFrom.Balance) - debtLeftAmount.Value
                        ? (Math.Abs(accountToTakeMoneyFrom.Balance) - debtLeftAmount.Value)
                        : costCounter;

                    costCounter -= currentCost;
                }

                if (currentCost > 0)
                {
                    var settings = _businessOperationSettingService.List(new ListQueryModel<BusinessOperationSettingFilter>
                    {
                        Page = null,
                        Model = new BusinessOperationSettingFilter
                        {
                            BusinessOperationId = operation.Id,
                            IsActive = true,
                            PayTypeId = payType?.Id,
                            AmountType = accountSetting.DefaultAmountType
                        }
                    }).List;

                    if (!settings.Any())
                        throw new PawnshopApplicationException($"Не найдены настройки для операции {operation.Name} с типом {operation.TypeId} для суммы {nameof(accountSetting.DefaultAmountType)}");

                    foreach (var setting in settings.OrderBy(x => x.OrderBy))
                    {
                        ContractActionRow row = new ContractActionRow()
                        {
                            PaymentType = setting.AmountType,
                            BusinessOperationSettingId = setting.Id,
                            BusinessOperationSetting = setting
                        };

                        RowBuildFromSetting(contract, row, setting, operationDate);

                        row.Cost = Math.Round(currentCost, 2);

                        if (row.Cost != 0) rows.Add(row);
                    }
                }
                if (costCounter == 0) return;
            }
        }

        private bool CheckOnRestucturedScheduleItem(int? actionId)
        {
            if (actionId.HasValue)
            {
                var actionType = _contractActionRepository.Get(actionId.Value).ActionType;
                return actionType != ContractActionType.RestructuringCred && actionType != ContractActionType.RestructuringTranches;
            }
            return true;
        }

        /// <summary>
        /// Рассчитывает избыточную сумму начислений процентов в текущем периоде.
        /// Необходим для случаев, когда автомат не начислил проценты к КД, и произошло доначисление процентов в следующие дни после наступления КД.
        /// </summary>
        /// <param name="contractId"></param>
        /// <param name="paymentSchedule"></param>
        /// <param name="operationDate"></param>
        /// <param name="scheduleDateForNextWorkingDate"></param>
        /// <returns></returns>
        private decimal GetLateAccrualAmount(int contractId, List<ContractPaymentSchedule> paymentSchedule, DateTime operationDate, DateTime scheduleDateForNextWorkingDate)
        {
            ContractPaymentSchedule nextPaymentSchedule =
                        paymentSchedule.Where(x => !x.Canceled.HasValue
                                                && !x.ActualDate.HasValue
                                                && x.Date > operationDate.Date)
                                        .OrderBy(x => x.Date)
                                        .FirstOrDefault();

            if (operationDate.Date == scheduleDateForNextWorkingDate.Date || nextPaymentSchedule == default || nextPaymentSchedule.PercentCost == 0)
            {
                return default;
            }

            List<string> accrualBoSettings = new List<string>
                {
                    Constants.BO_SETTING_INTEREST_ACCRUAL,
                    Constants.BO_SETTING_INTEREST_ACCRUAL_MIGRATION,
                    Constants.BO_SETTING_INTEREST_ACCRUAL_OVERDUEDEBT,
                    Constants.BO_SETTING_INTEREST_ACCRUAL_ON_HOLIDAYS
                };

            DateTime lastAccrualDate = _cashOrderService.GetContractLastOperationDate(contractId, accrualBoSettings, operationDate.Date.AddDays(1)).Result;

            if (lastAccrualDate == default)
            {
                return default;
            }

            decimal nextPeriodAmount = nextPaymentSchedule.PercentCost;
            var period = nextPaymentSchedule.Period != 0 ? nextPaymentSchedule.Period : 30;

            decimal nextPeriodPlannedAccrual = nextPeriodAmount * (lastAccrualDate.Date - scheduleDateForNextWorkingDate.Date).Days / nextPaymentSchedule.Period;

            decimal nextPeriodFactAccrual = 0;
            nextPeriodFactAccrual = _cashOrderService.GetContractTotalOperationAmount(contractId,
                                                                                        accrualBoSettings,
                                                                                        scheduleDateForNextWorkingDate.Date.AddDays(1),
                                                                                        lastAccrualDate.Date.AddDays(1)).Result;

            if (nextPeriodFactAccrual == 0)
            {
                return default;
            }
            
            decimal result = nextPeriodFactAccrual - nextPeriodPlannedAccrual;
            return result > 0 ? result : default;
        }

        private void RowBuildFromSetting(IContract contract, ContractActionRow row, BusinessOperationSetting setting, DateTime date, Inscription inscription = null)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            if (row == null)
                throw new ArgumentNullException(nameof(row));

            if (setting == null)
                throw new ArgumentNullException(nameof(setting));

            if (_branch == null)
                throw new InvalidOperationException($"{nameof(_branch)} не инициализирован");

            if (setting.DebitSettingId.HasValue)
            {
                var debitSetting = _accountSettingService.GetAsync(setting.DebitSettingId.Value).Result;
                if (debitSetting == null)
                    throw new PawnshopApplicationException($"Настройка счета {setting.DebitSettingId.Value} не найдена");

                Account debitAccount = _businessOperationService.FindAccountForOperation(_accounts, debitSetting, contract, _branch);
                if (debitAccount != null)
                {
                    row.DebitAccount = debitAccount;
                    row.DebitAccountId = debitAccount.Id;
                }
            }

            if (setting.CreditSettingId.HasValue)
            {
                AccountSetting creditSetting = _accountSettingService.GetAsync(setting.CreditSettingId.Value).Result;
                if (creditSetting == null)
                    throw new PawnshopApplicationException($"Настройка счета {setting.CreditSettingId.Value} не найдена");

                Account creditAccount = _businessOperationService.FindAccountForOperation(_accounts, creditSetting, contract, _branch);
                if (creditAccount != null)
                {
                    row.CreditAccount = creditAccount;
                    row.CreditAccountId = creditAccount.Id;
                }
            }

            row.Cost = CalculateAmount(setting, date, inscription);
        }
    }
}
