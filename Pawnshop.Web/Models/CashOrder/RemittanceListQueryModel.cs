using System;
using Pawnshop.Data.Models.CashOrders;

namespace Pawnshop.Web.Models.CashOrder
{
    public class RemittanceListQueryModel
    {
        public bool Incoming { get; set; }

        public int BranchId { get; set; }

        public DateTime? BeginDate { get; set; }

        public DateTime? EndDate { get; set; }

        public RemittanceStatusType? Status { get; set; }
    }
}
