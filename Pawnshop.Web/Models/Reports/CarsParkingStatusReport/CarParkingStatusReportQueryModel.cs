using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Reports.CarsParkingStatusReport
{
    public class CarParkingStatusReportQueryModel
    {
        public DateTime BeginDate { get; set; }

        public List<int> BranchIds { get; set; }
    }
}
