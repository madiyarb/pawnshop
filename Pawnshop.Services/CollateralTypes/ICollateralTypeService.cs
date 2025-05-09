using System.Collections.Generic;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Services.CollateralTypes
{
    public interface ICollateralTypeService
    {
        public List<CollateralTypeInfo> GetCollateralTypes();
    }
}