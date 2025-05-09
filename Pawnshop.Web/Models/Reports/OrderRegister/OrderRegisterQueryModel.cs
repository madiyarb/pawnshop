using System;
using Pawnshop.Data.Models.CashOrders;

namespace Pawnshop.Web.Models.Reports.OrderRegister
{
    public class OrderRegisterQueryModel
    {
        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }

        public int BranchId { get; set; }

        public int AccountType { get;set; }

        public int AccountPlanId { get; set; }

        public int ProcessingType { get; set; }
    }
}