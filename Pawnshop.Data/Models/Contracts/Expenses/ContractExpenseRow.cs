using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Data.Models.Contracts.Expenses
{
    public class ContractExpenseRow : IEntity
    {
        public int Id { get; set; }
        public int ContractExpenseId { get; set; }
        public int? ActionId { get; set; }
        public ExpensePaymentType ExpensePaymentType { get; set; }
        public decimal Cost { get; set; }
        public int AuthorId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public List<ContractExpenseRowOrder> ContractExpenseRowOrders { get; set; }
    }
}
