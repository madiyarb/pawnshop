using Pawnshop.AccountingCore.Models;
using System;

namespace Pawnshop.Data.Models.Contracts
{
    public class RestructuredContractPaymentSchedule : ContractPaymentSchedule
    {
        /// <summary>
        /// Платеж по остатку отсроченных процентов
        /// </summary>
        public decimal? PaymentBalanceOfDefferedPercent { get; set; } = 0;

        /// <summary>
        /// Аммортизированный остаток отсроченных процентов
        /// </summary>
        public decimal? AmortizedBalanceOfDefferedPercent { get; set; } = 0;

        /// <summary>
        /// Платеж по остатку просроченных процентов
        /// </summary>
        public decimal? PaymentBalanceOfOverduePercent { get; set; } = 0;

        /// <summary>
        /// Аммортизированный остаток по просроченным процентам
        /// </summary>
        public decimal? AmortizedBalanceOfOverduePercent { get; set; } = 0;

        /// <summary>
        /// Платеж по пене просроченного основного долга
        /// </summary>
        public decimal? PaymentPenaltyOfOverdueDebt { get; set; } = 0;

        /// <summary>
        /// Аммортизированная пеня просроченного основного долга
        /// </summary>
        public decimal? AmortizedPenaltyOfOverdueDebt { get; set; } = 0;

        /// <summary>
        /// Платеж по пене просроченных процентов
        /// </summary>
        public decimal? PaymentPenaltyOfOverduePercent { get; set; } = 0;

        /// <summary>
        /// Аммортизированная пеня просроченных процентов
        /// </summary>
        public decimal? AmortizedPenaltyOfOverduePercent { get; set; } = 0;

        public decimal? PenaltyOfOverduePaymentAmortizedPercent { get; set; } = 0;

        /// <summary>
        /// Дата удаления 
        /// </summary>
        public DateTime? DeleteDateRestructured { get; set; }

        /// <summary>
        /// Дата Создания 
        /// </summary>
        public DateTime? RestructuredCreateDate { get; set; }

        public RestructuredContractPaymentSchedule() { }

        public RestructuredContractPaymentSchedule(
            int? id = 0,
            int contractId = 0,
            DateTime? date = null,
            DateTime? actualDate = null,
            decimal debtLeft = 0,
            decimal debtCost = 0,
            decimal percentCost = 0,
            decimal? penaltyCost = 0,
            DateTime? createDate = null,
            DateTime? deleteDate = null,
            int? actionId = null,
            DateTime? canceled = null,
            DateTime? prolongated = null,
            ScheduleStatus status = 0,
            int period = 0,
            int revision = 0,
            int? actionType = 0,
            DateTime? nextWorkingDate = null,
            decimal? paymentBalanceOfDefferedPercent = 0,
            decimal? amortizedBalanceOfDefferedPercent = 0,
            decimal? paymentBalanceOfOverduePercent = 0,
            decimal? amortizedBalanceOfOverduePercent = 0,
            decimal? paymentPenaltyOfOverdueDebt = 0,
            decimal? amortizedPenaltyOfOverdueDebt = 0,
            decimal? paymentPenaltyOfOverduePercent = 0,
            decimal? amortizedPenaltyOfOverduePercent = 0)
        {
            Id = id.HasValue ? id.Value : 0;
            ContractId = contractId;
            Date = date.Value;
            ActualDate = actualDate;
            DebtLeft = debtLeft;
            DebtCost = debtCost;
            PercentCost = percentCost;
            PenaltyCost = penaltyCost;
            CreateDate = createDate.Value;
            DeleteDate = deleteDate;
            ActionId = actionId;
            Canceled = canceled;
            Prolongated = prolongated;
            Status = status;
            Period = period;
            Revision = revision;
            ActionType = actionType;
            NextWorkingDate = nextWorkingDate;
            RestructuredCreateDate = DateTime.Now;
            PaymentBalanceOfDefferedPercent = paymentBalanceOfDefferedPercent;
            AmortizedBalanceOfDefferedPercent = amortizedBalanceOfDefferedPercent;
            PaymentBalanceOfOverduePercent = paymentBalanceOfOverduePercent;
            AmortizedBalanceOfOverduePercent = amortizedBalanceOfOverduePercent;
            PaymentPenaltyOfOverdueDebt = paymentPenaltyOfOverdueDebt;
            AmortizedPenaltyOfOverdueDebt = amortizedPenaltyOfOverdueDebt;
            PaymentPenaltyOfOverduePercent = paymentPenaltyOfOverduePercent;
            AmortizedPenaltyOfOverduePercent = amortizedPenaltyOfOverduePercent;
        }
    }
}
