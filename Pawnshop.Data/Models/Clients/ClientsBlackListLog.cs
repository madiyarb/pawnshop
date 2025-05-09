using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Clients
{
    public class ClientsBlackListLog: ClientsBlackList
    {
        /// <summary>
        /// Идентификатор родительской записи
        /// </summary>
        public int ClientsBlackListId { get; set; }

        /// <summary>
        /// Обновлено пользователем
        /// </summary>
        public int? UpdatedByAuthorId { get; set; }

        /// <summary>
        /// Обновено в
        /// </summary>
        public DateTime? UpdateDate { get; set; }
    }
}
