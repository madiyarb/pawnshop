using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.AccountingCore.Abstractions
{
    public interface ITakeAwayToDelay
    {
        void TakeAwayToDelay(IContract contract, DateTime? date, DateTime? valueDate,int authorId, bool isMigration = false);
    }
}
