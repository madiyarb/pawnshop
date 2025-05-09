using Pawnshop.Services.Insurance.InsuranceCompanies.FridomFinance;
using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.OuterServiceSettings;
using Pawnshop.Data.Models.Insurances;

namespace Pawnshop.Services.Insurance.InsuranceCompanies
{
    public class InsuranceCompanyServiceFactory : IInsuranceCompanyServiceFactory
    {
        public IInsuranceCompanyService createInsuranceService(OuterServiceCompanyConfig config)
        {
            return config.InsuranceCompanyCode switch
            {
                InsuranceCompaniesCode.FFIN => new FridomFinanceService(config),
                _ => throw new ArgumentOutOfRangeException(nameof(config.InsuranceCompanyCode), config.InsuranceCompanyCode, @"Не определен сервис СК для {config.InsuranceCompanyCode}")
            };
        }
    }
}
