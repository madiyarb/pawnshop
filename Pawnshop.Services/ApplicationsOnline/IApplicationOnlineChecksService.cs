using Pawnshop.Data.Models.ApplicationsOnline;
using System.Collections.Generic;
using System;

namespace Pawnshop.Services.ApplicationsOnline
{
    public interface IApplicationOnlineChecksService
    {
        void ApproveF2FChecks(Guid applicationOnlineId, int authorId);

        IList<ApplicationOnlineCheck> GetList(Guid applicationOnlineId, string applicationOnlineStatus);

        bool IsCheckedChecks(Guid applicationOnlineId, string applicationOnlineStatus);
    }
}
