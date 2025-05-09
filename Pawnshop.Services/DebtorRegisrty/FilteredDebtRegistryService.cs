using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Access.LegalCollection;
using Pawnshop.Data.Models.DebtorRegistry;
using Pawnshop.Data.Models.LegalCollection;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Services.DebtorRegisrty.Dtos;
using Pawnshop.Services.DebtorRegisrty.HttpService;
using Pawnshop.Services.DebtorRegisrty.Interfaces;

namespace Pawnshop.Services.DebtorRegisrty
{
    public class FilteredDebtRegistryService : IFilteredDebtRegistryService
    {
        private readonly IDebtorRegisterHttpService _debtorRegisterHttpService;
        private readonly ClientRepository _clientRepository;
        private readonly ILegalCollectionRepository _legalCollectionRepository;

        public FilteredDebtRegistryService(
            IDebtorRegisterHttpService debtorRegisterHttpService,
            ClientRepository clientRepository,
            ILegalCollectionRepository legalCollectionRepository
            )
        {
            _debtorRegisterHttpService = debtorRegisterHttpService;
            _clientRepository = clientRepository;
            _legalCollectionRepository = legalCollectionRepository;
        }
        
        public async Task<PagedResponse<DebtRegistriesViewModel>> GetFilteringAsync(DebtorRegistriesQuery request)
        {
            request.Page = TransformPage(request);
            var iins = new List<string>();

            if (request.Model?.IdentityNumber != null)
            {
                iins.Add(request.Model.IdentityNumber);
            }

            if (request.Model?.ClientFullName != null)
            {
                var clients = await _legalCollectionRepository.GetClientByFullNameAsync(request.Model.ClientFullName);
                if (clients != null && clients.Any())
                {
                    iins.AddRange(clients.Select(c => c.IdentityNumber));
                }
            }
            
            var httpRequest = new FilteredDebtorRegisterHttpRequest
            {
                Page = request.Page.Offset,
                Size = request.Page.Limit,
                    
                CourtOfficerId = request.Model?.CourtOfficerId,
                Iins = iins.Any() ? iins : null,
                CourtOfficerRegion = request.Model?.CourtOfficerRegion,
                IsTravelBan = request.Model?.IsTravelBan
            };
            
            PagedResponse<DebtorDetailsDto> debtRegistryResponse = await _debtorRegisterHttpService.GetByFilters(httpRequest);
            if (debtRegistryResponse.List == null && !debtRegistryResponse.List.Any())
            {
                return new PagedResponse<DebtRegistriesViewModel>();
            }

            var viewModels = new List<DebtRegistriesViewModel>();

            foreach (var r in debtRegistryResponse.List)
            {
                viewModels.Add(new DebtRegistriesViewModel
                {
                    Id = r.Id,
                    ProtocolNumb = r.CourtExecProtocolNum,
                    IsTravelBan = r.IsTravelBan,
                    InitiationDate = r.InitiationDate,
                    CreateDate = r.CreateDate,
                    StartExecutionDate = r.StartExecutionDate,
                    TravelBanStartDate = r.TravelBanStartDate,
                    Client = await GetClient(r.IdentityNumber),
                    CourtOfficer = r.CourtOfficer,
                    RecoveryAmount = r.RecoveryAmount
                });
            }

            return new PagedResponse<DebtRegistriesViewModel>
            {
                Count = debtRegistryResponse != null ? debtRegistryResponse.Count : 0,
                List = viewModels
            };
        }

        private async Task<ClientDto> GetClient(string iin)
        {
            var clientInDb = await _clientRepository.GetByIdentityNumberAsync(iin);

            if (clientInDb is null)
            {
                return new ClientDto();
            }

            return new ClientDto
            {
                Id = clientInDb.Id,
                IdentityNumber = clientInDb.IdentityNumber,
                Name = clientInDb.Name,
                Surname = clientInDb.Surname,
                Patronymic = clientInDb.Patronymic,
                FullName = clientInDb.FullName
            };
        }
        
        private Page TransformPage(DebtorRegistriesQuery request)
        {
            if (request.Page is null)
            {
                return new Page
                {
                    Offset = 0,
                    Limit = 20
                };
            }
            
            return new Page
            {
                Offset = request.Page.Offset / request.Page.Limit,
                Limit = request.Page.Limit
            };
        }
    }
}
