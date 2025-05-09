using System;
using System.Linq;
using Hangfire;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Mail;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Web.Engine.Export;
using Pawnshop.Web.Engine.MessageSenders;
using Pawnshop.Services.Storage;

namespace Pawnshop.Web.Engine.Jobs
{
    public class TasOnlinePaymentsSendJob
    {
        private readonly JobLog _jobLog;
        private readonly EventLog _eventLog;
        private readonly EmailSender _emailSender;
        private readonly TasOnlinePaymentExcelBuilder _excelBuilder;
        private readonly IStorage _storage;
        private readonly TasOnlinePaymentRepository _tasOnlinePaymentRepository;

        public TasOnlinePaymentsSendJob(
            JobLog jobLog, 
            EventLog eventLog,
            EmailSender emailSender, 
            TasOnlinePaymentExcelBuilder excelBuilder, 
            IStorage storage, 
            TasOnlinePaymentRepository tasOnlinePaymentRepository)
        {
            _jobLog = jobLog;
            _eventLog = eventLog;
            _emailSender = emailSender;
            _excelBuilder = excelBuilder;
            _storage = storage;
            _tasOnlinePaymentRepository = tasOnlinePaymentRepository;
        }

        [Queue("senders")]
        public void Execute()
        {
            try
            {
                _jobLog.Log("TasOnlineReportSendJob", JobCode.Start, JobStatus.Success,
                    requestData: "Запуск джоба отправки отчета для сверки с ТасОнлайн");

                var date = DateTime.Now.Date.AddDays(-1);

                var payments = _tasOnlinePaymentRepository.List(new ListQuery(),
                    new
                    {
                        BeginDate = date,
                        EndDate = date.AddHours(23).AddMinutes(59).AddSeconds(59)
                    });

                if (!payments.Any())
                    _jobLog.Log("TasOnlineReportSendJob", JobCode.End, JobStatus.Success, responseData: "Список платежей для сверки с ТасОнлайн пуст");


                using (var stream = _excelBuilder.Build(payments))
                {
                    var fileName = _emailSender.SendEmail(MailingType.TasOnlinePayments, "tasonline/", stream, "payments.xlsx");

                    _jobLog.Log("TasOnlineReportSendJob", JobCode.End, JobStatus.Success, requestData: @$"Платежи ТасОнлайн сохранены в файл: {fileName}");
                }

            }
            catch (Exception e)
            {
                _eventLog.Log(
                    EventCode.TasOnlineReportSend,
                    EventStatus.Failed,
                    EntityType.None,
                    responseData: JsonConvert.SerializeObject(e),
                    userId: Constants.ADMINISTRATOR_IDENTITY
                );
                _jobLog.Log("TasOnlineReportSendJob", JobCode.Error, JobStatus.Failed, responseData: JsonConvert.SerializeObject(e));
            }
        }
    }
}