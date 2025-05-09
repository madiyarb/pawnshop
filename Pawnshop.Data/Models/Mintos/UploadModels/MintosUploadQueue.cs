using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Core;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Models.Mintos.UploadModels
{
    /// <summary>
    /// Очередь выгрузки в Mintos
    /// </summary>
    public class MintosUploadQueue : IEntity
    {
        /// <summary>
        /// Идентификатор
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор договора
        /// </summary>
        public int ContractId { get; set; }

        /// <summary>
        /// Идентификатор договора Mintos
        /// </summary>
        public int? MintosContractId { get; set; }

        /// <summary>
        /// Идентификатор валюты
        /// </summary>
        public int CurrencyId { get; set; }

        /// <summary>
        /// Идентификатор валюта
        /// </summary>
        public Currency Currency { get; set; }

        /// <summary>
        /// Статус выгрузки
        /// </summary>
        public MintosUploadStatus Status { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Дата выгрузки
        /// </summary>
        public DateTime? UploadDate { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
    }
}
