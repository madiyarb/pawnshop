using System;

namespace Pawnshop.Web.Models.CallBlackList
{
    public class CallBlackListUpdateView
    {
        public string Reason { get; set; }

        public DateTime? ExpireDate { get; set; }
    }
}
