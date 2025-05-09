using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Clients
{
    /// <summary>
    /// Логи актива клиента
    /// </summary>
    public class ClientAssetLog : ClientAsset
    {
        /// <summary>
        /// Идентификатор родительской записи
        /// </summary>
        public int ClientAssetId { get; set; }

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
