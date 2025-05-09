using System;

namespace Pawnshop.Web.Models.ApplicationOnlineCar
{
    public sealed class ApplicationOnlineCarBinding
    {

        /// <summary>
        /// Модель указаная в тех пасспорте 
        /// </summary>
        public string TechPassportModel { get; set; }

        /// <summary>
        /// Категория автомобиля
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Техническая Категория
        /// </summary>
        public string TechCategory { get; set; }

        /// <summary>
        /// Год выпуска
        /// </summary>
        public int? ReleaseYear { get; set; }

        /// <summary>
        /// Номер авто
        /// </summary>
        public string TransportNumber { get; set; }

        /// <summary>
        /// Номер двигателя
        /// </summary>
        public string MotorNumber { get; set; }

        /// <summary>
        /// Номер кузова
        /// </summary>
        public string BodyNumber { get; set; }

        /// <summary>
        /// Номер тех пасспорта
        /// </summary>
        public string TechPassportNumber { get; set; }

        /// <summary>
        /// Дата тех паспорта
        /// </summary>
        public DateTime? TechPassportDate { get; set; }

        /// <summary>
        /// Цвет автомобиля
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Идентификатор Марки авто
        /// </summary>
        public int? VehicleMarkId { get; set; }

        /// <summary>
        /// Идентификатор модели Авто
        /// </summary>
        public int? VehicleModelId { get; set; }

        /// <summary>
        /// Регион Регистрации 
        /// </summary>
        public string OwnerRegionName { get; set; }

        /// <summary>
        /// Регион регистрации англ
        /// </summary>
        public string OwnerRegionNameEn { get; set; }

        /// <summary>
        /// Заметки
        /// </summary>
        public string Notes { get; set; }
    }
}
