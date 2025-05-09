using Pawnshop.Core;
using Pawnshop.Data.Models.Contracts.Actions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.OnlinePayments
{

    /// <summary>
    /// Страховой договор
    /// </summary>
    public class OnlinePayment : IEntity
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
        /// Референс от процессинга
        /// </summary>
        public long? ProcessingId { get; set; }

        /// <summary>
        /// Корректировка просроченных процентов
        /// </summary>
        public ProcessingType? ProcessingType { get; set; }

        /// <summary>
        /// Название банка процессинга
        /// </summary>
        public string ProcessingBankName { get; set; }

        /// <summary>
        /// Сеть банка процессинга
        /// </summary>
        public string ProcessingBankNetwork { get; set; }

        /// <summary>
        /// Сумма
        /// </summary>
        public decimal? Amount { get; set; }

        /// <summary>
        /// Статус процессинга
        /// </summary>
        public ProcessingStatus? ProcessingStatus { get; set; }

        /// <summary>
        /// Идентификатор действия договора
        /// </summary>
        public int? ContractActionId { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Дата завершения действия
        /// </summary>
        public DateTime? FinishDate { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
    }
}
