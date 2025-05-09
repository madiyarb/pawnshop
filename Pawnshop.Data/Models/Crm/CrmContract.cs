using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Data.Models.Crm
{
    public class CrmContract
    {
        /// <summary>
        /// Идентификатор сделки/договора
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название сделки/договора
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Стадия сделки/договора в CRM
        /// </summary>
        public string StageId { get; set; }

        /// <summary>
        /// Статус сделки/договора в CRM (по канбану)
        /// </summary>
        public CrmStatus Status { get; set; }

        /// <summary>
        /// Сумма сделки
        /// </summary>
        public decimal Opportunity { get; set; }

        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        public int ContactId { get; set; }

        /// <summary>
        /// Идентификатор договора в нашей системе
        /// </summary>
        public string ContractId { get; set; }
        
        /// <summary>
        /// Идентификатор филиала/направления сделки
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Дата подписания договора
        /// </summary>
        public DateTime SignDate { get; set; }

        /// <summary>
        /// Канал привлечения
        /// </summary>
        public string AttractionChannel { get; set; }
    }
}
