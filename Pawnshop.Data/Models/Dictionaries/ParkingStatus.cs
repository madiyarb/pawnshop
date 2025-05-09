using System;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class ParkingStatus : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название статуса
        /// </summary>
        [Required(ErrorMessage = "Поле название статуса обязательно для заполнения")]
        public string StatusName { get; set; }
        /// <summary>
        /// Код статуса
        /// </summary>
        public string StatusCode { get; set; }
        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
    }
}
