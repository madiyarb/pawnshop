using System;

namespace Pawnshop.Data.Models.ApplicationOnlineCarLogItems.Views
{
    public sealed class ApplicationOnlineCarLogItemView
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
        public Guid ApplicationOnlineCarId { get; set; }
        public string? VehicleMarkName { get; set; }
        /// <summary>
        /// Идентификатор Марки авто
        /// </summary>
        public int? VehicleMarkId { get; set; }

        public string? VehicleModelName { get; set; }
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
    }
}
