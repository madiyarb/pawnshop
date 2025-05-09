using System.Threading.Tasks;
using System.Threading;

namespace Pawnshop.Services.DebtorRegistry
{
    public interface IDebtorRegistryService
    {
        public Task<DebtorRegistryResponse> GetInfoFromDebtorRegistry(string iin, int clientId,
            CancellationToken cancellationToken);
    }
}
