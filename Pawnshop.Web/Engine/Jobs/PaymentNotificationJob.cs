using Hangfire;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Core.Options;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Web.Engine.Audit;
using System;
using Pawnshop.Data.Models.Notifications.Interfaces;
using Pawnshop.Services.Notifications;
using Pawnshop.Data.Models.Sms;

namespace Pawnshop.Web.Engine.Jobs
{
    public class PaymentNotificationJob
    {
        private readonly ISmsNotificationService _notificationService;
        private readonly INotificationTemplateService _notificationTemplateService;
        private readonly EnviromentAccessOptions _options;
        private readonly JobLog _jobLog;
        private readonly ContractRepository _contractRepository;

        public PaymentNotificationJob(
            ISmsNotificationService notificationService, 
            INotificationTemplateService notificationTemplateService,
            IOptions<EnviromentAccessOptions> options,
            JobLog jobLog,
            ContractRepository contractRepository)
        {
            _notificationService = notificationService;
            _notificationTemplateService = notificationTemplateService;
            _options = options.Value;
            _jobLog = jobLog;
            _contractRepository = contractRepository;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        public void Execute()
        {
            try
            {
                _jobLog.Log("PaymentNotificationJob", JobCode.Start, JobStatus.Success);
                if (!_options.PaymentNotification)
                {
                    _jobLog.Log("PaymentNotificationJob", JobCode.End, JobStatus.Success);
                    return;
                }

                _jobLog.Log("PaymentNotificationJob", JobCode.Begin, JobStatus.Success, EntityType.Notification);
                
                SendLastPaymentNotification();
                SendPaymentNotification();
                
                _jobLog.Log("PaymentNotificationJob", JobCode.End, JobStatus.Success, EntityType.Notification);
            }
            catch (Exception ex)
            {
                _jobLog.Log("PaymentNotificationJob", JobCode.Error, JobStatus.Failed, EntityType.None, responseData: JsonConvert.SerializeObject(ex).ToString());
            }
        }

        private void SendLastPaymentNotification()
        {
            string responseData = $"Создано {0} количество смс уведомлений методом SendLastPaymentNotification";
            var template = _notificationTemplateService.GetTemplate(Constants.LAST_PAYMENT_NOTIFICATION);
            int smsCounter = 0;
            if(!_notificationTemplateService.IsValidTemplate(template))
            {
                return;
            }

            var lastPaymentNotificationContracts = _contractRepository.ListForLastPaymentNotification(DateTime.Now.Date);
            foreach (var item in lastPaymentNotificationContracts)
            {
                var message = string.Format(template.Message, item.PaymentDate.ToString("dd.MM.yyyy"), Math.Round(item.PaymentCost, 2), item.PaymentDate.ToString("dd.MM.yyyy"), Math.Round(item.PaymentCost, 2));
                var newSms = new SmsCreateNotificationModel(item.ClientId, item.BranchId, template.Subject, message, item.ContractId);
                _notificationService.CreateSmsNotification(newSms);
                smsCounter++;
            }

            _jobLog.Log("PaymentNotificationJob", JobCode.Processing, JobStatus.Success, EntityType.Notification, responseData: string.Format(responseData, smsCounter));
            return;
        }

        private void SendPaymentNotification()
        {
            string responseData = $"Создано {0} количество смс уведомлений методом SendPaymentNotification";
            var now = DateTime.Now;
            var template = _notificationTemplateService.GetTemplate(Constants.PAYMENT_NOTIFICATION);
            int smsCounter = 0;
            if (!_notificationTemplateService.IsValidTemplate(template))
            {
                return;
            }
            
            var paymentNotificationContracts = _contractRepository.ListForPaymentNotification(now.Date, Constants.DAYS_FOR_PAYMENT_NOTIFICATION);
            foreach (var item in paymentNotificationContracts)
            {
                var message = string.Format(template.Message, now.AddDays(Constants.DAYS_FOR_PAYMENT_NOTIFICATION).ToString("dd.MM.yyyy"));
                var smsMessage = new SmsCreateNotificationModel(item.ClientId, item.BranchId, template.Subject, message, item.ContractId);
                _notificationService.CreateSmsNotification(smsMessage);
                smsCounter++;
            }

            _jobLog.Log("PaymentNotificationJob", JobCode.Processing, JobStatus.Success, EntityType.Notification, responseData: string.Format(responseData, smsCounter));
            return;
        }
    }
}
