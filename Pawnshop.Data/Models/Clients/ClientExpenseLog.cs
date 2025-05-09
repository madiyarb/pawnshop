using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Clients
{
    /// <summary>
    /// Логи расходов клиента
    /// </summary>
    public class ClientExpenseLog : ClientExpense
    {
        /// <summary>
        /// Дата обновления
        /// </summary>
        public DateTime? UpdateDate { get; set; }

        /// <summary>
        /// Обновлено пользователем
        /// </summary>
        public int? UpdatedByAuthorId { get; set; }
    }
}
