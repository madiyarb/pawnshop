using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access.LegalCollection;
using Pawnshop.Data.Models.LegalCollection.Create;
using Pawnshop.Services.LegalCollection.Inerfaces;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Collection;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Services.Collection.http;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;

namespace Pawnshop.Services.LegalCollection
{
    public class LegalCollectionCreateService : ILegalCollectionCreateService
    {
        private readonly ILegalCaseContractsStatusRepository _legalCaseContractsRepository;
        private readonly ILegalCollectionRepository _legalCollectionsRepository;
        private readonly ISessionContext _sessionContext;
        private readonly ILegalCaseHttpService _legalCaseHttpService;
        private readonly ICollectionHttpService<CollectionReason> _collectionReasonHttpService;

        public LegalCollectionCreateService(
            ILegalCaseContractsStatusRepository legalCaseContractsRepository,
            ILegalCollectionRepository legalCollectionsRepository,
            ISessionContext sessionContext,
            ILegalCaseHttpService legalCaseHttpService,
            ICollectionHttpService<CollectionReason> collectionReasonHttpService)
        {
            _legalCaseContractsRepository = legalCaseContractsRepository;
            _legalCollectionsRepository = legalCollectionsRepository;
            _sessionContext = sessionContext;
            _legalCaseHttpService = legalCaseHttpService;
            _collectionReasonHttpService = collectionReasonHttpService;
        }
        
        public int Create(CreateLegalCaseCommand request)
        {
            if (!UserHasRightsForCreateLegalCase())
            {
                throw new PawnshopApplicationException("Нет прав для управления Legal collection");
            }

            if (!request.CaseReasonId.HasValue)
            {
                throw new PawnshopApplicationException("Отсутствует Id причины передачи!");
            }
            
            var legalCase = _legalCaseContractsRepository.GetContractLegalCase(request.ContractId);
            if (legalCase != null)
            {
                return legalCase.LegalCaseId;
            }
        
            var contractInfo = _legalCollectionsRepository.GetContractInfoCollectionStatus(request.ContractId);
            if (contractInfo is null)
            {
                throw new PawnshopApplicationException(
                    $"Не удалось получить данные по контракту с Id: {request.ContractId}");
            }
        
            if (contractInfo.FincoreStatusId != (int)ContractDisplayStatus.LegalCollection && 
                contractInfo.FincoreStatusId != (int)ContractDisplayStatus.LegalHardCollection)
            {
                throw new PawnshopApplicationException(
                    $"Нельзя создать дело Legal в статусе: {contractInfo.CollectionStatusCode}");
            }

            if (request.Reason is null)
            {
                var reason = _collectionReasonHttpService.Get(request.CaseReasonId.ToString()).Result;
                if (reason is null)
                {
                    throw new PawnshopApplicationException(
                        $"Не удалось получить \"CollectionReason\" с Id: {request.CaseReasonId.ToString()}");
                }

                request.Reason = reason;
            }
        
            var countDelayDays = _legalCollectionsRepository.GetDelayDays(request.ContractId);
            contractInfo.DelayDays = countDelayDays;
            request.DelayCurrentDay = countDelayDays;
            request.BranchName ??= contractInfo.GroupDisplayName;
            request.ClientFullName ??= contractInfo.ClientFullName;
            request.ClientIIN ??= contractInfo.ClientIdentityNumber;
            
            var response = _legalCaseHttpService.CreateLegalCaseHttpRequest(request).Result;
            _legalCaseContractsRepository.Insert(request.ContractId, legalCaseId: response);

            return response;
        }
        
        public async Task<int> CreateAsync(CreateLegalCaseCommand request)
        {
            if (!UserHasRightsForCreateLegalCase())
            {
                throw new PawnshopApplicationException("Нет прав для управления Legal collection");
            }
            
            if (!request.CaseReasonId.HasValue)
            {
                throw new PawnshopApplicationException("Отсутствует Id причины передачи!");
            }
        
            var legalCase = await _legalCaseContractsRepository
                .GetContractLegalCaseAsync(request.ContractId);
        
            if (legalCase != null)
            {
                return legalCase.LegalCaseId;
            }
        
            var contractInfo = await _legalCollectionsRepository.GetContractInfoCollectionStatusAsync(request.ContractId);
            if (contractInfo is null)
            {
                throw new PawnshopApplicationException(
                    $"Не удалось получить данные по контракту с Id: {request.ContractId}");
            }
        
            if (contractInfo.FincoreStatusId != (int)ContractDisplayStatus.LegalCollection && 
                contractInfo.FincoreStatusId != (int)ContractDisplayStatus.LegalHardCollection)
            {
                throw new PawnshopApplicationException(
                    $"Нельзя создать дело Legal в статусе: {contractInfo.CollectionStatusCode}");
            }
            
            if (request.Reason is null)
            {
                var reason = await _collectionReasonHttpService.Get(request.CaseReasonId.ToString());
                if (reason is null)
                {
                    throw new PawnshopApplicationException(
                        $"Не удалось получить \"CollectionReason\" с Id: {request.CaseReasonId.ToString()}");
                }

                request.Reason = reason;
            }
            
            var countDelayDays = await _legalCollectionsRepository.GetDelayDaysAsync(request.ContractId);
            contractInfo.DelayDays = countDelayDays;
            request.DelayCurrentDay = countDelayDays;
            request.BranchName ??= contractInfo.GroupDisplayName;
            request.ClientFullName ??= contractInfo.ClientFullName;
            request.ClientIIN ??= contractInfo.ClientIdentityNumber;
        
            var response = await _legalCaseHttpService.CreateLegalCaseHttpRequest(request);
            await _legalCaseContractsRepository.InsertAsync(request.ContractId, legalCaseId: response);

            return response;
        }
        
        private bool UserHasRightsForCreateLegalCase()
        {
            if (!_sessionContext.IsInitialized)
            {
                return true;
            }
                
            return _sessionContext.UserId == Constants.ADMINISTRATOR_IDENTITY || 
                   _sessionContext.Permissions.Contains(Permissions.LegalCollectionManage);
        }
    }
}