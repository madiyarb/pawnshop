using Pawnshop.Data.Models.LoanSettings;

namespace Pawnshop.Services.Models.Insurance
{
    public class InsurancePremiumModel
    {
        public int InsuranceCompanyId { get; set; }
        public decimal LoanCost { get; set; }
        public int SettingId { get; set; }
        public LoanPercentSetting? loanPercentSetting { get; set; }
    }
}