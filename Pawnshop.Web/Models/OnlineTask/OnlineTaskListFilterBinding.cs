using System;

namespace Pawnshop.Web.Models.OnlineTask
{
    public class OnlineTaskListFilterBinding
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? MinutesLeftAfterUpdate { get; set; }
        public string PartnerCode { get; set; }
    }
}
