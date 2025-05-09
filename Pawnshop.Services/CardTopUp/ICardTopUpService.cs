using System.Threading;
using System.Threading.Tasks;
using Pawnshop.Services.CardTopUp.StartTransaction;

namespace Pawnshop.Services.CardTopUp
{
    public interface ICardTopUpService
    {
        public Task<Envelope> StartTransaction(string referenceNr, string amount, int orderId,
            CancellationToken cancellationToken);

        public Task<GetTransactionStatusCode.Envelope?> GetTransactionStatusCode(string referenceNr,
            CancellationToken cancellationToken);

        public Task<CompleteTransaction.Envelope> CompleteTransaction(string referenceNr,
            CancellationToken cancellationToken);
    }
}
