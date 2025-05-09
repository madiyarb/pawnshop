using System;

namespace Pawnshop.Services.Models.Contracts.Kdn
{
    public class FcbKdnRequest
    {
        public int OrganizationId { get; set; }
        public string IIN { get; set; }
        public bool ConsentConfirmed { get; set; } = true;
        public bool HideDebts { get; set; } = false;
        public decimal Income { get; set; }
        public string Author { get; set; }
        public DateTime RequestDate { get; set; }
        public Guid? ApplicationOnlineId { get; set; }
    }
}