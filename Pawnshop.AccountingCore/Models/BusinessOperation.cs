using System;
using Pawnshop.AccountingCore.Abstractions;

namespace Pawnshop.AccountingCore.Models
{
    /// <summary>
    /// Бизнес-операции
    /// </summary>
    public class BusinessOperation : IDictionary, ICreateLogged, ISoftDelete
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
        /// Тип
        /// </summary>
        public int TypeId { get; set; }

        /// <summary>
        /// Организация
        /// </summary>
        public int OrganizationId { get; set; }

        /// <summary>
        /// Филиал
        /// </summary>
        public int? BranchId { get; set; }

        /// <summary>
        /// Признак ручных операций
        /// </summary>
        public bool IsManual { get; set; }

        /// <summary>
        /// Статус ордера при создании
        /// </summary>
        public int? OrdersCreateStatus { get; set; }

		/// <summary>
		/// Наличие статьи расхода
		/// </summary>
		public bool HasExpenseArticleType { get; set; }

	}
}
