using System;

namespace Pawnshop.Data.Models.ApplicationsOnline
{
    public class AdditionalContactLink
    {
        public Guid ApplicationOnlineId { get; set; }
        public int ApplicationNumber { get; set; }
        public string ClientFullName { get; set; }
        public int ContactOwnerTypeId { get; set; }
        public string ContactOwnerTypeName { get; set; }
    }
}
