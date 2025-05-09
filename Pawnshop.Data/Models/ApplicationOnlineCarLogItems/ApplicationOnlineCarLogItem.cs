using System;
using Dapper.Contrib.Extensions;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Models.ApplicationOnlineCarLogItems
{
    [Table("ApplicationOnlineCarLogItems")]
    public sealed class ApplicationOnlineCarLogItem : ApplicationOnlineCarLogData
    {
        [ExplicitKey]
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int? UserId { get; set; }

        public ApplicationOnlineCarLogItem()
        {
            
        }

        public ApplicationOnlineCarLogItem(ApplicationOnlineCarLogData data, int? userId)
        {
            Id = Guid.NewGuid();
            CreateDate = DateTime.Now;
            UserId = userId;
            ApplicationOnlineCarId = data.ApplicationOnlineCarId;
            VehicleMarkId = data.VehicleMarkId;
            VehicleModelId = data.VehicleModelId;
            ReleaseYear = data.ReleaseYear;
            TechPassportNumber = data.TechPassportNumber;
            BodyNumber = data.BodyNumber;
            TransportNumber = data.TransportNumber;
            Color = data.Color;
            TechPassportDate = data.TechPassportDate;
        }
    }
}
