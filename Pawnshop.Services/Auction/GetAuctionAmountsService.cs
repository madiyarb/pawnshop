using System.Threading.Tasks;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access.Interfaces;
using Pawnshop.Data.Models;
using Pawnshop.Data.Models.Auction.Dtos.Amounts;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Services.Auction.Interfaces;
using Pawnshop.Services.Contracts;

namespace Pawnshop.Services.Auction
{
    public class GetAuctionAmountsService : IGetAuctionAmountsService
    {
        private readonly ICalculationAuctionAmountsService _amountsService;
        private readonly IAuctionRepository _auctionRepository;
        private readonly IContractService _contractService;

        public GetAuctionAmountsService(ICalculationAuctionAmountsService amountsService,
            IAuctionRepository auctionRepository, IContractService contractService)
        {
            _amountsService = amountsService;
            _auctionRepository = auctionRepository;
            _contractService = contractService;
        }

        public async Task<AuctionAmountsCompositeViewModel> GetAmounts(int contractId)
        {
            var foundContract = await _contractService.GetOnlyContractAsync(contractId);
            if (foundContract is null)
            {
                throw new PawnshopApplicationException("Договор не нйден");
            }
            
            contractId = foundContract.ContractClass switch
            {
                ContractClass.Credit => foundContract.Id,
                ContractClass.Tranche => (int)foundContract.CreditLineId,
                ContractClass.CreditLine => foundContract.Id
            };
            
            CarAuction auction = await _auctionRepository.GetByContractIdAsync(contractId);

            if (auction is null)
            {
                throw new PawnshopApplicationException("Данные по аукциону не найдены!");
            }

            var getAmountsRequest = new AuctionAmountsRequest
            {
                ContractId = contractId,
                InputAmount = auction.Cost
            };

            return await _amountsService.GetCalculatedAmounts(getAmountsRequest);
        }
    }
}