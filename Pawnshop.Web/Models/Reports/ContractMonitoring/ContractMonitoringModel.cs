using System;
using System.Collections.Generic;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Web.Models.Reports.ContractMonitoring
{
    public class ContractMonitoringModel
    {
        public DateTime BeginDate { get; set; }

        public DateTime EndDate { get; set; }

        public CollateralType CollateralType { get; set; }

        public string BranchName { get; set; }

        public NumericCompare ProlongDayCount { get; set; }

        public ContractDisplayStatus? DisplayStatus { get; set; }

        public NumericCompare LoanCost { get; set; }

        public List<dynamic> List { get; set; }
    }
}