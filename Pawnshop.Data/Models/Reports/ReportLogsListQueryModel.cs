using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Reports
{
    public class ReportLogsListQueryModel
    {
        public DateTime? BeginDate { get; set; }

        public DateTime? EndDate { get; set; }

        public int? ClientId { get; set; }

        public int[] OwnerIds { get; set; }
    }
}
