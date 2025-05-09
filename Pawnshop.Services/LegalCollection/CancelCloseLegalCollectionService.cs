using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access.LegalCollection;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;
using Pawnshop.Services.LegalCollection.Inerfaces;

namespace Pawnshop.Services.LegalCollection
{
    public class CancelCloseLegalCollectionService : ICancelCloseLegalCollectionService
    {
        private readonly ILegalCaseContractsStatusRepository _legalCaseContractsRepository;
        private readonly ILegalCaseHttpService _legalCaseHttpService;
        private readonly ILogger<CancelCloseLegalCollectionService> _logger;

        public CancelCloseLegalCollectionService(
            ILegalCaseContractsStatusRepository legalCaseContractsRepository,
            ILegalCaseHttpService legalCaseHttpService,
            ILogger<CancelCloseLegalCollectionService> logger
            )
        {
            _legalCaseContractsRepository = legalCaseContractsRepository;
            _legalCaseHttpService = legalCaseHttpService;
            _logger = logger;
        }

        public async Task CancelCloseLegalCaseAsync(int contractId)
        {
            _logger.LogInformation("Начало операции отмены закрытия дела \"LegalCase\" с ContractId: {ContractId}", contractId);
            var legalCaseContract = await _legalCaseContractsRepository.GetNotActiveLegalCaseByContractIdAsync(contractId);
            if (legalCaseContract == null)
            {
                return;
            }

            try
            {
                var response = await _legalCaseHttpService.CancelLegalCaseHttpRequest(legalCaseContract.LegalCaseId);
                await _legalCaseContractsRepository.ChangeActivityAsync(legalCaseId: response, active: true);
            }
            catch (Exception e)
            {
                await _legalCaseHttpService.RollbackLegalCaseHttpRequest(legalCaseContract.LegalCaseId);
                throw new PawnshopApplicationException("Не удалось отменить закрытие дела \"LegalCase\"", e.Message);
            }
        }

        public void CancelCloseLegalCase(int contractId)
        {
            _logger.LogInformation("Начало операции отмены закрытия дела \"LegalCase\" с ContractId: {ContractId}", contractId);
            var legalCaseContract = _legalCaseContractsRepository.GetNotActiveLegalCaseByContractId(contractId);
            if (legalCaseContract == null)
            {
                return;
            }

            try
            {
                var response = _legalCaseHttpService.CancelLegalCaseHttpRequest(legalCaseContract.LegalCaseId).Result;
                _legalCaseContractsRepository.ChangeActivity(legalCaseId: response, active: true);
            }
            catch (Exception e)
            {
                _legalCaseHttpService.RollbackLegalCaseHttpRequest(legalCaseContract.LegalCaseId).Wait();
                throw new PawnshopApplicationException("Не удалось отменить закрытие дела \"LegalCase\"", e.Message);
            }
        }
    }
}