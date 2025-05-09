using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Data.Models.Contracts
{
    public class ContractHistory
    {

        /// <summary>
        /// Дата
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Предыдущий договор (родительский)
        /// </summary>
        public int ParentContractId { get; set; }

        /// <summary>
        /// Следующий договор (порожденный)
        /// </summary>
        public int ChildContractId { get; set; }

        /// <summary>
        /// Действие породившее
        /// </summary>
        public ContractActionType ActionType { get; set; }

        /// <summary>
        /// Предыдущий договор (родительский)
        /// </summary>
        public string ParentContractNumber { get; set; }

        /// <summary>
        /// Следующий договор (порожденный)
        /// </summary>
        public string ChildContractNumber { get; set; }
    }
}
