using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.Models.Filters
{
    public class AccountFilter : IFilter
    {
        public int? ContractId { get; set; }
        public int? AccountSettingId { get; set; }
        public bool? IsOpen { get; set; }
        public bool? IsOutmoded { get; set; }
        public bool? IsConsolidated { get; set; }
        public int? AccountPlanId { get; set; }
        public string[] SettingCodes { get; set; }
        public int? BranchId { get; set; }
    }
}
