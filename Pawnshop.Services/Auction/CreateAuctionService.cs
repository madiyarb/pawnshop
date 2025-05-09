using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Extensions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Access.Interfaces;
using Pawnshop.Data.Models;
using Pawnshop.Data.Models.Auction.HttpRequestModels;
using Pawnshop.Data.Models.Auction.HttpResponseModel;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.LegalCollection.Details;
using Pawnshop.Services.Auction.HttpServices.Interfaces;
using Pawnshop.Services.Auction.Interfaces;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;
using Pawnshop.Services.LegalCollection.Inerfaces;

namespace Pawnshop.Services.Auction
{
    public class CreateAuctionService : ICreateAuctionService
    {
        private readonly ILegalCollectionUpdateService _legalCollectionUpdateService;
        private readonly ICarAuctionHttpService _auctionHttpService;
        private readonly ICarAuctionService _carAuctionService;
        private readonly ISessionContext _sessionContext;
        private readonly ContractRepository _contractRepository;
        private readonly IAuctionRepository _auctionRepository;
        private readonly ILegalCaseHttpService _legalCaseHttpService;
        
        private int? _updatedLegalCaseId;
        private int? _createdAuctionId;

        public CreateAuctionService(
            ILegalCollectionUpdateService legalCollectionUpdateService,
            ICarAuctionHttpService auctionHttpService,
            ICarAuctionService carAuctionService,
            ISessionContext sessionContext,
            ContractRepository contractRepository,
            IAuctionRepository auctionRepository,
            ILegalCaseHttpService legalCaseHttpService)
        {
            _legalCollectionUpdateService = legalCollectionUpdateService;
            _auctionHttpService = auctionHttpService;
            _carAuctionService = carAuctionService;
            _sessionContext = sessionContext;
            _contractRepository = contractRepository;
            _auctionRepository = auctionRepository;
            _legalCaseHttpService = legalCaseHttpService;
        }

        public async Task<List<LegalCasesDetailsViewModel>> Create(CreateAuctionCommand command)
        {
            ValidateRequest(command);
            var contractId = await GetContractId(command);

            var auction = await _auctionRepository.GetByContractIdAsync((int)contractId);
            if (auction != null)
            {
                throw new PawnshopApplicationException("Аукцион уже создан!");
            }

            var legalCaseDetails = await UpdateLegalCase(command);
            var newAuction = await CreateAuction(command, (int)contractId);

            if (newAuction != null)
            {
                await CreateCarAuction(command, newAuction);
            }

            return legalCaseDetails;
        }

        private void ValidateRequest(CreateAuctionCommand command)
        {
            if (command.Auction.AuctionCost <= Constants.AUCTION_MIN_BYOUT_SUM || command.Auction.WithdrawCost <= Constants.AUCTION_MIN_BYOUT_SUM)
            {
                throw new PawnshopApplicationException($"Сумма ДКП должна быть больше {Constants.AUCTION_MIN_BYOUT_SUM}");
            }
        }

        private async Task<int?> GetContractId(CreateAuctionCommand command)
        {
            var contract = await _contractRepository.GetOnlyContractAsync(command.Car.Contract.ExternalContractId);
            if (contract is null)
            {
                throw new PawnshopApplicationException("Договор не найден!");
            }

            var contractId = contract.ContractClass switch
            {
                ContractClass.Credit => contract.Id,
                ContractClass.Tranche => contract.CreditLineId,
                ContractClass.CreditLine => contract.Id
            };
            
            return contractId;
        }
        
        /// <summary>
        /// Создание аукциона во внешнем сервисе
        /// </summary>
        /// <exception cref="PawnshopApplicationException">При получении Exception будет вызвано обратное действие</exception>
        private async Task<CreatedAuctionDto> CreateAuction(CreateAuctionCommand command, int contractId)
        {
            command.Car.Contract.ExternalContractId = contractId;
            try
            {
                var auction = await _auctionHttpService.CreateCarAuction(new CreateCarAuctionRequest
                {
                    AuthorId = _sessionContext.UserId.ToString(),
                    AuthorName = _sessionContext.UserName,
                    Car = command.Car,
                    Auction = new AuctionRequest
                    {
                        AuctionNumber = command.Auction.AuctionNumber,
                        AuctionContractNumber = command.Auction.AuctionContractNumber,
                        AuctionContractDate = command.Auction.AuctionContractDate,
                        AuctionDate = command.Auction.AuctionDate,
                        AuctionCost = command.Auction.AuctionCost,
                        Client = command.Auction.Client,
                        Note = command.Auction.Note
                    }
                });
                
                _createdAuctionId = auction?.AuctionId;
                return auction;
            }
            catch (Exception e)
            {
                await RollbackUpdatedLegalCase();
                throw new PawnshopApplicationException("При попытке создания аукциона произошла ошибка", e.Message);
            }
        }
        
        /// <summary>
        /// Обновление дела LC
        /// </summary>
        /// <exception cref="PawnshopApplicationException">При получении Exception будет вызвано обратное действие</exception>
        private async Task<List<LegalCasesDetailsViewModel>> UpdateLegalCase(CreateAuctionCommand command)
        {
            command.UpdateLegalCaseCommand.CaseCourtId = null;
            try
            {
                var legalCaseDetails = await _legalCollectionUpdateService.UpdateLegalCase(command.UpdateLegalCaseCommand);
                if (legalCaseDetails.IsNullOrEmpty())
                {
                    await RollbackCreatedAuction();
                }
                
                _updatedLegalCaseId = legalCaseDetails.First().Id;
                return legalCaseDetails;
            }
            catch (Exception e)
            {
                await RollbackCreatedAuction();
                throw new PawnshopApplicationException("При попытке обновить дело LC произошла ошибка!", e.Message);
            }
        }

        /// <summary>
        /// Создание записи в таблице CarAuction
        /// </summary>
        private async Task CreateCarAuction(CreateAuctionCommand command, CreatedAuctionDto auction)
        {
            try
            {
                await _carAuctionService.CreateAsync(new CarAuction
                {
                    ContractId = command.Car.Contract.ExternalContractId,
                    AuctionId = auction.AuctionId,
                    Cost = command.Auction.AuctionCost,
                    OrderRequestId = auction.OrderRequestExternalId,
                    WithdrawCost = command.Auction.WithdrawCost
                });
            }
            catch (Exception e)
            {
                await RollbackCreatedAuction();
                await RollbackUpdatedLegalCase();
                throw new PawnshopApplicationException("Не удалось создать аукцион!", e.Message);
            }
        }
        
        private async Task RollbackCreatedAuction()
        {
            if (_createdAuctionId.HasValue)
            {
                await _auctionHttpService.RollbackCreated((int)_createdAuctionId);
            }
        }
        
        private async Task RollbackUpdatedLegalCase()
        {
            if (_updatedLegalCaseId.HasValue)
            {
                await _legalCaseHttpService.RollbackLegalCaseHttpRequest((int)_updatedLegalCaseId);
            }
        }
    }
}