using Pawnshop.Data.Models.Insurances;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Insurance.InsuranceCompanies
{
    public interface IInsuranceCompanyService : IDisposable
    {
        RegisterInsuranceResponse RegisterPolicy(InsurancePoliceRequest insurancePoliceRequest);
        CancelInsuranceResponse CancelPolicy(InsurancePoliceRequest insurancePoliceRequest);
    }
}
