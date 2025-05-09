using ApplicationOnlineRejectionReason = Pawnshop.Data.Models.ApplicationOnlineRejectionReasons.ApplicationOnlineRejectionReason;
using Hangfire;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Services.ApplicationsOnline;
using Pawnshop.Web.Engine.Audit;
using System;
using Autofac.Core;
using Pawnshop.Data.Models.ApplicationsOnline.Events;
using KafkaFlow.Producers;
using System.Linq;

namespace Pawnshop.Web.Engine.Jobs
{
    public class CancelApplicationOnlineJob
    {
        private readonly ApplicationOnlineRejectionReasonsRepository _applicationOnlineRejectionReasonsRepository;
        private readonly ApplicationOnlineRepository _applicationOnlineRepository;
        private readonly IApplicationOnlineService _applicationOnlineService;
        private readonly ContractRepository _contractRepository;
        private readonly EventLog _eventLog;
        private readonly JobLog _jobLog;
        private readonly IProducerAccessor _producers;

        public CancelApplicationOnlineJob(
            ApplicationOnlineRejectionReasonsRepository applicationOnlineRejectionReasonsRepository,
            ApplicationOnlineRepository applicationOnlineRepository,
            IApplicationOnlineService applicationOnlineService,
            ContractRepository contractRepository,
            EventLog eventLog,
            JobLog jobLog,
            IProducerAccessor producers)
        {
            _applicationOnlineRejectionReasonsRepository = applicationOnlineRejectionReasonsRepository;
            _applicationOnlineRepository = applicationOnlineRepository;
            _applicationOnlineService = applicationOnlineService;
            _contractRepository = contractRepository;
            _eventLog = eventLog;
            _jobLog = jobLog;
            _producers = producers;
        }

        [Queue("applications")]
        public void Execute()
        {
            try
            {
                _jobLog.Log("CancelApplicationOnlineJob", JobCode.Start, JobStatus.Success, EntityType.ApplicationOnline);

                var appList = _applicationOnlineRepository.GetIdListForDecline();
                var rejectionReason = _applicationOnlineRejectionReasonsRepository.FindByCode(Constants.APPLICATION_ONLINE_REJECT_REASON_AUTOREJECTION).Result;

                foreach (var app in appList)
                {
                    DeclineApplicationOnline(app, rejectionReason);
                }

                _jobLog.Log("CancelApplicationOnlineJob", JobCode.End, JobStatus.Success, EntityType.ApplicationOnline);
            }
            catch (Exception ex)
            {
                _jobLog.Log("CancelApplicationOnlineJob", JobCode.End, JobStatus.Failed, EntityType.ApplicationOnline, null, null, ex.Message);

            }
        }

        private void DeclineApplicationOnline(ApplicationOnline app, ApplicationOnlineRejectionReason rejectionReason)
        {
            try
            {
                app.Cancel(Constants.ADMINISTRATOR_IDENTITY, rejectionReason.Id, rejectionReason.Code, $"Автоматический отказ по истечению срока.");
                _applicationOnlineRepository.Update(app).Wait();
                _applicationOnlineService.DeleteDraftContractEntities(app.ContractId);
                _applicationOnlineService.DeleteDraftContractEntities(app.CreditLineId);

                ApplicationOnlineContractInfo contractInfo = new ApplicationOnlineContractInfo();

                if (app.ContractId.HasValue)
                {
                    var contract = _contractRepository.Get(app.ContractId.Value);
                    contractInfo.MaturityDate = contract.MaturityDate;

                    var cps = contract.PaymentSchedule.OrderBy(psc => psc.Date).FirstOrDefault();
                    contractInfo.MonthlyPaymentAmount = cps?.DebtCost + cps?.PercentCost;
                }

                var message = new ApplicationOnlineStatusChanged
                {
                    ApplicationOnline = app,
                    Status = app.Status.ToString(),
                    ApplicationOnlineContractInfo = contractInfo
                };

                _producers["ApplicationOnline"]
                    .ProduceAsync(app.Id.ToString(), message)
                    .Wait();
            }
            catch (Exception ex)
            {
                _eventLog.Log(EventCode.CancelApplicationOnline, EventStatus.Failed, EntityType.ApplicationOnline, null, null, ex.Message);
            }
        }
    }
}
