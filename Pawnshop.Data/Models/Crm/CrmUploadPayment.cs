using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Core;
using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Data.Models.Crm
{
    public class CrmUploadPayment : IEntity
    {
        public int Id { get; set; }

        /// <summary>
        /// Договор
        /// </summary>
        public int ContractId { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Дата выгрузки
        /// </summary>
        public DateTime? UploadDate { get; set; }

        /// <summary>
        /// Дата постановки в очередь
        /// </summary>
        public DateTime? QueueDate { get; set; }
    }
}