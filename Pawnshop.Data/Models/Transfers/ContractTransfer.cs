using Pawnshop.Core;
using Pawnshop.Data.Models.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Transfers
{
    /// <summary>
    /// Переводы
    /// </summary>
    public class ContractTransfer : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Договор
        /// </summary>
        public int? ContractId { get; set; }

        public Contract Contract { get; set; }

        /// <summary>
        /// Дата перевода
        /// </summary>
        public DateTime TransferDate { get; set; }

        /// <summary>
        /// Дата возврата
        /// </summary>
        public DateTime? BackTransferDate { get; set; }

        /// <summary>
        /// Номер перевода
        /// </summary>
        public int PoolNumber { get; set; }
    }
}
