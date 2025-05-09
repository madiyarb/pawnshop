using System.Collections.Generic;

namespace Pawnshop.Web.Models.CallPurpose
{
    public class CallPurposeListView
    {
        public int Count { get; set; }
        public List<CallPurposeListItemView> List { get; set; } = new List<CallPurposeListItemView>();
    }
}
