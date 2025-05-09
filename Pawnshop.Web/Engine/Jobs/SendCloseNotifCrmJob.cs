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
    public class SendCloseNotifCrmJob
    {
        private readonly IAbsOnlineService _absOnlineService;
        private readonly ContractRepository _contractRepository;
        private readonly EventLog _eventLog;
        private readonly JobLog _jobLog;
        private readonly EnviromentAccessOptions _options;

        public SendCloseNotifCrmJob(
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

        [Queue("senders")]
        public void Execute()
        {
            _jobLog.Log("SendCloseNotifCrmJob", JobCode.Start, JobStatus.Success, EntityType.Contract);
            _jobLog.Log("SendCloseNotifCrmJob", JobCode.End, JobStatus.Success, EntityType.Contract);
        }


        private void SendNotif(int contractId)
        {
            try
            {
                var result = _absOnlineService.SendNotificationCloseContractAsync(contractId).Result;

                if (string.IsNullOrEmpty(result))
                    _eventLog.Log(EventCode.SendCloseNotifCrm, EventStatus.Success, EntityType.Contract, contractId);
                else
                    _eventLog.Log(EventCode.SendCloseNotifCrm, EventStatus.Failed, EntityType.Contract, contractId, null, $"Error send http request: {result}");
            }
            catch (Exception ex)
            {
                _eventLog.Log(EventCode.SendCloseNotifCrm, EventStatus.Failed, EntityType.Contract, contractId, null, $"Error call send method: {ex.Message}");
            }
        }
    }
}
