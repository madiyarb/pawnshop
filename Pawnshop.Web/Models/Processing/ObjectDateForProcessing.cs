using Pawnshop.Data.Models.Contracts.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Processing
{
    public class ObjectDateForProcessing
    {
        public DateTime? Date { get; set; }

        public ProcessingType ProcessingType { get; set; }
    }
}
