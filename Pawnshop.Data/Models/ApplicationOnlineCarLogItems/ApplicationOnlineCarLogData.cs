using System;
using Pawnshop.Data.Models.ApplicationsOnlineCar;

namespace Pawnshop.Data.Models.ApplicationOnlineCarLogItems
{
    public class ApplicationOnlineCarLogData
    {
        public Guid ApplicationOnlineCarId { get; set; }
        /// <summary>
        /// Идентификатор Марки авто
        /// </summary>
        public int? VehicleMarkId { get; set; }

        /// <summary>
        /// Идентификатор модели Авто
        /// </summary>
        public int? VehicleModelId { get; set; }

        /// <summary>
        /// Год выпуска
        /// </summary>
        public int? ReleaseYear { get; set; }

        /// <summary>
        /// Номер тех пасспорта
        /// </summary>
        public string TechPassportNumber { get; set; }

        /// <summary>
        /// Номер кузова/Vin code
        /// </summary>
        public string BodyNumber { get; set; }

        /// <summary>
        /// Номер авто
        /// </summary>
        public string TransportNumber { get; set; }

        /// <summary>
        /// Цвет автомобиля
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Дата тех паспорта
        /// </summary>
        public DateTime? TechPassportDate { get; set; }

        public ApplicationOnlineCarLogData()
        {
            
        }

        public ApplicationOnlineCarLogData(ApplicationOnlineCar car)
        {
            ApplicationOnlineCarId = car.Id;
            VehicleMarkId = car.VehicleMarkId;
            VehicleModelId = car.VehicleModelId;
            ReleaseYear = car.ReleaseYear;
            TechPassportNumber = car.TechPassportNumber;
            BodyNumber = car.BodyNumber;
            TransportNumber = car.TransportNumber;
            Color = car.Color;
            TechPassportDate = car.TechPassportDate;
        }
    }
}
