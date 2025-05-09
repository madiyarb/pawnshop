using System;
using System.Collections.Generic;
using System.Linq;
using Pawnshop.Core;
using Pawnshop.Data.Models.ApplicationsOnline;

namespace Pawnshop.Services.TasOnlinePermissionValidator
{
    public sealed class TasOnlinePermissionValidatorService : ITasOnlinePermissionValidatorService
    {
        private readonly ISessionContext _sessionContext;
        private readonly List<ApplicationOnlineStatus> validApplicationOnlineStatusForManager;
        private readonly List<ApplicationOnlineStatus> validApplicationOnlineStatusForVerificator;
        public TasOnlinePermissionValidatorService(ISessionContext sessionContext)
        {
            _sessionContext = sessionContext;
            validApplicationOnlineStatusForManager = new List<ApplicationOnlineStatus>
            {
                ApplicationOnlineStatus.Created,
                ApplicationOnlineStatus.Consideration,
                ApplicationOnlineStatus.RequisiteCheck,
                ApplicationOnlineStatus.ModificationFromVerification
            };

            validApplicationOnlineStatusForVerificator = new List<ApplicationOnlineStatus>
            {
                ApplicationOnlineStatus.Verification,
                ApplicationOnlineStatus.Approved,
                ApplicationOnlineStatus.OnEstimation,
                ApplicationOnlineStatus.EstimationCompleted,
                ApplicationOnlineStatus.RequiresCorrection,
                ApplicationOnlineStatus.BiometricCheck,
                ApplicationOnlineStatus.BiometricPassed,
                ApplicationOnlineStatus.Declined
            };
        }

        public bool ValidateApplicationOnlineStatusWithUserRole(ApplicationOnline applicationOnline)
        {
            if (!_sessionContext.IsInitialized)
                return false;

            var isVerificator = _sessionContext.Permissions.Contains("TasOnlineVerificator");
            var isManager = _sessionContext.Permissions.Contains("TasOnlineManager");
            var isAdmin = _sessionContext.Permissions.Contains("TasOnlineAdministrator");

            if (isAdmin)
                return true;

            if (isVerificator && isManager)
                return true;

            if (!(isVerificator || isManager))
                return false;

            ApplicationOnlineStatus status = Enum.Parse<ApplicationOnlineStatus>(applicationOnline.Status);
            if (isManager)
                return CanEditByManager(status);

            if (isVerificator)
                return CanEditByVerificator(status);

            return true;

        }

        public bool ManagerCanOwnApplication(ApplicationOnline applicationOnline)
        {
            if (_sessionContext.Permissions.Contains("TasOnlineAdministrator"))
                return true;
            ApplicationOnlineStatus status;
            Enum.TryParse(applicationOnline.Status, out status);
            if (!(status == ApplicationOnlineStatus.Created || status == ApplicationOnlineStatus.Consideration))
                return true;
            if (applicationOnline.ResponsibleManagerId == null && status != ApplicationOnlineStatus.Created)
                return true;
            return false;
        }

        public bool UserCanSetResponsibleManagerToNull(ApplicationOnline applicationOnline)
        {
            if (_sessionContext.Permissions.Contains("TasOnlineAdministrator"))
                return true;
            if (applicationOnline.ResponsibleManagerId == _sessionContext.UserId)
                return true;
            return false;
        }

        public bool IsAdministrator()
        {
            if (_sessionContext.Permissions.Contains("TasOnlineAdministrator"))
                return true;
            return false;
        }

        public bool AnyRole()
        {
            if (!_sessionContext.IsInitialized)
            {
                return false;
            }
            var isVerificator = _sessionContext.Permissions.Contains("TasOnlineVerificator");
            var isManager = _sessionContext.Permissions.Contains("TasOnlineManager");
            if (!(isVerificator || isManager))
            {
                return false;
            }

            return true;
        }

        public bool CanEditEncumbranceRegisteredState()
        {
            var isAdmin = _sessionContext.Permissions.Contains("TasOnlineAdministrator");
            var isCreditAdmin = _sessionContext.Permissions.Contains("TasOnlineCreditAdministrator");

            return (isAdmin || isCreditAdmin);
        }


        private bool CanEditByManager(ApplicationOnlineStatus status)
        {
            if (validApplicationOnlineStatusForManager.Contains(status))
                return true;
            return false;
        }

        private bool CanEditByVerificator(ApplicationOnlineStatus status)
        {
            if (validApplicationOnlineStatusForVerificator.Contains(status))
                return true;
            return false;
        }
    }
}
