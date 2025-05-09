using Pawnshop.Services.Applications;
using Pawnshop.Web.Engine.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using System.Threading;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Core;
using Newtonsoft.Json;
using Pawnshop.Data.Models.MobileApp;

namespace Pawnshop.Web.Engine.Jobs
{
    public class ApplicationsRejectAndDeleteRefContractsJob
    {
        private static readonly object _object = new object();
        private readonly JobLog _jobLog;
        private readonly IApplicationService _applicationService;
        private const int THREAD_SLEEP_MILLISECONDS = 300000;

        public ApplicationsRejectAndDeleteRefContractsJob(JobLog jobLog, IApplicationService applicationService)
        {
            _jobLog = jobLog;
            _applicationService = applicationService;
        }

        [Queue("applications")]
        public void Execute() 
        {
            bool tryEnter = false;
            if (tryEnter = Monitor.TryEnter(_object, new TimeSpan(0, 30, 0)))
            {
                var processingApplicationIds = new HashSet<int>();
                try
                {
                    DateTime today = DateTime.Today;
                    string requestData = $"Отмена Заявок МП и удаление драфтов Договоров из Заявок, дата - {today:dd.MM.yyyy}";
                    _jobLog.Log("ApplicationsRejectAndDeleteRefContractsJob", JobCode.Start, JobStatus.Success, requestData: requestData);
                    var applicationsForReject = _applicationService.GetApplicationsForReject();
                    if (!applicationsForReject.Any())
                    {
                        _jobLog.Log("ApplicationsRejectAndDeleteRefContractsJob", JobCode.End, JobStatus.Success, responseData: "Список Заявок МП для отмены пуст");
                        return;
                    }

                    _jobLog.Log("ApplicationsRejectAndDeleteRefContractsJob", JobCode.Begin, JobStatus.Success, EntityType.Application, null, requestData: requestData, $"Получено {applicationsForReject.Count} Заявок");
                    foreach (var application in applicationsForReject)
                    {
                        if (processingApplicationIds.Contains(application.Id))
                            continue;

                        processingApplicationIds.Add(application.Id);

                        BackgroundJob.Enqueue<ApplicationsRejectAndDeleteRefContractsJob>(x => x.RejectApplicationAndDeleteContract(application, today));
                    }
                }
                catch (Exception ex)
                {
                    _jobLog.Log("ApplicationsRejectAndDeleteRefContractsJob", JobCode.Error, JobStatus.Failed, EntityType.None, null, null, JsonConvert.SerializeObject(ex));
                    if (processingApplicationIds.Count == 0)
                        throw;
                }
                finally
                {
                    if (processingApplicationIds.Count > 0)
                        Thread.Sleep(THREAD_SLEEP_MILLISECONDS);

                    Monitor.Exit(_object);
                }
            }

            if (!tryEnter)
                _jobLog.Log("ApplicationsRejectAndDeleteRefContractsJob", JobCode.Error, JobStatus.Failed, EntityType.None, null, null, "Процесс не долждался своей очереди");
        }

        [Queue("applications")]
        public void RejectApplicationAndDeleteContract(Application application, DateTime date)
        {
            try
            {
                string requestData = $"Попытка отмены Заявки и удаления Договора {application.Id} на дату - {date:dd.MM.yyyy}";
                _jobLog.Log("ApplicationsRejectAndDeleteRefContractsJob.RejectApplicationAndDeleteContract", JobCode.Start, JobStatus.Success, EntityType.Application, application.Id, requestData, null);
                _applicationService.RejectApplicationAndDeleteContract(application, date);
                _jobLog.Log("ApplicationsRejectAndDeleteRefContractsJob.RejectApplicationAndDeleteContract", JobCode.End, JobStatus.Success, EntityType.Application, application.Id, requestData, null);
            }
            catch (Exception ex)
            {
                _jobLog.Log("ApplicationsRejectAndDeleteRefContractsJob.RejectApplicationAndDeleteContract", JobCode.Error, JobStatus.Failed, responseData: JsonConvert.SerializeObject(ex));
                throw;
            }
        }
    }
}
