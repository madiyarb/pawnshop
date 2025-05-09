using System;

namespace Pawnshop.Data.Models.ApplicationOnlineLog
{
    public class ApplicationOnlineLogData
    {
        /// <summary>
        /// Идентификатор заявки 
        /// </summary>
        public Guid ApplicationId { get; set; }
        /// <summary>
        /// Продукт
        /// </summary>
        public int ProductId { get; set; }
        /// <summary>
        /// Срок займа в месяцах
        /// </summary>
        public int LoanTerm { get; set; }

        /// <summary>
        /// Сумма заявки
        /// </summary>
        public decimal ApplicationAmount { get; set; }
    }
}
