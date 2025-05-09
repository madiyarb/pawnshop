using Pawnshop.Core;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using System;

namespace Pawnshop.Data.Models.OnlinePayments.OnlinePaymentRevises.OnlinePaymentRevisePows
{
    /// <summary>
    /// Содержания сверки с платежной системы
    /// </summary>
    public class OnlinePaymentReviseRow : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор сверки
        /// </summary>
        public int ReviseId { get; set; }

        /// <summary>
        /// Идентификатор транзакции
        /// </summary>
        public Int64 ProcessingId { get; set; }

        public string TransactionId => ProcessingId.ToString();

        /// <summary>
        /// Сумма транзакции
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// БИН организации получателя денег (TasCredit/TasFinance)
        /// </summary>
        public string CompanyBin { get; set; }

        /// <summary>
        /// Идентификатор организации получателя денег (TasCredit/TasFinance)
        /// </summary>
        public int OrganizationId { get; set; }

        /// <summary>
        /// Идентификатор договора
        /// </summary>
        public int ContractId { get; set; }

        public Contract Contract { get; set; }

        /// <summary>
        /// Идентификатор действия
        /// </summary>
        public int? ActionId { get; set; }

        public ContractAction Action { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        public OnlinePaymentReviseRowStatus Status { get; set; } = OnlinePaymentReviseRowStatus.Draft;

        /// <summary>
        /// Примечания
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Дата 
        /// </summary>
        public DateTime? Date { get; set; }
    }
}
