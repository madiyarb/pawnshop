using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.Dictionaries;
using System.Collections.Generic;
using System;

namespace Pawnshop.Web.Models.CreditLine
{
    public sealed class ContractDutyViewModel
    {
        public DateTime Date { get; set; }
        public List<ContractActionRowViewModel> Rows { get; set; }
        public ContractDutyDiscount Discount { get; set; }
        public decimal DisplayAmountForOnlinePayment { get; set; }
        public decimal ExtraExpensesCost { get; set; }
        public decimal Cost { get; set; }
        public List<ContractActionCheck> Checks { get; set; }
        public string Reason { get; set; }
        public List<ContractExpense> ExtraContractExpenses { get; set; }
    }
}
