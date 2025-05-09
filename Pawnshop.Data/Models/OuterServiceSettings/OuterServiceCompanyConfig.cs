using Pawnshop.Data.Models.Insurances;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.OuterServiceSettings
{
    public class OuterServiceCompanyConfig
    {
        public string BasicAuth { get; set; }
        public string BasicPass { get; set; }
        public string BodyAuth { get; set; }
        public string BodyPass { get; set; }
        public string Url { get; set; }
        public string ControllerUrl { get; set; }
        public InsuranceCompaniesCode InsuranceCompanyCode { get; set; }
    }
}
