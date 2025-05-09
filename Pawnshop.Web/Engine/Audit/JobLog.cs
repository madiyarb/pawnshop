using System;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;

namespace Pawnshop.Web.Engine.Audit
{
    public class JobLog : IJobLog
    {
        private readonly JobLogRepository _repository;

        public JobLog(JobLogRepository repository)
        {
            _repository = repository;
        }

        public void Log(string jobName, JobCode code, JobStatus status, EntityType entityType = EntityType.None, int? entityId = null, string requestData = null, string responseData = null)
        {
            var item = new JobLogItem();

            item.JobName = jobName;
            item.JobCode = code;
            item.JobStatus = status;

            item.EntityType = entityType;
            item.EntityId = entityId;
            item.RequestData = requestData;
            item.ResponseData = responseData;

            item.CreateDate = DateTime.Now;
            _repository.Insert(item);
        }
    }
}
