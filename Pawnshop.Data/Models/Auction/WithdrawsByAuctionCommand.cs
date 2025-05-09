using System;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Data.Models.Auction
{
    /// <summary>
    /// Запрос для регистрации Аукциона для авто
    /// </summary>
    public class WithdrawsByAuctionCommand
    {
        /// <summary>
        /// Идентификатор запроса для связки с внешним сервисом Аукцион
        /// </summary>
        public Guid RequestId { get; set; }

        public int ContractId { get; set; }
        public Contract? Contract { get; set; }
        public int AuthorId { get; set; }
        public decimal Amount { get; set; }
        
        /// <summary>
        /// Идентификатор ContractAction
        /// </summary>
        public int? ContractActionId { get; set; }

        public CarAuction? Auction { get; set; }
        
        /// <summary>
        /// заметка
        /// </summary>
        public string? Note { get; set; }
    }
}