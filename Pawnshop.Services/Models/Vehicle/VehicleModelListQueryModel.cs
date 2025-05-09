using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.Vehicle
{
    public class VehicleModelListQueryModel
    {
        public int? VehicleMarkId { get; set; }
        public int? VehicleModelId { get; set; }
        public int? LiquidDefault { get; set; }
        public int? LiquidByAdditionCondition { get; set; }
    }
}
