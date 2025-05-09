using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Data.Models.Migration
{
    public class MigrationOperationSetting : IEntity
    {
        public int Id { get; set; }
        public ContractActionType ActionType { get; set; }
        public AmountType PaymentType { get; set; }
        public int? BusinessOperationId { get; set; }
        public int? BusinessOperationSettingId { get; set; }
        public int ContractTypeId { get; set; }
        public int ContractPeriodId { get; set; }
        public int DebitAccountId { get; set; }
        public int CreditAccountId { get; set; }
        public int? PayTypeId { get; set; }
    }
}
