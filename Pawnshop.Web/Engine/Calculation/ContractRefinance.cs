using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Web.Models.Contract.Refinance;
using System;
using System.Collections.Generic;
using System.Linq;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Services.Calculation;
using Pawnshop.Services.PaymentSchedules;

namespace Pawnshop.Web.Engine.Calculation
{
    public class ContractRefinance //: ICalculation
    {
        private IContractAmount _contractAmount;
        private PercentPaymentTypeToLoanPeriodConverter _converter;
        private readonly BranchContext _branchContext;
        private readonly LoanPercentRepository _loanPercentRepository;
        private readonly IPaymentScheduleService _paymentScheduleService;

        public ContractRefinance(
            IContractAmount contractAmount,
            BranchContext branchContext,
            LoanPercentRepository loanPercentRepository,
            IPaymentScheduleService paymentScheduleService)
        {
            _contractAmount = contractAmount;
            _converter = new PercentPaymentTypeToLoanPeriodConverter();
            _branchContext = branchContext;
            _loanPercentRepository = loanPercentRepository;
            _paymentScheduleService = paymentScheduleService;
        }

        public ContractRefinanceConfig Check(Contract contract, ContractRefinanceCheck check)
        {
            ContractRefinanceConfig checkResult = new ContractRefinanceConfig();
            checkResult.Errors = new List<string>();

            _contractAmount.Init(contract, check.Date);

            if (contract.ContractData.PrepaymentCost < (_contractAmount.BuyoutRow.PercentAmount + _contractAmount.BuyoutRow.PenaltyAmount))
            {
                checkResult.Errors.Add($"Не хватает денег на погашение задолженности: пошлина({_contractAmount.BuyoutRow.PercentAmount}) + штраф({_contractAmount.BuyoutRow.PenaltyAmount}) - аванс({contract.ContractData.PrepaymentCost}) = ({_contractAmount.BuyoutRow.PercentAmount + _contractAmount.BuyoutRow.PenaltyAmount - contract.ContractData.PrepaymentCost})");
            }

            if (check.FirstPaymentDate == check.Date)
            {
                check.FirstPaymentDate = null;
            }

            checkResult.PercentPaymentType = check.PercentPaymentType;
            checkResult.PossiblePaymentQuantity = PossiblePaymentQuantity(contract, check.PercentPaymentType);
            checkResult.LoanPeriod = _converter.Convert(check.PercentPaymentType);

            var setting = LoanSetting(contract, checkResult.LoanPeriod, check.Date);
            if (setting.Item1 == null)
            {
                checkResult.Errors.Add("Не найдены настройки процентов");
                checkResult.LoanPercent = contract.LoanPercent;
                //checkResult.PenaltyPercent = contract.PenaltyPercent;
            }
            else
            {
                checkResult.LoanPercent = setting.Item1.LoanPercent;
                //checkResult.PenaltyPercent = setting.Item1.PenaltyPercent;
            }

            checkResult.PercentSettingsTakenFromParentContract = setting.Item2;

            checkResult.LoanCost = (int)Math.Ceiling(contract.LeftLoanCost);

            checkResult.PaymentQuantity = checkResult.PossiblePaymentQuantity.Contains(check.PaymentQuantity ?? CurrentMonthCount(contract)) ? check.PaymentQuantity ?? CurrentMonthCount(contract) : checkResult.PossiblePaymentQuantity.LastOrDefault();
            checkResult.Schedule = _paymentScheduleService.Build(ScheduleType.Annuity, checkResult.LoanCost, checkResult.LoanPercent, check.Date, check.Date.AddMonths(checkResult.PaymentQuantity), check.PercentPaymentType == PercentPaymentType.EndPeriod ? check.Date.AddDays(30) : check.FirstPaymentDate);

            checkResult.FirstPaymentDate = checkResult.Schedule.Min(x => x.Date.Date);
            checkResult.MaturityDate = checkResult.Schedule.OrderBy(x => x.Date).LastOrDefault().Date;

            checkResult.OldBranchId = contract.Branch.Id;
            checkResult.OldBranchName = contract.Branch.DisplayName;
            checkResult.NewBranchId = _branchContext.Branch.Id;
            checkResult.NewBranchName = _branchContext.Branch.DisplayName;

            if (checkResult.Errors.Count > 0) checkResult.CheckStatus = RefinanceCheckStatus.Declined;
            return checkResult;
        }

        private (LoanPercentSetting, bool) LoanSetting(Contract contract, int loanPeriod, DateTime date)
        {
            var discounts = contract.Discounts.Where(x => x.IsTypical).Where(x => x.PersonalDiscount.ActionType == ContractActionType.Refinance && x.BeginDate <= date && x.EndDate >= date);

            LoanPercentSetting setting = new LoanPercentSetting()
            {
                LoanPercent = contract.LoanPercent,
                //PenaltyPercent = contract.PenaltyPercent
            };
            bool fromParentContract = true;

            if (discounts.Count() > 0)
            {
                foreach (var discount in discounts)
                {
                    if (discount.PersonalDiscount.PercentAdjustment != 0)
                    {
                        setting.LoanPercent -= discount.PersonalDiscount.PercentAdjustment;
                        setting.LoanPercent = setting.LoanPercent < 0 ? 0 : setting.LoanPercent;
                    }

                    if (discount.PersonalDiscount.DebtPenaltyAdjustment != 0)
                    {
                        //setting.PenaltyPercent -= discount.PersonalDiscount.DebtPenaltyAdjustment;
                        //setting.PenaltyPercent = setting.PenaltyPercent < 0 ? 0 : setting.PenaltyPercent;
                    }
                }
            }
            else
            {
                setting = _loanPercentRepository.Find(new
                {
                    BranchId = _branchContext.Branch.Id,
                    contract.CollateralType,
                    contract.ContractData.Client.CardType,
                    LoanCost = (int)Math.Ceiling(contract.LeftLoanCost),
                    LoanPeriod = loanPeriod
                });
                fromParentContract = false;
            }
            return (setting, fromParentContract);
        }

        private List<int> PossiblePaymentQuantity(Contract contract, PercentPaymentType percentPaymentType)
        {
            List<int> result = new List<int>();
            switch (percentPaymentType)
            {
                case PercentPaymentType.EndPeriod:
                    if (percentPaymentType == contract.PercentPaymentType) result.Add(CurrentMonthCount(contract));
                    result.Add(1);
                    break;
                case PercentPaymentType.AnnuityTwelve:
                    if (percentPaymentType == contract.PercentPaymentType) result.Add(CurrentMonthCount(contract));
                    result.AddRange(new List<int>() { 3, 6, 9, 12 });
                    break;
                case PercentPaymentType.AnnuityTwentyFour:
                    if (percentPaymentType == contract.PercentPaymentType) result.Add(CurrentMonthCount(contract));
                    result.AddRange(new List<int>() { 15, 18, 21, 24 });
                    break;
                case PercentPaymentType.AnnuityThirtySix:
                    if (percentPaymentType == contract.PercentPaymentType) result.Add(CurrentMonthCount(contract));
                    result.AddRange(new List<int>() { 27, 30, 33, 36 });
                    break;
                default:
                    break;
            }
            return result.Distinct().OrderBy(x => x).ToList();
        }

        private int CurrentMonthCount(Contract contract)
        {
            return contract.PercentPaymentType switch
            {
                PercentPaymentType.EndPeriod => 1,
                PercentPaymentType.AnnuityTwelve => contract.PaymentSchedule
                    .Count(x => x.Status == ScheduleStatus.FuturePayment),
                PercentPaymentType.AnnuityTwentyFour => contract.PaymentSchedule
                    .Count(x => x.Status == ScheduleStatus.FuturePayment),
                PercentPaymentType.AnnuityThirtySix => contract.PaymentSchedule
                    .Count(x => x.Status == ScheduleStatus.FuturePayment),
                _ => 0
            };
        }
    }
}
