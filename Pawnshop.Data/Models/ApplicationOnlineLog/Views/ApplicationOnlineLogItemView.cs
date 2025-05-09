using System;

namespace Pawnshop.Data.Models.ApplicationOnlineLog.Views
{
    public sealed class ApplicationOnlineLogItemView
    {
        public Guid Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; }
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

        /// <summary>
        /// Название продукта 
        /// </summary>
        public string ProductName { get; set; }
    }
}
