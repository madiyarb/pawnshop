using System;

namespace Pawnshop.Data.Models.Auction
{
    /// <summary>
    /// Запрос для регистрации продажи авто по аукциону
    /// </summary>
    public class RegisterAuctionSaleRequest
    {
        /// <summary>
        /// Идентификатор запроса для связки с внешним сервисом
        /// </summary>
        public Guid RequestId { get; set; }
        public DateTime CreateDate { get; set; }
        
        /// <summary>
        /// Иденификатор филиала отправителя
        /// </summary>
        public int BranchId { get; set; }
        public int AuthorId { get; set; }
        public decimal Amount { get; set; }
        
        /// <summary>
        /// Положительная/отрицательная маржа
        /// </summary>
        public decimal Profit { get; set; }
        
        /// <summary>
        /// заметка
        /// </summary>
        public string? Note { get; set; }
        
        /// <summary>
        /// Сумма расходов по авто в аукционе
        /// </summary>
        public decimal? Expenses { get; set; }

        /// <summary>
        /// Идентификатор контрагента регистрации бизнес операции
        /// </summary>
        public int OrderUserId { get; set; }
        
        /// <summary>
        /// Идентификатор клиента покупатеоя(контр агент)
        /// </summary>
        public int ClientId { get; set; }
    }
}