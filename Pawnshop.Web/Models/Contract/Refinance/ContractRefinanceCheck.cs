using Pawnshop.Core;
using Pawnshop.Data.Models.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Web.Models.Contract.Refinance
{
    public class ContractRefinanceCheck : ILoggableToEntity
    {
        /// <summary>
        /// Дата рефинанса
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Идентификатор договора
        /// </summary>
        public int ContractId { get; set; }
        /// <summary>
        /// Вид начисления процентов в новом договоре
        /// </summary>
        public PercentPaymentType PercentPaymentType { get; set; }
        /// <summary>
        /// Дата первой оплаты
        /// </summary>
        public DateTime? FirstPaymentDate { get; set; }
        /// <summary>
        /// Количество платежей
        /// </summary>
        public int? PaymentQuantity { get; set; }

        public int GetLinkedEntityId()
        {
            return ContractId;
        }
    }
}
