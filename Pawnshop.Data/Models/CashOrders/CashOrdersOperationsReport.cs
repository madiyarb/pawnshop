using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.CashOrders
{
    public class CashOrdersOperationsReport
    {
        public int OperationType { get; set; }
        public int OperationsCount { get; set; }
        public decimal OperationSum { get; set; }
        public List<CashOrdersPayment> CashOrdersPayments { get; set; }
    }

    public class CashOrdersPayment
    {
        public int OperationType { get; set; }
        public int PaymentType { get; set; }
        public decimal PaymentSum { get; set; }
    }
}
