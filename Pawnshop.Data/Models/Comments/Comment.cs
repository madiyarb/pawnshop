using Pawnshop.Core;
using Pawnshop.Data.Models.Membership;
using System;

namespace Pawnshop.Data.Models.Comments
{
    public class Comment : IEntity
    {
        public int Id { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? DeleteDate { get; set; }

        public int? AuthorId { get; set; }

        public User Author { get; set; }

        public string AuthorName { get; set; }

        public string CommentText { get; set; }

        public ApplicationOnlineComment ApplicationOnlineComment { get; set; }
    }
}
