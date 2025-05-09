using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.Vehicle
{
    public class PositionDetails
    {
        public string Mark { get; set; }
        public string Model { get; set; }
        public string VehicleType { get; set; }
        public bool IsDisabled { get; set; }
        public int VehicleMarkId { get; set; }
        public int VehicleModelId { get; set; }
        public int ReleaseYear { get; set; }
    }
}
