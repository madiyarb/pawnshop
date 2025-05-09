using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Справочник расчетных показателей
    /// </summary>
    public class NotionalRate : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        public int RateTypeId { get; set; }
        public DateTime Date { get; set; }
        public decimal RateValue { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime DeleteDate { get; set; }
        public int AuthorId { get; set; }
    }
}
