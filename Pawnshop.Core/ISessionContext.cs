namespace Pawnshop.Core
{
    public interface ISessionContext
    {
        int UserId { get; }

        string UserName { get; }

        int OrganizationId { get; }

        string OrganizationName { get; }

        string OrganizationUid { get; }

        bool ForSupport { get; }

        string[] Permissions { get; }

        bool HasPermission(string permission);

        bool IsInitialized { get; }

        void Init(int userId, string userName, int organizationId, string organizationName, string organizationUid, bool forSupport, string[] permissions);
    }
}
