using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Data.Models.Migration
{
    public class MigrationOperationSettingFilter
    {
        public ContractActionType? ActionType { get; set; }
        public AmountType? PaymentType { get; set; }
        public int? ContractTypeId { get; set; }
        public int? ContractPeriodId { get; set; }
        public int? DebitAccountId { get; set; }
        public int? CreditAccountId { get; set; }
        public int? PayTypeId { get; set; }
    }
}
