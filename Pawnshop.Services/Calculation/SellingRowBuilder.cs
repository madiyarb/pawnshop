using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Discounts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Sellings;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Models.Calculation;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pawnshop.Services.Insurance;

namespace Pawnshop.Services.Calculation
{
    public class SellingRowBuilder : ContractActionRowBuilder, ISellingRowBuilder
    {
        private readonly ISessionContext _sessionContext;

        public SellingRowBuilder(BlackoutRepository blackoutRepository,
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
            IContractActionOperationPermisisonService contractActionOperationPermisisonService, ISessionContext sessionContext,
            IInsurancePolicyService insurancePolicyService,
            IInsurancePoliceRequestService insurancePoliceRequestService,
            ICashOrderService cashOrderService,
            ContractActionRepository contractActionRepository
            ) : base(blackoutRepository,
                contractDiscountRepository,
                payTypeRepository,
                loanProductTypeRepository,
                holidayRepository,
                branchService,
                businessOperationService,
                businessOperationSettingService,
                accountService,
                accountSettingService,
                accountPlanSettingService,
                postponementService,
                contractService,
                paymentOrderService,
                contractActionOperationPermisisonService,
                insurancePolicyService,
                insurancePoliceRequestService,
                cashOrderService,
                contractActionRepository

                )
        {
            _sessionContext = sessionContext;
        }
        private Group _branch;

        public Selling GetSellingDuty(Contract contract, Selling selling, bool saveDiscount, int? branchId = null)
        {
            selling.ActionRows = ActionRowBuild(contract);

            CalcPriceCost(contract, selling);

            var sellingActionRows = SellingRowBuild(contract, selling, branchId);
            if (sellingActionRows != null && sellingActionRows.Any())
            {
                selling.SellingActionRows = sellingActionRows;
            }

            selling.Discount = CreateDiscount(contract, selling, saveDiscount);

            RecalcActionRowsWithDisc(selling);

            return selling;
        }

        private void RecalcActionRowsWithDisc(Selling selling)
        {
            if (!selling.Discount.Discounts.Any()) return;
            if (selling.Discount.Discounts.Count>1)
                throw new PawnshopApplicationException($"По Договору с Id {selling.ContractId} больше одной скидки при Реализации");

            var amountTypeActionRowsDict = new Dictionary<AmountType, List<ContractActionRow>>();
            foreach (ContractActionRow actionRow in selling.ActionRows)
            {
                List<ContractActionRow> actionRows = null;
                if (!amountTypeActionRowsDict.TryGetValue(actionRow.PaymentType, out actionRows))
                {
                    actionRows = new List<ContractActionRow>();
                    amountTypeActionRowsDict[actionRow.PaymentType] = actionRows;
                }

                actionRows.Add(actionRow);
            }

            selling.Discount.Discounts.First().Rows.ForEach(r =>
            {
                List<ContractActionRow> actionRows;
                if (amountTypeActionRowsDict.TryGetValue(r.PaymentType, out actionRows))
                {
                    actionRows.ForEach(ar =>
                    {
                        if (r.PaymentType == ar.PaymentType)
                        {
                            decimal diff = ar.Cost - r.SubtractedCost;
                            if (diff < 0)
                                throw new PawnshopApplicationException($"На скидке по {AmountType.OverdueLoan.GetDisplayName()} обнаружен возможный остаток после выполнения действия");
                            ar.Cost = diff;
                        }
                    });
                }
            });
            selling.ActionRows = selling.ActionRows.Where(ar=>ar.Cost!=0).ToList();
        }

        //todo заменить на схему погашения для Реализации
        private List<SellingDiscountOrder> GetAmountTypeAccBalance(DateTime date)
        {
            var amountTypeAccBalance = new List<SellingDiscountOrder>();
        
            amountTypeAccBalance.Add(new SellingDiscountOrder() { DiscOrder = 1, AmountType = AmountType.OverdueLoan, Outstanding = CalculateAmountByAmountType(date, AmountType.OverdueLoan)});
            amountTypeAccBalance.Add(new SellingDiscountOrder() { DiscOrder = 2, AmountType = AmountType.Loan, Outstanding = CalculateAmountByAmountType(date, AmountType.Loan) });
            amountTypeAccBalance.Add(new SellingDiscountOrder() { DiscOrder = 3, AmountType = AmountType.DebtPenalty, Outstanding = CalculateAmountByAmountType(date, AmountType.DebtPenalty) });
            amountTypeAccBalance.Add(new SellingDiscountOrder() { DiscOrder = 4, AmountType = AmountType.LoanPenalty, Outstanding = CalculateAmountByAmountType(date, AmountType.LoanPenalty) });

            return amountTypeAccBalance;
        }

        private ContractDutyDiscount CreateDiscount(Contract contract, Selling selling, bool saveDiscount)
        {
            if (selling.PriceCost <= 0)
                throw new PawnshopApplicationException($"Некорректное значение суммы выкупа по Договору c Id {selling.ContractId}");

            var dutyDiscount = new ContractDutyDiscount { Discounts = new List<Discount>() };

            var date = selling.SellingDate ?? DateTime.Now;

            var contractDiscount = new ContractDiscount()
            {
                ContractId = contract.Id,
                IsTypical = false,
                BeginDate = date.Date,
                EndDate = date.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                CreateDate = DateTime.Now,
                AuthorId = _sessionContext.UserId
            };

            Discount discount = new Discount();
            
            var priceCost = selling.PriceCost; //себестоимость
            var expense = selling.ExtraExpensesCost; //расходы
            var totalDebit = CalculateAmountByAmountType(date, AmountType.Debt, AmountType.OverdueDebt);

            if (totalDebit > priceCost)
                throw new PawnshopApplicationException($"Суммы себестоимости недостаточно для погашения задолженности по основному долгу по Договору c Id {selling.ContractId}");
            if (totalDebit + expense > priceCost)
                throw new PawnshopApplicationException($"Суммы себестоимости недостаточно для погашения задолженности по основному долгу + расходы по Договору c Id {selling.ContractId}");

            priceCost = expense > priceCost ? 0 : priceCost - expense;
            priceCost = totalDebit > priceCost ? 0 : priceCost - totalDebit;

            foreach (var item in GetAmountTypeAccBalance(date).OrderBy(x=>x.DiscOrder))
            {
                var discSum = item.Outstanding > priceCost ? item.Outstanding - priceCost : 0;
                priceCost = discSum > 0 ? 0 : priceCost - item.Outstanding;
                if (discSum > 0)
                {
                    discount.Rows.Add(new DiscountRow
                    {
                        PaymentType = item.AmountType,
                        SubtractedCost = discSum,
                        OriginalCost = item.Outstanding
                    });

                    switch (item.AmountType)
                    {
                        case AmountType.OverdueLoan:
                            contractDiscount.OverduePercentDiscountSum = discSum;
                            break;
                        case AmountType.Loan:
                            contractDiscount.PercentDiscountSum = discSum;
                            break;
                        case AmountType.DebtPenalty:
                            contractDiscount.DebtPenaltyDiscountSum = discSum;
                            break;
                        case AmountType.LoanPenalty:
                            contractDiscount.PercentPenaltyDiscountSum = discSum;
                            break;
                    }
                }
            }

            if (discount.Rows.Count > 0)
            {
                if (saveDiscount) SaveContractDiscount(contractDiscount);

                discount.ContractDiscountId = contractDiscount.Id;
                discount.ContractDiscount = contractDiscount;

                dutyDiscount.Discounts.Add(discount);
            }

            return dutyDiscount;
        }

        private List<ContractActionRow> ActionRowBuild(Contract contract)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            var dutyRows = Build(contract, new ContractDutyCheckModel
            {
                ActionType = contract.ContractClass == ContractClass.Credit && contract.IsContractRestructured ? ContractActionType.BuyoutRestructuringCred : ContractActionType.Buyout,
                ContractId = contract.Id,
                Date = DateTime.Now
            });

            return dutyRows; 
        }

        private List<ContractActionRow> SellingRowBuild(Contract contract, Selling selling, int? branchId = null)
        {
            if (!branchId.HasValue)
                branchId = contract.BranchId;

            _branch = _branchService.GetAsync(branchId.Value).Result;
            if (_branch == null)
                throw new PawnshopApplicationException($"Филиал {branchId} не найден");

            var operationCode = _businessOperationService.GetOperationCode(
                ContractActionType.Selling,
                contract.CollateralType);

            var operation = _businessOperationService.FindBusinessOperation(contract.ContractTypeId,
                operationCode,
                _branch.Id,
                _branch.OrganizationId);

            List<BusinessOperationSetting> settings = _businessOperationSettingService.List(new ListQueryModel<BusinessOperationSettingFilter>
            {
                Page = null,
                Model = new BusinessOperationSettingFilter
                {
                    BusinessOperationId = operation.Id,
                    IsActive = true,
                    AmountType = (AmountType)selling.GetSellingPaymentType()
                }
            }).List;

            if (!settings.Any())
                throw new PawnshopApplicationException($"Настройки для выбранной операции(Код:{operation.Code}; Наименование:{operation.Name}; Идентификатор:{operation.Id}) не найдены!");

            var rows = new List<ContractActionRow>();

            foreach (var setting in settings.OrderBy(x => x.OrderBy))
            {
                ContractActionRow row = new ContractActionRow()
                {
                    PaymentType = setting.AmountType,
                    BusinessOperationSettingId = setting.Id,
                    BusinessOperationSetting = setting
                };

                SellingRowBuildFromSetting(contract, row, setting, selling);

                if (row.Cost != 0) rows.Add(row);
            }

            return rows;

        }

        private void SellingRowBuildFromSetting(IContract contract, ContractActionRow row, BusinessOperationSetting setting, Selling selling)
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

            row.Cost = Math.Abs(selling.GetDiffSellingCostAndPriceCost());
        }

        private void CalcPriceCost(Contract contract, Selling selling)
        {
            var date = selling.SellingDate ?? DateTime.Now;
            var extraExpensesCost = _contractService.GetExtraExpensesCost(contract.Id);

            if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
            {
                selling.PriceCost = BuyoutRow.DebtAmount + Math.Min(BuyoutRow.PercentAmount, contract.LoanPercentCost * Constants.SELLING_DISCRETE_LOAN_PERIOD);
            }
            else if (contract.PercentPaymentType == PercentPaymentType.AnnuityTwelve || contract.PercentPaymentType == PercentPaymentType.AnnuityTwentyFour || 
                     contract.PercentPaymentType == PercentPaymentType.AnnuityThirtySix || contract.PercentPaymentType == PercentPaymentType.Product)
            {
                selling.PriceCost = BuyoutRow.DebtAmount + Math.Min(BuyoutRow.PercentAmount, contract.PaymentSchedule.Where(x => !x.ActionId.HasValue).OrderBy(x => x.Date).Take(Constants.SELLING_ANNUITY_LOAN_MONTH).Sum(x => x.PercentCost));
            }

            if (extraExpensesCost > 0)
            {
                selling.ExtraExpensesCost = extraExpensesCost;
                selling.PriceCost += extraExpensesCost;
            }

            selling.PrepaymentBalance = _contractService.GetPrepaymentBalance(contract.Id);
        }
    }
}
