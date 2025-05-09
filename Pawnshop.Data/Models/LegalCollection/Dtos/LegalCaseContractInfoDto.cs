using System;

namespace Pawnshop.Data.Models.LegalCollection.Dtos
{
    public class LegalCaseContractInfoDto
    {
        public DateTime ContractDate { get; set; }
        public int? BranchId { get; set; }
        public string? BranchName { get; set; }
        public ProductDto? Product { get; set; }
        public ClientDto? Client { get; set; }
        public CarDto? Car { get; set; }
        public int? DelayDays { get; set; }
    }
}