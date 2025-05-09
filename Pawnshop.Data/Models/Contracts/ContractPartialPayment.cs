using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Contracts
{
    public class ContractPartialPayment
    {
        public string ParentContractNumber { get; set; }
        public DateTime ParentContractDate { get; set; }
        public decimal PartialPaymentCost { get; set; }
        public decimal DebtLeft { get; set; }
        public int AdditionNumber { get; set; }
        public DateTime ActionDate { get; set; }
        public DateTime? ActionDateTime { get; set; }
        public DateTime? MaturityDate { get; set; }
        public int? LoanPeriod { get; set; }
        public int? ActionType { get; set; }
        public List<ContractPaymentSchedule> PaymentSchedules { get; set; }
    }
}
