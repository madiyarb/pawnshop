using System;

namespace Pawnshop.Data.Models.ApplicationsOnline
{
    public class ApplicationOnlineDelayApproved
    {
        public Guid Id { get; set; }
        public int? ClientId { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
