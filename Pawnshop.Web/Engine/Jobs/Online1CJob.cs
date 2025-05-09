using System;
using Hangfire;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Services.Integrations.Online1C;
using Pawnshop.Data.Models.Online1C;

namespace Pawnshop.Web.Engine.Jobs
{
    public class Online1CJob
    {
        private readonly IJobLog _jobLog;
        private readonly IOnline1CService _online1CService;

        public Online1CJob(IJobLog jobLog, IOnline1CService online1CService)
        {
            _jobLog = jobLog;
            _online1CService = online1CService;
        }

        [Queue("online1c")]
        public void Execute()
        {
            try
            {
                var date = DateTime.Now.Date.AddDays(-1);
                var data = new Online1CReportData { 
                    Date = date
                };

                var cashFlowsData = new Online1CReportData { 
                    Date = date,
                    BeginDate = date,
                    EndDate = date
                };

                _jobLog.Log("Online1CJob", JobCode.Start, JobStatus.Success, EntityType.Online1C, null, "Запуск джоба отправки отчета в 1C", JsonConvert.SerializeObject(data));

                var result = _online1CService.SendReport(
                    data, 
                    _online1CService.GetAccrualsJson(data), 
                    _online1CService.GetPaymentJson(data), 
                    _online1CService.GetIssuesJson(data), 
                    _online1CService.GetPrepaymentJson(data), 
                    _online1CService.GetDebitDeposJson(data),
                    _online1CService.GetCashFlowsJson(cashFlowsData)
                ).Result;

                _jobLog.Log("Online1CJob", result.Item1 ? JobCode.End : JobCode.Error, result.Item1 ? JobStatus.Success : JobStatus.Failed, EntityType.Online1C, null, "Джоб отправки отчета в 1C завершился", null);
            }
            catch (Exception e)
            {
                _jobLog.Log("Online1CJob", JobCode.Error, JobStatus.Failed, EntityType.Online1C, null, null, e.Message);
            }
        }
    }
}
