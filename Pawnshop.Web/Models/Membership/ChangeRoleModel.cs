using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Membership
{
    public class ChangeRoleModel
    {
        public int TakeFromUserId { get; set; }
        public int GiveToUserId { get; set; }
        public Role Role { get; set; }
        public string Note { get; set; }
    }
}
