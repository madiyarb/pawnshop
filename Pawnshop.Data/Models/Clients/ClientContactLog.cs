using Pawnshop.Data.Models.Membership;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Clients
{
    /// <summary>
    /// Логи контакта клиента
    /// </summary>
    public class ClientContactLog : ClientContact
    {
        /// <summary>
        /// Идентификатор контакта
        /// </summary>
        public int ClientContactId { get; set; }

        /// <summary>
        /// Обновлено пользователем
        /// </summary>
        public int? UpdatedByAuthorId { get; set; }

        /// <summary>
        /// Обновлено в
        /// </summary>
        public DateTime? UpdateDate { get; set; }
    }
}
