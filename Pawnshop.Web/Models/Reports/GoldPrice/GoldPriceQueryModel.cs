using System;

namespace Pawnshop.Web.Models.Reports.GoldPrice
{
    public class GoldPriceQueryModel
    {
        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }

        public int BranchId { get; set; }

        public double QueryPrice { get; set; }
    }
}