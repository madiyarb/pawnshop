using System;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Core;
using Pawnshop.Data.Models.AccountingCore;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Data.Models.LoanSettings
{
    /// <summary>
    /// Настройки процентных ставок шаблона договора
    /// </summary>
    public class LoanSettingRate : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Идентификатор шаблона договора
        /// </summary>
        public int ProductSettingId { get; set; }
        public LoanPercentSetting ProductSetting { get; set; }
        /// <summary>
        /// Идентификатор счета процентной ставки
        /// </summary>
        public int RateSettingId { get; set; }
        public AccountSetting RateSetting { get; set; }
        /// <summary>
        /// Ставка
        /// </summary>
        [CustomValidation(typeof(LoanSettingRate), "RateValidate")]
        public decimal Rate { get; set; }
        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }
        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
        /// <summary>
        /// Индекс
        /// </summary>
        public int? Index { get; set; }

		public static ValidationResult RateValidate(decimal value)
        {
            if (value < 0)
                return new ValidationResult("Ставка должна быть больше 0");

            if (value > 100)
                return new ValidationResult("Ставка должна быть меньше или равна 100");

            return ValidationResult.Success;
        }


    }
}