using System.Threading.Tasks;
using System.Threading;
using Pawnshop.Data.Models.ApplicationsOnline;

namespace Pawnshop.Services.PDF
{
    public interface IPdfService
    {
        Task<byte[]> GetFile(int contractId, int? creditLineId, ApplicationOnlineSignType signType, CancellationToken cancellationToken);
    }
}
