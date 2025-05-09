using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.AccountingCore
{
    public class AccountRecord : Pawnshop.AccountingCore.Models.AccountRecord, IEntity
    {
        public AccountRecord()
        {

        }

        public AccountRecord(Pawnshop.AccountingCore.Models.AccountRecord model)
        {
            Id = model.Id;
            AuthorId = model.AuthorId;
            CreateDate = model.CreateDate;
            AccountId = model.AccountId;
            CorrAccountId = model.CorrAccountId;
            BusinessOperationSettingId = model.BusinessOperationSettingId;
            Date = model.Date;
            Amount = model.Amount;
            AmountNC = model.AmountNC;
            IncomingBalance = model.IncomingBalance;
            IncomingBalanceNC = model.IncomingBalanceNC;
            OutgoingBalance = model.OutgoingBalance;
            OutgoingBalanceNC = model.OutgoingBalanceNC;
            IsDebit = model.IsDebit;
            Reason = model.Reason;
            DeleteDate = model.DeleteDate;
            OrderId = model.OrderId;
        }
    }
}
