using System;

namespace Pawnshop.Data.Models.Auction
{
    /// <summary>
    /// Запрос для регистрации расхода авто по аукциону
    /// </summary>
    public class RegisterAuctionExpenseRequest
    {
        /// <summary>
        /// Идентификатор запроса для связки с внешним сервисом
        /// </summary>
        public Guid RequestId { get; set; }
        public DateTime CreateDate { get; set; }
        
        /// <summary>
        /// Иденификатор филиала получаетля
        /// </summary>
        public int BranchId { get; set; }

        public int AuthorId { get; set; } = 1;
        public decimal Amount { get; set; }
        
        /// <summary>
        /// Заметка
        /// </summary>
        public string? Note { get; set; }
        
        /// <summary>
        /// Идентификатор контрагента регистрации бизнес операции
        /// </summary>
        public int OrderUserId { get; set; }
    }
}