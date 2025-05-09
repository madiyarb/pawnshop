using System.Threading.Tasks;
using System.Threading;

namespace Pawnshop.Services.SUSN
{
    public interface ITasLabSUSNService
    {
        public Task GetSUSNStatus(string iin, int clientId, CancellationToken cancellationToken);
    }
}
