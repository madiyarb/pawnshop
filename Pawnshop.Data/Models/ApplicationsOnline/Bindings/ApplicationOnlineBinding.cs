using System;

namespace Pawnshop.Data.Models.ApplicationsOnline.Bindings
{
    public sealed class ApplicationOnlineBinding
    {

        /// <summary>
        /// Сумма заявки
        /// </summary>
        public decimal? ApplicationAmount { get; set; }

        /// <summary>
        /// Срок займа в месяцах
        /// </summary>
        public int? LoanTerm { get; set; }

        /// <summary>
        /// Стадия
        /// </summary>
        public int? Stage { get; set; }
        /// <summary>
        /// Идентификатор продукта
        /// </summary>
        public int? ProductId { get; set; }

        /// <summary>
        /// Источник заявки
        /// </summary>
        public string? ApplicationSource { get; set; }

        /// <summary>
        /// Цель кредита
        /// </summary>
        public int? LoanPurposeId { get; set; }

        /// <summary>
        /// Цель кредита
        /// </summary>
        public int? BusinessLoanPurposeId { get; set; }

        /// <summary>
        /// Идентификатор для физических лиц (ОКЭД)
        /// если Цель кредита - бизнеса
        /// </summary>
        public int? OkedForIndividualsPurposeId { get; set; }

        /// <summary>
        /// Идентификатор целевой цели кредита
        /// если Цель кредита - бизнеса
        /// </summary>
        public int? TargetPurposeId { get; set; }

        /// <summary>
        /// Канал привлечения
        /// </summary>
        public int? AttractionChannelId { get; set; }

        /// <summary>
        /// Привязка к филиалу
        /// </summary>
        public int? BranchId { get; set; }

        /// <summary>
        /// Дата первого платежа
        /// </summary>
        public DateTime? FirstPaymentDate { get; set; }
    }
}
