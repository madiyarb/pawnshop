using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Clients
{
    /// <summary>
    /// Логи места работы клиента
    /// </summary>
    public class ClientEmploymentLog : ClientEmployment
    {
        /// <summary>
        /// Идентификатор родительской записи
        /// </summary>
        public int ClientEmploymentId { get; set; }

        /// <summary>
        /// Обновлено пользователем
        /// </summary>
        public int? UpdatedByAuthorId { get; set; }

        /// <summary>
        /// Дата обновления
        /// </summary>
        public DateTime? UpdateDate { get; set; }
    }
}
