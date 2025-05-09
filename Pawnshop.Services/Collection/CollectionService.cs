using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Collection;
using Pawnshop.Data.Models.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Data.Models.LegalCollection.Create;
using Pawnshop.Services.LegalCollection;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Collection.http;
using MediatR;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.MobileApp.HardCollection.Commands;
using Pawnshop.Services.LegalCollection.Inerfaces;
using Pawnshop.Services.ClientDeferments.Interfaces;
using Pawnshop.Data.Models.Restructuring;

namespace Pawnshop.Services.Collection
{
    public class CollectionService : ICollectionService
    {
        private readonly ICollectionHttpService<CollectionHistory> _httpService;
        private readonly ICollectionHttpService<CollectionStatusScenario> _httpScenarioService;
        private readonly ICollectionHttpService<CollectionActions> _httpActionService;
        private readonly ICollectionHttpService<CollectionReason> _httpReasonService;
        private readonly ICollectionHttpService<CollectionStatus> _httpStatusService;
        private readonly ICancelCloseLegalCollectionService _cancelCloseLegalCollectionService;
        private readonly ContractRepository _contractRepository;
        private readonly IContractService _contractService;
        private readonly CollectionStatusRepository _repository;
        private static List<CollectionStatusScenario> _scenarioList;
        private static List<CollectionReason> _reasonList;
        private static List<CollectionActions> _actionList;
        private static List<CollectionStatus> _statusList;
        private static DateTime _dictionaryListDate;
        private readonly IMediator _mediator;
        private readonly GroupRepository _groupRepository;
        private readonly ClientRepository _clientRepository;
        private readonly ILegalCollectionCreateService _legalCollectionCreateService;
        private readonly IClientDefermentService _clientDefermentService;
        
        
        public CollectionService(
            CollectionStatusRepository repository,
            ICollectionHttpService<CollectionHistory> httpService,
            ICollectionHttpService<CollectionStatusScenario> httpScenarioService,
            ICollectionHttpService<CollectionActions> httpActionService,
            ICollectionHttpService<CollectionStatus> httpStatusService,
            ICollectionHttpService<CollectionReason> httpReasonService,
            ContractRepository contractRepository,
            IContractService contractService,
            GroupRepository groupRepository,
            ClientRepository clientRepository,
            IMediator mediator,
            ICancelCloseLegalCollectionService cancelCloseLegalCollectionService,
            ILegalCollectionCreateService legalCollectionCreateService,
            IClientDefermentService clientDefermentService)
        {
            _repository = repository;
            _httpService = httpService;
            _httpScenarioService = httpScenarioService;
            _contractRepository = contractRepository;
            _httpStatusService = httpStatusService;
            _httpActionService = httpActionService;
            _httpReasonService = httpReasonService;
            _contractService = contractService;
            _mediator = mediator;
            _legalCollectionCreateService = legalCollectionCreateService;
            _cancelCloseLegalCollectionService = cancelCloseLegalCollectionService;
            _groupRepository = groupRepository;
            _clientRepository = clientRepository;
            _clientDefermentService = clientDefermentService;
        }

        public CollectionModel GetCollection(int contractId)//GetCollectionModel
        {
            FillDictionaryListDateListIfEmpty();

            var contract = _contractRepository.Get(contractId);
            if (contract == null)
                return null;

            var contractStatus = _repository.GetByContractId(contractId);

            var statusCode = contractStatus != null ? contractStatus.CollectionStatusCode : Constants.NOCOLLECTION_STATUS;
            var reasonList = _scenarioList.Where(x => x.StatusBefore.statusCode == statusCode && x.Reason.autoChange == false).Select(x=>x.Reason).ToList();
            var scenario = _scenarioList.Where(x => x.StatusBefore.statusCode == statusCode && x.Reason.autoChange == false).FirstOrDefault();

            var model = new CollectionModel()
            {
                CollectionContractStatusId = contractStatus.Id,
                ContractId = contractId,
                FincoreStatusId = GetFincoreStatus(scenario.StatusAfter.statusCode),
                CollectionActionId = scenario.ActionId,
                CollectionActions = scenario.Action,
                CollectionReason = reasonList,
                CollectionReasonId = reasonList.Select(x => x.Id.Value).ToList(),
                CollectionStatus = scenario.StatusBefore,
                CollectionStatusId = scenario.StatusBefore.Id.Value,
                CollectionStatusCode = contractStatus.CollectionStatusCode,
                CollectionStatusAfter = scenario.StatusAfter,
                CollectionStatusAfterId = scenario.StatusAfterId,
                DelayDays = contract.DelayDays,
                CategoryCode = "All",
                IsActive = true,
                StartDelayDate = contractStatus.StartDelayDate
            };

            return model;
        }

        public bool CloseContractCollection(CollectionClose close)
        {
            if (IsBalanceOK(close.ContractId) || IsInStatusFrozen(close.ContractId))
                return CloseCollection(close);

            return false;
        }

        public bool ChangeCollectionStatus(CollectionModel model)
        {
            return ChangeStatus(model);
        }

        public bool ParkingChangeStatus(int contractId)
        {
            FillDictionaryListDateListIfEmpty();

            var contract = _contractRepository.Get(contractId);
            if (contract == null)
                return false;

            var contractStatus = _repository.GetByContractId(contractId);
            if (contractStatus == null)
                return false;

            var scenario = _scenarioList.Where(x => x.Action.actionCode == Constants.SEND_LEGALCOLLECTION_ACTION
                                                    && x.StatusBefore.statusCode == contractStatus.CollectionStatusCode
                                                    && x.StatusAfter.statusCode == Constants.LEGALCOLLECTION_STATUS
                                                    && x.Reason.reasonCode == Constants.PARKING_STATUS_CHANGE_REASONCODE
                                                    && x.Reason.autoChange).FirstOrDefault();
            if (scenario == null)
                return false;

            var model = new CollectionModel()
            {
                CollectionContractStatusId = contractStatus.Id,
                ContractId = contractId,
                FincoreStatusId = contractStatus.FincoreStatusId,
                CollectionActionId = scenario.ActionId,
                CollectionStatusId = scenario.StatusBeforeId,
                CollectionStatusAfterId = scenario.StatusAfterId,
                CollectionStatusAfter = scenario.StatusAfter,
                SelectedReasonId = scenario.ReasonId,
                DelayDays = contract.DelayDays,
                Note = "Автоматическая смена статуса коллекшн при изменений статуса стоянки на \"На стоянке (Ждем)\"",
                IsActive = true,
                StartDelayDate = contractStatus.StartDelayDate
            };

            return ChangeStatus(model);
        }
        
        public bool OverdueChangeStatus(CollectionOverdueContract contract = null)
        {
            try
            {
                FillDictionaryListDateListIfEmpty();

                FillOverdueContract(contract);

                if (contract.Action == null || contract.FincoreStatusId == 0)
                    return false;

                CollectionModel model = new CollectionModel()
                {
                    ContractId = contract.ContractId,
                    FincoreStatusId = contract.FincoreStatusId,
                    CollectionActionId = contract.Action.Id.Value,
                    CollectionStatusId = contract.StatusBefore.Id.Value,
                    CollectionStatusAfterId = contract.StatusAfter.Id.Value,
                    CollectionStatusAfter = contract.StatusAfter,
                    DelayDays = contract.DelayDays,
                    SelectedReasonId = contract.ReasonId,
                    Note = $"Автоматическая смена статуса коллекшн день просрочки: {contract.DelayDays}",
                    IsActive = true,
                    CategoryCode = "All",
                    StartDelayDate = contract.StartDelayDate
                };

                return ChangeStatus(model);
            }
            catch 
            {
                return false;
            }
        }

        public int GetFincoreStatus(string statusCode)
        {
            if (statusCode == Constants.SOFTCOLLECTION_STATUS)
                return (int)ContractDisplayStatus.SoftCollection;
            else if (statusCode == Constants.HARDCOLLECTION_STATUS)
                return (int)ContractDisplayStatus.HardCollection;
            else if (statusCode == Constants.LEGALCOLLECTION_STATUS)
                return (int)ContractDisplayStatus.LegalCollection;
            else if (statusCode == Constants.LEGALHARDCOLLECTION_STATUS)
                return (int)ContractDisplayStatus.LegalHardCollection;
            else return 0;
        }

        public void CancelCloseCollection(int contractId, int actionId)
        {
            var history = _httpService.GetByContractId(contractId.ToString()).Result.FirstOrDefault(x => x.FincoreActionId == actionId);
            if (history == null)
                return;

            var statusCollection = _repository.GetByContractId(history.ContractId);
            if (statusCollection == null)
                return;

            var result = _httpService.Delete(history.Id.ToString()).Result;
            if (result <= 0)
                return;

            var status = new CollectionContractStatus()
            {
                Id = statusCollection.Id,
                ContractId = statusCollection.ContractId,
                FincoreStatusId = GetFincoreStatus(history.StatusBefore.statusCode),
                IsActive = true,
                CollectionStatusCode = history.StatusBefore.statusCode,
                StartDelayDate = statusCollection.StartDelayDate
            };

            _repository.Update(status);
            if (CanCancelLegalCase(status))
            {
                _cancelCloseLegalCollectionService.CancelCloseLegalCase(status.ContractId);
            }
        }

        private bool CloseCollection(CollectionClose close)
        {
            FillDictionaryListDateListIfEmpty();

            var contractStatus = _repository.GetByContractId(close.ContractId);
            if (contractStatus == null
                || !contractStatus.IsActive
                || contractStatus.DeleteDate.HasValue
                || contractStatus.CollectionStatusCode == Constants.NOCOLLECTION_STATUS)
                return false;

            var collectionAction = _actionList.FirstOrDefault(x => x.actionCode == Constants.CANCEL_COLLECTION_ACTION);
            var collectionReason = _reasonList.FirstOrDefault(x => x.reasonCode == Constants.CLOSE_COLLECTION_REASONCODE);
            var collectionStatus = _statusList.FirstOrDefault(x => x.statusCode == contractStatus.CollectionStatusCode);
            var collectionStatusAfter = _statusList.FirstOrDefault(x => x.statusCode == Constants.NOCOLLECTION_STATUS);

            var model = new CollectionModel()
            {
                CollectionContractStatusId = contractStatus.Id,
                ContractId = close.ContractId,
                FincoreStatusId = GetFincoreStatus(Constants.NOCOLLECTION_STATUS),
                FincoreActionId = close.ActionId,
                CollectionActionId = collectionAction.Id.Value,
                CollectionActions = collectionAction,
                CollectionReason = new List<CollectionReason>(),
                CollectionReasonId = new List<int>(),
                CollectionStatus = collectionStatus,
                CollectionStatusId = collectionStatus.Id.Value,
                CollectionStatusCode = contractStatus.CollectionStatusCode,
                CollectionStatusAfter = collectionStatusAfter,
                CollectionStatusAfterId = collectionStatusAfter.Id.Value,
                SelectedReasonId = collectionReason.Id,
                DelayDays = close.DelayDays < 0 ? 0 : close.DelayDays,
                CategoryCode = "All",
                IsActive = false,
                Note = "Исключение договора из коллекшн",
                StartDelayDate = contractStatus.StartDelayDate
            };

            return ChangeStatus(model);
        }

        private bool ChangeStatus(CollectionModel model)
        {
            var collectionHistory = new CollectionHistory()
            {
                ContractId = model.ContractId,
                ActionId = model.CollectionActionId,
                DelayDays = model.DelayDays,
                CreateUserId = Constants.ADMINISTRATOR_IDENTITY,
                CreateDate = DateTime.Now,
                IsActive = model.IsActive,
                Note = model.Note,
                ReasonId = (int)model.SelectedReasonId,
                StatusBeforeId = model.CollectionStatusId,
                StatusAfterId = model.CollectionStatusAfterId,
                FincoreActionId = model.FincoreActionId
            };

            var nextContractStatus = new CollectionContractStatus()
            {
                Id = model.CollectionContractStatusId,
                ContractId = model.ContractId,
                FincoreStatusId = GetFincoreStatus(model.CollectionStatusAfter.statusCode),
                CollectionStatusCode = model.CollectionStatusAfter.statusCode,
                IsActive = model.IsActive,
                StartDelayDate = model.StartDelayDate
            };
            
            if (!model.SelectedReasonId.HasValue)
            {
                throw new PawnshopApplicationException("\"Причина передачи\" не найдена!");
            }

            var prevContractStatus = _repository.GetByContractId(model.ContractId);

            if (prevContractStatus != null && prevContractStatus.FincoreStatusId != 0)
            {
                if (!(prevContractStatus.FincoreStatusId < nextContractStatus.FincoreStatusId ||
                            (
                                (prevContractStatus.CollectionStatusCode == Constants.LEGALHARDCOLLECTION_STATUS && nextContractStatus.CollectionStatusCode == Constants.LEGALCOLLECTION_STATUS) ||
                                (nextContractStatus.CollectionStatusCode == Constants.NOCOLLECTION_STATUS)
                            )
                    ))
                    return false;

                if (prevContractStatus.FincoreStatusId == nextContractStatus.FincoreStatusId)
                    return false;
            }

            var httpResult = _httpService.Create(collectionHistory).Result;
            if (httpResult <= 0)
                return false;

            if (prevContractStatus != null)
            {
                nextContractStatus.Id = prevContractStatus.Id;
                _repository.Update(nextContractStatus);
            }
            else
            {
                _repository.Insert(nextContractStatus);
            }

            CreateLegalCase(nextContractStatus, model);
            //SendToHardCollection(nextContractStatus);
            return true;
        }
        
        private void SendToHardCollection(CollectionContractStatus model)
        {
            if (model.CollectionStatusCode == Constants.HARDCOLLECTION_STATUS)
            {
                _mediator.Send(new SendContractDataCommand() { ContractId = model.ContractId, IsJobWorking = true }).Wait();
            }
            else if(model.CollectionStatusCode == Constants.LEGALHARDCOLLECTION_STATUS)
            {
                _mediator.Send(new SendContractOnlyCommand() { ContractId = model.ContractId }).Wait();
            }
        }

        private void FillDictionaryListDateListIfEmpty()
        {
            if (_scenarioList == null || _dictionaryListDate.Date != DateTime.Now.Date)
            {
                _scenarioList = _httpScenarioService.List().Result;
                _actionList = _httpActionService.List().Result;
                _reasonList = _httpReasonService.List().Result;
                _statusList = _httpStatusService.List().Result;
                _dictionaryListDate = DateTime.Now;
            }
        }

        private bool IsBalanceOK(int contractId)
        {
            var contractBalance = _contractService.GetBalances(new List<int>() { contractId });
            if(contractBalance.Any())
            {
                var balance = contractBalance.FirstOrDefault();
                if (balance.OverdueAccountAmount == 0 && balance.OverdueProfitAmount == 0 && balance.PenyAmount == 0)
                    return true;
                else
                    return false;
            }
            return false;
        }
        
        private bool IsInStatusFrozen(int contractId)
        {
            bool result = false;
            var deferment = _clientDefermentService.GetActiveDeferment(contractId);
            if (deferment == null)
                result = false;
            else if(deferment.Status == RestructuringStatusEnum.Frozen)
                result = true;

            return result;
        }

        private string GetCurrentStatusCode(int contractId)
        {
            var currentStatus = _repository.GetByContractId(contractId);
            if (currentStatus == null)
                return Constants.NOCOLLECTION_STATUS;
            else
                return currentStatus.CollectionStatusCode;
        }

        private void FillOverdueContract(CollectionOverdueContract contract)
        {
            string collectionStatusBeforeCode = GetCurrentStatusCode(contract.ContractId);

            switch (contract.StatusAfterCode)
            {
                case Constants.SOFTCOLLECTION_STATUS:
                    contract.Action = _actionList.FirstOrDefault(x => x.actionCode == Constants.SEND_SOFTCOLLECTION_ACTION);
                    contract.StatusBefore = _statusList.FirstOrDefault(x => x.statusCode == collectionStatusBeforeCode);
                    contract.StatusAfter = _statusList.First(x => x.statusCode == contract.StatusAfterCode);
                    contract.ReasonId = _reasonList.FirstOrDefault(x => x.reasonCode == Constants.DELAY1_CHANGE_REASONCODE).Id.Value;
                    contract.FincoreStatusId = (int)ContractDisplayStatus.SoftCollection;
                    break;

                case Constants.HARDCOLLECTION_STATUS:
                    contract.Action = _actionList.FirstOrDefault(x => x.actionCode == Constants.SEND_HARDCOLLECTION_ACTION);
                    contract.StatusBefore = _statusList.FirstOrDefault(x => x.statusCode == collectionStatusBeforeCode);
                    contract.StatusAfter = _statusList.First(x => x.statusCode == contract.StatusAfterCode);
                    contract.ReasonId = _reasonList.FirstOrDefault(x => x.reasonCode == Constants.DELAY2_CHANGE_REASONCODE).Id.Value;
                    contract.FincoreStatusId = (int)ContractDisplayStatus.HardCollection;
                    break;

                case Constants.LEGALCOLLECTION_STATUS:
                    contract.Action = _actionList.FirstOrDefault(x => x.actionCode == Constants.SEND_LEGALCOLLECTION_ACTION);
                    contract.StatusBefore = _statusList.FirstOrDefault(x => x.statusCode == collectionStatusBeforeCode);
                    contract.StatusAfter = _statusList.First(x => x.statusCode == contract.StatusAfterCode);
                    contract.ReasonId = _reasonList.FirstOrDefault(x => x.reasonCode == Constants.DELAY1_CHANGE_REASONCODE).Id.Value;
                    contract.FincoreStatusId = (int)ContractDisplayStatus.LegalCollection;
                    break;

                case Constants.LEGALHARDCOLLECTION_STATUS:
                    contract.Action = _actionList.FirstOrDefault(x => x.actionCode == Constants.SEND_LEGALHARDCOLLECTION_ACTION);
                    contract.StatusBefore = _statusList.FirstOrDefault(x => x.statusCode == collectionStatusBeforeCode);
                    contract.StatusAfter = _statusList.First(x => x.statusCode == contract.StatusAfterCode);
                    contract.ReasonId = _reasonList.FirstOrDefault(x => x.reasonCode == Constants.DELAY3_CHANGE_REASONCODE).Id.Value;
                    contract.FincoreStatusId = (int)ContractDisplayStatus.LegalHardCollection;
                    break;
            }
        }
        
        private void CreateLegalCase(CollectionContractStatus nextContractStatus, CollectionModel model)
        {
            if (nextContractStatus.FincoreStatusId == (int)ContractDisplayStatus.LegalCollection || 
                nextContractStatus.FincoreStatusId == (int)ContractDisplayStatus.LegalHardCollection)
            {
                Contract contract = _contractRepository.GetOnlyContract(model.ContractId);
                var client = _clientRepository.GetOnlyClient(contract.ClientId);
                CollectionReason reason = null;

                if ((model.CollectionReason is null || !model.CollectionReason.Any()) && !model.SelectedReasonId.HasValue)
                {
                    throw new PawnshopApplicationException("\"Причина передачи\" не найдена!");
                }

                if (model.CollectionReason != null && model.SelectedReasonId.HasValue)
                {
                    reason = _reasonList.FirstOrDefault(r => r.Id == model.SelectedReasonId);
                }

                reason ??= _reasonList.FirstOrDefault(r => r.Id == model.SelectedReasonId);

                var branchName = _groupRepository.Get(contract.BranchId).DisplayName;
                _legalCollectionCreateService.Create(new CreateLegalCaseCommand
                {
                    ContractId = contract.Id,
                    ContractNumber = contract.ContractNumber,
                    DelayCurrentDay = model.DelayDays,
                    CaseReasonId = model.SelectedReasonId,
                    BranchName = branchName,
                    ClientFullName = client.FullName,
                    ClientIIN = client.IdentityNumber,
                    Reason = reason
                });
            }
        }

        private bool CanCancelLegalCase(CollectionContractStatus collectionStatus)
        {
            var result =
                collectionStatus.IsActive &&
                (collectionStatus.CollectionStatusCode == Constants.LEGALCOLLECTION_STATUS ||
                 collectionStatus.CollectionStatusCode == Constants.LEGALHARDCOLLECTION_STATUS);
            
            return result;
        }
    }
}