using System;
using System.Linq;

namespace Pawnshop.Core.Impl
{
    public class SessionContext : ISessionContext
    {
        private int _userId;
        private string _userName;
        private int _organizationId;
        private string _organizationName;
        private string _organizationUid;
        private bool _forSupport;
        private string[] _permissions;

        public SessionContext()
        {
            IsInitialized = false;
        }

        public void Init(
            int userId, string userName,
            int organizationId, string organizationName, string organizationUid,
            bool forSupport,
            string[] permissions)
        {
            _userId = userId;
            _userName = userName;
            _organizationId = organizationId;
            _organizationName = organizationName;
            _organizationUid = organizationUid;
            _forSupport = forSupport;
            _permissions = permissions;
            IsInitialized = true;
        }

        public bool HasPermission(string permission)
        {
            CheckIfInitialized();
            return Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
        }

        public bool IsInitialized { get; private set; }

        public int UserId
        {
            get {
                CheckIfInitialized();
                return _userId;
            }
        }

        public string UserName
        {
            get
            {
                CheckIfInitialized();
                return _userName;
            }
        }

        public int OrganizationId
        {
            get
            {
                CheckIfInitialized();
                return _organizationId;
            }
        }

        public string OrganizationName
        {
            get
            {
                CheckIfInitialized();
                return _organizationName;
            }
        }

        public string OrganizationUid
        {
            get
            {
                CheckIfInitialized();
                return _organizationUid;
            }
        }

        public bool ForSupport
        {
            get
            {
                CheckIfInitialized();
                return _forSupport;
            }
        }

        public string[] Permissions
        {
            get
            {
                CheckIfInitialized();
                return _permissions;
            }
        }

        private void CheckIfInitialized()
        {
            if (!IsInitialized)
                throw new InvalidOperationException("Session Context not initialized");
        }
    }
}