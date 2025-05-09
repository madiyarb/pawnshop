using System;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Core;
using Pawnshop.Core.Validation;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Счет
    /// </summary>
    public class VehicleWMI : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор Марки
        /// </summary>
        /// 
        [RequiredId(ErrorMessage = "Поле Марка обязательно для заполнения")]
        public int VehicleMarkId { get; set; }
        
        public VehicleMark VehicleMark { get; set; }

        /// <summary>
        /// Код
        /// </summary>
        [Required(ErrorMessage = "Поле Код обязательно для заполнения")]
        [RegularExpression("^[A-Za-z0-9]{1,4}$", ErrorMessage = "Поле код должно содержать не более 4 символов")]
        public string Code { get; set; }

        /// <summary>
        /// Дополнительная информация
        /// </summary>
        public string AdditionalInfo { get; set; }

        /// <summary>
        /// Производитель 
        /// </summary>
        [RequiredId(ErrorMessage = "Поле Производитель обязательно для заполнения")]
        public int VehicleManufacturerId { get; set; }

        public VehicleManufacturer VehicleManufacturer { get; set; }

        /// <summary>
        /// Производитель 
        /// </summary>
        [RequiredId(ErrorMessage = "Поле Код страны обязательно для заполнения")]
        public int VehicleCountryCodeId { get; set; }

        public VehicleCountryCode VehicleCountryCode { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
    }
}