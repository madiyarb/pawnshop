using System;
using Pawnshop.Core;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Data.Models.Transfers.TransferContracts
{
    /// <summary>
    /// Трансфер 
    /// </summary>
    public class TransferContract : IEntity, ILoggableToEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Номер пула
        /// </summary>
        public int PoolTransferId { get; set; }

        /// <summary>
        /// Номер договора
        /// </summary>
        public int? ContractId { get; set; }

        /// <summary>
        /// Статус трансфера
        /// </summary>
        public TransferContractStatus Status { get; set; } = TransferContractStatus.New;

        /// <summary>
        /// Ссылка на действие
        /// </summary>
        public int? ActionId { get; set; }

        /// <summary>
        /// Сообщения об ошибках
        /// </summary>
        public string ErrorMessages { get; set; }

        /// <summary>
        /// Дата трансфера договора
        /// </summary>
        public DateTime CreateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Входная позиция
        /// </summary>
        public int EntryPosition { get; set; }

        /// <summary>
        /// Входной номер договора
        /// </summary>
        public string EntryСontractNumber { get; set; }

        /// <summary>
        /// Входной ИИН/БИН
        /// </summary>
        public string EntryСlientIdentityNumber { get; set; }

        /// <summary>
        /// Сумма перевода
        /// </summary>
        public decimal Amount { get; set; } = 0;

        public int GetLinkedEntityId()
        {
            return (int)ContractId;
        }

        public Contract Contract { get; set; }
    }
}
