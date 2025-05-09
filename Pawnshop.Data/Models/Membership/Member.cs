using System;
using System.Collections.Generic;
using Pawnshop.Core;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Models.Membership
{
    /// <summary>
    /// Участник
    /// </summary>    
    public abstract class Member : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор организации
        /// </summary>
        public int OrganizationId { get; set; }

        /// <summary>
        /// Организация
        /// </summary>
        public Organization Organization { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Заблокирован
        /// </summary>
        public bool Locked { get; set; }
    }
}