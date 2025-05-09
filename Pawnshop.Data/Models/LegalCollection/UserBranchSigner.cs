using System;

namespace Pawnshop.Data.Models.LegalCollection
{
    public class UserBranchSigner
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BranchId { get; set; }
        public DateTimeOffset? DeleteDate { get; set; }
    }
}