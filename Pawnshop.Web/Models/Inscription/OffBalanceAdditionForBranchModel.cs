using System;

namespace Pawnshop.Web.Models.Inscription
{
    public class OffBalanceAdditionForBranchModel
    {
        public int BranchId { get; set; }
        public DateTime? EndDate { get; set; } = null;
    }
}
