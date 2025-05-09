using Pawnshop.Data.Models.Membership;
using System.Collections.Generic;
using System.Linq;
using Pawnshop.Core;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Web.Models.Membership
{
    public class CardModel<T> : ILoggable where T : Member
    {
        public T Member { get; set; }

        public List<Group> Groups { get; set; }

        public List<Role> Roles { get; set; }

        public object Format()
        {
            return new
            {
                Member,
                Groups = Groups.Select(g => new {g.Id, g.Name}).ToArray(),
                Roles = Roles.Select(r => new {r.Id, r.Name}).ToArray(),
            };
        }
    }
}