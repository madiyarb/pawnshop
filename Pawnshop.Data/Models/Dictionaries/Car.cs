using Pawnshop.Core.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Автомобиль
    /// </summary>
    public class Car : Position, IVehicle, IBodyNumber
    {
        private string _mark;

        /// <summary>
        /// Марка
        /// </summary>
        [Required(ErrorMessage = "Поле марка обязательно для заполнения")]
        public string Mark
        {
            get { return _mark.ToUpper(); }
            set { _mark = value.ToUpper(); }
        }

        private string _model;

        /// <summary>
        /// Модель
        /// </summary>
        [Required(ErrorMessage = "Поле модель обязательно для заполнения")]
        public string Model
        {
            get { return _model.ToUpper(); }
            set { _model = value.ToUpper(); }
        }

        /// <summary>
        /// Год выпуска
        /// </summary>
        [Required(ErrorMessage = "Поле год выпуска обязательно для заполнения")]
        public int ReleaseYear { get; set; }

        private string _transportNumber;

        /// <summary>
        /// Гос номер
        /// </summary>
        [Required(ErrorMessage = "Поле ГРНЗ обязательно для заполнения")]
        public string TransportNumber
        {
            get { return _transportNumber.ToUpper(); }
            set { _transportNumber = value.ToUpper(); }
        }

        /// <summary>
        /// Номер двигателя
        /// </summary>
        public string MotorNumber { get; set; }

        /// <summary>
        /// Номер кузова
        /// </summary>
        [Required(ErrorMessage = "Поле VIN код обязательно для заполнения")]
        public string BodyNumber { get; set; }

        /// <summary>
        /// Номер техпаспорта
        /// </summary>
        [Required(ErrorMessage = "Поле номер техпаспорта обязательно для заполнения")]
        public string TechPassportNumber { get; set; }

        /// <summary>
        /// Дата техпаспорта
        /// </summary>
        [Required(ErrorMessage = "Поле дата техпаспорта обязательно для заполнения")]
        public DateTime? TechPassportDate { get; set; }

        private string _color;

        /// <summary>
        /// Цвет
        /// </summary>
        [Required(ErrorMessage = "Поле цвет обязательно для заполнения")]
        public string Color
        {
            get { return _color.ToUpper(); }
            set { _color = value.ToUpper(); }
        }

        public int? ParkingStatusId { get; set; }

        /// <summary>
        /// Марка
        /// </summary>
        [RequiredId(ErrorMessage = "Поле марка обязательно для заполнения")]
        public int VehicleMarkId { get; set; }

        /// <summary>
        /// Марка
        /// </summary>
        public VehicleMark VehicleMark { get; set; }

        /// <summary>
        /// Модель
        /// </summary>
        [RequiredId(ErrorMessage = "Поле модель обязательно для заполнения")]
        public int VehicleModelId { get; set; }

        /// <summary>
        /// Модель
        /// </summary>
        public VehicleModel VehicleModel { get; set; }

        /// <summary>
        /// Идентификатор контракта
        /// </summary>
        public int? ContractId { get; set; }
    }
}
