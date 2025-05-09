using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Contracts.Kdn
{
    /// <summary>
    /// Данные запросов в ПКБ
    /// </summary>
    public class ContractKdnRequest : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        public int KdnCalculationId { get; set; }
        public int ClientId { get; set; }
        public decimal IncomeRequest { get; set; }
        public string FCBRequestId { get; set; }
        public decimal KDNScore { get; set; }
        public decimal Debt { get; set; }
        public decimal IncomeResponse { get; set; }
        public int CreditInfoId { get; set; }
        public Guid CorrelationId { get; set; }
        public DateTime RequestDate { get; set; }
    }
}
