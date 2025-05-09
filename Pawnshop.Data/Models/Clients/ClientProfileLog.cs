using Pawnshop.Data.Models.Domains;
using Pawnshop.Data.Models.Membership;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Clients
{
    /// <summary>
    /// Логи профайла клиента
    /// </summary>
    public class ClientProfileLog : ClientProfile
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
