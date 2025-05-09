using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.AccountingCore.Abstractions
{
    public interface IPenaltyAccrual
    {
        void Execute(IContract contract, DateTime accrualDate, int authorId, IPaymentScheduleItem? lastScheduleItem = null,
            DateTime? startDate = null);
    }
}