using System;
using Pawnshop.AccountingCore.Abstractions;

namespace Pawnshop.AccountingCore.Models
{
    public class AccountSetting : IDictionary, ICreateLogged, ISoftDelete
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
        /// Признак аналитического счета
        /// </summary>
        public bool IsConsolidated { get; set; }

        /// <summary>
        /// Иерархия типов
        /// </summary>
        public int TypeId { get; set; }

        /// <summary>
        /// Тип суммы по умолчанию
        /// </summary>
        public AmountType? DefaultAmountType { get; set; }

        public bool SearchBranchBySessionContext { get; set; }
    }
}
