using System;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Web.Models.Reports.ContractMonitoring
{
    public class ContractMonitoringQueryModel
    {
        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }

        public CollateralType CollateralType { get; set; }

        public int BranchId { get; set; }

        public NumericCompare ProlongDayCount { get; set; }

        public ContractDisplayStatus? DisplayStatus { get; set; }

        public NumericCompare LoanCost { get; set; }

        public bool IsTransferred { get; set; }
    }
}