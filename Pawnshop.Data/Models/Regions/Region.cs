using System;
using System.Collections.Generic;
using Pawnshop.Data.Models.Membership;
using Pawnshop.AccountingCore.Abstractions;

namespace Pawnshop.Data.Models.Regions
{
    public class Region : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Название
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Код
        /// </summary>
        public string Code { get; set; }
        
        /// <summary>
        /// Перевод
        /// </summary>
        public string? NameAlt { get; set; }

        public List<Group>? Groups { get; set; }
        
        public DateTime? DeleteDate { get; set; }
    }
}