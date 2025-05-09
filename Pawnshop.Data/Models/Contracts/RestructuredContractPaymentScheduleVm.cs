using System;
using System.Collections.Generic;

namespace Pawnshop.Data.Models.Contracts
{
    public class RestructuredContractPaymentScheduleVm
    {
        public List<RestructuredContractPaymentSchedule> RestructuredContractPaymentScheduleList { get; set; }
        public decimal Apr { get; set; }
    }
}
