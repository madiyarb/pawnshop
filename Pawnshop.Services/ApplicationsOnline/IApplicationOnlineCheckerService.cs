using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using Pawnshop.Data.Models.ApplicationsOnline;

namespace Pawnshop.Services.ApplicationsOnline
{
    public interface IApplicationOnlineCheckerService
    {
        public List<string> ReadyForVerification(Guid applicationOnlineId);
        public List<string> ReadyForApprove(Guid applicationOnlineId);
        public List<string> ReadyForSign(Guid applicationOnlineId);
        public bool AgePermittedForInsurance(Guid applicationOnlineId);
        public Task<CheckClientCoborrowerResult> CheckClientCoborrowerAccountAmountLimitAsync(ApplicationOnline application);
    }
}