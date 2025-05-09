using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.UKassa
{
    public class UKassaReconciliationReport
    {
        public string Side { get; set; }
        public decimal Cash { get; set; }
        public List<ReconciliationMoneyPlacement> ReconciliationMoneyPlacements { get; set; }
        public List<ReconciliationCheckOperation> ReconciliationCheckOperations { get; set; }
    }

    public class ReconciliationMoneyPlacement
    {
        public int OperationType { get; set; }
        public int OperationsCount { get; set; }
        public decimal MoneyPlacementSum { get; set; }
    }

    public class ReconciliationCheckOperation
    {
        public int OperationType { get; set; }
        public int OperationsCount { get; set; }
        public decimal OperationSum { get; set; }
        public List<ReconciliationPayment> ReconciliationPayments { get; set; }
    }

    public class ReconciliationPayment
    {
        public int PaymentType { get; set; }
        public decimal PaymentSum { get; set; }
    }
}
