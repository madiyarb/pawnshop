using System.Collections.Generic;

namespace Pawnshop.Web.Models.InteractionResult
{
    public class InteractionResultListView
    {
        public int Count { get; set; }
        public List<InteractionResultListItemView> List { get; set; } = new List<InteractionResultListItemView>();
    }
}
