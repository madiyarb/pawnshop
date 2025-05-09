using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Mintos
{
    public class MintosInvestorPaymentScheduleItem : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор договора
        /// </summary>
        public int ContractId { get; set; }

        /// <summary>
        /// Идентификатор договора Mintos
        /// </summary>
        public int MintosContractId { get; set; }
        
        /// <summary>
        /// Номер платежа
        /// </summary>
        public int Number { get; set; }

        /// <summary>
        /// Дата платежа(отправлено)
        /// </summary>
        public DateTime SendedDate { get; set; }

        /// <summary>
        /// Дата платежа(получено)
        /// </summary>
        public DateTime MintosDate { get; set; }

        /// <summary>
        /// Платеж погашен
        /// </summary>
        public bool IsRepaid { get; set; } = false;

        /// <summary>
        /// Погашение ОД(отправлено)
        /// </summary>
        public decimal SendedPrincipalAmount { get; set; } = 0;

        /// <summary>
        /// Погашение ОД(получено)
        /// </summary>
        public decimal MintosPrincipalAmount { get; set; } = 0;

        /// <summary>
        /// Погашение ОД(выплачено)
        /// </summary>
        public decimal PrincipalAmountPaid { get; set; } = 0;

        /// <summary>
        /// Погашение процентов(отправлено)
        /// </summary>
        public decimal SendedInterestAmount { get; set; } = 0;

        /// <summary>
        /// Погашение процентов(получено)
        /// </summary>
        public decimal MintosInterestAmount { get; set; } = 0;

        /// <summary>
        /// Погашение процентов(выплачено)
        /// </summary>
        public decimal InterestAmountPaid { get; set; } = 0;

        /// <summary>
        /// Погашение штрафов(отправлено)
        /// </summary>
        public decimal SendedDelayedAmount { get; set; } = 0;

        /// <summary>
        /// Погашение штрафов(получено)
        /// </summary>
        public decimal MintosDelayedAmount { get; set; } = 0;

        /// <summary>
        /// Погашение штрафов(выплачено)
        /// </summary>
        public decimal DelayedAmountPaid { get; set; } = 0;

        /// <summary>
        /// Общая сумма погашения(отправлено)
        /// </summary>
        public decimal SendedTotalSum { get; set; } = 0;

        /// <summary>
        /// Общая сумма погашения(получено)
        /// </summary>
        public decimal MintosTotalSum { get; set; } = 0;

        /// <summary>
        /// Общая сумма погашения(выплачено)
        /// </summary>
        public decimal TotalSumPaid { get; set; } = 0;

        /// <summary>
        /// Остаток ОД до выплаты(отправлено)
        /// </summary>
        public decimal SendedTotalRemainingPrincipal { get; set; } = 0;

        /// <summary>
        /// Остаток ОД до выплаты(получено)
        /// </summary>
        public decimal MintosTotalRemainingPrincipal { get; set; } = 0;

        /// <summary>
        /// Статус платежа
        /// </summary>
        public MintosInvestorPaymentStatus Status { get; set; }

        /// <summary>
        /// Дата отмены платежа
        /// </summary>
        public DateTime? CancelDate { get; set; }

        public int GetLinkedEntityId()
        {
            return MintosContractId;
        }
    }
}
