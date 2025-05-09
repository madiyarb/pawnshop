using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Clients
{
    /// <summary>
    /// Актив клиента
    /// </summary>
    public class ClientAsset : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// Идентификатор типа актива
        /// </summary>
        public int AssetTypeId { get; set; }

        /// <summary>
        /// Количество
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Создано в
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Удалено в
        /// </summary>
        public DateTime? DeleteDate { get; set; }
    }
}
