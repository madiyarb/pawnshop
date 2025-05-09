using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Restructuring
{
    public class CalculateAPRModel
    {
        public List<AprScheduleModel> ScheduleData { get; set; }
        public double LoanCost { get; set; }
    }
}
