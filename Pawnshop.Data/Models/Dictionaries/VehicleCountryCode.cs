using System;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Core;
using Pawnshop.Core.Validation;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Счет
    /// </summary>
    public class VehicleCountryCode : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Код
        /// </summary>
        [Required(ErrorMessage = "Поле Код обязательно")]
        public string Code { get; set; }

        /// <summary>
        /// Страна 
        /// </summary>
        [RequiredId(ErrorMessage = "Поле Страна обязательно для заполнения")]
        public int CountryId { get; set; }

        public Country Country { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
    }
}