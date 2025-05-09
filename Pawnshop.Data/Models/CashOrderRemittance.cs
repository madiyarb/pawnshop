using System;
using Pawnshop.Core;

namespace Pawnshop.Data.Models
{
    /// <summary>
    /// Сущность для связки кассового ордера и перевода
    /// </summary>
    public class CashOrderRemittance : IEntity
    {
        public int Id { get; set; }
        public int CashOrderId { get; set; }
        public int RemittanceId { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
    }
}
