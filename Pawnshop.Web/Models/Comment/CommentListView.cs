using Pawnshop.Web.Models.Page;
using System.Collections.Generic;

namespace Pawnshop.Web.Models.Comment
{
    public class CommentListView : BasePageResponse
    {
        public IEnumerable<CommentView> Comments { get; set; }
    }
}
