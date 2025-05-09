using System;
using Hangfire;
using Microsoft.Extensions.Options;
using Pawnshop.Core.Options;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Web.Engine.Audit;
using Newtonsoft.Json;
using Pawnshop.Core;

namespace Pawnshop.Web.Engine.Jobs
{
    /// <summary>
    /// Генерация данных для PowerBI
    /// </summary>
    [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
    public class ReportDataJob
    {
        private readonly EnviromentAccessOptions _options;
        private readonly ReportDataRepository _reportDataRepository;
        private readonly JobLog _jobLog;

        public ReportDataJob(IOptions<EnviromentAccessOptions> options, ReportDataRepository reportDataRepository, JobLog jobLog)
        {
            _options = options.Value;
            _reportDataRepository = reportDataRepository;
            _jobLog = jobLog;
        }

        public void Execute()
        {
            if (!_options.PaymentNotification)
            {
               return;
            }

            try
            {
                _jobLog.Log("ReportDataJob", JobCode.Begin, JobStatus.Success, EntityType.ReportData, requestData: "Запуск джоба ReportDataJob");
                _reportDataRepository.CreateDataForYesterday();
                _jobLog.Log("ReportDataJob", JobCode.End, JobStatus.Success, EntityType.ReportData, requestData: $"Завершение джоба ReportDataJob");
            }
            catch (Exception ex)
            {
                _jobLog.Log("ReportDataJob", JobCode.Error, JobStatus.Failed, EntityType.ReportData, responseData: JsonConvert.SerializeObject(ex));
            }
            
        }
    }
}
