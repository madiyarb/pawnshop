using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Processing
{
    /// <summary>
    /// Входная информация от платежной системы для сверки
    /// </summary>
    public class OnlinePaymentsManageModel
    {
        /// <summary>
        /// Дата и время операции
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Идентификатор онлайн оплаты
        /// </summary>
        public Int64 Receipt { get; set; }

        /// <summary>
        /// Идентификатор договора
        /// </summary>
        public int? ContractId { get; set; }

        /// <summary>
        /// ИИН клиента
        /// </summary>
        public string IdentityNumber { get; set; }

        /// <summary>
        /// Сумма
        /// </summary>
        public decimal Amount { get; set; }
    }
}
