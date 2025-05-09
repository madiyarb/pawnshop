using System;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.CashOrders;

namespace Pawnshop.Web.Models.CashOrder
{
    public class CashOrderListQueryModel
    {
        public OrderType? OrderType { get; set; }

        public DateTime? BeginDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? ClientId { get; set; }

        public int? UserId { get; set; }

        public int? AccountId { get; set; }
        public int? AccountPlanId { get; set; }

        public int? OrderNumber { get; set; }

        public bool? IsDelete { get; set; }

        public int OwnerId { get; set; }
        
        public bool? IsApproved { get; set; }
    }
}