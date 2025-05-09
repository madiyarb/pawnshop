using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Services.Models.Filters
{
    public class VehicleLiquidityFilter
    {
        public int? VehicleMarkId { get; set; }

        public int? VehicleModelId { get; set; }

        public int? LiquidDefault { get; set; }

        public int? LiquidByAdditionCondition { get; set; }
    }
}
