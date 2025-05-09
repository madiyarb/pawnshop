using System;
using Pawnshop.Core;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.Clients
{
    /// <summary>
    /// Виды экономической деятельности клиента
    /// </summary>
    public class ClientEconomicActivityType : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Код
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Альтернативное наименование
        /// </summary>
        public string NameAlt { get; set; }
        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }

        public User Author { get; set; }
        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
        /// <summary>
        /// Ссылка на родителя
        /// </summary>
        public int? ParentId { get; set; }
        /// <summary>
        /// Признак наследника
        /// </summary>
        public bool HasChild { get; set; }
    }
}