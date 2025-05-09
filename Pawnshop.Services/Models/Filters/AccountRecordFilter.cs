using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.Filters
{
    public class AccountRecordFilter
    {
        public int? AccountId { get; set; }
        public int? CorrAccountId { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? BusinessOperationSettingId { get; set; }
        public int? BusinessOperationId { get; set; }
        public bool? IsDebit { get; set; }
        public int? OrderId { get; set; }
        public int? AmountMax { get; set; }
        public int? AmountMin { get; set; }
        public int? IncomingBalanceMax { get; set; }
        public int? IncomingBalanceMin { get; set; }
        public int? OutgoingBalanceMax { get; set; }
        public int? OutgoingBalanceMin { get; set; }
        public bool? OnlyFirst { get; set; }
    }
}
