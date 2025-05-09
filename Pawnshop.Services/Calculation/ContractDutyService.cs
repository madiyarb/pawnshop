using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Discounts;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Models.Calculation;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using System.Collections.Generic;
using System.Linq;
using System;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Services.CreditLines;
using Discount = Pawnshop.Data.Models.Contracts.Actions.Discount;

namespace Pawnshop.Services.Calculation
{
    public class ContractDutyService : IContractDutyService
    {
        private readonly ContractRepository _contractRepository;
        private readonly ExpenseRepository _expenseRepository;
        private readonly ContractActionCheckRepository _contractActionCheckRepository;
        private readonly IContractExpenseService _contractExpenseService;
        private readonly IContractService _contractService;
        private readonly IContractActionRowBuilder _contractActionRowBuilder;
        private readonly IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> _accountSettingsService;
        private readonly ContractDiscountRepository _contractDiscountRepository;
        private readonly IAccountService _accountService;
        
        public ContractDutyService(ContractRepository contractRepository, ExpenseRepository expenseRepository,
            IContractExpenseService contractExpenseService, IContractActionRowBuilder contractActionRowBuilder,
            ContractActionCheckRepository contractActionCheckRepository, ContractDiscountRepository contractDiscountRepository,
            IContractService contractService, IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> accountSettingsService,
            IAccountService accountService)
        {
            _contractRepository = contractRepository;
            _expenseRepository = expenseRepository;
            _contractExpenseService = contractExpenseService;
            _contractActionRowBuilder = contractActionRowBuilder;
            _contractActionCheckRepository = contractActionCheckRepository;
            _contractDiscountRepository = contractDiscountRepository;
            _contractService = contractService;
            _accountSettingsService = accountSettingsService;
            _accountService = accountService;
        }

        public ContractDuty GetContractDuty(ContractDutyCheckModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var contract = _contractRepository.Get(model.ContractId);
            if (contract == null)
                throw new PawnshopApplicationException("Договор с таким идентификатором не найден");

            if (model.ActionType == ContractActionType.Buyout && contract.IsContractRestructured && contract.ContractClass == ContractClass.Credit)
                model.ActionType = ContractActionType.BuyoutRestructuringCred;

            ContractDiscount activeAtypicalContractDiscount = null;
            var discountableActionTypes = new HashSet<ContractActionType> {
                ContractActionType.Payment,
                ContractActionType.MonthlyPayment,
                ContractActionType.Prolong,
                ContractActionType.Buyout,
                ContractActionType.BuyoutRestructuringCred,
                ContractActionType.PartialPayment,
                ContractActionType.PartialBuyout,
                ContractActionType.Addition
            };

            List<ContractDiscount> contractDiscounts = _contractDiscountRepository.GetByContractId(contract.Id);
            List<ContractDiscount> activeContractDiscounts =
                contractDiscounts.Where(d => d.Status == ContractDiscountStatus.Accepted).ToList();

            if (activeContractDiscounts.Any(d => d.PersonalDiscountId.HasValue && d.PersonalDiscount == null))
                throw new PawnshopApplicationException($"{nameof(activeContractDiscounts)} содержит элементы с отсутствующими персональными скидками");

            List<ContractDiscount> activeTypicalContractDiscounts =
                activeContractDiscounts
                .Where(d => d.IsTypical
                    && d.PersonalDiscount.ActionType == model.ActionType).ToList();
            bool doesActionCanUseDiscount = discountableActionTypes.Contains(model.ActionType);
            if (doesActionCanUseDiscount)
            {
                List<ContractDiscount> activeAtypicalContractDiscounts = activeContractDiscounts.Where(d => !d.IsTypical).ToList();
                if (activeAtypicalContractDiscounts.Count > 0)
                {
                    activeAtypicalContractDiscount = activeAtypicalContractDiscounts.First();
                    if (activeAtypicalContractDiscounts.Count > 1)
                        throw new PawnshopApplicationException("По договору существует больше одной персональной скидки по сумме, обратитесь в тех. поддержку");
                }
            }

            List<ContractExpense> payableExpensesList = null;
            if (model.ActionType == ContractActionType.Payment || (model.ActionType == ContractActionType.Addition && model.Cost > 0))
            {
                if (activeAtypicalContractDiscount != null)
                {
                    decimal totalDue = _contractService.GetTotalDue(contract.Id, DateTime.Now);
                    model.Cost = totalDue;
                }

                payableExpensesList = new List<ContractExpense>();
                decimal tempTotalCost = model.Cost;
                List<Expense> expenseTypes = _expenseRepository.List(new ListQuery { Page = null });
                HashSet<int> extraExpenseTypesIds = expenseTypes.Where(e => e.ExtraExpense).Select(e => e.Id).ToHashSet();
                ListModel<ContractExpense> contractExpensesListModel = _contractExpenseService.List(new ListQueryModel<ContractExpenseFilter> { Page = null, Model = new ContractExpenseFilter { ContractId = contract.Id, IsPayed = false } });
                List<ContractExpense> contractExpenses = contractExpensesListModel.List;
                List<ContractExpense> contractExtraExpenses = contractExpenses.Where(e => extraExpenseTypesIds.Contains(e.ExpenseId)).ToList();

                if (contract.ContractClass == Data.Models.Contracts.ContractClass.Tranche)
                {
                    ListModel<ContractExpense> creditLineExpensesListModel = _contractExpenseService.List(new ListQueryModel<ContractExpenseFilter> { Page = null, Model = new ContractExpenseFilter { ContractId = contract.CreditLineId, IsPayed = false } });
                    List<ContractExpense> creditLineExpenses = creditLineExpensesListModel.List;
                    List<ContractExpense> creditLineExtraExpenses = creditLineExpenses.Where(e => extraExpenseTypesIds.Contains(e.ExpenseId)).ToList();
                    contractExtraExpenses.AddRange(creditLineExtraExpenses);
                }

                int contractExpensesWithApprovedOrdersCount = 0;
                foreach (ContractExpense contractExpense in contractExtraExpenses)
                {
                    if (tempTotalCost < 0)
                        break;

                    ContractExpense contractExpenseWithRowsAndOrders = _contractExpenseService.GetAsync(contractExpense.Id).Result;
                    if (contractExpenseWithRowsAndOrders == null)
                        throw new PawnshopApplicationException($"Ожидалось {nameof(contractExpenseWithRowsAndOrders)} не будет null");

                    // берем те расходы у которых все ордера апрувнуты
                    if (contractExpenseWithRowsAndOrders.ContractExpenseRows.Any(r => !r.ContractExpenseRowOrders.Any(r => r.Order.ApproveStatus != OrderStatus.Approved)))
                    {
                        tempTotalCost -= contractExpense.TotalCost;
                        if (tempTotalCost >= 0)
                            payableExpensesList.Add(contractExpense);

                        contractExpensesWithApprovedOrdersCount++;
                    }
                }

                model.Cost = tempTotalCost < 0 ? 0 : tempTotalCost;
            }

            _contractActionRowBuilder.Init(contract, model.Date, model.ActionType, refinance: model.Refinance.Value);
            decimal extraExpensesCost = _contractActionRowBuilder.ExtraExpensesCost;
            if (payableExpensesList != null)
            {
                extraExpensesCost = 0;
                if (payableExpensesList.Count > 0)
                    extraExpensesCost = payableExpensesList.Sum(e => e.TotalCost);
            }

            ContractDuty duty = new ContractDuty
            {
                Date = model.Date,
                DisplayAmountForOnlinePayment = _contractActionRowBuilder.DisplayAmount,
                ExtraExpensesCost = extraExpensesCost,
                ExtraContractExpenses = payableExpensesList,
                Checks = _contractActionCheckRepository.Find(new Core.Queries.ListQuery() { Page = null }, new { model.ActionType, model.PayTypeId }),
            };

            if (model.ActionType == ContractActionType.Addition ||
                model.ActionType == ContractActionType.PartialPayment)
            {
                //подтверждения для действий, которые делают порожденное подписание
                var signChecks = _contractActionCheckRepository.Find(new Core.Queries.ListQuery() { Page = null },
                        new { ActionType = ContractActionType.Sign, model.PayTypeId })
                    .Where(w => !duty.Checks.Any(x => w.Id == x.Id));

                duty.Checks.AddRange(signChecks);
            }

            if (contract.InscriptionId.HasValue)
            {
                if (contract.Inscription.Status == InscriptionStatus.Executed)
                    duty.Discount = null;
            }

            var amountTypeRowsDict = new Dictionary<AmountType, List<ContractActionRow>>();
            var amountTypeDict = new Dictionary<AmountType, decimal>();

            duty.Rows = _contractActionRowBuilder.Build(contract, model, model.BranchId, model.Refinance.Value);

            duty.Reason = _contractActionRowBuilder.Reason;
            foreach (ContractActionRow actionRow in duty.Rows)
            {
                decimal amount;
                if (amountTypeDict.TryGetValue(actionRow.PaymentType, out amount))
                {
                    if (amount != actionRow.Cost)
                        throw new PawnshopApplicationException("Суммы по одинаковым типам проводок не сходятся");
                }

                amountTypeDict[actionRow.PaymentType] = actionRow.Cost;
                List<ContractActionRow> actionRows = null;
                if (!amountTypeRowsDict.TryGetValue(actionRow.PaymentType, out actionRows))
                {
                    actionRows = new List<ContractActionRow>();
                    amountTypeRowsDict[actionRow.PaymentType] = actionRows;
                }

                actionRows.Add(actionRow);
            }

            var dutyDiscount = new ContractDutyDiscount { Discounts = new List<Discount>() };
            foreach (ContractDiscount contractDiscount in activeTypicalContractDiscounts)
            {
                Discount discount = new Discount
                {
                    ContractDiscountId = contractDiscount.Id,
                    ContractDiscount = contractDiscount,
                };
                dutyDiscount.Discounts.Add(discount);
            }

            if (activeAtypicalContractDiscount != null)
            {
                var discount = new Discount
                {
                    ContractDiscountId = activeAtypicalContractDiscount.Id,
                    ContractDiscount = activeAtypicalContractDiscount,
                    Rows = new List<DiscountRow>()
                };

                if (activeAtypicalContractDiscount.OverduePercentDiscountSum > 0)
                {
                    List<ContractActionRow> actionRows;
                    if (amountTypeRowsDict.TryGetValue(AmountType.OverdueLoan, out actionRows))
                    {
                        foreach (ContractActionRow actionRow in actionRows)
                        {
                            decimal diff = actionRow.Cost - activeAtypicalContractDiscount.OverduePercentDiscountSum;
                            if (diff < 0)
                                throw new PawnshopApplicationException($"На скидке по {AmountType.OverdueLoan.GetDisplayName()} обнаружен возможный остаток после выполнения действия");

                            actionRow.Cost = diff;
                        }

                        discount.Rows.Add(new DiscountRow
                        {
                            PaymentType = AmountType.OverdueLoan,
                            SubtractedCost = activeAtypicalContractDiscount.OverduePercentDiscountSum,
                            OriginalCost = _contractService.GetOverdueProfitBalance(contract.Id)
                        });
                    }
                }

                if (activeAtypicalContractDiscount.PercentDiscountSum > 0)
                {
                    List<ContractActionRow> actionRows;
                    if (amountTypeRowsDict.TryGetValue(AmountType.Loan, out actionRows))
                    {
                        foreach (ContractActionRow actionRow in actionRows)
                        {
                            decimal diff = actionRow.Cost - activeAtypicalContractDiscount.PercentDiscountSum;
                            if (diff < 0)
                                throw new PawnshopApplicationException($"На скидке по {AmountType.Loan.GetDisplayName()} обнаружен возможный остаток после выполнения действия");

                            actionRow.Cost = diff;
                        }

                        discount.Rows.Add(new DiscountRow
                        {
                            PaymentType = AmountType.Loan,
                            SubtractedCost = activeAtypicalContractDiscount.PercentDiscountSum,
                            OriginalCost = _contractService.GetProfitBalance(contract.Id)
                        });
                    }
                }

                if (activeAtypicalContractDiscount.DebtPenaltyDiscountSum > 0)
                {
                    List<ContractActionRow> actionRows;
                    if (amountTypeRowsDict.TryGetValue(AmountType.DebtPenalty, out actionRows))
                    {
                        foreach (ContractActionRow actionRow in actionRows)
                        {
                            decimal diff = actionRow.Cost - activeAtypicalContractDiscount.DebtPenaltyDiscountSum;
                            if (diff < 0)
                                throw new PawnshopApplicationException($"На скидке по {AmountType.DebtPenalty.GetDisplayName()} обнаружен возможный остаток после выполнения действия");

                            actionRow.Cost = diff;
                        }

                        discount.Rows.Add(new DiscountRow
                        {
                            PaymentType = AmountType.DebtPenalty,
                            SubtractedCost = activeAtypicalContractDiscount.DebtPenaltyDiscountSum,
                            OriginalCost = _contractService.GetPenyAccountBalance(contract.Id)
                        });
                    }
                }

                if (activeAtypicalContractDiscount.PercentPenaltyDiscountSum > 0)
                {
                    List<ContractActionRow> actionRows;
                    if (amountTypeRowsDict.TryGetValue(AmountType.LoanPenalty, out actionRows))
                    {
                        foreach (ContractActionRow actionRow in actionRows)
                        {
                            decimal diff = actionRow.Cost - activeAtypicalContractDiscount.PercentPenaltyDiscountSum;
                            if (diff < 0)
                                throw new PawnshopApplicationException($"На скидке по {AmountType.LoanPenalty.GetDisplayName()} обнаружен возможный остаток после выполнения действия");

                            actionRow.Cost = diff;
                        }

                        discount.Rows.Add(new DiscountRow
                        {
                            PaymentType = AmountType.LoanPenalty,
                            SubtractedCost = activeAtypicalContractDiscount.PercentPenaltyDiscountSum,
                            OriginalCost = _contractService.GetPenyProfitBalance(contract.Id)
                        });
                    }
                }

                if (discount.Rows.Count > 0)
                    dutyDiscount.Discounts.Add(discount);
            }

            duty.Discount = dutyDiscount;
            if (duty.Discount != null && duty.Discount.Discounts.Any() && (model.ActionType == ContractActionType.Transfer || model.ActionType == ContractActionType.ReTransfer))
            {
                duty.Rows.FirstOrDefault(x => x.PaymentType == AmountType.Loan).Cost -= duty.Discount.Discounts.FirstOrDefault().Rows.FirstOrDefault(x => x.PaymentType == AmountType.Loan).AddedCost;
                duty.Rows.FirstOrDefault(x => x.PaymentType == AmountType.Loan).Period -= duty.Discount.Discounts.FirstOrDefault().Rows.FirstOrDefault(x => x.PaymentType == AmountType.Loan).AddedDays;
            }

            AccountSetting depoAccountSetting = new AccountSetting();
            if (contract.ContractClass != Data.Models.Contracts.ContractClass.CreditLine)
            {
                /*
                var depoAccountSettingsFilterListQuery = new ListQueryModel<AccountSettingFilter>
                {
                    Model = new AccountSettingFilter
                    {
                        Code = Constants.ACCOUNT_SETTING_DEPO
                    }
                };
                ListModel<AccountSetting> depoAccountSettingsListModel = _accountSettingsService.List(depoAccountSettingsFilterListQuery);
                if (depoAccountSettingsFilterListQuery == null)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(depoAccountSettingsFilterListQuery)} не будет null");

                List<AccountSetting> depoAccountSettings = depoAccountSettingsListModel.List;
                if (depoAccountSettings == null)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(depoAccountSettings)} не будет null");

                if (depoAccountSettings.Count != 1)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(depoAccountSettings)} будет содержать только один элемент");

                depoAccountSetting = depoAccountSettings.Single();
                */
                var accountListQueryModel = new ListQueryModel<AccountFilter>
                {
                    Page = null,
                    Model = new AccountFilter
                    {
                        ContractId = contract.Id,
                        IsOpen = true,
                        SettingCodes = new List<string> { Constants.ACCOUNT_SETTING_DEPO }.ToArray()
                    }
                };
                ListModel<Account> accountListModel = _accountService.List(accountListQueryModel);
                if (accountListModel == null)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(accountListModel)} не будет null");

                if (accountListModel.List == null)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(accountListModel)}.{nameof(accountListModel.List)} не будет null");

                List<Account> accounts = accountListModel.List;
                Account account = accounts.Where(x => x.AccountSetting.Code == Constants.ACCOUNT_SETTING_DEPO && !x.DeleteDate.HasValue && !x.CloseDate.HasValue).FirstOrDefault();
                if (account == null)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(account)} не будет null");
                if (account.AccountSetting == null)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(account.AccountSetting)} не будет null");

                depoAccountSetting = account.AccountSetting;
            }

            var newAmountTypeDict = new Dictionary<AmountType, decimal>();
            foreach (ContractActionRow actionRow in duty.Rows)
            {
                BusinessOperationSetting businessOperationSetting = actionRow.BusinessOperationSetting;
                if (businessOperationSetting == null)
                    continue;

                if (businessOperationSetting.DebitSettingId == depoAccountSetting.Id)
                    newAmountTypeDict[actionRow.PaymentType] = actionRow.Cost;
            }

            decimal actionRowsTotalCost = newAmountTypeDict.Count > 0 ? newAmountTypeDict.Values.Sum() : 0;
            decimal cost = actionRowsTotalCost + extraExpensesCost;
            duty.Cost = cost;
            
            duty.BalanceWithoutPrepayment = _contractActionRowBuilder.DisplayAmountWithoutPrepayment;
            
            return duty;
        }
    }
}
