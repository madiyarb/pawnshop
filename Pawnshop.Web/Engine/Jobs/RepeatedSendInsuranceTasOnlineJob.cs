using Hangfire;
using Microsoft.Extensions.Options;
using Pawnshop.Core.Options;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.OnlineApplications;
using Pawnshop.Services.AbsOnline;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Web.Engine.MessageSenders;
using System;
using Pawnshop.Services.MessageSenders;

namespace Pawnshop.Web.Engine.Jobs
{
    public class RepeatedSendInsuranceTasOnlineJob
    {
        private readonly IAbsOnlineService _absOnlineService;
        private readonly EmailSender _emailSender;
        private readonly EventLog _eventLog;
        private readonly JobLog _jobLog;
        private readonly OnlineApplicationRetryInsuranceRepository _onlineApplicationRetryInsuranceRepository;
        private readonly EnviromentAccessOptions _options;
        private readonly int _attempts = 2;

        public RepeatedSendInsuranceTasOnlineJob(
            IAbsOnlineService absOnlineService,
            EmailSender emailSender,
            EventLog eventLog,
            JobLog jobLog,
            OnlineApplicationRetryInsuranceRepository onlineApplicationRetryInsuranceRepository,
            IOptions<EnviromentAccessOptions> options
            )
        {
            _absOnlineService = absOnlineService;
            _emailSender = emailSender;
            _eventLog = eventLog;
            _jobLog = jobLog;
            _onlineApplicationRetryInsuranceRepository = onlineApplicationRetryInsuranceRepository;
            _options = options.Value;
        }

        [Queue("insurance")]
        public void Execute()
        {
            try
            {
                _jobLog.Log("RepeatedSendInsuranceTasOnline", JobCode.Start, JobStatus.Success, EntityType.Contract);

                var records = _onlineApplicationRetryInsuranceRepository.List(null, new { Attempts = _attempts });

                foreach (var record in records)
                {
                    SendInsurance(record);
                }

                _jobLog.Log("RepeatedSendInsuranceTasOnline", JobCode.End, JobStatus.Success, EntityType.Contract);
            }
            catch (Exception ex)
            {
                _jobLog.Log("RepeatedSendInsuranceTasOnline", JobCode.End, JobStatus.Failed, EntityType.Contract, null, null, ex.Message);

            }
        }


        private void SendEmail(string message)
        {
            try
            {
                var messageReceiver = new MessageReceiver
                {
                    ReceiverAddress = _options.InsuranceErrorNotifierAddress,
                    ReceiverName = _options.InsuranceErrorNotifierName
                };

                _emailSender.SendEmail("Ошибка отправки страхового полиса в СК для Тас Онлайн.", message, messageReceiver);
            }
            catch (Exception ex)
            {
                _jobLog.Log("OnlinePaymentJob", JobCode.Error, JobStatus.Failed, responseData: $"Не удалось отправить уведомление об ошибке на емейл {_options.ErrorNotifierAddress}\r\n{ex.Message}");
            }
        }

        private void SendInsurance(OnlineApplicationRetryInsurance record)
        {
            try
            {
                var result = _absOnlineService.RegisterPolicy(record.ContractId);

                record.Attempts++;

                if (string.IsNullOrEmpty(result))
                {
                    record.IsSuccessful = true;
                    _eventLog.Log(EventCode.RepeatedSendInsuranceTasOnline, EventStatus.Success, EntityType.Contract, record.ContractId);
                }
                else if (record.Attempts >= 2)
                {
                    var message = $@"Не удалось отправить страховой полис в СК по займу <strong>{record.ContractId}</strong>.
Ошибка: <i>{result}</i>.";
                    SendEmail(message);
                }
                else
                {
                    _eventLog.Log(EventCode.RepeatedSendInsuranceTasOnline, EventStatus.Failed, EntityType.Contract, record.ContractId, null, result);
                }

                _onlineApplicationRetryInsuranceRepository.Update(record);
            }
            catch (Exception ex)
            {
                var message = $@"Техническая ошибка отправки страховки полиса в СК по займу <strong>{record.ContractId}</strong>.
Ошибка: <i>{ex.Message}</i>.";
                SendEmail(message);

                _eventLog.Log(EventCode.RepeatedSendInsuranceTasOnline, EventStatus.Failed, EntityType.Contract, record.ContractId, null, ex.Message);
            }
        }
    }
}
