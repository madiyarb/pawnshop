using System;

namespace Pawnshop.Data.Models.OnlineApplications
{
    public sealed class OnlineApplicationRefinance
    {
        /// <summary>
        /// Идентификатор записи
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Номер займа
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Номер рефинансируемого займа
        /// </summary>
        public string RefinancedContractNumber { get; set; }

        /// <summary>
        /// Идентификатор займа
        /// </summary>
        public int? ContractId { get; set; }
        /// <summary>
        /// Идентификатор рефинансируемого займа
        /// </summary>
        public int? RefinancedContractId { get; set; }

        /// <summary>
        /// Дата создания записи
        /// </summary>
        public DateTime? CreateDate { get; set; }

        /// <summary>
        /// Дата удаления записи
        /// </summary>
        public DateTime? DeleteDate { get; set; }
    }
}
