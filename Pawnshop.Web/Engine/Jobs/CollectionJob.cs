using Hangfire;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Web.Engine.Audit;
using System;
using Pawnshop.Services.Collection;
using System.Linq;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Collection;
using Pawnshop.Services.HardCollection.Service.Interfaces;
using Pawnshop.Services.LegalCollection.Inerfaces;

namespace Pawnshop.Web.Engine.Jobs
{
    public class CollectionJob
    {
        private readonly ContractRepository _contractRepository;
        private readonly ICollectionService _collectionService;
        private readonly JobLog _jobLog;
        private readonly ClientsBlackListRepository _clientsBlackListRepository;
        private readonly IHardCollectionService _hardCollectionService;
        private readonly ILegalCollectionCloseService _closeLegalCaseService;
        
        
        public CollectionJob(ContractRepository contractRepository, ICollectionService collectionService, 
            JobLog jobLog, ClientsBlackListRepository clientsBlackListRepository, 
            IHardCollectionService hardCollectionService,
            ILegalCollectionCloseService closeLegalCaseService) 
        {
            _contractRepository = contractRepository;
            _collectionService = collectionService;
            _jobLog = jobLog;
            _clientsBlackListRepository = clientsBlackListRepository;
            _hardCollectionService = hardCollectionService;
            _closeLegalCaseService = closeLegalCaseService;
        }
        [Queue("senders")]
        public void Execute()
        {
            try
            {
                _jobLog.Log("CollectionJob", JobCode.Begin, JobStatus.Success, EntityType.Collection, requestData: "Запуск джоба для отправки просроченных договоров в Коллекшн");
                
                var overdueDaysValues = new CollectionOverdueDays
                {
                    SoftCollection = Constants.SOFT_OVERDUE_DAYS,
                    Legalhard = Constants.LEGALHARD_OVERDUE_DAYS,
                    LegalhardByRealestate = Constants.LEGALHARD_OVERDUE_DAYS_BY_REALESTATE
                };
                int succesCounter = 0;
                int closedCounter = 0;
                var contractStartOverduelist = _contractRepository.GetStartOverdueContractList(overdueDaysValues.SoftCollection);

                foreach (var contract in contractStartOverduelist)
                {
                    if (_collectionService.OverdueChangeStatus(contract))
                    {
                        succesCounter++;
                        _jobLog.Log("CollectionJob", JobCode.Start, JobStatus.Success, EntityType.SoftCollection, requestData: $"Договор и ИД {contract.ContractId} отправлен в коллекшн со статусом {contract.StatusAfterCode} успешно");
                    }
                    else
                    {
                        _jobLog.Log("CollectionJob", JobCode.Start, JobStatus.Failed, EntityType.SoftCollection, requestData: $"Договор и ИД {contract.ContractId} отправлен в коллекшн со статусом {contract.StatusAfterCode} не успешно");
                    }
                }

                var contractOverduelist = _contractRepository.GetOverdueContractList(overdueDaysValues);
                var toClose = new CollectionClose();

                foreach (var contract in contractOverduelist)
                {
                    toClose.ContractId = contract.ContractId;
                    toClose.ActionId = 0;
                    toClose.DelayDays = contract.DelayDays;

                    if (_collectionService.CloseContractCollection(toClose))
                    {
                        closedCounter++;
                        _jobLog.Log("CollectionJob", JobCode.Start, JobStatus.Success, EntityType.CloseCollection, requestData: $"Договор и ИД {contract.ContractId} исключен из Collection успешно");

                        try
                        {
                            _closeLegalCaseService.Close(contract.ContractId);
                            _jobLog.Log("CollectionJob", JobCode.Start, JobStatus.Success, EntityType.CloseCollection, requestData: $"Было закрыто дело с Id контракта: {contract.ContractId}");
                        }
                        catch (Exception ex)
                        {
                            _jobLog.Log("CollectionJob", JobCode.Start, JobStatus.Failed, EntityType.CloseCollection, requestData: $"Не удалось закрыть дело LC с ContractId: {contract.ContractId}. Ошибка: {ex.Message}");
                        }
                    }

                    else if (_collectionService.OverdueChangeStatus(contract))
                    {
                        succesCounter++;
                        if (contract.StatusAfterCode == Constants.HARDCOLLECTION_STATUS)
                            _jobLog.Log("CollectionJob", JobCode.Start, JobStatus.Success, EntityType.HardCollection, requestData: $"Договор и ИД {contract.ContractId} отправлен в коллекшн со статусом {contract.StatusAfterCode} успешно");
                        else if (contract.StatusAfterCode == Constants.LEGALHARDCOLLECTION_STATUS)
                            _jobLog.Log("CollectionJob", JobCode.Start, JobStatus.Success, EntityType.LegalHardCollection, requestData: $"Договор и ИД {contract.ContractId} отправлен в коллекшн со статусом {contract.StatusAfterCode} успешно");
                        else if (contract.StatusAfterCode == Constants.LEGALCOLLECTION_STATUS)
                            _jobLog.Log("CollectionJob", JobCode.Start, JobStatus.Success, EntityType.LegalCollection, requestData: $"Договор и ИД {contract.ContractId} отправлен в коллекшн со статусом {contract.StatusAfterCode} успешно");
                    }
                    else
                    {
                        if (contract.StatusAfterCode == Constants.HARDCOLLECTION_STATUS)
                            _jobLog.Log("CollectionJob", JobCode.Start, JobStatus.Failed, EntityType.HardCollection, requestData: $"Договор и ИД {contract.ContractId} отправлен в коллекшн со статусом {contract.StatusAfterCode} не успешно");
                        else if (contract.StatusAfterCode == Constants.LEGALHARDCOLLECTION_STATUS)
                            _jobLog.Log("CollectionJob", JobCode.Start, JobStatus.Failed, EntityType.LegalHardCollection, requestData: $"Договор и ИД {contract.ContractId} отправлен в коллекшн со статусом {contract.StatusAfterCode} не успешно");
                        else if (contract.StatusAfterCode == Constants.LEGALCOLLECTION_STATUS)
                            _jobLog.Log("CollectionJob", JobCode.Start, JobStatus.Failed, EntityType.LegalCollection, requestData: $"Договор и ИД {contract.ContractId} отправлен в коллекшн со статусом {contract.StatusAfterCode} не успешно");
                    }
                }

                _hardCollectionService.HCollectionJobSender().Wait();

                var frozenContracts = _contractRepository.GetFrozenContractToClose();
                foreach(var item in frozenContracts)
                {
                    _collectionService.CloseContractCollection(item);
                }

                _jobLog.Log("CollectionJob", JobCode.End, JobStatus.Success, EntityType.Collection, requestData: $"Отправлено в коллекшн {succesCounter} количество договоров");
                _jobLog.Log("CollectionJob", JobCode.End, JobStatus.Success, EntityType.Collection, requestData: $"Исключен из Collection {closedCounter} количество договоров");
            }
            catch(Exception ex)
            {
                _jobLog.Log("CollectionJob", JobCode.Error, JobStatus.Failed, EntityType.Collection, responseData: JsonConvert.SerializeObject(ex));
            }
            try
            {
                _jobLog.Log("AssingBlackoutStatus", JobCode.Start, JobStatus.Success, EntityType.Collection, requestData: $"Запуск по заполнению у клиента признака неблагонадежный плательщик");

                var blackOutList = _contractRepository.ListForDelayNotification(DateTime.Now, Constants.DAYS_DELAY91);
                var counter = 0;
                foreach (var contract in blackOutList)
                {
                    var clientBlackoutList = _clientsBlackListRepository.GetClientsBlackListsByClientId(contract.ClientId);
                    bool isAlreadyExists = clientBlackoutList.Any(x => x.ReasonId == 4);
                    if (isAlreadyExists)
                        continue;

                    ClientsBlackList entity = new ClientsBlackList()
                    {
                        ClientId = contract.ClientId,
                        ReasonId = 4,
                        AddedBy = Constants.ADMINISTRATOR_IDENTITY,
                        AddReason = "90+ дней просрочки",
                        AddedAt = DateTime.Now
                    };
                    _clientsBlackListRepository.Insert(entity);
                    counter++;
                }

                _jobLog.Log("AssingBlackoutStatus", JobCode.Start, JobStatus.Success, EntityType.Collection, requestData: $"Заполнено у {counter} клиентов признак неблагонадежный плательщик");
            }
            catch (Exception ex)
            {
                _jobLog.Log("AssingBlackoutStatus", JobCode.Error, JobStatus.Failed, EntityType.Collection, responseData: JsonConvert.SerializeObject(ex).ToString());
            }
        }
    }
}
