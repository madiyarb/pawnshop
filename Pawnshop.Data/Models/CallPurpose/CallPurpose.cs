using Pawnshop.Core;
using Pawnshop.Data.Models.Membership;
using System;

namespace Pawnshop.Data.Models.CallPurpose
{
    public class CallPurpose : IEntity 
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string Title { get; set; }
        public int AuthorId { get; set; }
        public User Author { get; set; }
        public int? UpdateAuthorId { get; set; }
        public User UpdateAuthor { get; set; }
    }
}
