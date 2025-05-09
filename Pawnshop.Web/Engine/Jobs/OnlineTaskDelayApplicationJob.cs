using Hangfire;
using KafkaFlow.Producers;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.OnlineTasks.Events;
using Pawnshop.Data.Models.OnlineTasks;
using Pawnshop.Web.Engine.Audit;
using System;
using Pawnshop.Data.Models.ApplicationsOnline;

namespace Pawnshop.Web.Engine.Jobs
{
    public class OnlineTaskDelayApplicationJob
    {
        private readonly ApplicationOnlineRepository _applicationOnlineRepository;
        private readonly JobLog _jobLog;
        private readonly OnlineTasksRepository _onlineTasksRepository;
        private readonly IProducerAccessor _producers;

        public OnlineTaskDelayApplicationJob(
            ApplicationOnlineRepository applicationOnlineRepository,
            JobLog jobLog,
            OnlineTasksRepository onlineTasksRepository,
            IProducerAccessor producers)
        {
            _applicationOnlineRepository = applicationOnlineRepository;
            _jobLog = jobLog;
            _onlineTasksRepository = onlineTasksRepository;
            _producers = producers;
        }


        [Queue("applications")]
        public void Execute()
        {
            try
            {
                _jobLog.Log("OnlineTaskDelayApplicationJob", JobCode.Start, JobStatus.Success, EntityType.None);

                var applicationApprovedList = _applicationOnlineRepository.GetDelayApprovedIds();

                foreach (var item in applicationApprovedList)
                {
                    CreateOnlineTask(item);
                }

                _jobLog.Log("OnlineTaskDelayApplicationJob", JobCode.End, JobStatus.Success, EntityType.None);
            }
            catch (Exception ex)
            {
                _jobLog.Log("OnlineTaskDelayApplicationJob", JobCode.End, JobStatus.Failed, EntityType.None, null, null, ex.Message);
            }

        }

        private void CreateOnlineTask(ApplicationOnlineDelayApproved item)
        {
            try
            {
                var onlineTask = new OnlineTask(Guid.NewGuid(), OnlineTaskType.DelayApplication.ToString(),
                            Constants.ADMINISTRATOR_IDENTITY, $"Задержка по заявке {item.Id}, на стороне клиента [ID:{item.ClientId}] с {item.UpdateDate:yyyy-MM-dd HH:mm}.",
                            "Considerate", null, item.ClientId, item.Id);

                _onlineTasksRepository.Insert(onlineTask);

                var onlineTaskCreated = new OnlineTaskCreated
                {
                    ApplicationId = onlineTask.ApplicationId,
                    ClientId = onlineTask.ClientId,
                    CompleteDate = onlineTask.CompleteDate,
                    CreateDate = onlineTask.CreateDate,
                    CreationUserId = onlineTask.CreationUserId,
                    Description = onlineTask.Description,
                    Done = onlineTask.Done,
                    Id = onlineTask.Id,
                    ShortDescription = onlineTask.ShortDescription,
                    Status = onlineTask.Status,
                    UserId = onlineTask.UserId
                };

                _producers["OnlineTask"]
                   .ProduceAsync(onlineTask.Id.ToString(), onlineTaskCreated)
                   .Wait();
            }
            catch (Exception ex)
            {
                _jobLog.Log("OnlineTaskDelayApplicationJob", JobCode.Error, JobStatus.Failed, EntityType.None, null, null, $"Delay application {item.Id} notification error: {ex.Message}");
            }
        }
    }
}
