using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access.LegalCollection;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Data.Models.LegalCollection;
using Pawnshop.Data.Models.LegalCollection.Details;
using Pawnshop.Data.Models.LegalCollection.Details.HttpService;
using Pawnshop.Data.Models.LegalCollection.Documents;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Domains;
using Pawnshop.Services.LegalCollection.HttpServices.Interfaces;
using Pawnshop.Services.LegalCollection.Inerfaces;

namespace Pawnshop.Services.LegalCollection
{
    public class LegalCollectionUpdateService : ILegalCollectionUpdateService
    {
        private readonly ISessionContext _sessionContext;
        private readonly ILegalCaseContractsStatusRepository _legalCaseContracts;
        private readonly IContractService _contractService;
        private readonly ILegalCaseHttpService _legalCaseHttpService;
        private readonly IInscriptionService _inscriptionService;
        private readonly ILegalCasesDetailConverter _legalCasesDetailConverterService;
        private readonly ILegalCollectionDocumentsService _legalCollectionDocumentsService;
        private readonly ILegalCollectionCheckClientDeathService _legalCollectionCheckClientDeathService;
        private readonly IDomainService _domainService;

        public LegalCollectionUpdateService(
            ISessionContext sessionContext,
            ILegalCaseContractsStatusRepository legalCaseContracts,
            IContractService contractService,
            ILegalCaseHttpService legalCaseHttpService,
            IInscriptionService inscriptionService,
            ILegalCasesDetailConverter legalCasesDetailConverterService,
            ILegalCollectionDocumentsService legalCollectionDocumentsService,
            ILegalCollectionCheckClientDeathService legalCollectionCheckClientDeathService,
            IDomainService domainService
            )
        {
            _sessionContext = sessionContext;
            _legalCaseContracts = legalCaseContracts;
            _contractService = contractService;
            _legalCaseHttpService = legalCaseHttpService;
            _inscriptionService = inscriptionService;
            _legalCasesDetailConverterService = legalCasesDetailConverterService;
            _legalCollectionDocumentsService = legalCollectionDocumentsService;
            _legalCollectionCheckClientDeathService = legalCollectionCheckClientDeathService;
            _domainService = domainService;
        }

        public async Task<List<LegalCasesDetailsViewModel>> UpdateLegalCase(UpdateLegalCaseCommand request)
        {
            if (!UserHasRightsManageLegalCollection())
            {
                throw new PawnshopApplicationException("Нет прав для управления Legal collection");
            }

            var settings = GetSettings(Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE);
            if (settings is null)
            {
                throw new PawnshopApplicationException(
                    $"Не удалось получить настройки ко коду: {Constants.LEGAL_COLLECTION_INTEGRATION_SETTING_CODE}");
            }

            if (IsDeadClientAction(settings, request.Action.ActionCode))
            {
                await _legalCollectionCheckClientDeathService.CheckBlackListClientDeath(request.ClientId);
            }

            var legalCaseContractsStatus = await _legalCaseContracts.GetByLegalCaseIdAsync(request.LegalCaseId);
            if (legalCaseContractsStatus is null)
            {
                throw new PawnshopApplicationException($"Дело с Id: {request.LegalCaseId} не найдено");
            }

            if (request.FileId.HasValue)
            {
                var documentModel = new UploadLegalCaseDocumentCommand
                {
                    LegalCaseId = request.LegalCaseId,
                    LegalCaseActionId = request.Action?.Id,
                    FileId = request.FileId.Value,
                    DocumentTypeId = request.DocumentTypeId
                };
                await _legalCollectionDocumentsService.UploadDocumentAsync(documentModel);
            }

            if (CanStopAccrualsByAction(settings, request.Action.ActionCode))
            {
                return await StopAccruals(request);
            }

            if (CanResumeAccrualsByAction(settings, request.Action.ActionCode))
            {
                return await ResumeAccruals(request);
            }
            
            if (CanExecuteInscriptionByAction(settings, request.Action.ActionCode))
            {
                return await ExecuteInscription(request);
            }
            
            if (request.StateFeeAmount.HasValue)
            {
                var dutyInscriptionRow = await GetDutyInscriptionRow(request.ContractId);
                if (dutyInscriptionRow != null)
                {
                    throw new PawnshopApplicationException(
                        $"У контракта с Id: {request.ContractId} уже была записана гос. пошлина");
                }
            
                return await AddFeeAmount(request.ContractId, null, request);
            }
            
            var response = await MakeUpdateRequest(request);
            
            return await _legalCasesDetailConverterService.ConvertAsync(response);
        }
        
        private bool UserHasRightsManageLegalCollection()
        {
            var result = _sessionContext.UserId == Constants.ADMINISTRATOR_IDENTITY ||
                         _sessionContext.Permissions.Contains(Permissions.LegalCollectionManage);

            return result;
        }
        
        private bool IsDeadClientAction(List<DomainValue> settings, string actionCode)
        {
            var actionForDeadClientsWork = settings
                .Where(d => d.Name == Constants.LEGAL_COLLECTION_DEAD_CLIENT_KEY).ToList();

            if (actionForDeadClientsWork is null || !actionForDeadClientsWork.Any())
            {
                throw new PawnshopApplicationException("Не удалось получить список действий для работы по умершим");
            }
            
            var result = actionForDeadClientsWork.Select(d => d.Code).Contains(actionCode);

            return result;
        }
        
        private List<DomainValue> GetSettings(string settingsCode)
        {
            var settings = _domainService.GetDomainValues(settingsCode);
            if (settings is null)
            {
                throw new PawnshopApplicationException("Не удалось получить настройки LegalCollection");
            }

            return settings;
        }
        
        private bool CanStopAccrualsByAction(List<DomainValue> settings, string actionCode)
        {
            var actionsForResumeAccruals = settings
                .Where(d => d.Name == Constants.LEGAL_COLLECTION_STOP_ACCRUALS_KEY).ToList();

            if (actionsForResumeAccruals is null || !actionsForResumeAccruals.Any())
            {
                throw new PawnshopApplicationException("Не удалось получить список действий для остановки начислений");
            }
            
            var result = actionsForResumeAccruals.Select(d => d.Code).Contains(actionCode);

            return result;
        }
        
        private bool CanResumeAccrualsByAction(List<DomainValue> settings, string actionCode)
        {
            var actionsForResumeAccruals = settings
                .Where(d => d.Name == Constants.LEGAL_COLLECTION_RESUME_ACCRUALS_KEY).ToList();

            if (actionsForResumeAccruals is null || !actionsForResumeAccruals.Any())
            {
                throw new PawnshopApplicationException("Не удалось получить список действий для возобновления начислений");
            }
            
            var result = actionsForResumeAccruals.Select(d => d.Code).Contains(actionCode);

            return result;
        }
        
        private bool CanExecuteInscriptionByAction(List<DomainValue> settings, string actionCode)
        {
            var actionsForExecuteInscription = settings
                .Where(d => d.Name == Constants.LEGAL_COLLECTION_EXECUTE_INSCRIPTION_KEY).ToList();

            if (actionsForExecuteInscription is null || !actionsForExecuteInscription.Any())
            {
                throw new PawnshopApplicationException("Не удалось получить список действий для исполнения исп. надписи");
            }
            
            var result = actionsForExecuteInscription.Select(d => d.Code).Contains(actionCode);

            return result;
        }
        
        private async Task<List<LegalCasesDetailsViewModel>> StopAccruals(UpdateLegalCaseCommand request)
        {
            if (!UserHasRightsForAccrualsLegalCollection())
            {
                throw new PawnshopApplicationException("Нет прав для управления начислениями legal collection");
            }

            if (request.Rows == null || !request.Rows.Any())
            {
                throw new PawnshopApplicationException("Не переданы суммы InscriptionRows");
            }

            var contract = await _contractService.GetOnlyContractAsync(request.ContractId);
            if (!contract.InscriptionId.HasValue || !contract.IsOffBalance)
            {
                var dutyInscriptionRow = await GetDutyInscriptionRow(contract.Id, contract);
                if (dutyInscriptionRow != null)
                {
                    throw new PawnshopApplicationException(
                        $"У контракта с Id: {contract.Id} уже была записана гос. пошлина");
                }
                
                var response = await MakeUpdateRequest(request);

                try
                {
                    await _inscriptionService.StopAccruals(request);
                }
                catch (Exception e)
                {
                    try
                    {
                        await _legalCaseHttpService.RollbackLegalCaseHttpRequest(request.LegalCaseId);
                    }
                    catch (Exception rollbackException)
                    {
                        throw new PawnshopApplicationException("Не удалось выполнить откат начислений", rollbackException);
                    }

                    throw new PawnshopApplicationException("Не удалось остановить начисления", e);
                }

                return await _legalCasesDetailConverterService.ConvertAsync(response);
            }
            else
            {
                var inscription = await _inscriptionService.GetAsync(contract.InscriptionId);
                if (inscription == null)
                {
                    throw new PawnshopApplicationException(
                        $"Исполнительная надпись с Id: {contract.InscriptionId} не найдена");
                }
                if (inscription.Status == InscriptionStatus.New)
                {
                    var result = await _inscriptionService.ApproveInscriptionAsync(inscription.Id, inscription);
                    if (!result)
                    {
                        throw new PawnshopApplicationException(
                            $"Не удалось утвердить исп. надпись с Id: {inscription.Id}");
                    }
                }
                var response = await MakeUpdateRequest(request);
                return await _legalCasesDetailConverterService.ConvertAsync(response);
            }
        }
        
        private async Task<List<LegalCasesDetailsViewModel>> ResumeAccruals(UpdateLegalCaseCommand request)
        {
            if (!UserHasRightsForAccrualsLegalCollection())
            {
                throw new PawnshopApplicationException("Нет прав для управление начислениями legal collection");
            }
            
            var response = await MakeUpdateRequest(request);

            try
            {
                await _inscriptionService.ResumeAccruals(request);
            }
            catch (Exception e)
            {
                await _legalCaseHttpService.RollbackLegalCaseHttpRequest(request.LegalCaseId);
                throw new PawnshopApplicationException("Не удалось возобновить начисления", e.Message);
            }
            
            return await _legalCasesDetailConverterService.ConvertAsync(response);
        }
        
        private async Task<List<LegalCasesDetailsViewModel>> ExecuteInscription(UpdateLegalCaseCommand request)
        {
            var response = await MakeUpdateRequest(request);
            try
            {
                await _inscriptionService.ExecuteInscription(request);
            }
            catch (Exception e)
            {
                await _legalCaseHttpService.RollbackLegalCaseHttpRequest(request.LegalCaseId);
                throw new PawnshopApplicationException("Не удалось исполнить Исп. надпись", e.Message);
            }

            return await _legalCasesDetailConverterService.ConvertAsync(response);
        }
        
        private bool UserHasRightsForAccrualsLegalCollection()
        {
            var result = _sessionContext.UserId == Constants.ADMINISTRATOR_IDENTITY ||
                         _sessionContext.Permissions.Contains(Permissions.LegalCollectionManageAccrual);

            return result;
        }
        
        private async Task<InscriptionRow> GetDutyInscriptionRow(int contractId, Contract? contract = null)
        {
            contract ??= await _contractService.GetOnlyContractAsync(contractId);
            if (!contract.InscriptionId.HasValue)
            {
                return null;
            }
            
            var inscription = await _inscriptionService.GetAsync(contract.InscriptionId);

            return inscription?.Rows?.FirstOrDefault(r => r.PaymentType == AmountType.Duty);
        }
        
        private async Task<List<LegalCaseDetailsResponse>> MakeUpdateRequest(UpdateLegalCaseCommand request)
        {
            var response = await _legalCaseHttpService.UpdateLegalCase(request);
            if (response is null)
            {
                throw new PawnshopApplicationException($"Не удалось обновить дело c Id: {request.LegalCaseId}");
            }

            return response;
        }
        
        private async Task<List<LegalCasesDetailsViewModel>> AddFeeAmount(int contractId, Contract? contract, UpdateLegalCaseCommand request)
        {
            contract ??= await _contractService.GetOnlyContractAsync(contractId);
            if (contract is null)
            {
                throw new PawnshopApplicationException($"Договор с Id: {contractId} не найден");
            }

            var response = await MakeUpdateRequest(request);
            
            try
            {
                await _inscriptionService.AddInscriptionRow((int)contract.InscriptionId, null,
                    new InscriptionRow
                    {
                        PaymentType = AmountType.Duty,
                        Cost = (decimal)request.StateFeeAmount
                    });
            }
            catch (Exception e)
            {
                await _legalCaseHttpService.RollbackLegalCaseHttpRequest(request.LegalCaseId);
                throw new PawnshopApplicationException("Не удалось добавить гос. пошлину", e.Message);
            }
            
            return await _legalCasesDetailConverterService.ConvertAsync(response);
        }
    }
}