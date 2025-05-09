using Pawnshop.Data.Models.CardCashOutTransaction;
using System.Threading.Tasks;
using System.Threading;
using System;
using Pawnshop.Services.TasCore.Models.JetPay;

namespace Pawnshop.Services.TasCore
{
    public interface ITasCoreJetPayService
    {
        Task<TasCoreJetPayCardCashoutResponse> CreateCardCashoutAsync(TasCoreJetPayCardCashoutRequest request, CancellationToken cancellationToken);
        Task<CardCashOutTransactionStatus> GetStatusAsync(Guid paymentId, CancellationToken cancellationToken);
    }
}
