using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;
using Pawnshop.Data.Models.Audit;

namespace Pawnshop.Services.Models.Filters
{
    public class EventLogFilter
    {
        public int? BranchId { get; set; }

        public EventCode? EventCode { get; set; }

        public DateTime? BeginDate { get; set; }

        public DateTime? EndDate { get; set; }

        public EntityType? EntityType { get; set; }

        public int? EntityId { get; set; }
    }
}
