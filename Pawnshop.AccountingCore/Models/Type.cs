using System;
using Pawnshop.AccountingCore.Abstractions;

namespace Pawnshop.AccountingCore.Models
{
    /// <summary>
    /// Иерархия типов
    /// </summary>
    public class Type : IDictionary, ICreateLogged, IInheritable, ISoftDelete
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
        /// Родитель
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Родитель
        /// </summary>
        public Type Parent { get; set; }

        /// <summary>
        /// Тип группы
        /// </summary>
        public TypeGroup TypeGroup { get; set; }
    }
}
