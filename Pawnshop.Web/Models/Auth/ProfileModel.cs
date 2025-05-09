using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Web.Models.Auth
{
    public class ProfileModel
    {
        public User User { get; set; }

        public Organization Organization { get; set; }

        public Group[] Branches { get; set; }
    }
}