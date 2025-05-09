using Hangfire;
using Microsoft.Extensions.Options;
using Pawnshop.Core.Options;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Services.AbsOnline;
using Pawnshop.Web.Engine.Audit;
using System.Linq;
using System;

namespace Pawnshop.Web.Engine.Jobs
{
    public class SendOverdueNotifCrmJob
    {
        private readonly IAbsOnlineService _absOnlineService;
        private readonly ContractRepository _contractRepository;
        private readonly EventLog _eventLog;
        private readonly JobLog _jobLog;
        private readonly EnviromentAccessOptions _options;

        public SendOverdueNotifCrmJob(
            IAbsOnlineService absOnlineService,
            ContractRepository contractRepository,
            EventLog eventLog,
            JobLog jobLog,
            IOptions<EnviromentAccessOptions> options
            )
        {
            _absOnlineService = absOnlineService;
            _contractRepository = contractRepository;
            _eventLog = eventLog;
            _jobLog = jobLog;
            _options = options.Value;
        }

        [Queue("SendOverdueNotifCrm")]
        public void Execute()
        {
            _jobLog.Log("SendOverdueNotifCrmJob", JobCode.Start, JobStatus.Success, EntityType.Contract);
            _jobLog.Log("SendOverdueNotifCrmJob", JobCode.End, JobStatus.Success, EntityType.Contract);
        }
    }
}
