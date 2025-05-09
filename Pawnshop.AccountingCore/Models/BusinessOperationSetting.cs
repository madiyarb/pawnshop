using System;
using Pawnshop.AccountingCore.Abstractions;

namespace Pawnshop.AccountingCore.Models
{
    /// <summary>
    /// Настройки бизнес-операций
    /// </summary>
    public class BusinessOperationSetting : IDictionary, ICreateLogged, ISoftDelete
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Наименование альтернативное
        /// </summary>
        public string NameAlt { get; set; }

        /// <summary>
        /// Уникальный код
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Операция
        /// </summary>
        public int BusinessOperationId { get; set; }

        /// <summary>
        /// Порядковый номер
        /// </summary>
        public int? OrderBy { get; set; }

        /// <summary>
        /// Настройка счета по дебету
        /// </summary>
        public int? DebitSettingId { get; set; }

        /// <summary>
        /// Настройка счета по кредиту
        /// </summary>
        public int? CreditSettingId { get; set; }

        /// <summary>
        /// Тип суммы операции
        /// </summary>
        public AmountType AmountType { get; set; }

        /// <summary>
        /// Назначение/причина
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Назначение/причина на казахском
        /// </summary>
        public string ReasonKaz { get; set; }

        /// <summary>
        /// Признак использования
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Настройка типа платежа
        /// </summary>
        public int? PayTypeId { get; set; }

        /// <summary>
        /// Тип ордера
        /// </summary>
        public OrderType OrderType { get; set; }

        /// <summary>
        /// Статья расходов по умолчанию
        /// </summary>
        public int? DefaultArticleTypeId { get; set; }

    }
}
