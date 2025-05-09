using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Services.Models.Filters
{
    public class CashOrderFilter
    {
        public OrderType? OrderType { get; set; }

        public DateTime? BeginDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? ClientId { get; set; }

        public int? ContractId { get; set; }

        public int? UserId { get; set; }

        public int? AccountId { get; set; }

        public int? OrderNumber { get; set; }

        public bool? IsDelete { get; set; }

        public int? OwnerId { get; set; }

        public OrderStatus? ApproveStatus { get; set; }

        public int? BusinessOperationSettingId { get; set; }

        public ContractActionType? ActionType { get; set; }
        public int? AccountPlanId { get; set; }
    }
}
