using System;
using Dapper.Contrib.Extensions;

namespace Pawnshop.Data.Models.ApplicationOnlineRefinances
{
    [Table("ApplicationOnlineRefinances")]
    public sealed class ApplicationOnlineRefinance
    {

        /// <summary>
        /// Идентификатор
        /// </summary>
        [ExplicitKey]
        public Guid Id { get; set; }

        /// <summary>
        /// Идентификатор заявки
        /// </summary>
        public Guid ApplicationOnlineId { get; set; }

        /// <summary>
        /// Идентификатор контракта
        /// </summary>
        public int? ContractId { get; set; }

        /// <summary>
        /// Номер контракта
        /// </summary>
        public string? ContractNumber { get; set; }

        /// <summary>
        /// Идентификатор рефинансируемого кнтракта
        /// </summary>
        public int RefinancedContractId { get; set; }
        /// <summary>
        /// Номер рефинансируемого контракта
        /// </summary>
        public string RefinancedContractNumber { get; set; }

        /// <summary>
        /// Рефинансирование необходимо
        /// </summary>
        public bool RefinanceRequired { get; set; }

        /// <summary>
        /// Дата создания 
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        public ApplicationOnlineRefinance() { }

        public ApplicationOnlineRefinance(Guid applicationOnlineId, int refinancedContractId, string refinancedContractNumber, bool refinanceRequired)
        {
            Id = Guid.NewGuid();
            ApplicationOnlineId = applicationOnlineId;
            RefinancedContractId = refinancedContractId;
            RefinancedContractNumber = refinancedContractNumber;
            CreateDate = DateTime.Now;
            RefinanceRequired = refinanceRequired;
        }

        public void Delete()
        {
            DeleteDate = DateTime.Now;
        }

        public void SetContractId(int contractId)
        {
            ContractId = contractId;
        }
    }
}
