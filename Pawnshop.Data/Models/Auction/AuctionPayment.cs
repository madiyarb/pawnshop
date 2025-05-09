using Pawnshop.Core;
using System;

namespace Pawnshop.Data.Models.Auction
{
    /// <summary>
    /// Сущность для хранения Cash order и модуля Аукцион
    /// </summary>
    public class AuctionPayment : IEntity
    {
        public int Id { get; set; }
        public int CashOrderId { get; set; }
        
        /// <summary>
        /// Идентификатор запроса
        /// </summary>
        public Guid RequestId { get; set; }
        public int AuthorId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
    }
}
