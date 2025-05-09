using Pawnshop.Data.Models.Dictionaries;
using System.Threading.Tasks;

namespace Pawnshop.Services.Cars
{
    public interface IVehicleMarkService
    {
        Task<VehicleMark> GetOrCreateMarkNameAsync(string name);
    }
}
