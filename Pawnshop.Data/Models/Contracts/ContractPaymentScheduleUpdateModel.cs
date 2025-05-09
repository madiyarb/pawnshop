using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Files;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.Contracts
{
    /// <summary>
    /// Договор займа
    /// </summary>
    public class ContractPaymentScheduleUpdateModel : ILoggableToEntity
    {
        /// <summary>
        /// Идентификатор договора
        /// </summary>
        public int ContractId { get; set; }

        /// <summary>
        /// Дата первой оплаты
        /// </summary>
        public DateTime? FirstPaymentDate { get; set; }  
        
        /// <summary>
        /// Контрольная дата
        /// </summary>
        public DateTime? ChangedControlDate { get; set; }

        /// <summary>
        /// График погашения
        /// </summary>
        public List<ContractPaymentSchedule> Schedule { get; set; }

        public int GetLinkedEntityId()
        {
            return ContractId;
        }
    }
}