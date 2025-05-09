using Pawnshop.Data.Models.OnlineApplications;
using System.Threading.Tasks;

namespace Pawnshop.Services.OnlineApplications
{
    public interface IOnlineApplicationCarService
    {
        Task<OnlineApplicationCar> GetEntityForCreateAsync(OnlineApplicationCar car);
    }
}
