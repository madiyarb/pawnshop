using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.ContractPaymentSchedules
{
    public sealed class CreditLinePaymentScheduleOnlineView : ContractPaymentScheduleOnlineView
    {
        public IEnumerable<ContractPaymentScheduleOnlineView> TranchesPaymentSchedules { get; set; }
    }
}
