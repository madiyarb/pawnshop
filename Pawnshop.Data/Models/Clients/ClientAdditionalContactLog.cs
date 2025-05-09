using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Clients
{
    /// <summary>
    /// Логи доп контактов клиента
    /// </summary>
    public class ClientAdditionalContactLog : ClientAdditionalContact
    {
        /// <summary>
        /// Идентификатор дополнительного контакта клиента
        /// </summary>
        public int ClientAdditionalContactId { get; set; }

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
