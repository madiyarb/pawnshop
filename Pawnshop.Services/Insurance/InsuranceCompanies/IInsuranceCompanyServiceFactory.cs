using Pawnshop.Services.Insurance.InsuranceCompanies;
using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.OuterServiceSettings;

namespace Pawnshop.Services.Insurance
{
    public interface IInsuranceCompanyServiceFactory
    {
        IInsuranceCompanyService createInsuranceService(OuterServiceCompanyConfig config);
    }
}
