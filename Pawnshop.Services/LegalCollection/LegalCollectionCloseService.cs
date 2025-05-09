using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access.LegalCollection;
using Pawnshop.Data.Models.LegalCollection.Dtos;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;
using Pawnshop.Services.LegalCollection.Inerfaces;

namespace Pawnshop.Services.LegalCollection
{
    public class LegalCollectionCloseService : ILegalCollectionCloseService
    {
        private readonly ILegalCaseContractsStatusRepository _legalCaseContractsRepository;
        private readonly ILegalCollectionRepository _legalCollectionsRepository;
        private readonly ILegalCaseHttpService _legalCaseHttpService;
        private readonly ISessionContext _sessionContext;
        private readonly ILogger<LegalCollectionCloseService> _logger;

        public LegalCollectionCloseService(
            ILegalCaseContractsStatusRepository legalCaseContractsRepository,
            ILegalCollectionRepository legalCollectionsRepository,
            ILegalCaseHttpService legalCaseHttpService,
            ISessionContext sessionContext,
            ILogger<LegalCollectionCloseService> logger)
        {
            _legalCaseContractsRepository = legalCaseContractsRepository;
            _legalCollectionsRepository = legalCollectionsRepository;
            _legalCaseHttpService = legalCaseHttpService;
            _sessionContext = sessionContext;
            _logger = logger;
        }


        public async Task CloseAsync(int contractId)
        {
            _logger.LogInformation("Попытка закрыть дело \"LegalCase\" с ContractId: {ContractId}", contractId);
            var legalCaseContract = await _legalCaseContractsRepository.GetActiveLegalCaseByContractIdAsync(contractId);
            if (legalCaseContract != null)
            {
                var contractInfo = await _legalCollectionsRepository.GetContractInfoCollectionStatusAsync(contractId);
                if (CanCloseLegalCase(contractInfo))
                {
                    try
                    {
                        var authorId = GetAuthorId();
                        var response = await _legalCaseHttpService
                            .CloseLegalCaseHttpRequest(legalCaseContract.LegalCaseId, authorId);
                        await _legalCaseContractsRepository.CloseAsync(legalCaseId: response);
                        _logger.LogInformation("Дело \"LegalCase\" с ContractId: {ContractId} закрыто", contractId);
                    }
                    catch (Exception e)
                    {
                        await _legalCaseHttpService.RollbackLegalCaseHttpRequest(legalCaseContract.LegalCaseId);
                        _logger.LogError(
                            "При попытке закрытия дела \"LegalCase\" с ContractId: {ContractId} возникла ошибка!. {Error}", 
                            contractId, e.Message);
                        throw new PawnshopApplicationException("Не удалось закрыть дело!", e.Message);
                    }
                }
            }
        }

        public void Close(int contractId)
        {
            _logger.LogInformation("Попытка закрыть дело \"LegalCase\" с ContractId: {ContractId}", contractId);
            var legalCaseContract = _legalCaseContractsRepository.GetActiveLegalCaseByContractId(contractId);
            if (legalCaseContract != null)
            {
                var contractInfo = _legalCollectionsRepository.GetContractInfoCollectionStatus(contractId);
                if (CanCloseLegalCase(contractInfo))
                {
                    try
                    {
                        var authorId = GetAuthorId();
                        var response = _legalCaseHttpService
                            .CloseLegalCaseHttpRequest(legalCaseContract.LegalCaseId, authorId).Result;
                        
                        _legalCaseContractsRepository.Close(legalCaseId: response);
                        _logger.LogInformation("Дело \"LegalCase\" с ContractId: {ContractId} закрыто", contractId);
                    }
                    catch (Exception e)
                    {
                        _legalCaseHttpService.RollbackLegalCaseHttpRequest(legalCaseContract.LegalCaseId);
                        _logger.LogError(
                            "При попытке закрытия дела \"LegalCase\" с ContractId: {ContractId} возникла ошибка!. {Error}", 
                            contractId, e.Message);
                        throw new PawnshopApplicationException("Не удалось закрыть дело", e.Message);
                    }
                }
            }
            else
            {
                _logger.LogWarning("При попытке закрыть дело с ContractId: {ContractId} не было найдено!", contractId);
            }
        }
        
        private int GetAuthorId()
        {
            if (!_sessionContext.IsInitialized)
            {
                return Constants.ADMINISTRATOR_IDENTITY;
            }

            return _sessionContext.UserId;
        }

        private bool CanCloseLegalCase(ContractInfoCollectionStatusDto contractInfo)
        {
            _logger.LogInformation("Проверка условий закрытия дела");
            
            var resultInfo = new StringBuilder($"Дней просрочки: {contractInfo?.DelayDays}, ");
            resultInfo.Append($"CollectionStatus: {contractInfo?.CollectionStatusCode}, ");
            resultInfo.Append($"Дело активно: {contractInfo?.IsActive}");
            
            var result =
                (contractInfo.DelayDays <= 0 || contractInfo.DelayDays == null) &&
                contractInfo.CollectionStatusCode == "NOT_INCOLLECTION" &&
                contractInfo.IsActive == false;
            
            if (!result)
            {
                _logger.LogInformation("Дело не может быть закрыто!. {Info}", resultInfo.ToString());
            }
            else
            {
                _logger.LogInformation("Дело может быть закрыто. {Info}", resultInfo.ToString());
            }

            return result;
        }
    }
}