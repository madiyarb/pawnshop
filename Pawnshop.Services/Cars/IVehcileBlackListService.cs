using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Cars
{
    public interface IVehcileBlackListService
    {
        void Validate(VehiclesBlackListItem model);
    }
}
