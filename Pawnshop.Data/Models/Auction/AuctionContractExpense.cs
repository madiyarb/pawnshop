using System;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Auction
{
    /// <summary>
    /// Сущность для связки договора и расхода
    /// </summary>
    public class AuctionContractExpense : IEntity
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        public int ContractExpenseId { get; set; }
        public string? AuthorName { get; set; }
        public DateTimeOffset CreateDate { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset? DeleteDate { get; set; }
    }
}