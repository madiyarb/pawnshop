using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Insurances;
using System;

namespace Pawnshop.Services.Insurance
{
    public interface IInsurancePolicyService : IBaseService<InsurancePolicy>
    {
        void CancelNewPolicy(LogMetaData logMetaData, InsurancePoliceRequest insurancePoliceRequest);
        CancelInsuranceResponse CancelPolicy(LogMetaData logMetaData, InsurancePoliceRequest insurancePoliceRequest);
        InsurancePolicy GetInsurancePolicy(int policeRequestId, bool isCancel = false);
        InsurancePolicy GetInsurancePolicy(InsurancePoliceRequest insurancePoliceRequest);
        InsurancePolicy GetInsurancePolicyForAddition(Contract parentContract, decimal actionCost, DateTime actionDate, decimal? childContractCost = null, int? settingId = null);
        RegisterInsuranceResponse RegisterPolicy(LogMetaData insuranceMetaData, InsurancePoliceRequest insurancePoliceRequest);
    }
}