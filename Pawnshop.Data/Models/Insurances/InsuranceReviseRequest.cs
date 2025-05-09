using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Insurances
{
    public class InsuranceReviseRequest
    {
        public IFormFile file { get; set; }
        public int insuranceCompanyId { get; set; }
        public DateTime beginDate { get; set; }
        public DateTime endDate { get; set; }
    }
}
