using KafkaFlow.Producers;
using KafkaFlow;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationOnlineStatusChangeHistories;
using Pawnshop.Data.Models.ApplicationsOnline.Events;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Data.Models.OnlineTasks.Events;
using Pawnshop.Data.Models.OnlineTasks;
using Pawnshop.Services.ApplicationsOnline;
using Pawnshop.Web.Engine.Services.Interfaces;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Pawnshop.Web.Handlers
{
    public class ApplicationOnlineStatusChangedEventHandler : IMessageHandler<ApplicationOnlineStatusChanged>
    {
        private readonly OnlineTasksRepository _onlineTasksRepository;
        private readonly IProducerAccessor _producers;
        private readonly ApplicationOnlineStatusChangeHistoryRepository _onlineStatusChangeHistoryRepository;
        private readonly ApplicationOnlinePositionRepository _applicationOnlinePositionRepository;
        private readonly ISignalRNotificationService _signalRNotificationService;
        private readonly IApplicationOnlineCheckCreationService _applicationOnlineCheckCreationService;
        private readonly ILogger _logger;

        public ApplicationOnlineStatusChangedEventHandler(
            OnlineTasksRepository onlineTasksRepository,
            IProducerAccessor producers,
            ApplicationOnlineStatusChangeHistoryRepository onlineStatusChangeHistoryRepository,
            ApplicationOnlinePositionRepository applicationOnlinePositionRepository,
            ISignalRNotificationService signalRNotificationService,
            IApplicationOnlineCheckCreationService applicationOnlineCheckCreationService,
            ILogger logger)
        {
            _onlineTasksRepository = onlineTasksRepository;
            _producers = producers;
            _onlineStatusChangeHistoryRepository = onlineStatusChangeHistoryRepository;
            _applicationOnlinePositionRepository = applicationOnlinePositionRepository;
            _signalRNotificationService = signalRNotificationService;
            _applicationOnlineCheckCreationService = applicationOnlineCheckCreationService;
            _logger = logger;
        }

        public async Task Handle(IMessageContext context, ApplicationOnlineStatusChanged message)
        {
            var position = _applicationOnlinePositionRepository.GetByApplicationOnlineId(message.ApplicationOnline.Id);

            _onlineStatusChangeHistoryRepository.Insert(new ApplicationOnlineStatusChangeHistory(message.ApplicationOnline.Id, message.ApplicationOnline.LastChangeAuthorId,
                DateTime.Now, message.ApplicationOnline.ApplicationAmount, position.EstimatedCost, message.Status, message.ApplicationOnline.RejectReasonComment)).Wait();

            if (message.Status.Equals(ApplicationOnlineStatus.Created.ToString()))
            {
                _applicationOnlineCheckCreationService.CreateChecksForManager(message.ApplicationOnline.Id);
                var task = _onlineTasksRepository.GetByApplicationId(message.ApplicationOnline.Id);
                if (task == null)//Idempotention check
                {
                    task = new OnlineTask(Guid.NewGuid(), OnlineTaskType.Considerate.ToString(),
                        Constants.ADMINISTRATOR_IDENTITY, $"Рассмотреть заявку {message.ApplicationOnline.Id}",
                        "Рассмотреть", null, message.ApplicationOnline.ClientId, message.ApplicationOnline.Id);
                    _onlineTasksRepository.Insert(task);
                    var onlineTaskCreated = new OnlineTaskCreated
                    {
                        ApplicationId = task.ApplicationId,
                        ClientId = task.ClientId,
                        CompleteDate = task.CompleteDate,
                        CreateDate = task.CreateDate,
                        CreationUserId = task.CreationUserId,
                        Description = task.Description,
                        Done = task.Done,
                        Id = task.Id,
                        ShortDescription = task.ShortDescription,
                        Status = task.Status,
                        UserId = task.UserId
                    };
                    await _producers["OnlineTask"]
                        .ProduceAsync(onlineTaskCreated.Id.ToString(), onlineTaskCreated);
                }
            }

            if (message.Status.Equals(ApplicationOnlineStatus.Consideration.ToString()))
            {
                var task = _onlineTasksRepository.GetByApplicationId(message.ApplicationOnline.Id, new List<OnlineTaskStatus> { OnlineTaskStatus.Created }, OnlineTaskType.Considerate.ToString());
                if (message.ApplicationOnline.ResponsibleManagerId.HasValue)
                {
                    if (task != null)
                    {
                        task.Processing(message.ApplicationOnline.ResponsibleManagerId.Value);
                        _onlineTasksRepository.Update(task);
                    }
                }
                else
                {
                    _logger.Warning($"Task not found for consideration {message.ApplicationOnline.Id}");
                }
            }

            if (message.Status.Equals(ApplicationOnlineStatus.Verification.ToString()))
            {
                _applicationOnlineCheckCreationService.CreateChecksForVerificator(message.ApplicationOnline.Id);

                var taskModify = _onlineTasksRepository.GetByApplicationId(message.ApplicationOnline.Id,
                    new List<OnlineTaskStatus> { OnlineTaskStatus.Processing, OnlineTaskStatus.Created }, OnlineTaskType.Modification.ToString());
                if (taskModify != null)
                {
                    taskModify.Complete();
                    _onlineTasksRepository.Update(taskModify);
                }

                var task = _onlineTasksRepository.GetByApplicationId(message.ApplicationOnline.Id,
                    new List<OnlineTaskStatus> { OnlineTaskStatus.Processing }, OnlineTaskType.Considerate.ToString());
                if (task != null)
                {
                    task.Complete();
                    _onlineTasksRepository.Update(task);
                }
                task = new OnlineTask(Guid.NewGuid(), OnlineTaskType.Verify.ToString(),
                    Constants.ADMINISTRATOR_IDENTITY, $"Верифицировать заявку {message.ApplicationOnline.Id}",
                    "Верифицировать", null, message.ApplicationOnline.ClientId, message.ApplicationOnline.Id);
                _onlineTasksRepository.Insert(task);
                var onlineTaskCreated = new OnlineTaskCreated
                {
                    ApplicationId = task.ApplicationId,
                    ClientId = task.ClientId,
                    CompleteDate = task.CompleteDate,
                    CreateDate = task.CreateDate,
                    CreationUserId = task.CreationUserId,
                    Description = task.Description,
                    Done = task.Done,
                    Id = task.Id,
                    ShortDescription = task.ShortDescription,
                    Status = task.Status,
                    UserId = task.UserId
                };
                await _producers["OnlineTask"]
                    .ProduceAsync(onlineTaskCreated.Id.ToString(), onlineTaskCreated);
            }

            if (message.Status.Equals(ApplicationOnlineStatus.Approved.ToString()))
            {
                _applicationOnlineCheckCreationService.CreateChecksForVerificator(message.ApplicationOnline.Id);
                var task = _onlineTasksRepository.GetByApplicationId(message.ApplicationOnline.Id,
                    new List<OnlineTaskStatus> { OnlineTaskStatus.Processing, OnlineTaskStatus.Created }, OnlineTaskType.Verify.ToString());
                if (task != null)
                {
                    task.Complete();
                    _onlineTasksRepository.Update(task);
                }
            }

            if (message.Status.Equals(ApplicationOnlineStatus.BiometricCheck.ToString()))
            {
                _applicationOnlineCheckCreationService.CreateAutoChecksForBiometric(message.ApplicationOnline.Id);
                var task = new OnlineTask(Guid.NewGuid(), OnlineTaskType.CheckBiometric.ToString(),
                    Constants.ADMINISTRATOR_IDENTITY, $"Сверить биометрию по заявку {message.ApplicationOnline.Id}",
                    "Сверить биометрию", null, message.ApplicationOnline.ClientId, message.ApplicationOnline.Id);
                _onlineTasksRepository.Insert(task);
                var onlineTaskCreated = new OnlineTaskCreated
                {
                    ApplicationId = task.ApplicationId,
                    ClientId = task.ClientId,
                    CompleteDate = task.CompleteDate,
                    CreateDate = task.CreateDate,
                    CreationUserId = task.CreationUserId,
                    Description = task.Description,
                    Done = task.Done,
                    Id = task.Id,
                    ShortDescription = task.ShortDescription,
                    Status = task.Status,
                    UserId = task.UserId
                };
                await _producers["OnlineTask"]
                    .ProduceAsync(onlineTaskCreated.Id.ToString(), onlineTaskCreated);
            }

            if (message.Status.Equals(ApplicationOnlineStatus.BiometricPassed.ToString()))
            {
                var task = _onlineTasksRepository.GetByApplicationId(message.ApplicationOnline.Id,
                    new List<OnlineTaskStatus> { OnlineTaskStatus.Processing }, OnlineTaskType.CheckBiometric.ToString());
                if (task != null)
                {
                    task.Complete();
                    _onlineTasksRepository.Update(task);
                }
            }

            if (message.Status.Equals(ApplicationOnlineStatus.RequisiteCheck.ToString()))
            {
                _applicationOnlineCheckCreationService.CreateAutoChecksForBiometric(message.ApplicationOnline.Id);//Проверка прошла автоматически нужно отметить как выполненые 
                _applicationOnlineCheckCreationService.CreateAutoChecksForRequisiteValidation(message.ApplicationOnline.Id);
                var task = new OnlineTask(Guid.NewGuid(), OnlineTaskType.RequisiteCheck.ToString(),
                    Constants.ADMINISTRATOR_IDENTITY, $"Сверка реквизитов для оплаты {message.ApplicationOnline.Id}",
                    "Сверка реквизитов для оплаты", null, message.ApplicationOnline.ClientId, message.ApplicationOnline.Id);
                _onlineTasksRepository.Insert(task);
                var onlineTaskCreated = new OnlineTaskCreated
                {
                    ApplicationId = task.ApplicationId,
                    ClientId = task.ClientId,
                    CompleteDate = task.CompleteDate,
                    CreateDate = task.CreateDate,
                    CreationUserId = task.CreationUserId,
                    Description = task.Description,
                    Done = task.Done,
                    Id = task.Id,
                    ShortDescription = task.ShortDescription,
                    Status = task.Status,
                    UserId = task.UserId
                };
                await _producers["OnlineTask"]
                    .ProduceAsync(onlineTaskCreated.Id.ToString(), onlineTaskCreated);
            }

            if (message.Status.Equals(ApplicationOnlineStatus.ContractConcluded.ToString()))
            {
                var task = _onlineTasksRepository.GetByApplicationId(message.ApplicationOnline.Id,
                    new List<OnlineTaskStatus> { OnlineTaskStatus.Processing, OnlineTaskStatus.Created }, OnlineTaskType.RequisiteCheck.ToString());
                if (task != null)
                {
                    task.Complete();
                    _onlineTasksRepository.Update(task);
                }
            }

            if (message.Status.Equals(ApplicationOnlineStatus.Declined.ToString()) ||
                message.Status.Equals(ApplicationOnlineStatus.Declined.ToString()))
            {
                var tasks = _onlineTasksRepository.GetOnlineTasksByApplicationId(message.ApplicationOnline.Id,
                    new List<OnlineTaskStatus> { OnlineTaskStatus.Created, OnlineTaskStatus.Processing }).Result;

                foreach (var task in tasks)
                {
                    task.Complete();
                    _onlineTasksRepository.Update(task);
                }
            }

            if (message.Status.Equals(ApplicationOnlineStatus.ModificationFromVerification.ToString()))
            {
                _applicationOnlineCheckCreationService.CreateChecksForManager(message.ApplicationOnline.Id);

                var taskVerify = _onlineTasksRepository.GetByApplicationId(message.ApplicationOnline.Id,
                    new List<OnlineTaskStatus> { OnlineTaskStatus.Created, OnlineTaskStatus.Processing, OnlineTaskStatus.Created }, OnlineTaskType.Verify.ToString());
                if (taskVerify != null)
                {
                    taskVerify.Complete();
                    _onlineTasksRepository.Update(taskVerify);
                }

                var taskModify = _onlineTasksRepository.GetByApplicationId(message.ApplicationOnline.Id,
                    new List<OnlineTaskStatus> { OnlineTaskStatus.Processing, OnlineTaskStatus.Created }, OnlineTaskType.Modification.ToString());

                if (taskModify == null)//Idempotention check
                {
                    taskModify = new OnlineTask(Guid.NewGuid(), OnlineTaskType.Modification.ToString(),
                        Constants.ADMINISTRATOR_IDENTITY, $"Доработать заявку {message.ApplicationOnline.Id} по комментарию верификатора",
                        "Доработка от верификатора", null, message.ApplicationOnline.ClientId, message.ApplicationOnline.Id);
                    _onlineTasksRepository.Insert(taskModify);
                    var onlineTaskCreated = new OnlineTaskCreated
                    {
                        ApplicationId = taskModify.ApplicationId,
                        ClientId = taskModify.ClientId,
                        CompleteDate = taskModify.CompleteDate,
                        CreateDate = taskModify.CreateDate,
                        CreationUserId = taskModify.CreationUserId,
                        Description = taskModify.Description,
                        Done = taskModify.Done,
                        Id = taskModify.Id,
                        ShortDescription = taskModify.ShortDescription,
                        Status = taskModify.Status,
                        UserId = taskModify.UserId
                    };
                    await _producers["OnlineTask"]
                        .ProduceAsync(onlineTaskCreated.Id.ToString(), onlineTaskCreated);
                }
            }

            await _signalRNotificationService.NotifyAllUsers(message, CancellationToken.None);

            var delayTask = _onlineTasksRepository.GetByApplicationId(message.ApplicationOnline.Id,
                new List<OnlineTaskStatus> { OnlineTaskStatus.Created }, OnlineTaskType.DelayApplication.ToString());
            if (delayTask != null)
            {
                delayTask.Complete();
                _onlineTasksRepository.Update(delayTask);
            }
        }
    }
}
