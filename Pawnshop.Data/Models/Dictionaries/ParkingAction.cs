using System;
using System.ComponentModel.DataAnnotations;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class ParkingAction : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название действия
        /// </summary>
        [Required(ErrorMessage = "Поле название действия обязательно для заполнения")]
        public string ActionName { get; set; }

        /// <summary>
        /// Статус до действия
        /// </summary>
        public int StatusBeforeId { get; set; }

        public ParkingStatus StatusBefore { get; set; }

        /// <summary>
        /// Статус после действия
        /// </summary>
        public int StatusAfterId { get; set; }

        public ParkingStatus StatusAfter { get; set; }

        /// <summary>
        /// Категория аналитики
        /// </summary>
        public int CategoryId { get; set; }

        public Category Category { get; set; }

        /// <summary>
        /// Вид залога
        /// </summary>
        public CollateralType CollateralType { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
    }
}
