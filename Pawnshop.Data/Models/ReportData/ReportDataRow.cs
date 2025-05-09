using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.ReportData
{
    public class ReportDataRow : IEntity
    {
        public int Id { get; set; }
        public int ReportDataId { get; set; }
        public ReportDataKey Key { get; set; }
        public decimal Value { get; set; }
    }
}
