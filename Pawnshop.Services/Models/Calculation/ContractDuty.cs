using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.Calculation
{
    public class ContractDuty
    {
        public DateTime Date { get; set; }
        public List<ContractActionRow> Rows { get; set; }
        public ContractDutyDiscount Discount { get; set; }
        public decimal DisplayAmountForOnlinePayment { get; set; }
        public decimal ExtraExpensesCost { get; set; }
        public decimal Cost { get; set; }
        public List<ContractActionCheck> Checks { get; set; }
        public string Reason { get; set; }
        public List<ContractExpense> ExtraContractExpenses { get; set; }
        
        /// <summary>
        /// Сумма без аванса 
        /// </summary>
        public decimal? BalanceWithoutPrepayment { get; set; }
    }
}
