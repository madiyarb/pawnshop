namespace Pawnshop.Web.Models.Membership
{
    public class UserListQueryModel
    {
        public int OrganizationId { get; set; }

        public bool Locked { get; set; }

        public int? BranchId { get; set; }
        public int? RoleId { get; set; }
    }
}