using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Data.Models.CashOrders;

namespace Pawnshop.Services.Models.Filters
{
    public class ContractExpenseFilter
    {
        public int? ContractId { get; set; }
        public bool? IsPayed { get; set; }
    }
}
