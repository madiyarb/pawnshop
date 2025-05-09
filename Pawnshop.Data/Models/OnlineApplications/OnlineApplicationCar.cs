using Pawnshop.Core;
using System;

namespace Pawnshop.Data.Models.OnlineApplications
{
    public class OnlineApplicationCar : IEntity
    {
        public int Id { get; set; }
        public string Mark { get; set; }
        public string Model { get; set; }
        public int? ReleaseYear { get; set; }
        public string TransportNumber { get; set; }
        public string MotorNumber { get; set; }
        public string BodyNumber { get; set; }
        public string TechPassportNumber { get; set; }
        public DateTime? TechPassportDate { get; set; }
        public string Color { get; set; }
        public int? VehicleMarkId { get; set; }
        public int? VehicleModelId { get; set; }
    }
}
