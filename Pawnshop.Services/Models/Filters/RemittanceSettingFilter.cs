using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.Models.Filters
{
    public class RemittanceSettingFilter : IFilter
    {
        public int? SendBranchId { get; set; }
        public int? ReceiveBranchId { get; set; }
        public int? ExpenseTypeId { get; set; }
        public int? CashOutUserId { get; set; }
        public int? CashInUserId { get; set; }
        public int? CashOutBusinessOperationId { get; set; }
        public int? CashInBusinessOperationId { get; set; }
    }
}
