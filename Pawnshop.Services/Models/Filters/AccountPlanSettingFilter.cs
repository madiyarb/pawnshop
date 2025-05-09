using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.Models.Filters
{
    public class AccountPlanSettingFilter : IFilter
    {
        public int? AccountPlanId { get; set; }
        public int? AccountSettingId { get; set; }
        public int? ContractTypeId { get; set; }
        public int? PeriodTypeId { get; set; }
        public int? AccountId { get; set; }
        public int? OrganizationId { get; set; }
        public int? BranchId { get; set; }
    }
}
