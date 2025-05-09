using System;

namespace Pawnshop.Web.Models.Investment
{
    public class InvestmentListQueryModel
    {
        public DateTime? BeginDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int OwnerId { get; set; }
    }
}
