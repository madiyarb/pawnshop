using System;
using System.Threading;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Auction.HttpRequestModels;

namespace Pawnshop.Services.Auction.HttpServices.Interfaces
{
    public interface IAuctionOperationHttpService
    {
        Task<bool> Approve(ApproveAuctionCommand command);
        Task<bool> Approve(ApproveAuctionCommand command, CancellationToken cancellationToken);
        Task<bool> Reject(Guid requestId);
    }
}