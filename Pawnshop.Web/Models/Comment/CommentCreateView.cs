namespace Pawnshop.Web.Models.Comment
{
    public class CommentCreateView
    {
        public string Comment { get; set; }
        public string EntityId { get; set; }
        public CommentEntityType EntityType { get; set; }
    }
}
