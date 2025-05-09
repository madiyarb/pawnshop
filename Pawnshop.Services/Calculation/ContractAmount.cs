using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Discounts;
using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Data.Models.Contracts.Postponements;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Models.Calculation;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Pawnshop.Services.Calculation
{
    public class ContractAmount : IContractAmount
    {
        private readonly BlackoutRepository _blackoutRepository;
        private readonly ContractDiscountRepository _contractDiscountRepository;
        private readonly List<Blackout> _blackouts;
        private List<ContractDiscount> _discounts;
        private List<Account> _accounts;
        public List<AccountSetting> _accountSettings;
        private DateTime _date = DateTime.Now.Date;

        private readonly IAccountService _accountService;
        private readonly IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> _accountSettingService;
        private readonly IPostponementService _postponementService;
        private readonly IContractService _contractService;
        private readonly IDictionaryWithSearchService<Holiday, HolidayFilter> _holidayService;
        private readonly IContractActionOperationPermisisonService _contractActionOperationPermisisonService;
        private readonly ICashOrderService _cashOrderService;

        public ContractAmount(BlackoutRepository blackoutRepository,
            ContractDiscountRepository contractDiscountRepository,
            IDictionaryWithSearchService<Holiday, HolidayFilter> holidayService,
            IAccountService accountService,
            IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> accountSettingService,
            IPostponementService postponementService,
            IContractService contractService,
            IContractActionOperationPermisisonService contractActionOperationPermisisonService, 
            ICashOrderService cashOrderService)
        {
            _blackoutRepository = blackoutRepository;
            _contractDiscountRepository = contractDiscountRepository;
            _holidayService = holidayService;
            _blackouts = new List<Blackout>();
            _accountService = accountService;
            _accountSettingService = accountSettingService;
            _postponementService = postponementService;
            _contractService = contractService;
            _contractActionOperationPermisisonService = contractActionOperationPermisisonService;
            _cashOrderService = cashOrderService;
        }

        private string GetAccountDepoSettingCode(Contract contract, ContractActionType? actionType)
        {
            bool checkContractStates()
            {
                if (contract.ProductType != null && contract.ProductTypeId.HasValue && contract.ProductType.Code == Constants.PRODUCT_BUYCAR && !contract.Locked)
                {
                    return true;
                }
                return false;
            }

            string depoSettingCode = Constants.ACCOUNT_SETTING_DEPO;

            if (checkContractStates() &&
                    (((contract.Status == ContractStatus.AwaitForSign || contract.Status == ContractStatus.InsuranceApproved) && actionType == ContractActionType.Sign) ||
                      (contract.Status == ContractStatus.PositionRegistration && actionType == ContractActionType.PrepaymentReturn))
               )
            {
                depoSettingCode = Constants.ACCOUNT_SETTING_DEPO_MERCHANT;
            }

            return depoSettingCode;
        }

        public void Init(Contract contract, DateTime? date = null, ContractActionType? actionType = null, bool balanceAccountsOnly = false, decimal refinance = 0)
        {

            Refinance = refinance;
            if (!date.HasValue)
                date = DateTime.Now;

            if (contract == null) throw new InvalidOperationException();

            //поиск скидок на дату расчета
            InitDiscountsAndBlackouts(contract.Id);

            //поиск счетов договора
            InitAccountsAndSettings(contract);

            Reason = contract.ContractNumber;

            _date = date.Value.Date;

            PrepaymentCost = 0;

            string depoSettingCode = GetAccountDepoSettingCode(contract, actionType);

            IEnumerable<Account> depoAccounts = _accounts
                .Join(_accountSettings, a => a.AccountSettingId, s => s.Id,
                    (a, o) => new { Setting = o, Account = a })
                .Where(model => model.Setting.Code.Equals(depoSettingCode))
                .Select(model => model.Account);

            foreach (var account in depoAccounts)
            {
                PrepaymentCost += _accountService.GetAccountBalance(account.Id, _date);
            }

            var penaltyAndBlackDays = CalculatePenaltyDays(contract);
            PenaltyDays = penaltyAndBlackDays.Item1;
            BlackDays = penaltyAndBlackDays.Item2;
            PenaltyMonthCount = CalculatePenaltyMonthCount(contract);

            if (PenaltyDays > 0 || BlackDays > 0) IsDelayed = true;

            if (!contract.BuyoutDate.HasValue)
            {
                NextPaymentDate = contract.NextPaymentDate;
                List<ContractPostponement> postponements = _postponementService.GetByContractId(contract.Id);
                if (postponements != null && postponements.Any())
                {
                    var postponementDate = postponements.Max(x => x.Date);
                    if (NextPaymentDate < postponementDate) NextPaymentDate = postponementDate;
                }
            }

            DutyDiscount = new ContractDutyDiscount();
            DutyDiscount.Discounts = new List<Discount>();

            ProlongRow = CalculateRow(contract, ContractAmountRowType.ProlongRow, PenaltyDays, BlackDays, date.Value, balanceAccountsOnly: balanceAccountsOnly);
            ProlongAmount = Math.Round(ProlongRow.DebtAmount + ProlongRow.PercentAmount + ProlongRow.PenaltyAmount + ProlongRow.ReceivableOnlinePaymentAmount, 2);

            BuyoutRow = CalculateRow(contract, ContractAmountRowType.BuyoutRow, PenaltyDays, BlackDays, date.Value, balanceAccountsOnly: balanceAccountsOnly);
            BuyoutAmount = Math.Round(BuyoutRow.DebtAmount + BuyoutRow.PercentAmount + BuyoutRow.PenaltyAmount + BuyoutRow.ReceivableOnlinePaymentAmount +
            BuyoutRow.DefermentLoan + BuyoutRow.AmortizedLoan + BuyoutRow.AmortizedDebtPenalty + BuyoutRow.AmortizedLoanPenalty, 2);

            if (contract.NextPaymentDate == date)
            {
                MonthlyRow = CalculateRow(contract, ContractAmountRowType.MonthlyRow, PenaltyDays, BlackDays, date.Value, balanceAccountsOnly: balanceAccountsOnly);
                MonthlyAmount = Math.Round(MonthlyRow.DebtAmount + MonthlyRow.PercentAmount + MonthlyRow.PenaltyAmount, 2);
            }

            if (contract.ContractClass == ContractClass.CreditLine)
            {
                CreditLineCost = contract.LoanCost;
            }

            #region персональные скидки

            //_discounts.ForEach(d =>
            //{
            //    Discount discount = new Discount();
            //    discount.ContractDiscount = d;
            //    discount.ContractDiscountId = d.Id;
            //    if (d.IsTypical)
            //    {
            //        if (d.PersonalDiscount.ActionType == ContractActionType.Prolong && d.PersonalDiscount.ActionType == actionType)
            //        {
            //            discount.Rows.AddRange(BuildDiscountRows(ProlongRow, d));
            //        }
            //        else if (d.PersonalDiscount.ActionType == ContractActionType.Buyout && d.PersonalDiscount.ActionType == actionType)
            //        {
            //            discount.Rows.AddRange(BuildDiscountRows(BuyoutRow, d));
            //        }
            //        else if (d.PersonalDiscount.ActionType == ContractActionType.PartialBuyout && d.PersonalDiscount.ActionType == actionType)
            //        {
            //            discount.Rows.AddRange(BuildDiscountRows(BuyoutRow, d));
            //        }
            //        else if (d.PersonalDiscount.ActionType == ContractActionType.PartialPayment && d.PersonalDiscount.ActionType == actionType)
            //        {
            //            discount.Rows.AddRange(BuildDiscountRows(BuyoutRow, d));
            //        }
            //        else if (d.PersonalDiscount.ActionType == ContractActionType.Addition && d.PersonalDiscount.ActionType == actionType)
            //        {
            //            discount.Rows.AddRange(BuildDiscountRows(BuyoutRow, d));
            //        }
            //        else if (d.PersonalDiscount.ActionType == ContractActionType.Refinance && d.PersonalDiscount.ActionType == actionType)
            //        {
            //            discount.Rows.AddRange(BuildDiscountRows(BuyoutRow, d));
            //        }
            //        else if (d.PersonalDiscount.ActionType == ContractActionType.MonthlyPayment)
            //        {
            //            discount.Rows.AddRange(BuildDiscountRows(MonthlyRow, d));
            //        }
            //    }
            //    else
            //    {
            //        if (d.PercentDiscountSum > 0)
            //        {
            //            discount.Rows.Add(new DiscountRow()
            //            {
            //                PaymentType = AmountType.Loan,
            //                SubtractedCost = d.PercentDiscountSum,
            //                OriginalCost = actionType == ContractActionType.MonthlyPayment ? MonthlyRow.PercentAmount : actionType == ContractActionType.Prolong ? ProlongRow.PercentAmount : BuyoutRow.PercentAmount
            //            });
            //            if (actionType == ContractActionType.MonthlyPayment)
            //            {
            //                MonthlyRow.PercentAmount -= d.PercentDiscountSum;
            //                MonthlyRow.PercentAmount = MonthlyRow.PercentAmount < 0 ? 0 : MonthlyRow.PercentAmount;
            //            }
            //            else if (actionType == ContractActionType.Prolong)
            //            {
            //                ProlongRow.PercentAmount -= d.PercentDiscountSum;
            //                ProlongRow.PercentAmount = ProlongRow.PercentAmount < 0 ? 0 : ProlongRow.PercentAmount;
            //            }
            //            else
            //            {
            //                BuyoutRow.PercentAmount -= d.PercentDiscountSum;
            //                BuyoutRow.PercentAmount = BuyoutRow.PercentAmount < 0 ? 0 : BuyoutRow.PercentAmount;
            //            }
            //        }
            //        if (d.PenaltyDiscountSum > 0)
            //        {
            //            discount.Rows.Add(new DiscountRow()
            //            {
            //                PaymentType = AmountType.Penalty,
            //                SubtractedCost = d.PenaltyDiscountSum,
            //                OriginalCost = actionType == ContractActionType.MonthlyPayment ? MonthlyRow.PenaltyAmount : actionType == ContractActionType.Prolong ? ProlongRow.PenaltyAmount : BuyoutRow.PenaltyAmount
            //            });
            //            if (actionType == ContractActionType.MonthlyPayment)
            //            {
            //                MonthlyRow.PenaltyAmount -= d.PenaltyDiscountSum;
            //                MonthlyRow.PenaltyAmount = MonthlyRow.PenaltyAmount < 0 ? 0 : MonthlyRow.PenaltyAmount;
            //            }
            //            else if (actionType == ContractActionType.Prolong)
            //            {
            //                ProlongRow.PenaltyAmount -= d.PenaltyDiscountSum;
            //                ProlongRow.PenaltyAmount = ProlongRow.PenaltyAmount < 0 ? 0 : ProlongRow.PenaltyAmount;
            //            }
            //            else
            //            {
            //                BuyoutRow.PenaltyAmount -= d.PenaltyDiscountSum;
            //                BuyoutRow.PenaltyAmount = BuyoutRow.PenaltyAmount < 0 ? 0 : BuyoutRow.PenaltyAmount;
            //            }
            //        }
            //    }

            //    if (discount.HasChanges)
            //    {
            //        DutyDiscount.Discounts.Add(discount);
            //    }
            //});

            #endregion


            if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
            {
                #region Массовые скидки(BlackOut/blackDays)

                /*
                if (BlackDays > 0)
                {

                    foreach (var blackout in _blackouts)
                    {
                        Discount discount = new Discount();
                        discount.Blackout = blackout;
                        discount.BlackoutId = blackout.Id;
                        if (ProlongRow.PercentAmount != ProlongRow.OriginalPercentAmount)
                            discount.Rows.Add(new DiscountRow()
                            {
                                PaymentType = AmountType.Loan,
                                AddedCost = ProlongRow.PercentAmount - ProlongRow.OriginalPercentAmount,
                                OriginalCost = ProlongRow.OriginalPercentAmount,
                                AddedDays = ProlongRow.PercentDays - ProlongRow.OriginalPercentDays,
                                OriginalDays = ProlongRow.OriginalPercentDays
                            });

                        if (ProlongRow.OriginalPenaltyAmount != ProlongRow.PenaltyAmount)
                            discount.Rows.Add(new DiscountRow()
                            {
                                PaymentType = AmountType.Penalty,
                                SubtractedCost = ProlongRow.OriginalPenaltyAmount - ProlongRow.PenaltyAmount,
                                OriginalCost = ProlongRow.OriginalPenaltyAmount,
                                SubtractedDays = ProlongRow.OriginalPenaltyDays - ProlongRow.PenaltyDays,
                                OriginalDays = ProlongRow.OriginalPenaltyDays
                            });

                        if (discount.HasChanges)
                        {
                            DutyDiscount.Discounts.Add(discount);
                        }
                    }
                }
                */

                #endregion

                DisplayAmountWithoutPrepayment = ProlongAmount + _contractService.GetExtraExpensesCost(contract.Id);
                DisplayAmountWithoutPrepaymentAndExpenses = ProlongAmount;
            }
            else if (contract.PercentPaymentType == PercentPaymentType.AnnuityTwelve ||
            contract.PercentPaymentType == PercentPaymentType.AnnuityTwentyFour ||
            contract.PercentPaymentType == PercentPaymentType.AnnuityThirtySix ||
            contract.PercentPaymentType == PercentPaymentType.Product)
            {
                #region Массовые скидки(BlackOut/blackDays)

                /*
                if (BlackDays > 0)
                {
                    foreach (var blackout in _blackouts)
                    {
                        Discount discount = new Discount();
                        discount.Blackout = blackout;
                        discount.BlackoutId = blackout.Id;
                        if (MonthlyRow.PercentAmount != MonthlyRow.OriginalPercentAmount)
                        {
                            if (actionType == ContractActionType.MonthlyPayment)
                                discount.Rows.Add(new DiscountRow()
                                {
                                    PaymentType = AmountType.Loan,
                                    AddedCost = MonthlyRow.PercentAmount - MonthlyRow.OriginalPercentAmount,
                                    OriginalCost = MonthlyRow.OriginalPercentAmount,
                                    AddedDays = MonthlyRow.PercentDays - MonthlyRow.OriginalPercentDays,
                                    OriginalDays = MonthlyRow.OriginalPercentDays
                                });
                            if (actionType == ContractActionType.Buyout)
                                discount.Rows.Add(new DiscountRow()
                                {
                                    PaymentType = AmountType.Loan,
                                    AddedCost = BuyoutRow.PercentAmount - BuyoutRow.OriginalPercentAmount,
                                    OriginalCost = BuyoutRow.OriginalPercentAmount,
                                    AddedDays = BuyoutRow.PercentDays - BuyoutRow.OriginalPercentDays,
                                    OriginalDays = BuyoutRow.OriginalPercentDays
                                });
                        }
                        if (MonthlyRow.PenaltyAmount != MonthlyRow.OriginalPenaltyAmount)
                        {
                            if (actionType == ContractActionType.MonthlyPayment)
                                discount.Rows.Add(new DiscountRow()
                                {
                                    PaymentType = AmountType.Penalty,
                                    SubtractedCost = MonthlyRow.OriginalPenaltyAmount - MonthlyRow.PenaltyAmount,
                                    OriginalCost = MonthlyRow.OriginalPenaltyAmount,
                                    SubtractedDays = MonthlyRow.OriginalPenaltyDays - MonthlyRow.PenaltyDays,
                                    OriginalDays = MonthlyRow.OriginalPenaltyDays
                                });
                            if (actionType == ContractActionType.Buyout)
                                discount.Rows.Add(new DiscountRow()
                                {
                                    PaymentType = AmountType.Penalty,
                                    SubtractedCost = BuyoutRow.OriginalPenaltyAmount - BuyoutRow.PenaltyAmount,
                                    OriginalCost = BuyoutRow.OriginalPenaltyAmount,
                                    SubtractedDays = BuyoutRow.OriginalPenaltyDays - BuyoutRow.PenaltyDays,
                                    OriginalDays = BuyoutRow.OriginalPenaltyDays
                                });
                        }
                        if (discount.HasChanges)
                        {
                            DutyDiscount.Discounts.Add(discount);
                        }
                    }
                }
                */

                #endregion

                DisplayAmountWithoutPrepayment = (contract.PercentPaymentType == PercentPaymentType.Product && contract.Status == ContractStatus.AwaitForInitialFee && contract.RequiredInitialFee.HasValue)
                    ? contract.RequiredInitialFee.Value
                    : CalculateDisplayAmountForAnnuity(contract, balanceAccountsOnly: balanceAccountsOnly) + _contractService.GetExtraExpensesCost(contract.Id);

                DisplayAmountWithoutPrepaymentAndExpenses = (contract.PercentPaymentType == PercentPaymentType.Product && contract.Status == ContractStatus.AwaitForInitialFee && contract.RequiredInitialFee.HasValue)
                    ? contract.RequiredInitialFee.Value
                    : CalculateDisplayAmountForAnnuity(contract, balanceAccountsOnly: balanceAccountsOnly);

            }
            else
            {
                throw new ArgumentOutOfRangeException($@"Значение PercentPaymentType={contract.PercentPaymentType} вне диапазона. ContractAmount.");
            }

            #region Дополнительные расходы

            InitExpenseCost(contract, actionType);

            #endregion

            DisplayAmount = Math.Round(DisplayAmountWithoutPrepayment - PrepaymentCost, 2);
            DisplayAmountWithoutExpenses = Math.Round(DisplayAmountWithoutPrepaymentAndExpenses - PrepaymentCost, 2);
            if (DisplayAmount < 0)
                DisplayAmount = 0;
            if (DisplayAmountWithoutExpenses < 0)
                DisplayAmountWithoutExpenses = 0;

            DisplayAmountWithoutExpenses = Math.Round(DisplayAmountWithoutExpenses, 2);
            DisplayAmountWithoutPrepaymentAndExpenses = Math.Round(DisplayAmountWithoutPrepaymentAndExpenses, 2);
        }

        public void SaveContractDiscount(ContractDiscount contractDiscount)
        {
            _contractDiscountRepository.Insert(contractDiscount);
        }

        private void InitExpenseCost(Contract contract, ContractActionType? actionType = null)
        {
            ExtraExpensesCost = 0;

            if (!actionType.HasValue)
                return;

            if (!_contractActionOperationPermisisonService.CanPayExtraExpenses(actionType.Value))
                return;

            if (contract.ContractClass == ContractClass.Credit)
            {
                ExtraExpensesCost = _contractService.GetExtraExpensesCost(contract.Id);
            }
            else if (contract.ContractClass == ContractClass.Tranche)
            {
                ExtraExpensesCost = _contractService.GetExtraExpensesCost(contract.CreditLineId.Value);
            }
            else if (contract.ContractClass == ContractClass.CreditLine)
            {
                var activeTranches = _contractService.GetAllSignedTranches(contract.Id).Result;

                if (!activeTranches.Any())
                    ExtraExpensesCost = _contractService.GetExtraExpensesCost(contract.Id);
            }
        }

        private void InitDiscountsAndBlackouts(int contractId)
        {
            _discounts = _contractDiscountRepository.GetByContractId(contractId).Where(d => _date.Date <= d.EndDate.Date && _date.Date >= d.BeginDate.Date && d.Status != ContractDiscountStatus.Canceled).ToList();
        }

        private void InitAccountsAndSettings(Contract contract)
        {
            _accounts = _accountService.List(new ListQueryModel<AccountFilter> { Page = null, Model = new AccountFilter { ContractId = contract.Id, IsOpen = true } }).List;

            if (contract.InscriptionId.HasValue && contract.Inscription?.Status == InscriptionStatus.Executed)
                _accounts = _accounts.Where(t => !t.AccountSetting.Code.Contains("_OFFBALANCE")).ToList();

            _accountSettings = _accountSettingService.List(new ListQuery { Page = null }).List;
            if (_accounts == null) throw new PawnshopApplicationException("Счета для договора не найдены");
        }

        /// <summary>
        /// Сумма для отображения в онлайн системах
        /// </summary>
        public decimal DisplayAmount { get; set; }

        /// <summary>
        /// Сумма для отображения в онлайн системах без учета доп расходов
        /// </summary>
        public decimal DisplayAmountWithoutExpenses { get; set; }
        /// <summary>
        /// Сумма для отображения в онлайн системах без учета аванса
        /// </summary>
        public decimal DisplayAmountWithoutPrepayment { get; set; }

        /// <summary>
        /// Сумма для отображения в онлайн системах без учета аванса и доп расходов
        /// </summary>
        public decimal DisplayAmountWithoutPrepaymentAndExpenses { get; set; }

        /// <summary>
        /// Сумма для продления
        /// </summary>
        public decimal ProlongAmount { get; set; }

        public ContractAmountRow ProlongRow { get; set; }

        /// <summary>
        /// Сумма для ежемесячного погашения
        /// </summary>
        public decimal MonthlyAmount { get; set; }

        public ContractAmountRow MonthlyRow { get; set; }

        /// <summary>
        /// Сумма для выкупа
        /// </summary>
        public decimal BuyoutAmount { get; set; }

        public ContractAmountRow BuyoutRow { get; set; }

        /// <summary>
        /// Количество просроченных дней
        /// </summary>
        public int PenaltyDays { get; set; }

        /// <summary>
        /// Дата следующей оплаты
        /// </summary>
        public DateTime? NextPaymentDate { get; set; }

        /// <summary>
        /// Количество просроченных месяцев
        /// </summary>
        public int PenaltyMonthCount { get; set; }

        /// <summary>
        /// Сумма аванса
        /// </summary>
        public decimal PrepaymentCost { get; set; }

        /// <summary>
        /// Добавляемые или вычитаемые дни
        /// </summary>
        public int BlackDays { get; set; }

        /// <summary>
        /// Скидки
        /// </summary>
        public ContractDutyDiscount DutyDiscount { get; set; }

        /// <summary>
        /// Признак просроченного договора
        /// </summary>
        public bool IsDelayed { get; set; } = false;

        /// <summary>
        /// Признак просроченного договора
        /// </summary>
        public decimal ExtraExpensesCost { get; set; }

        /// <summary>
        /// Сумма дебиторской задолженности при сверке с онлайн системами на договоре
        /// </summary>
        public decimal ReceivableOnlinePaymentAmount { get; set; }

        /// <summary>
        /// Причина
        /// </summary>
        public string Reason { get; set; } = "Причина";

        /// <summary>
        /// Сумма кредитного лимита
        /// </summary>
        public decimal CreditLineCost { get; set; }

        /// <summary>
        /// Сумма рефинансирования
        /// </summary>
        public decimal Refinance { get; set; }

        public (int, int) CalculatePenaltyDays(IContract contract)
        {
            var days = 0;
            var blackdays = 0;

            if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
            {
                var holidays = _holidayService.List(new Models.List.ListQueryModel<HolidayFilter>()
                {
                    Page = null,
                    Model = new HolidayFilter
                    {
                        BeginDate = contract.MaturityDate.Date,
                        EndDate = _date.Date
                    }
                }).List;

                holidays.AddRange(_holidayService.List(new Models.List.ListQueryModel<HolidayFilter>()
                {
                    Page = null,
                    Model = new HolidayFilter
                    {
                        PayDate = _date
                    }
                }).List);

                var holiday = (holidays == null || holidays.Count == 0) ? null : holidays.FirstOrDefault(x => x.Date.Date == contract.MaturityDate.Date && _date.Date <= x.PayDate.Date);
                var date = holiday != null ? holiday.Date : _date;

                blackdays = Math.Abs(_blackouts
                    .Where(b => contract.MaturityDate.Date <= b.EndDate.Date &&
                                date.Date >= b.BeginDate.Date).Sum(x =>
                        ((date.Date <= x.EndDate.Date
                             ? date.Date
                             : x.EndDate.Date)
                         -
                         (contract.MaturityDate.Date > x.BeginDate.Date
                             ? contract.MaturityDate.Date
                             : x.BeginDate.Date.AddDays(-1))
                        ).Days));
                if (date.Date > contract.MaturityDate)
                    days = (date.Date - contract.MaturityDate.Date).Days - blackdays;
                if (holiday != null) blackdays += Math.Abs((contract.MaturityDate - _date.Date).Days);
            }
            else if (contract.PercentPaymentType == PercentPaymentType.AnnuityTwelve ||
            contract.PercentPaymentType == PercentPaymentType.AnnuityTwentyFour ||
            contract.PercentPaymentType == PercentPaymentType.AnnuityThirtySix ||
            contract.PercentPaymentType == PercentPaymentType.Product)
            {
                if (contract.GetSchedule() == null) throw new InvalidOperationException();
                var schedules = contract.GetSchedule().Where(x => x.Status == ScheduleStatus.Overdue);
                foreach (var schedule in schedules)
                {

                    var blackday = Math.Abs(_blackouts
                        .Where(b => schedule.Date.Date <= b.EndDate.Date &&
                                    _date.Date >= b.BeginDate.Date).Sum(x =>
                            ((_date.Date <= x.EndDate.Date
                                 ? _date.Date
                                 : x.EndDate.Date)
                             -
                             (schedule.Date.Date > x.BeginDate.Date
                                 ? schedule.Date.Date
                                 : x.BeginDate.Date.AddDays(-1))
                            ).Days));
                    days += (_date.Date - schedule.Date.Date).Days - blackday;
                    blackdays += blackday;
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException($@"Значение PercentPaymentType={contract.PercentPaymentType} вне диапазона. ContractAmount.");
            }

            return (days, blackdays);
        }

        private int CalculatePenaltyMonthCount(IContract contract)
        {
            var monthCount = 0;

            if (contract.PercentPaymentType == PercentPaymentType.AnnuityTwelve ||
            contract.PercentPaymentType == PercentPaymentType.AnnuityTwentyFour ||
            contract.PercentPaymentType == PercentPaymentType.AnnuityThirtySix ||
            contract.PercentPaymentType == PercentPaymentType.Product)
            {
                if (contract.GetSchedule() == null) throw new InvalidOperationException();
                var schedules = contract.GetSchedule().Where(x => x.Status == ScheduleStatus.Overdue);
                monthCount = schedules.Count();
            }

            return monthCount;
        }

        private ContractAmountRow CalculateRow(IContract contract, ContractAmountRowType type, int penDays, int blackDays, DateTime date, bool balanceAccountsOnly = false)
        {
            var row = new ContractAmountRow();

            row.OriginalPenaltyAmount = row.PenaltyAmount = CalculateAmountByAmountType(date, balanceAccountsOnly: balanceAccountsOnly, AmountType.Penalty, AmountType.DebtPenalty, AmountType.LoanPenalty);

            row.OriginalPercentAmount = row.PercentAmount = CalculateAmountByAmountType(date, balanceAccountsOnly: balanceAccountsOnly, AmountType.Loan, AmountType.OverdueLoan);

            row.DebtAmount = CalculateAmountByAmountType(date, balanceAccountsOnly: balanceAccountsOnly, AmountType.Debt, AmountType.OverdueDebt);

            row.ReceivableOnlinePaymentAmount = CalculateAmountByAmountType(date, balanceAccountsOnly: balanceAccountsOnly, AmountType.Receivable);

            if(type == ContractAmountRowType.BuyoutRow)
            {
                row.DefermentLoan = CalculateAmountByAmountType(date, balanceAccountsOnly: balanceAccountsOnly, AmountType.DefermentLoan);

                row.AmortizedLoan = CalculateAmountByAmountType(date, balanceAccountsOnly: balanceAccountsOnly, AmountType.AmortizedLoan); //Амортизированое вознаграждение

                row.AmortizedDebtPenalty = CalculateAmountByAmountType(date, balanceAccountsOnly: balanceAccountsOnly, AmountType.AmortizedDebtPenalty);

                row.AmortizedLoanPenalty = CalculateAmountByAmountType(date, balanceAccountsOnly: balanceAccountsOnly, AmountType.AmortizedLoanPenalty); //Амортизированая пеня на проценты просроченные
            }



            if (type == ContractAmountRowType.ProlongRow)
            {
                row.DebtAmount = 0;
            }

            if (type == ContractAmountRowType.MonthlyRow && _date.Date == contract.NextPaymentDate)
            {
                var scheduleItem = contract.GetSchedule()
                    .FirstOrDefault(item => item.Date.Date == contract.GetSchedule()
                        .Where(schedule => !schedule.ActionId.HasValue)
                        .Min(x => x.Date.Date));


                row.OriginalPenaltyDays = row.PenaltyDays = 0;
                row.OriginalPenaltyAmount = row.PenaltyAmount = 0;

                row.OriginalPercentAmount = row.PercentAmount = scheduleItem?.PercentCost ?? 0;
                row.OriginalPercentDays = row.PercentDays = scheduleItem?.Period ?? 0;  

                row.DebtAmount = scheduleItem?.DebtCost ?? 0;
            }
            else if (type == ContractAmountRowType.MonthlyRow)
            {
                throw new PawnshopApplicationException("Невозможно расчитать, потому что сегодня не дата оплаты");
            }


            #region Дискретный договор(старый расчёт)
            /*
            if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
            {
                if (contract.PaymentSchedule == null) throw new InvalidOperationException();

                switch (type)
                {
                    case ContractAmountRowType.ProlongRow:
                    case ContractAmountRowType.MonthlyRow:
                        var scheduleItem = contract.PaymentSchedule.FirstOrDefault(x => x.ActionId == null);

                        var holidays = new List<Holiday>();
                        Holiday holiday = null;

                        if (scheduleItem != null)
                        {
                            holidays = _holidayRepository.List(new ListQuery() { Page = null }, new
                            {
                                BeginDate = scheduleItem.Date,
                                EndDate = _date.Date
                            });

                            holidays.AddRange(_holidayRepository.List(new ListQuery() { Page = null }, new
                            {
                                PayDate = _date
                            }));

                            holiday = holidays.FirstOrDefault(x => x.Date.Date == scheduleItem.Date.Date && _date.Date <= x.PayDate.Date);

                            row.DebtAmount = Math.Round(scheduleItem.DebtCost, 2);
                            if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
                            {
                                var startDate = contract.ProlongDate != null ? contract.ProlongDate.Value.Date : contract.ContractDate.Date;
                                var usedDays = (_date.Date - startDate.Date).Days;
                                usedDays += (contract.ProlongDate.HasValue || contract.Locked) ? 0 : 1;
                                if (usedDays > scheduleItem.Period) usedDays = scheduleItem.Period;
                                if (usedDays != scheduleItem.Period)
                                {
                                    var amount = Math.Round((scheduleItem.PercentCost/scheduleItem.Period) * usedDays, 2);

                    row.PercentAmount = row.OriginalPercentAmount = amount;
                    row.PercentDays = row.OriginalPercentDays = usedDays;

                    if (blackDays > 0)
                    {
                        row.PercentAmount += Math.Round(contract.LoanPercentCost * blackDays, 2);
                        row.PercentDays += blackDays;
                    }
                }
                else
                {
                    var usedDays = (_date.Date - startDate.Date).Days;
                    usedDays += (contract.ProlongDate.HasValue || contract.Locked) ? 0 : 1;
                    if (usedDays > 30) usedDays = 30;
                    var amount = Math.Round(contract.LoanPercentCost * usedDays, 2);

                    row.PercentAmount = row.OriginalPercentAmount = amount;
                    row.PercentDays = row.OriginalPercentDays = usedDays;
                }
            }
            */
            #endregion

            #region Аннуитетный договор(старый расчёт)
            /*
            else if (contract.PercentPaymentType == PercentPaymentType.AnnuityTwelve ||
            contract.PercentPaymentType == PercentPaymentType.AnnuityTwentyFour ||
            contract.PercentPaymentType == PercentPaymentType.AnnuityThirtySix ||
            contract.PercentPaymentType == PercentPaymentType.Product)
            {
                if (contract.PaymentSchedule == null) throw new InvalidOperationException();

                switch (type)
                {
                    case ContractAmountRowType.ProlongRow:
                        break;
                    case ContractAmountRowType.MonthlyRow:
                        var scheduleItem = contract.PaymentSchedule.Where(x => x.ActionId == null).FirstOrDefault();

                        var holidays = _holidayService.List(new ListQuery() {Page = null}, new
                        {
                            BeginDate = scheduleItem?.Date ?? _date.Date,
                            EndDate = _date.Date
                        });

                        holidays.AddRange(_holidayService.List(new ListQuery() { Page = null }, new
                        {
                            PayDate = _date
                        }));


                        }

                        var delayedPayment = contract.PaymentSchedule.FirstOrDefault(x => x.ActionId == null && x.Date.Date < _date.Date);
                        if (delayedPayment != null && holiday == null)
                        {
                            var monthlyCost = delayedPayment.DebtCost + delayedPayment.PercentCost;
                            var blackday = Math.Abs(_blackouts
                                .Where(b => delayedPayment.Date.Date <= b.EndDate.Date &&
                                            _date.Date >= b.BeginDate.Date).Sum(x =>
                                    ((_date.Date <= x.EndDate.Date
                                         ? _date.Date
                                         : x.EndDate.Date)
                                     -
                                     (delayedPayment.Date.Date > x.BeginDate.Date
                                         ? delayedPayment.Date.Date
                                         : x.BeginDate.Date.AddDays(-1))
                                    ).Days));
                            var penaltyDays = (_date.Date - scheduleItem.Date.Date).Days;
                            var penaltyPeriod = penaltyDays - blackday;
                            row.PenaltyAmount = Math.Round(monthlyCost * (penaltyPeriod * contract.PenaltyPercent / 100), 2);
                            row.PenaltyDays = penaltyPeriod;

                            row.OriginalPenaltyAmount = Math.Round(monthlyCost * (penaltyDays * contract.PenaltyPercent / 100), 2);
                            row.OriginalPenaltyDays = penaltyDays;
                        }
                        break;
                    case ContractAmountRowType.BuyoutRow:
                        row.DebtAmount = Math.Round(contract.LeftLoanCost, 2);

                        var delayedPayments = contract.PaymentSchedule.Where(x => x.ActionId == null && x.Date.Date < _date.Date);
                        var payedPayment = contract.PaymentSchedule.LastOrDefault(x => x.ActionId > 0);

                        var lastPaymentDate = payedPayment?.Date.Date ?? contract.ContractDate.Date;

                        if (delayedPayments.Any())
                        {
                            row.PercentAmount += delayedPayments.Sum(x => x.PercentCost);
                            row.PercentDays += delayedPayments.Sum(x => x.Period);

                            var nextPayment = contract.PaymentSchedule.FirstOrDefault(x => x.ActionId == null && x.Date.Date >= _date.Date);
                            if (nextPayment != null)
                            {
                                var useDays = (_date.Date - delayedPayments.LastOrDefault().Date.Date).Days;
                                row.PercentAmount += (nextPayment.PercentCost / nextPayment.Period) * useDays;
                                row.PercentDays += useDays;
                            }

                            row.OriginalPercentDays = row.PercentDays;

                            // Расчет штрафа
                            foreach (var delay in delayedPayments)
                            {
                                var monthlyCost = delay.DebtCost + delay.PercentCost;
                                var blackday = Math.Abs(_blackouts
                                    .Where(b => delay.Date.Date <= b.EndDate.Date &&
                                                _date.Date >= b.BeginDate.Date).Sum(x =>
                                        ((_date.Date <= x.EndDate.Date
                                             ? _date.Date
                                             : x.EndDate.Date)
                                         -
                                         (delay.Date.Date > x.BeginDate.Date
                                             ? delay.Date.Date
                                             : x.BeginDate.Date.AddDays(-1))
                                        ).Days));
                                var penaltyDays = (_date.Date - delay.Date.Date).Days;
                                var penaltyPeriod = penaltyDays - blackday;
                                row.PenaltyAmount += Math.Round(monthlyCost * (penaltyPeriod * contract.PenaltyPercent / 100), 2);
                                row.PenaltyDays += penaltyPeriod;

                                row.OriginalPenaltyAmount += Math.Round(monthlyCost * (penaltyDays * contract.PenaltyPercent / 100), 2);
                                row.OriginalPenaltyDays += penaltyDays;
                            }
                        }
                        else
                        {
                            var period = (_date.Date - lastPaymentDate.Date).Days;
                            var nextPayment = contract.PaymentSchedule.FirstOrDefault(x => x.ActionId == null);

                            if (period > 0 && nextPayment != null)
                            {
                                row.PercentAmount += (nextPayment.PercentCost / nextPayment.Period) * period;
                                row.PercentDays = period;

                                row.OriginalPercentDays = row.PercentDays;
                            }
                        }

                        row.OriginalPercentAmount = row.PercentAmount = Math.Round(row.PercentAmount, 2);
                        row.PenaltyAmount = Math.Round(row.PenaltyAmount, 2);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            */
            #endregion

            return row;
        }

        public decimal GetTransitAccountBalance()
        {
            var account = _accountService.GetByAccountSettingCode(null, "TRANSFER_RESTRUCTURING");
            var balance = Math.Abs(_accountService.GetAccountBalance(account.Id, DateTime.Now));

            return balance;
        }

        public decimal CalculateAmountByAmountType(DateTime date, params AmountType[] amountTypes)
        {
            return _accounts
                .Join(_accountSettings, a => a.AccountSettingId, s => s.Id,
                    (a, o) => new { Setting = o, Account = a })
                .Where(x =>
                {
                    return !amountTypes.Any() || amountTypes.Any(amountType => amountType == x.Setting.DefaultAmountType);
                })
                .Sum(x => Math.Abs(_accountService.GetAccountBalance(x.Account.Id, date)));
        }

        public decimal CalculateAmountByAmountType(DateTime date, bool balanceAccountsOnly = false, params AmountType[] amountTypes)
        {
            if (!balanceAccountsOnly)
            {
                return _accounts
                    .Join(_accountSettings, a => a.AccountSettingId, s => s.Id,
                        (a, o) => new { Setting = o, Account = a })
                    .Where(x =>
                    {
                        return !amountTypes.Any() || amountTypes.Any(amountType => amountType == x.Setting.DefaultAmountType);
                    })
                    .Sum(x => Math.Abs(_accountService.GetAccountBalance(x.Account.Id, date)));
            }
            return _accounts
                .Join(_accountSettings, a => a.AccountSettingId, s => s.Id,
                    (a, o) => new { Setting = o, Account = a })
                .Where(x =>
                {
                    return (!amountTypes.Any() || amountTypes.Any(amountType => amountType == x.Setting.DefaultAmountType && !x.Setting.Code.Contains("_OFFBALANCE")));
                })
                .Sum(x => Math.Abs(_accountService.GetAccountBalance(x.Account.Id, date)));
        }

        private List<DiscountRow> BuildDiscountRows(ContractAmountRow calculationRow, ContractDiscount discount)
        {
            List<DiscountRow> discountRows = new List<DiscountRow>();



            if ((calculationRow.PercentAmount * discount.PersonalDiscount.PercentDiscountCoefficient) != calculationRow.OriginalPercentAmount || discount.PersonalDiscount.PercentAdjustment != 0)
            {
                discountRows.Add(new DiscountRow()
                {
                    PaymentType = AmountType.Loan,
                    SubtractedCost = calculationRow.PercentAmount - (calculationRow.PercentAmount * discount.PersonalDiscount.PercentDiscountCoefficient),
                    OriginalCost = calculationRow.PercentAmount,
                    PercentAdjustment = discount.PersonalDiscount.PercentAdjustment
                });
                calculationRow.PercentAmount *= discount.PersonalDiscount.PercentDiscountCoefficient;

            }
            if ((calculationRow.PenaltyAmount * discount.PersonalDiscount.DebtPenaltyDiscountCoefficient) != calculationRow.OriginalPenaltyAmount || discount.PersonalDiscount.DebtPenaltyAdjustment != 0)
            {
                discountRows.Add(new DiscountRow()
                {
                    PaymentType = AmountType.Penalty,
                    SubtractedCost = calculationRow.PenaltyAmount - (calculationRow.PenaltyAmount * discount.PersonalDiscount.DebtPenaltyDiscountCoefficient),
                    OriginalCost = calculationRow.PenaltyAmount,
                    PercentAdjustment = discount.PersonalDiscount.DebtPenaltyAdjustment
                });
                calculationRow.PenaltyAmount *= discount.PersonalDiscount.DebtPenaltyDiscountCoefficient;
            }

            return discountRows;
        }

        private bool CheckScheduleDate(IPaymentScheduleItem schedule, DateTime currentDate)
        {
            if (schedule == null)
            {
                return false;
            }

            if (schedule.NextWorkingDate.HasValue)
            {
                return currentDate >= schedule.Date && currentDate <= schedule.NextWorkingDate.Value;
            }

            return schedule.Date == currentDate.Date;
        }

        private decimal CalculateNextPeriodAccruals(DateTime scheduleDate, DateTime currentDate, List<IPaymentScheduleItem> schedule)
        {
            decimal result = default;

            var nextScheduleItem = schedule
                .FirstOrDefault(item => !item.ActionId.HasValue && item.Date > _date);

            if (nextScheduleItem == null)
            {
                return result;
            }

            List<string> accrualBoSettings = new List<string>
                {
                    Constants.BO_SETTING_INTEREST_ACCRUAL,
                    Constants.BO_SETTING_INTEREST_ACCRUAL_MIGRATION,
                    Constants.BO_SETTING_INTEREST_ACCRUAL_OVERDUEDEBT,
                    Constants.BO_SETTING_INTEREST_ACCRUAL_ON_HOLIDAYS
                };

            DateTime lastAccrualDate = _cashOrderService.GetContractLastOperationDate(nextScheduleItem.ContractId, accrualBoSettings, currentDate.Date.AddDays(1)).Result;

            if (lastAccrualDate == default || lastAccrualDate <= scheduleDate)
            {
                return result;
            }

            int period = nextScheduleItem.Period != default ? nextScheduleItem.Period : 30;

            result = nextScheduleItem.PercentCost * (lastAccrualDate.Date - scheduleDate).Days / period;
            
            return result;
        }


        private decimal CalculateDisplayAmountForAnnuity(Contract contract, bool balanceAccountsOnly = false)
        {
            decimal amount = CalculateAmountByAmountType(_date, balanceAccountsOnly: balanceAccountsOnly, AmountType.OverdueDebt, AmountType.OverdueLoan, AmountType.DebtPenalty, AmountType.LoanPenalty, AmountType.Receivable);

            var schedule = contract.GetSchedule();
            var scheduleItem = schedule
                .FirstOrDefault(item => !item.ActionId.HasValue && CheckScheduleDate(item, _date));

            if (scheduleItem != null)
            {
                decimal nextPeriodAccruals = default;

                if (scheduleItem.NextWorkingDate.HasValue)
                {
                    nextPeriodAccruals = CalculateNextPeriodAccruals(scheduleItem.Date, _date, schedule);
                }
                amount += CalculateAmountByAmountType(_date, balanceAccountsOnly: balanceAccountsOnly, AmountType.Loan) + (CalculateAmountByAmountType(_date, balanceAccountsOnly: balanceAccountsOnly, AmountType.Debt) - scheduleItem.DebtLeft);
                amount -= nextPeriodAccruals;
            }
            else
            {
                if (amount == 0)
                {
                    var scheduleNextItem = schedule.OrderBy(item => item.Date).FirstOrDefault(item => !item.ActionId.HasValue && item.Date.Date > _date.Date);
                    if (scheduleNextItem != null)
                    {
                        amount += scheduleNextItem.DebtCost + scheduleNextItem.PercentCost;
                    }
                }
            }

            var activeAtypicalContractDiscounts = _discounts?.Where(t => !t.IsTypical && t.Status == ContractDiscountStatus.Accepted);

            if (activeAtypicalContractDiscounts.Any())
            {
                var activeAtypicalContractDiscount = activeAtypicalContractDiscounts.First();
                if (activeAtypicalContractDiscounts.Count() > 1)
                    throw new PawnshopApplicationException(
                        "По договору существует больше одной персональной скидки по сумме, обратитесь в тех. поддержку");

                if (activeAtypicalContractDiscount != null)
                {
                    amount -=
                        activeAtypicalContractDiscount.DebtPenaltyDiscountSum
                        - activeAtypicalContractDiscount.DebtPenaltyDiscountSum
                        - activeAtypicalContractDiscount.PercentDiscountSum
                        - activeAtypicalContractDiscount.PercentPenaltyDiscountSum;
                }
            }

            return amount;
        }

        public decimal GetLoanCostLeft()
        {
            decimal loanCostLeft = 0;
            string accountSettingCode = Constants.ACCOUNT_SETTING_ACCOUNT;
            string overdueAccountSettingCode = Constants.ACCOUNT_SETTING_OVERDUE_ACCOUNT;

            IEnumerable<Account> accountAndOverdueAccountAccounts = _accounts
                .Join(_accountSettings, a => a.AccountSettingId, s => s.Id,
                    (a, o) => new { Setting = o, Account = a })
                .Where(model => model.Setting.Code.Equals(accountSettingCode) || model.Setting.Code.Equals(overdueAccountSettingCode))
                .Select(model => model.Account);

            foreach (var account in accountAndOverdueAccountAccounts)
                loanCostLeft += _accountService.GetAccountBalance(account.Id, _date);

            return loanCostLeft;
        }
    }
}
