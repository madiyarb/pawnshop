using System;
using System.Collections.Generic;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Data.Models.Crm
{
    public class CrmBitrixIVROverduePayment
    {
        public int ContractId { get; set; }
        public string ContractNumber { get; set; }
        public int OverduePaymentsCount { get; set; }
        public DateTime NextPaymentDate { get; set; }
        public decimal NextPaymentAmount { get; set; }
        public ContractBalance ContractBalance { get; set; }
    }
}
