using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.PayOperations
{
    public class PayOperationNumberCounter : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Год
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Филиал
        /// </summary>
        public int BranchId { get; set; }
        /// <summary>
        /// Филиал
        /// </summary>
        public int BranchCode { get; set; }

        /// <summary>
        /// Вид оплаты
        /// </summary>
        public int PayTypeId { get; set; }

        /// <summary>
        /// Счетчик
        /// </summary>
        public int Counter { get; set; }
    }
}
