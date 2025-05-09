using Pawnshop.Data.Models.CashOrders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.TMF
{
    public class TMFPayment : Pawnshop.Core.IEntity
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public CashOrder Order { get; set; }
        public string TmfDocumentId { get; set; }
        public TMFPaymentStatus Status { get; set; }
        public string TMFContractId { get; set; }
        public string Message { get; set; } = String.Empty;
    }
}
