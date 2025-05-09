using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.ClientDeferments
{
    public class ClientDefermentsListQueryModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? DefermentTypeId { get; set; }
    }
}
