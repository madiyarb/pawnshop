using System;

namespace Pawnshop.Data.Models.LegalCollection.Dtos
{
    /// <summary>
    /// DTO Для данных онтракта длдя legal case
    /// </summary>
    public class LegalCaseContractDto
    {
        public DateTime ContractDate { get; set; }
        public int? BranchId { get; set; }
        public ProductDto? Product { get; set; }
        public ClientDto? Client { get; set; }
        public CarDto? Car { get; set; }
        public int? DelayDays { get; set; }
        
        /// <summary>
        /// Причина передачи дела в Legal collection
        /// </summary>
        public ReasonDto? Reason { get; set; }
    }
}