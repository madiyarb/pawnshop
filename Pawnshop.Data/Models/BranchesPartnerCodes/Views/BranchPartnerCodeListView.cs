using System.Collections.Generic;

namespace Pawnshop.Data.Models.BranchesPartnerCodes.Views
{
    public sealed class BranchPartnerCodeListView
    {
        public int Count { get; set; }
        public IEnumerable<BranchPartnerCodeView> Items { get; set; }
    }
}
