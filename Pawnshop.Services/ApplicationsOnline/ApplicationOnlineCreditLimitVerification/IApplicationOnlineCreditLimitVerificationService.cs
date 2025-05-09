using Pawnshop.Data.Models.ApplicationsOnline;
using System.Threading.Tasks;

namespace Pawnshop.Services.ApplicationsOnline.ApplicationOnlineCreditLimitVerification
{
    public interface  IApplicationOnlineCreditLimitVerificationService
    {
        public Task<ApplicationOnlineCreditLimitVerificationResult> Check(ApplicationOnline application);
        public Task ValidateSumsAndCreateCheck(ApplicationOnline application);
    }
}
