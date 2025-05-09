using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Services.Models.Filters;

namespace Pawnshop.Services.AccountingCore
{
    public interface IAccountRecordService : IDictionaryWithSearchService<AccountRecord, AccountRecordFilter>
    {
        List<AccountRecord> Build(CashOrder order, bool isMigration = false);
        void RecalculateBalanceWithNewRecord(IAccount account, AccountRecord newRecord, bool isMigration = false);
        void RecalculateBalanceOnAccount(int accountId, bool isMigration = false, DateTime? beginDate = null);
    }
}
