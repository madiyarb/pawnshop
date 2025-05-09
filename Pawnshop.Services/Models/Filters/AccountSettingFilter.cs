using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.Models.Filters
{
    public class AccountSettingFilter : IFilter
    {
        public int? TypeId { get; set; }
        public bool? IsConsolidated { get; set; }
        public bool? WithAllParents { get; set; }
        public string Code { get; set; }
    }
}
