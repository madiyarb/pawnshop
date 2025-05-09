using System;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Core;
using Pawnshop.Core.Validation;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>Максимальный срок от ликвидности автомобиля</summary>
    public class ContractPeriodVehicleLiquidity : IEntity
    {
        /// <summary>Идентификатор</summary>
        public int Id { get; set; }

        /// <summary>Возраст автомобиля</summary>
        [Required(ErrorMessage = "Поле Возраст обязательно для заполнения")]
        public int? Age { get; set; }

        /// <summary>Ликвидность автомобиля</summary>
        [Required(ErrorMessage = "Поле Ликвидность обязательно для заполнения")]
        public int LiquidValue { get; set; }

        /// <summary>Максимальный срок в месяцах</summary>
        [Required(ErrorMessage = "Поле Максимальный срок обязательно для заполнения")]
        public int MaxMonthsCount { get; set; }

        /// <summary>Идентификатор автора</summary>
        [RequiredId(ErrorMessage = "Поле Идентификатор автора обязательно для заполнения")]
        public int AuthorId { get; set; }

        /// <summary>Дата удаления</summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>Дата создания</summary>
        public DateTime CreateDate { get; set; }
    }
}