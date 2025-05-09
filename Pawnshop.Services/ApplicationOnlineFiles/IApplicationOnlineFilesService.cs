using System;
using System.Threading;
using Pawnshop.Data.Models.ApplicationsOnline;

namespace Pawnshop.Services.ApplicationOnlineFiles
{
    public interface IApplicationOnlineFilesService
    {
        bool CreateLoanContractFile(ApplicationOnline applicationOnline, out string error, Func<string> originalUrlBuilder,
            Guid fileId, string language = null, CancellationToken cancellationToken = default, bool sendToNpck = false);
    }
}
