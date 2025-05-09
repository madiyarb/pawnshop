using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.Models.Filters
{
    public class AccrualBaseFilter : IFilter
    {
        
        public bool? IsActive{ get; set; }
        public AccrualType? AccrualType  { get; set; }
        public AmountType? AmountType { get; set; }
        public int? BaseSettingId { get; set; }
        public ContractClass? ContractClass { get; set; }
    }
}
