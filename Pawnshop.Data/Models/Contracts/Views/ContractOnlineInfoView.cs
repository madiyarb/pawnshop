using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Pawnshop.Data.Models.Cars.Views;
using Pawnshop.Data.Models.LoanSettings;

namespace Pawnshop.Data.Models.Contracts.Views
{
    public sealed class ContractOnlineInfoView
    {
        public BaseContractInfoOnlineView BaseContractInfo { get; set; }
        public CreditLineLimitInfoView CreditLineLimitInfo { get; set; }
        public ContractExpenseOnlineInfo ContractExpenseOnlineInfo { get; set; }
        public IEnumerable<OnlineCarInfoView> CarInfo { get; set; }
        public InsurancePolicyView InsuranceInfo { get; set; }
        public IEnumerable<RealtyInfoOnlineVIew> RealtyInfo { get; set; }
        public decimal? NetLoanCost { get; set; }
        public decimal? LoanCost { get; set; }
        public ContractOnlineInfoView(BaseContractInfoOnlineView baseContractInfo,
            CreditLineLimitInfoView creditLineLimitInfo,
            ContractExpenseOnlineInfo contractExpenseOnlineInfo,
            OnlineCarInfoView carInfo,
            IEnumerable<RealtyInfoOnlineVIew> realtyInfo,
            InsurancePolicyView insuranceInfo)
        {
            BaseContractInfo = baseContractInfo;
            InsuranceInfo = insuranceInfo;
            if (BaseContractInfo.ContractClass == (int) ContractClass.CreditLine)
            {
                CreditLineLimitInfo = creditLineLimitInfo;
            }
            else
            {
                NetLoanCost = creditLineLimitInfo?.InitialCreditLineLimit - (InsuranceInfo?.InsurancePremium ?? (decimal)0.0);
                LoanCost = creditLineLimitInfo?.InitialCreditLineLimit;
            }

            if (contractExpenseOnlineInfo != null)
            {
                ContractExpenseOnlineInfo = contractExpenseOnlineInfo;
            }
            else
            {
                ContractExpenseOnlineInfo = new ContractExpenseOnlineInfo
                {
                    ContractId = baseContractInfo.Id,
                    HasAdditionalExpenses = false
                };
            }

            CarInfo = new List<OnlineCarInfoView> { carInfo }; 
            RealtyInfo = realtyInfo;
        }
    }
}
