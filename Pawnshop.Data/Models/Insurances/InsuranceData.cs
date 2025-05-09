using System;
using Pawnshop.Data.CustomTypes;

namespace Pawnshop.Data.Models.Insurances
{
    public class InsuranceData : IJsonObject
    {
        /// <summary>
        /// Номер счета на оплату
        /// </summary>
        public string InvoiceNumber { get; set; }

        /// <summary>
        /// Номер платежного поручения банка
        /// </summary>
        public string PaymentNumber { get; set; }

        /// <summary>
        /// Дата платежного поручения банка
        /// </summary>
        public DateTime? PaymentDate { get; set; }
    }
}
