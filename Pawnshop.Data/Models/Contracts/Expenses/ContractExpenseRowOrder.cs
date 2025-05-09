using Pawnshop.Core;
using Pawnshop.Data.Models.CashOrders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Contracts.Expenses
{
    public class ContractExpenseRowOrder : IEntity
    {
        public int Id { get; set; }
        public int ContractExpenseRowId { get; set; }
        public int OrderId { get; set; }
        public int AuthorId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public CashOrder Order { get; set; }
        public ContractExpenseRow ContractExpenseRow { get; set; }
    }
}
