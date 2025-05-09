using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Contracts
{
    public class ContractPartialSign : IEntity
    {
        /// <summary>
        /// Идентификатор детализации
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Общая сумма договора
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Идентификатор заявки
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        public DateTime DeleteDate { get; set; }
    }
}
