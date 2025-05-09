using System;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.ApplicationsOnlineCar.Views
{
    public sealed class ApplicationOnlineCarView
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Марка
        /// </summary>
        public string Mark { get; set; }

        /// <summary>
        /// Модель
        /// </summary>
        public string Model { get; set; }

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

        /// <summary>
        /// Ликвидность
        /// </summary>
        public string Liquidity { get; set; }

        public int? CarId { get; set; }

        public ApplicationOnlineCarView(Guid id, string mark, string model, string techPassportModel, string category, 
            string techCategory, int? releaseYear, string transportNumber, string motorNumber, string bodyNumber,
            string techPassportNumber, DateTime? techPassportDate, string color, int? vehicleMarkId, int? vehicleModelId,
            string ownerRegionName, string ownerRegionNameEn, string notes, string liquidity, int? carId)
        {
            Id = id;
            Mark = mark;
            Model = model;
            TechPassportModel = techPassportModel;
            Category = category;
            TechCategory = techCategory;
            ReleaseYear = releaseYear;
            TransportNumber = transportNumber;
            MotorNumber = motorNumber;
            BodyNumber = bodyNumber;
            TechPassportNumber = techPassportNumber;
            TechPassportDate = techPassportDate;
            Color = color;
            VehicleMarkId = vehicleMarkId;
            VehicleModelId = vehicleModelId;
            OwnerRegionName = ownerRegionName;
            OwnerRegionNameEn = ownerRegionNameEn;
            Notes = notes;
            CarLiquidity liquidityEnum;
            if (Enum.TryParse(liquidity, out liquidityEnum))
            {
                Liquidity = liquidityEnum.GetDisplayName();
            }
            else
            {
                Liquidity = liquidity;
            }
            CarId = carId;

        }
    }
}
