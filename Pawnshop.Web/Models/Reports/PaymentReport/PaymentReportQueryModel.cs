using Pawnshop.AccountingCore.Models;
using System;
using System.Collections.Generic;

namespace Pawnshop.Web.Models.Reports.PaymentReport
{
    public class PaymentReportQueryModel
    {
        public DateTime CurrentDate { get; set; }

        public List<int> BranchIds { get; set; }

        public CollateralType? CollateralType { get; set; }
    }
}
