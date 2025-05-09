using System;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Membership
{
    /// <summary>
    /// Компания (ломбард)
    /// </summary>
    public class Organization : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Наименование организации
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Заблокирован
        /// </summary>
        public bool Locked { get; set; }

        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        public string Uid { get; set; }

        /// <summary>
        /// Конфигурация
        /// </summary>
        public Configuration Configuration { get; set; }
    }
}