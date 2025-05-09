using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Models.List;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Auction.Dtos.Car;

namespace Pawnshop.Services.Cars
{
    public interface ICarService
    {
        ListModel<Car> ListWithCount(ListQuery listQuery);
        Car Get(int id);
        Car Save(Car car);
        void Delete(int id);
        List<string> Colors();
        void Validate(Car car);
        Car Find(object query);
        Task<IEnumerable<Car>> ListByClientId(int clientid);
        Task<List<AuctionCarDto>> GetByTransportNumber(string transportNumber);
    }
}
