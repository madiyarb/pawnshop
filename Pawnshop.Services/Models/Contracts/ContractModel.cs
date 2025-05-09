using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Insurances;
using Pawnshop.Data.Models.MobileApp;
using Pawnshop.Services.Models.Contracts.Kdn;
using System.Collections.Generic;

namespace Pawnshop.Services.Models.Contracts
{
    public class ContractModel
    {
        public Contract Contract { get; set; }
        public List<InsurancePoliceRequest> PoliceRequests { get; set; }
        public ApplicationDetails ApplicationDetails { get; set; }
        public ContractKdnModel ContractKdnModel { get; set; }
        public string PayTypeOperationCode { get; set; }
        public decimal ApplicationAdditionalLimit { get; set; }
        public TrancheLimit TrancheLimit { get; set; }
        public bool IsInsuranceAdditionalLimitOn { get; set; }
        public bool IsLiquidityOn { get; set; }
    }
}