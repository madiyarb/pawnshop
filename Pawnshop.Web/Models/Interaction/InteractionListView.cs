using System.Collections.Generic;

namespace Pawnshop.Web.Models.Interaction
{
    public class InteractionListView
    {
        public int Count { get; set; }
        public List<InteractionListItemView> List { get; set; } = new List<InteractionListItemView>();
    }
}
