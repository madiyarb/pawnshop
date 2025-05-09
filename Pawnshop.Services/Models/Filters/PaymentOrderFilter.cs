using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.Models.Filters
{
    public class PaymentOrderFilter : IFilter
    {
        
        public bool? IsActive{ get; set; }
        public bool? NotOnScheduleDateAllowed { get; set; }
    }
}
