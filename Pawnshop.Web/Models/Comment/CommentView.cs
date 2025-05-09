using System;

namespace Pawnshop.Web.Models.Comment
{
    public class CommentView
    {
        public int? Id { get; set; }
        public int? AuthorId { get; set; }
        public string AuthorName { get; set; }
        public string Comment { get; set; }
        public DateTime CreateDate { get; set; }
        public string EntityId { get; set; }
        public CommentEntityType EntityType { get; set; }
    }
}
