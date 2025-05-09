using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Contracts
{
    public class ContractPaymentScheduleRevision : ContractPaymentSchedule
    {
        /// <summary>
        /// Идентификатор дополнительного контакта клиента
        /// </summary>
        public int ContractPaymentScheduleId { get; set; }

        /// <summary>
        /// Идентификатор пользователя который обновил запись
        /// </summary>
        public int? UpdatedByAuthorId { get; set; }

        /// <summary>
        /// Дата обновления
        /// </summary>
        public DateTime? UpdateDate { get; set; }
    }
}
