using System.Threading.Tasks;
using System.Threading;

namespace Pawnshop.Services.TasLabBankrupt
{
    public interface ITasLabBankruptInfoService
    {
        public Task<bool> IsClientBankruptFromDatabase(string iin, CancellationToken cancellationToken);
        public Task<bool> IsClientBankruptOnline(string iin, CancellationToken cancellationToken);
    }
}
