using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Services.Models.Filters
{
    public class BusinessOperationSettingFilter
    {
        public int? BusinessOperationId { get; set; }
        public string Code { get; set; }
        public bool? IsActive { get; set; }
        public AmountType? AmountType { get; set; }
        public int? PayTypeId { get; set; }
    }
}
