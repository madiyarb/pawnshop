using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Dictionaries;
using System.Threading.Tasks;

namespace Pawnshop.Services.Cars
{
    public class VehicleMarkService : IVehicleMarkService
    {
        private readonly VehicleMarkRepository _vehicleMarkRepository;

        public VehicleMarkService(VehicleMarkRepository vehicleMarkRepository)
        {
            _vehicleMarkRepository = vehicleMarkRepository;
        }


        public async Task<VehicleMark> GetOrCreateMarkNameAsync(string name)
        {
            var mark = await _vehicleMarkRepository.FindByNameOrCodeAsync(name);

            if (mark == null)
            {
                mark = new VehicleMark
                {
                    Code = name.Replace(" ", "_"),
                    Name = name,
                };

                _vehicleMarkRepository.Insert(mark);
            }

            return mark;
        }
    }
}
