using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Membership
{
    public class MemberRoleChangesHistory
    {
        public int Id { get; set; }
        public int TakenFromUserId { get; set; }
        public int GivenToUserId { get; set; }
        public int RoleId { get; set; }
        public DateTime CreateDate { get; set; }
        public int AuthorId { get; set; }
        public string Note { get; set; }
    }
}
