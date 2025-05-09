using Pawnshop.Core;
using Pawnshop.Data.Models.CashOrders;

namespace Pawnshop.Data.Models.TasOnline
{
    public class TasOnlinePayment : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Идентификатор платежного ордера
        /// </summary>
        public int OrderId { get; set; }

        public CashOrder Order { get; set; }
        /// <summary>
        /// Статус оплаты
        /// </summary>
        public TasOnlinePaymentStatus Status { get; set; }
        /// <summary>
        /// Идентификатор транзакции в ТасОнлайн
        /// </summary>
        public string TasOnlineDocumentId { get; set; }
        public string TasOnlineContractId { get; set; }
    }
}