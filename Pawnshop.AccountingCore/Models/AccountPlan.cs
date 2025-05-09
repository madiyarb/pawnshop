using System;
using Pawnshop.AccountingCore.Abstractions;

namespace Pawnshop.AccountingCore.Models
{
    public class AccountPlan : IDictionary, ICreateLogged, ISoftDelete
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
        /// Организация
        /// </summary>
        public int OrganizationId { get; set; }

        /// <summary>
        /// Является активным
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Является балансовым
        /// </summary>
        public bool IsBalance { get; set; }
    }
}
