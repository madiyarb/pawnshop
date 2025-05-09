using Pawnshop.AccountingCore.Abstractions;
using System;

namespace Pawnshop.Data.Models
{
    /// <summary>
    ///  Запрос во внешний сервис Аукцион.
    /// /// </summary>
    public class CarAuction : IEntity
    {
        public int Id { get; set; }
        public int ContractId { get; set; }
        
        /// <summary>
        /// Идентификатор аукциона предоставляемый внешним сервисом
        /// </summary>
        public int AuctionId { get; set; }

        /// <summary>
        /// Сумма фактической реализации
        /// </summary>
        public decimal Cost { get; set; }
        
        /// <summary>
        /// Сумма списания(Суммы на авансовых счетах + сумма факт. реализации)
        /// </summary>
        public decimal WithdrawCost { get; set; }

        /// <summary>
        /// Идентификатор-связка регистрации кассовых ордеров с Аукционом
        /// </summary>
        public Guid OrderRequestId { get; set; }
        public DateTimeOffset? DeleteDate { get; set; }
    }
}
