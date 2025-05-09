using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.CashOrders
{
    public class MoneyPlacementsReport
    {
        public int OperationType { get; set; }
        public int OperationsCount { get; set; }
        public decimal MoneyPlacementSum { get; set; }
    }
}
