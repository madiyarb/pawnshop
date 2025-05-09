using System;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Социально уязвимая группа
    /// </summary>
    public class SociallyVulnerableGroup : IDictionary
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
        /// Категория
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime DeleteDate { get; set; }
    }
}