using Pawnshop.Data.Models.Clients;
using System.Threading.Tasks;
using System.Threading;

namespace Pawnshop.Services.ClientExternalValidation
{
    public interface IClientExternalValidationService
    {
        public Task<bool> CheckClientForExternalIntegration(Client client, CancellationToken cancellationToken);
    }
}
