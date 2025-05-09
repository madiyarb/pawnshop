using Pawnshop.Data.Models.Insurances;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Insurance
{
    public interface IInsuranceService
    {
        Task BPMRegisterPolicy(InsurancePoliceRequest policeRequest);
        void BPMAcceptPolicy(InsurancePoliceRequest policeRequest, int companyCode);
        InsurancePolicy BPMKillPolicy(InsurancePoliceRequest policeRequest);
        Task BPMCancelPolicy(InsurancePoliceRequest policeRequest);
        void BPMAcceptCancelPolicy(InsurancePoliceRequest policeRequest);
        void BPMKillCancelPolicy(InsurancePoliceRequest policeRequest);
    }
}
