using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.CashOrders
{
    /// <summary>
    /// Счетчик номеров кассовых ордеров
    /// </summary>
    public class CashOrderNumberCounter : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Тип кассового ордера
        /// </summary>
        public OrderType OrderType { get; set; }

        /// <summary>
        /// Год
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Код филиала
        /// </summary>
        public int BranchId { get; set; }

        /// <summary>
        /// Счетчик
        /// </summary>
        public int Counter { get; set; }
    }
}