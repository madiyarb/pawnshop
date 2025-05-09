using System;

namespace Pawnshop.Data.Models.Comments
{
    public class ApplicationOnlineComment
    {
        public int Id { get; set; }

        public Guid ApplicationOnlineId { get; set; }

        public int CommentId { get; set; }

        public ApplicationOnlineCommentTypes CommentType { get; set; }
    }
}
