using Pawnshop.Data.Models.Insurances;
using System;

namespace Pawnshop.Services.Insurance
{
    public interface IInsurancePremiumCalculator
    {
        void CheckInsurancePremium(decimal cost, InsurancePolicy policy, int settingId);
        decimal GetAdditionInsuranceAmount(int contractId, decimal cost, DateTime? date, int? closedParentId = null);
        InsuranceRequestData GetInsuranceData(decimal loanCost, int insuranceCompanyId, int settingId);
        InsuranceRequestData GetInsuranceDataV2(decimal cost, int insuranceCompanyId, int settingId, DateTime? additionDate = null, int? contractId = null, int? closedParentId = null);
        decimal GetLoanCostWithoutInsurancePremium(decimal loanCost);
        bool LoanCostCanUseInsurance(decimal loanCost);
    }
}