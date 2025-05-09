using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Reports.ClientExpensesReport
{
    public class ClientExpensesReportQueryModel
    {
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public int ExpenseType { get; set; }
        public List<int> BranchIds { get; set; }
    }
}
