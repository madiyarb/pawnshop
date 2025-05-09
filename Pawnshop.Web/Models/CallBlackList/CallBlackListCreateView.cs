using System;

namespace Pawnshop.Web.Models.CallBlackList
{
    public class CallBlackListCreateView
    {
        public DateTime? ExpireDate { get; set; }

        public string PhoneNumber { get; set; }

        public string Reason { get; set; }
    }
}
