using Pawnshop.Data.Models.ApplicationsOnline;

namespace Pawnshop.Services.TasOnlinePermissionValidator
{
    public interface ITasOnlinePermissionValidatorService
    {
        public bool ValidateApplicationOnlineStatusWithUserRole(ApplicationOnline applicationOnline);
        public bool ManagerCanOwnApplication(ApplicationOnline applicationOnline);
        public bool UserCanSetResponsibleManagerToNull(ApplicationOnline applicationOnline);
        public bool IsAdministrator();
        public bool AnyRole();
        public bool CanEditEncumbranceRegisteredState();
    }
}
