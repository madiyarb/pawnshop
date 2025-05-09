using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts.Actions;
using IEntity = Pawnshop.Core.IEntity;

namespace Pawnshop.Data.Models.Contracts.Inscriptions
{
    public class InscriptionRow : IEntity
    {
        public int Id { get; set; }
        public int InscriptionId { get; set; }
        public int? InscriptionActionId { get; set; }
        /// <summary>
        /// Тип погашения
        /// </summary>
        public AmountType PaymentType { get; set; }
        /// <summary>
        /// Счет дебета
        /// </summary>
        public int? DebitAccountId { get; set; }

        public Account DebitAccount { get; set; }
        /// <summary>
        /// Счет кредита
        /// </summary>
        public int? CreditAccountId { get; set; }
        public Account CreditAccount { get; set; }
        /// <summary>
        /// Процент
        /// </summary>
        public decimal? Percent { get; set; }
        /// <summary>
        /// Период
        /// </summary>
        public int? Period { get; set; }
        /// <summary>
        /// Сумма
        /// </summary>
        public decimal Cost { get; set; }
        /// <summary>
        /// Тип погашения
        /// </summary>
        public int? OrderId { get; set; }
    }
}