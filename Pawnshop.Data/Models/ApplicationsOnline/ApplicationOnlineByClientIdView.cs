using System;

namespace Pawnshop.Data.Models.ApplicationsOnline
{
    public class ApplicationOnlineByClientIdView
    {
        public Guid Id { get; set; }
        public string ApplicationNumber { get; set; }
        public int? ContractId { get; set; }
        public string ContractNumber { get; set; }
    }
}
