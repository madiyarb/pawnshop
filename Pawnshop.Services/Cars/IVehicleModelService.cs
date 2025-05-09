using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Models.List;
using Pawnshop.Services.Models.Vehicle;
using System.Threading.Tasks;

namespace Pawnshop.Services.Cars
{
    public interface IVehicleModelService
    {
        ListModel<VehicleModelDto> List(ListQueryModel<VehicleModelListQueryModel> listQuery);
        VehicleModelDto Card(int id);
        VehicleModelDto Save(VehicleModelDto vehicleModelDto);
        void Delete(int id);
        Task<VehicleModel> GetOrCreateModelNameWithMarkIdAsync(string name, int markId);
    }
}
