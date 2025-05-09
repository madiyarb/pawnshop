using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Access.LegalCollection;
using Pawnshop.Data.Models.Collection;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.LegalCollection;
using Pawnshop.Data.Models.LegalCollection.GetFiltered.HttpService;
using Pawnshop.Data.Models.LegalCollection.HttpService;
using Pawnshop.Services.Collection.http;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;
using Pawnshop.Services.LegalCollection.Inerfaces;
using RestSharp.Extensions;

namespace Pawnshop.Services.LegalCollection
{
    public class LegalCollectionsFilteringService : ILegalCollectionsFilteringService
    {
        private readonly ILegalCollectionRepository _legalCollectionRepository;
        private readonly ILegalCaseHttpService _legalCaseHttpService;
        private readonly ICollectionHttpService<CollectionReason> _collectionService;
        private readonly GroupRepository _groupRepository;

        public LegalCollectionsFilteringService(
            ILegalCollectionRepository legalCollectionRepository,
            ILegalCaseHttpService legalCaseHttpService,
            ICollectionHttpService<CollectionReason> collectionService,
            GroupRepository groupRepository
        )
        {
            _legalCollectionRepository = legalCollectionRepository;
            _legalCaseHttpService = legalCaseHttpService;
            _collectionService = collectionService;
            _groupRepository = groupRepository;
        }

        public async Task<PagedResponse<LegalCasesViewModel>> GetFilteredAsync(LegalCasesQuery request)
        {
            request.Page = TransformPage(request);
            var filteredLegalCasesRequest = new FilteredHttpRequest
            {
                Page = request.Page.Offset,
                Size = request.Page.Limit
            };

            // если фильтры не переданы
            if (request.Model is null || HasNoFilters(request))
            {
                return await GetHttpFilteredResponse(filteredLegalCasesRequest);
            }

            if (HasOnlyLegalCollectionFilters(request))
            {
                filteredLegalCasesRequest.CourseId = request.Model.CourseId;
                filteredLegalCasesRequest.StageId = request.Model.StageId;
                filteredLegalCasesRequest.StatusId = request.Model.StatusId;
                filteredLegalCasesRequest.TaskStatusId = request.Model.TaskStatusId;

                filteredLegalCasesRequest.HasDebtProcessByContractIin = request.Model.HasDebtProcessByContractIin;
                return await GetHttpFilteredResponse(filteredLegalCasesRequest);
            }

            var countResult = await _legalCollectionRepository.GetContractsByFilterDataAsync(
                page: request.Page.Offset,
                size: request.Page.Limit,
                contractNumber: request.Model.ContractNumber,
                identityNumber: request.Model.IdentityNumber,
                fullName: request.Model.FullName,
                carNumber: request.Model.CarNumber,
                RKA: request.Model.RKA,
                parkingStatusId: request.Model.ParkingStatusId,
                regionId: request.Model.RegionId,
                branchId: request.Model.BranchId,
                collateralType: request.Model.CollateralType
            );

            filteredLegalCasesRequest.HasDebtProcessByContractIin = request.Model.HasDebtProcessByContractIin;
            return await GetFilteredResponse(request, countResult, filteredLegalCasesRequest);
        }
        
        
        private Page TransformPage(LegalCasesQuery request)
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
        
        private async Task<PagedResponse<LegalCasesViewModel>> GetHttpFilteredResponse(FilteredHttpRequest request,
            CountResult<Contract> countResult = null)
        {
            var response = await _legalCaseHttpService.GetFilteredLegalCase(request);
            var legalCasesViewModels = await ConvertToViewModels(response.List);

            var totalCount = countResult?.TotalCount > 0 ? countResult.TotalCount : response.Count;

            return new PagedResponse<LegalCasesViewModel>
            {
                Count = totalCount,
                List = legalCasesViewModels.ToArray()
            };
        }
        
        private async Task<List<LegalCasesViewModel>> ConvertToViewModels(IEnumerable<FilteredLegalCasesResponse> responses)
        {
            var legalCasesViewModels = new List<LegalCasesViewModel>();

            if (!responses.Any())
            {
                return legalCasesViewModels;
            }

            var reasons = await _collectionService.List();
            if (reasons is null)
            {
                throw new PawnshopApplicationException("Не удалось получить Reasons сервиса Collection");
            }

            foreach (var caseResponse in responses)
            {
                var reason = reasons.FirstOrDefault(r => r.Id == caseResponse.CaseReasonId);
                var viewModel = await ConvertToFilteredCasesResponse(caseResponse, reason?.reasonName);
                if (viewModel != null)
                {
                    legalCasesViewModels.Add(viewModel);
                }
            }

            return legalCasesViewModels;
        }
        
        private bool HasOnlyLegalCollectionFilters(LegalCasesQuery request)
        {
            return (request.Model.StatusId.HasValue
                    || request.Model.CourseId.HasValue
                    || request.Model.StageId.HasValue
                    || request.Model.TaskStatusId.HasValue
                    ||request.Model.HasDebtProcessByContractIin.HasValue)
                   && !request.Model.ContractNumber.HasValue()
                   && !request.Model.IdentityNumber.HasValue()
                   && !request.Model.FullName.HasValue()
                   && !request.Model.CarNumber.HasValue()
                   && !request.Model.RKA.HasValue()
                   && request.Model.BranchId == null
                   && request.Model.ParkingStatusId == null;
        }
        
        private async Task<PagedResponse<LegalCasesViewModel>> GetFilteredResponse(LegalCasesQuery request,
            CountResult<Contract> countResult, FilteredHttpRequest filteredHttpRequest)
        {
            // Если нет контрактов, возвращаем пустой результат
            if (!countResult.Data.Any())
            {
                return new PagedResponse<LegalCasesViewModel> { Count = 0, List = new List<LegalCasesViewModel>() };
            }

            filteredHttpRequest.ContractIds = countResult.Data.Select(c => c.Id).ToList();
            filteredHttpRequest.CourseId = request.Model.CourseId;
            filteredHttpRequest.StageId = request.Model.StageId;
            filteredHttpRequest.StatusId = request.Model.StatusId;

            return await GetHttpFilteredResponse(filteredHttpRequest, countResult);
        }
        
        private async Task<LegalCasesViewModel> ConvertToFilteredCasesResponse(FilteredLegalCasesResponse response,
            string? reason)
        {
            var contractInfo = await _legalCollectionRepository.GetContractInfoAsync(response.ContractId);
            if (contractInfo == null)
            {
                return null;
            }
            
            if (contractInfo.BranchId == 530)
            {
                var branch = await _groupRepository.GetOnLineGroupAsync(response.ContractId);
                contractInfo.BranchId = branch?.Id;
            }

            var legalCasesViewModel = new LegalCasesViewModel
            {
                Id = response.Id,
                ContractId = response.ContractId,
                LegalCaseNumber = response.LegalCaseNumber,
                CreateDate = response.CreateDate,
                StatusId = response.StatusId,
                CourseId = response.CourseId,
                StageId = response.StageId,
                ContractNumber = response.ContractNumber,
                Reason = reason,
                ContractData = contractInfo,
                DelayCurrentDay = contractInfo.DelayDays,
                DaysUntilExecution = response.DaysUntilExecution,
                CaseTaskStatusId = response.CaseTaskStatusId,
                HasRecoveryProcessByContractIin = response.HasDebtProcessByContractIin
            };

            return legalCasesViewModel;
        }
        
        private bool HasNoFilters(LegalCasesQuery request)
        {
            if (request.Model is null)
            {
                return true;
            }

            return !request.Model.ContractNumber.HasValue()
                   && !request.Model.IdentityNumber.HasValue()
                   && !request.Model.FullName.HasValue()
                   && !request.Model.CarNumber.HasValue()
                   && !request.Model.RKA.HasValue()
                   && request.Model.TaskStatusId is null
                   && request.Model.BranchId is null
                   && request.Model.StatusId is null
                   && request.Model.CourseId is null
                   && request.Model.StageId is null
                   && request.Model.ParkingStatusId is null
                   && request.Model.CollateralType is null
                   && request.Model.RegionId is null
                   && !request.Model.HasDebtProcessByContractIin.HasValue;
        }
    }
}