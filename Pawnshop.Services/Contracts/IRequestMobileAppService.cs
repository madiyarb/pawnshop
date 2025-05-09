using Pawnshop.Data.Models.MobileApp;
using System.Threading.Tasks;

namespace Pawnshop.Services.Contracts
{
    public interface IRequestMobileAppService : IService
    {
        Task UpdateStatusInMobileApp(UpdateOnStatusPositionRegistration appId);
    }
}