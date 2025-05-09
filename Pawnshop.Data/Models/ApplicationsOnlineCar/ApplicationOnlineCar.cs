using System;
using System.Collections.Generic;
using Dapper.Contrib.Extensions;

namespace Pawnshop.Data.Models.ApplicationsOnlineCar
{
    [Table("ApplicationOnlineCars")]
    public sealed class ApplicationOnlineCar
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [ExplicitKey]
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
        /// Номер кузова/Vin code
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

        /// <summary>
        /// CarId
        /// </summary>
        public int? CarId { get; set; }

        public ApplicationOnlineCar() { }

        public ApplicationOnlineCar(Guid id)
        {
            Id = id;
        }

        /// <summary>
        /// Это конструктор для создания из мобильного приложения когда автомобиль найден в гос базе
        /// </summary>
        /// <param name="id"></param>
        /// <param name="techPassportModel"></param>
        /// <param name="category"></param>
        /// <param name="techCategory"></param>
        /// <param name="releaseYear"></param>
        /// <param name="transportNumber"></param>
        /// <param name="motorNumber"></param>
        /// <param name="bodyNumber"></param>
        /// <param name="techPassportNumber"></param>
        /// <param name="techPassportDate"></param>
        /// <param name="color"></param>
        /// <param name="ownerRegionName"></param>
        /// <param name="ownerRegionNameEn"></param>
        /// <param name="notes"></param>
        public ApplicationOnlineCar(string techPassportModel, string category, string techCategory,
            string releaseYear, string transportNumber, string motorNumber, string bodyNumber, string techPassportNumber,
            long? techPassportDate, string color, string ownerRegionName, string ownerRegionNameEn, string notes)
        {
            Id = Guid.NewGuid();
            TechPassportModel = techPassportModel;
            Category = category;
            TechCategory = techCategory;
            int year;
            if (int.TryParse(releaseYear, out year))
            {
                ReleaseYear = year;
            }
            else
            {
                ReleaseYear = 0;
            }
            TransportNumber = transportNumber;
            MotorNumber = motorNumber;
            BodyNumber = bodyNumber;
            TechPassportNumber = techPassportNumber;
            if (techPassportDate == null)
            {
                TechPassportDate = DateTime.UnixEpoch;
            }
            else
            {
                DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                dateTime = dateTime.AddMilliseconds(techPassportDate.Value).ToLocalTime();
                TechPassportDate = dateTime;
            }
            Color = color;
            OwnerRegionName = ownerRegionName;
            OwnerRegionNameEn = ownerRegionNameEn;
            Notes = notes;
        }

        public void Update(string? mark, string? model, string? techPassportModel, 
            string? category, string? techCategory, int? releaseYear, string? transportNumber, 
            string? motorNumber, string? bodyNumber, string? techPassportNumber, DateTime? techPassportDate,
            string? color, int? vehicleMarkId, int? vehicleModelId, string? ownerRegionName,
            string? ownerRegionNameEn, string? notes)
        {
            if (!string.IsNullOrEmpty(mark) && mark != Mark)
            {
                Mark = mark;
            }

            if (!string.IsNullOrEmpty(model) && model != Model)
            {
                Model = model;
            }

            if (!string.IsNullOrEmpty(techPassportModel) && techPassportModel != TechPassportModel)
            {
                TechPassportModel = techPassportModel;
            }

            if (!string.IsNullOrEmpty(category) && category != Category)
            {
                Category = category;
            }

            if (!string.IsNullOrEmpty(techCategory) && techCategory != TechCategory)
            {
                TechCategory = techCategory;
            }

            if (releaseYear.HasValue && releaseYear != ReleaseYear)
            {
                ReleaseYear = releaseYear;
            }

            if (!string.IsNullOrEmpty(transportNumber) && transportNumber != TransportNumber)
            {
                TransportNumber = transportNumber;
            }

            if (!string.IsNullOrEmpty(motorNumber) && motorNumber != MotorNumber)
            {
                MotorNumber = motorNumber;
            }

            if (!string.IsNullOrEmpty(bodyNumber) && bodyNumber != BodyNumber)
            {
                BodyNumber = bodyNumber;
            }

            if (!string.IsNullOrEmpty(techPassportNumber) && techPassportNumber != TechPassportNumber)
            {
                TechPassportNumber = techPassportNumber;
            }

            if (techPassportDate.HasValue && techPassportDate != TechPassportDate)
            {
                TechPassportDate = techPassportDate;
            }

            if (!string.IsNullOrEmpty(color) && color != Color)
            {
                Color = color;
            }

            if (vehicleMarkId.HasValue && vehicleMarkId != VehicleMarkId)
            {
                VehicleMarkId = vehicleMarkId;
            }

            if (vehicleModelId.HasValue && vehicleModelId != VehicleModelId)
            {
                VehicleModelId = vehicleModelId;
            }

            if (!string.IsNullOrEmpty(ownerRegionName) && ownerRegionName != OwnerRegionName)
            {
                OwnerRegionName = ownerRegionName;
            }

            if (!string.IsNullOrEmpty(ownerRegionNameEn) && ownerRegionNameEn != OwnerRegionNameEn)
            {
                OwnerRegionNameEn = ownerRegionNameEn;
            }

            if (!string.IsNullOrEmpty(notes) && notes != Notes)
            {
                Notes = notes;
            }
        }

        public void UpdateLiquidity(string liquidity)
        {
            Liquidity = liquidity;
        }

        public void FillExtensionFieldsFromMobileApp(string techPassportModel, string category, string techCategory, string motorNumber, string ownerRegionName, string ownerRegionNameEn, string notes)
        {
            TechPassportModel = techPassportModel;
            Category = category;
            TechCategory = techCategory;
            MotorNumber = motorNumber;
            OwnerRegionName = ownerRegionName;
            OwnerRegionNameEn = ownerRegionNameEn;
            Notes = notes;
        }

        public List<string> EmptyFieldsList()
        {
            List<string> emptyStringList = new List<string>();
            if (string.IsNullOrEmpty(BodyNumber))
                emptyStringList.Add("Vin CODE");
            if (!VehicleModelId.HasValue)
                emptyStringList.Add("Модель авто");
            if (!VehicleMarkId.HasValue)
                emptyStringList.Add("Марка авто");
            if (string.IsNullOrEmpty(TransportNumber))
                emptyStringList.Add("Номер авто");
            if (!ReleaseYear.HasValue)
                emptyStringList.Add("Год выпуска");
            if (string.IsNullOrEmpty(Color))
                emptyStringList.Add("Цвет авто");
            if (string.IsNullOrEmpty(TechPassportNumber))
                emptyStringList.Add("Номер тех. пасспорта");
            if (!TechPassportDate.HasValue)
                emptyStringList.Add("Дата тех. пасспорта");
            return emptyStringList;
        }

        public bool CarsEqualsForCreation(ApplicationOnlineCar car)
        {
            if (MotorNumber != car.MotorNumber || TechPassportNumber != car.TechPassportNumber || Color != car.Color ||
                TechPassportDate != car.TechPassportDate )
            {
                return false;
            }

            return true;
        }

    }
}
