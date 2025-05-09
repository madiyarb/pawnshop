using System;

namespace Pawnshop.Services.Models.Filters
{
    public class InsurancePolicyFilter
    {
        public int? RootContractId { get; set; }
        public DateTime? StartDate { get; set; }
        public int? ContractId { get; set; }
    }
}