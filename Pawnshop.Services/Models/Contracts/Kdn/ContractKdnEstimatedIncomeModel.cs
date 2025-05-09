using Pawnshop.Data.Models.Contracts.Kdn;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.Contracts.Kdn
{
    public class ContractKdnEstimatedIncomeModel
    {
        public int ClientId { get; set; }
        public string IIN { get; set; }
        public bool IsSubject { get; set; } = false;
        public decimal TotalPositionsIncome { get; set; }
        public string FIO { get; set; }
        public bool ReceivesASP { get; set; }
        public decimal TotalFormalIncome { get; set; }
        public decimal TotalInformalApprovedIncome { get; set; }
        public decimal TotalInformalUnapprovedIncome { get; set; }
        public decimal FamilyDebt { get; set; }
        public decimal FcbDebt { get; set; }
        public decimal OtherClientPayments { get; set; }
        public decimal ClientExpenses { get; set; }
        public decimal TotalFamilyDebt { get; set; }
        public bool IsGambler { get; set; }
        public bool IsStopCredit { get; set; }
        public decimal? ClientAllLoanExpense { get; set; }
        public List<ContractKdnEstimatedIncome> ContractKdnEstimatedIncomes { get; set; }
    }
}
