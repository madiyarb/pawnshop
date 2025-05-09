using Pawnshop.Data.Models.ApplicationsOnline.Kdn;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Membership;
using System.Collections.Generic;

namespace Pawnshop.Services.ApplicationsOnline
{
    public interface IApplicationOnlineKdnService
    {
        ApplicationOnlineKdnLog CalculateKdn(ApplicationOnline applicationOnline, List<ContractPaymentSchedule> virtualPaymentSchedule, User author);
        List<string> CheckCallCalcKdnToApprove(ApplicationOnline applicationOnline);
        List<string> CheckCallCalcKdnToVerification(ApplicationOnline applicationOnline);
    }
}
