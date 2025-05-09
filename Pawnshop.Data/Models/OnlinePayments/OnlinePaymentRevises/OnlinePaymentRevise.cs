using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.OnlinePayments.OnlinePaymentRevises.OnlinePaymentRevisePows;
using System;
using Pawnshop.Core;
using System.Collections.Generic;

namespace Pawnshop.Data.Models.OnlinePayments.OnlinePaymentRevises
{
    /// <summary>
    /// Результат сверки с платежной системы
    /// </summary>
    public class OnlinePaymentRevise : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Дата совершения операции
        /// </summary>
        public string TransactionDate { get; set; }

        /// <summary>
        /// Тип платежной системы
        /// </summary>
        public ProcessingType ProcessingType { get; set; }

        /// <summary>
        /// Содержания сверки с платежной системы
        /// </summary>
        public List<OnlinePaymentReviseRow> Rows { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        public OnlinePaymentReviseStatus Status { get; set; } = OnlinePaymentReviseStatus.Draft;

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public int AuthorId { get; set; }

        public User Author { get; set; }
    }
}
