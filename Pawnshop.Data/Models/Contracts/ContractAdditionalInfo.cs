using System;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Contracts
{
    public class ContractAdditionalInfo : IEntity
    {
        public int Id { get; set; }
        public string SmsCode { get; set; }
        public string CbContractCode { get; set; }
        public int? SelectedBranchId { get; set; }
        public bool ClosedIsSend { get; set; }
        public string PartnerCode { get; set; }

        /// <summary>
        /// Измененная контрольная дата
        /// </summary>
        public DateTime? ChangedControlDate { get; set; }

        /// <summary>
        /// Дата изменения контрольной даты
        /// </summary>
        public DateTime? DateOfChangeControlDate { get; set; }

        /// <summary>
        /// Идентификатор списка файлов в файловом хранилище
        /// </summary>
        public int? StorageListId { get; set; }

        /// <summary>
        /// Идентификатор файла договора в хранилище ДРПП
        /// </summary>
        public Guid? LoanStorageFileId { get; set; }
    }
}
