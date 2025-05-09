using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.AccountingCore
{
    public class RemittanceSetting : CashOrders.RemittanceSetting, IEntity
    {
        public RemittanceSetting()
        {

        }

        public RemittanceSetting(CashOrders.RemittanceSetting model)
        {
            Id = model.Id;
            SendBranchId = model.SendBranchId;
            ReceiveBranchId = model.ReceiveBranchId;
            CashOutDebitId = model.CashOutDebitId;
            CashOutCreditId = model.CashOutCreditId;
            CashInDebitId = model.CashInDebitId;
            CashInCreditId = model.CashInCreditId;
            ExpenseTypeId = model.ExpenseTypeId;
            CashOutUserId = model.CashOutUserId;
            CashInUserId = model.CashInUserId;
            CashOutBusinessOperationId = model.CashOutBusinessOperationId;
            CashInBusinessOperationId = model.CashInBusinessOperationId;
        }
    }
}
