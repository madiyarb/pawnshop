using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pawnshop.Data.Models.Dictionaries
{
    public interface IVehicle : IEntity
    {
        public int VehicleMarkId { get; set; }
        public VehicleMark VehicleMark { get; set; }
        public int VehicleModelId { get; set; }
        public VehicleModel VehicleModel { get; set; }
    }
}
