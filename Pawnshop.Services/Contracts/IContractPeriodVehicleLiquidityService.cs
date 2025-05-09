using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.Contracts
{
    public interface IContractPeriodVehicleLiquidityService
    {
        ListModel<ContractPeriodVehicleLiquidity> List(ListQuery listQuery);
        ContractPeriodVehicleLiquidity Get(int id);
        ContractPeriodVehicleLiquidity GetPeriodByLiquidity(int releaseYear, int vehicleMarkId, int vehicleModelId);
        ContractPeriodVehicleLiquidity Save(ContractPeriodVehicleLiquidity car);
        int GetLiquidityByVehicleMarkAndModel(int releaseYear, int vehicleMarkId, int vehicleModelId);
        void Delete(int id);
    }
}
