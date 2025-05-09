using Pawnshop.Data.Models.ApplicationOnlineInsurances;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Services.Models.Contracts;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Services.ApplicationsOnline
{
    public interface IApplicationOnlineService
    {
        Task ChangeDetailForInsurance(ApplicationOnline applicationOnline, int authorId, int? productId = null, decimal? newApplicationAmount = null, decimal? estimateAmount = null);
        Task CreateContract(ApplicationOnline applicationOnline, int? branchId = null);
        Task CreateInsurance(ApplicationOnline applicationOnline, decimal applicationAmount, LoanPercentSetting product, int authorId);
        void DeleteDraftContractEntities(int? contractId);
        Task DeleteInsurance(ApplicationOnline applicationOnline, ApplicationOnlineInsurance insurance, int authorId);
        Task<decimal> GetMaxApplicationAmount(Guid applicationOnlineId, ApplicationOnline applicationOnline = null);
        Task<LoanPercentSetting> GetProduct(int productId, bool? insurance = null);
        TrancheLimit GetAddtionalLimitForInsurance(int contractId);
        Task SendInsurance(int contractId);
        Task CheckEncumbranceRegisteredForCashIssue(int contractId);
    }
}
