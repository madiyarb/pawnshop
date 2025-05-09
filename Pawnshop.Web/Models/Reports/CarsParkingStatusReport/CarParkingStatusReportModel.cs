using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Reports.CarsParkingStatusReport
{
    public class CarParkingStatusReportModel
    {
        public DateTime CurrentDate { get; set; }

        public string BranchName { get; set; }

        public List<dynamic> List { get; set; }

        public dynamic Group { get; set; }
    }
}
