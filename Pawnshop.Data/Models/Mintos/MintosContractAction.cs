using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Mintos.UploadModels;

namespace Pawnshop.Data.Models.Mintos
{
    public class MintosContractAction : IEntity
    {
        public MintosContractAction()
        {

        }

        public MintosContractAction(ContractAction action)
        {
            ContractActionId = action.Id;
            ContractId = action.ContractId;
            Status = MintosUploadStatus.Await;
        }

        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор договора
        /// </summary>
        public int ContractId { get; set; }

        /// <summary>
        /// Идентификатор договора в Mintos
        /// </summary>
        public int MintosContractId { get; set; }

        /// <summary>
        /// Идентификатор действия по договору
        /// </summary>
        public int ContractActionId { get; set; }

        /// <summary>
        /// Действие по договору
        /// </summary>
        public ContractAction ContractAction { get; set; }

        /// <summary>
        /// Идентификатор платежа инвестору
        /// </summary>
        public int? InvestorScheduleId { get; set; }

        /// <summary>
        /// Статус выгрузки платежа
        /// </summary>
        public MintosUploadStatus Status { get; set; }

        /// <summary>
        /// Дата загрузки в Mintos
        /// </summary>
        public DateTime? UploadDate { get; set; }

        /// <summary>
        /// Даата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        public int GetLinkedEntityId()
        {
            return MintosContractId;
        }
    }
}