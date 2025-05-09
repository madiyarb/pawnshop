using Microsoft.Extensions.Options;
using Pawnshop.Core.Options;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.OnlinePayments;
using Pawnshop.Web.Engine.MessageSenders;
using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Web.Engine.Audit;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Services.MessageSenders;

namespace Pawnshop.Web.Engine.Jobs
{
    public class OnlinePaymentCheckStatusJob
    {
        private readonly OnlinePaymentRepository _onlinePaymentRepository;
        private readonly NotificationRepository _notificationRepository;
        private readonly NotificationReceiverRepository _notificationReceiverRepository;
        private readonly EmailSender _emailSender;
        private readonly EnviromentAccessOptions _options;
        private readonly JobLog _jobLog;

        public OnlinePaymentCheckStatusJob(OnlinePaymentRepository onlinePaymentRepository, NotificationRepository notificationRepository,
            NotificationReceiverRepository notificationReceiverRepository, EmailSender emailSender, IOptions<EnviromentAccessOptions> options, JobLog jobLog)
        {
            _onlinePaymentRepository = onlinePaymentRepository;
            _notificationReceiverRepository = notificationReceiverRepository;
            _notificationRepository = notificationRepository;
            _emailSender = emailSender;
            _options = options.Value;
            _jobLog = jobLog;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        public void Execute()
        {
            _jobLog.Log("OnlinePaymentCheckStatusJob", JobCode.Start, JobStatus.Success);
            if (!_options.PaymentNotification)
            {
                _jobLog.Log("OnlinePaymentCheckStatusJob", JobCode.End, JobStatus.Success);
                return;
            }

            try
            {
                List<OnlinePayment> onlinePayments = _onlinePaymentRepository.Select();
                if (onlinePayments.Count == 0)
                {
                    _jobLog.Log("OnlinePaymentCheckStatusJob", JobCode.End, JobStatus.Success);
                    SuccessEmailProcessingNotifications();
                    return;
                }
                _jobLog.Log("OnlinePaymentCheckStatusJob", JobCode.Begin, JobStatus.Success, requestData: JsonConvert.SerializeObject(onlinePayments));
                var count = 0;

                foreach (var onlinePayment in onlinePayments)
                {
                    if (DateTime.Now >= onlinePayment.CreateDate.AddMinutes(30)) count++;
                }

                if (count > 0) EmailProcessingNotifications(count);
                _jobLog.Log("OnlinePaymentCheckStatusJob", JobCode.End, JobStatus.Success, responseData: $@"В автоматическом списание поступивших денежных денег имеются не списанные поступление в количестве: {count.ToString()}");
            }
            catch (Exception ex)
            {
                _jobLog.Log("OnlinePaymentCheckStatusJob", JobCode.Error, JobStatus.Failed, responseData: JsonConvert.SerializeObject(ex).ToString());
            }
        }

        /// <summary>
        /// Email уведомление об ошибке
        /// </summary>
        private void EmailProcessingNotifications(int count)
        {
            var messageForFront = $@"ОШИБКА! В автоматическом списание поступивших денежных денег";
            var message = $@"<p><strong>ОШИБКА! В автоматическом списание поступивших денежных денег</strong></p>
                            <p><strong>Количество : {count.ToString()}</strong></p>";
           
            var messageReceiver = new MessageReceiver
            {
                ReceiverAddress = _options.ErrorNotifierAddress,
                ReceiverName = _options.ErrorNotifierName
            };

            _emailSender.SendEmail("[ОШИБКА] В автоматическом списание поступивших денежных денег", message, messageReceiver);
        }

        private void SuccessEmailProcessingNotifications()
        {
            var messageForFront = $@"Автоматическое списание поступивших денежных денег прошло успешно";
            var message = $@"<p><strong>Все платежи были освоены</strong></p>";

            var messageReceiver = new MessageReceiver
            {
                ReceiverAddress = _options.ErrorNotifierAddress,
                ReceiverName = _options.ErrorNotifierName
            };

            _emailSender.SendEmail("[Success] Автоматическое списание поступивших денежных денег прошло успешно", message, messageReceiver);
        }
    }
}
