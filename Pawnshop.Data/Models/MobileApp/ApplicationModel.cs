using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp
{
    public class ApplicationModel
    {
        public Application Application { get; set; }
        public ApplicationDetails ApplicationDetails { get; set; }
        public decimal ApplicationAdditionalLimit { get; set; }
        public DateTime? CreditLineMaturityDate { get; set; }
    }
}
