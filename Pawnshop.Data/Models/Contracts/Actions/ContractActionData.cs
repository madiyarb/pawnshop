using Pawnshop.Data.CustomTypes;
using Pawnshop.Data.Models.InnerNotifications;
using Pawnshop.Data.Models.Membership;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Contracts.Actions
{
    public class ContractActionData : IJsonObject
    {
        /// <summary>
        /// Выбранные позиции
        /// </summary>
        public ContractPosition[] Positions { get; set; }

        /// <summary>
        /// Количество дней продления
        /// </summary>
        public int ProlongPeriod { get; set; }

        /// <summary>
        /// Порожденный перевод
        /// </summary>
        public int? RemittanceId { get; set; }

        /// <summary>
        /// Филиал в котором делали
        /// </summary>
        public Group Branch { get; set; }

        /// <summary>
        /// Созданное уведомление
        /// </summary>
        public InnerNotification Notification { get; set; }

        /// <summary>
        /// Использованная сумма предоплаты
        /// </summary>
        public decimal PrepaymentUsed { get; set; }

        /// <summary>
        /// Идентификатор аннуитетных платежей
        /// </summary>
        public int? PaymentScheduleId { get; set; }

        /// <summary>
        /// Итого с клиента
        /// </summary>
        public decimal? TotalLeft { get; set; }

        /// <summary>
        /// Номер пула в СФК
        /// </summary>
        public int? PoolNumber { get; set; }

        /// <summary>
        /// Идентификатор переведенных договоров  
        /// </summary>
        public int? TransferContractId { get; set; }

        /// <summary>
        /// Название банка 
        /// </summary>
        public string ProcessingBankName { get; set; }

        /// <summary>
        /// Способ оплаты в банке   
        /// </summary>
        public string ProcessingBankNetwork { get; set; }

        public decimal MigratedPrepaymentCost { get; set; }

        public bool CategoryChanged { get; set; }

        public decimal Apr { get; set; }

        public DateTime? MaturityDate { get; set; }

        public DateTime? NextPaymentDate { get; set; }

        public DateTime? CreditLineMaturityDate { get; set; }

        public int? ClosedAccountId { get; set; }
        
        public int? OpenedAccountId { get; set; }
    }
}
