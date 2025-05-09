using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Cars
{
    public class VehcileBlackListService : IVehcileBlackListService
    {
        private readonly IVehcileService _vehcileService;

        public VehcileBlackListService(IVehcileService vehcileService)
        {
            _vehcileService = vehcileService;
        }

        public void Validate(VehiclesBlackListItem model)
        {
            _vehcileService.BodyNumberValidate(model.BodyNumber);
            _vehcileService.BodyNumberValidate(model.ConfirmedBodyNumber);
        }
    }
}
