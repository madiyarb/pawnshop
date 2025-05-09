using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Services.Models.Insurance;
using Pawnshop.Services.Models.List;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Insurance
{
    public interface IInsuranceCompanySettingService
    {
        LoanPercentSettingInsuranceCompany Save(LoanPercentSettingInsuranceCompany model, int userId);
        ListModel<LoanPercentSettingInsuranceCompany> List(ListQueryModel<InsuranceSettingQueryModel> listQuery);
        LoanPercentSettingInsuranceCompany Card(int id);
        List<LoanPercentSettingInsuranceCompany> InsuranceCompaniesList();
    }
}
