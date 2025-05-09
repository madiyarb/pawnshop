using System;
using Pawnshop.AccountingCore.Abstractions;

namespace Pawnshop.AccountingCore.Models
{
    /// <summary>
    /// Настройки порядка погашения
    /// </summary>
    public class AccrualBase : IEntity, ICreateLogged
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Тип начисления
        /// </summary>
        public AccrualType AccrualType { get; set; }

        /// <summary>
        /// Счет - база для начисления
        /// </summary>
        public int BaseSettingId { get; set; }

        /// <summary>
        /// Сумма для начисления
        /// </summary>
        public AmountType AmountType { get; set; }

        /// <summary>
        /// Описание базы для начисления
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Альтернативное описание базы для начисления
        /// </summary>
        public string NameAlt { get; set; }
        
        /// <summary>
        /// Признак использования
        /// </summary>
        public bool IsActive { get; set; }
        
        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// Идентификатор счета процентной ставки
        /// </summary>
        public int? RateSettingId { get; set; }
        public AccountSetting RateSetting { get; set; }
    }
}
