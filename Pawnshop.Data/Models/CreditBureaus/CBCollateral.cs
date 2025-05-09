using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;
using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class CBCollateral : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор КБ контракта
        /// </summary>
        public int CBContractId { get; set; }
        
        /// <summary>
        /// Вид залога
        /// </summary>
        public int TypeId { get; set; }
        
        /// <summary>
        /// Идентификатор населенного пункта
        /// </summary>
        public int? LocationId { get; set; }
        
        /// <summary>
        /// КАТО код
        /// </summary>
        public string KATOID { get; set; }
        
        /// <summary>
        /// Стоимость залога
        /// </summary>
        public Decimal Value { get; set; }
        
        /// <summary>
        /// Валюта
        /// </summary>
        public string Currency { get; set; }
        
        /// <summary>
        /// Тип стоимости
        /// </summary>
        public int ValueTypeId { get; set; }

    }
}