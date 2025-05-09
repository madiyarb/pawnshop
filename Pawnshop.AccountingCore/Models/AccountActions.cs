using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Abstractions;

namespace Pawnshop.AccountingCore.Models
{
    public partial class Account : IAccountActions
    {
        public void Close()
        {
            //TODO:просто пример, переделать в боевую логику
            CloseDate = DateTime.Now;
        }

        public Account(int authorId)
        {
            AuthorId = authorId;
            CreateDate = OpenDate = (DateTime)(LastMoveDate = DateTime.Now);
            IsOutmoded = false;
            Balance = BalanceNC = 0;
        }

        public AccountRecord MoveToOtherAccount(IAccount account)
        {
            //TODO:просто пример, переделать в боевую логику
            return new AccountRecord();
        }
    }
}
