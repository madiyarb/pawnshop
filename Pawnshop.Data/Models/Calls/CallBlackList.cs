using Pawnshop.Core;
using Pawnshop.Data.Models.Membership;
using System;

namespace Pawnshop.Data.Models.Calls
{
    public class CallBlackList : IEntity
    {
        public int Id { get; set; }

        public string PhoneNumber { get; set; }

        public string Reason { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime? UpdateDate { get; set; }

        public DateTime? DeleteDate { get; set; }

        public DateTime? ExpireDate { get; set; }

        public int? AuthorId { get; set; }

        public User Author { get; set; }
    }
}
