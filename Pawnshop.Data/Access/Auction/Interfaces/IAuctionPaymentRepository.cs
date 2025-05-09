using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Auction;

namespace Pawnshop.Data.Access.Auction.Interfaces
{
    public interface IAuctionPaymentRepository
    {
        IDbTransaction BeginTransaction();
        Task InsertAsync(AuctionPayment auctionPayment);
        Task InsertMultipleAsync(List<AuctionPayment> auctionPayments);
        Task<IEnumerable<AuctionPayment>> GetByRequestIdAsync(Guid requestId);
        Task<IEnumerable<AuctionPayment>> GetByCashOrderIdAsync(int cashOrderId);
    }
}