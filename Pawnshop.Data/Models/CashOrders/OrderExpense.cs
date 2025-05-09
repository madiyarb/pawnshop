using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.CashOrders
{
    public class OrderExpense : IEntity
    {
		public int Id { get; set; }
        public CashOrder Order { get; set; }
        public int? BranchId { get; set; }
        public Group Branch { get; set; } 
        public int ArticleTypeId { get; set; }
        public ExpenseArticleType ArticleType { get; set; }

	}
}
