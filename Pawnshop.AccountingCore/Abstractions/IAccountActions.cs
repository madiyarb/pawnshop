using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.AccountingCore.Abstractions
{
    public interface IAccountActions
    {
        AccountRecord MoveToOtherAccount(IAccount account);
        void Close();
    }
}
