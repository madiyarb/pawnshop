using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Cars
{
    public interface IVehicleLiquidityService
    {
        ListModel<VehicleLiquidity> List(ListQuery listQuery);
        ListModel<VehicleLiquidity> List(ListQueryModel<VehicleLiquidityFilter> listQuery);
        VehicleLiquidity Get(int id);
        int Get(int vehicleMarkId, int vehicleModelId, int releaseYear);
        int GetByYearCondition(VehicleLiquidity entity, int releaseYear);
        VehicleLiquidity Save(VehicleLiquidity entity);
        void Delete(int id);
    }
}
