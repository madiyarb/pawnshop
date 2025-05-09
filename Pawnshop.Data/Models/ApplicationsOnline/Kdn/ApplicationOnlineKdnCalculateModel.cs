using System.Collections.Generic;

namespace Pawnshop.Data.Models.ApplicationsOnline.Kdn
{
    public class ApplicationOnlineKdnCalculateModel
    {
        public decimal TotalAspIncome { get; set; }
        public decimal TotalPositionsIncome { get; set; }
        public decimal TotalFormalIncome { get; set; }
        public decimal TotalInformalApprovedIncome { get; set; }
        public decimal TotalInformalUnapprovedIncome { get; set; }
        public decimal FamilyDebt { get; set; }
        public decimal TotalFamilyDebt { get; set; }
        public decimal ClientExpenses { get; set; }
        public decimal TotalFcbDebt { get; set; }
        public decimal ApprovedOtherPaymentsAmount { get; set; }
        public decimal AverageMonthlyPayment { get; set; }
        public decimal AverageMonthlyPaymentDelta { get; set; }
        public decimal Kdn { get; set; }

        public decimal TotalIncome { get; set; }
        public decimal TotalDebt { get; set; }

        public decimal TotalAccordingClientIncome { get; set; }
        public decimal TotalAccordingClientPaymentsExpenses { get; set; }

        public List<string> Message { get; set; } = new List<string>();
        public bool IsGambler { get; set; } = false;
        public string IIN { get; set; }
        public bool IsStopCredit { get; set; } = false;
        public bool IsSusn { get; set; } = false;
        public decimal AvgPaymentToday { get; set; }
        public decimal? AllLoan { get; set; }
        public decimal TotalIncomeK4 { get; set; }
        public decimal KdnK4 { get; set; }
        public int? LoanPurposeId { get; set; }
        public decimal ApplicationAmount { get; set; }
    }
}
