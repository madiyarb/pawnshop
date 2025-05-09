using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.LoanSettings;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pawnshop.Services.LoanPercent
{
    public interface ILoanPercentService
    {
        LoanPercentSetting Get(int id);

        Task<List<LoanPercentSetting>> GetChild(int id);

        Task<List<LoanPercentFromMobile>> GetListFromMobile(string productTypeCode);

        List<LoanPercentOnlineView> GetListForOnline(ContractClass contractClass);
    }
}
