using Microsoft.Extensions.Options;
using Pawnshop.Core.Options;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Web.Engine.Audit;
using System;
using Pawnshop.Core;
using Pawnshop.Services.Notifications;
using Pawnshop.Data.Models.Notifications.Interfaces;
using Newtonsoft.Json;
using Pawnshop.Data.Models.Sms;

namespace Pawnshop.Web.Engine.Jobs
{
    public class ClientsBirthdayNotificationsJob
    {
        private readonly INotificationTemplateService _notificationTemplateService;
        private readonly ISmsNotificationService _smsService;
        private readonly ClientRepository _clientRepository;
        private readonly JobLog _jobLog;
        private readonly IOptions<EnviromentAccessOptions> _options;
        public ClientsBirthdayNotificationsJob(INotificationTemplateService notificationTemplateService,
            ISmsNotificationService smsService,
            ClientRepository clientRepository,
            IOptions<EnviromentAccessOptions> options, 
            JobLog jobLog)
        {
            _notificationTemplateService = notificationTemplateService;
            _smsService = smsService;
            _clientRepository = clientRepository;
            _options = options;
            _jobLog = jobLog;
        }
        public void Execute()
        {
            try
            {
                _jobLog.Log("ClientsBirthdayNotificationsJob", JobCode.Start, JobStatus.Success);
                if (!_options.Value.GenerateBirthdayNotifications)
                {
                    _jobLog.Log("ClientsBirthdayNotificationsJob", JobCode.Cancel, JobStatus.Success, responseData: $"Value of GenerateBirthdayNotifications option = {_options.Value.GenerateBirthdayNotifications}");
                    return;
                }

                _jobLog.Log("ClientsBirthdayNotificationsJob", JobCode.Begin, JobStatus.Success);
                var template = _notificationTemplateService.GetTemplate(Constants.BIRTHDAY);
                if(!_notificationTemplateService.IsValidTemplate(template))
                {
                    _jobLog.Log("ClientsBirthdayNotificationsJob", JobCode.Cancel, JobStatus.Success, responseData: $"Invalid NotificationTemplate with Code = {Constants.BIRTHDAY}");
                }

                var now = DateTime.Now;
                var clientBirthdayList = _clientRepository.GetByBirthday(now.Month, now.Day);
                int smsCounter = 0;
                foreach (var item in clientBirthdayList)
                {
                    var newSms = new SmsCreateNotificationModel(item.ClientId, item.BranchId, template.Subject, template.Message);
                    _smsService.CreateSmsNotification(newSms);
                    smsCounter++;
                }

                _jobLog.Log("ClientsBirthdayNotificationsJob", JobCode.End, JobStatus.Success, responseData: $"Созданы поздравления с днем рождения для {smsCounter} клиентов");
            }
            catch (Exception ex)
            {
                _jobLog.Log("ClientsBirthdayNotificationsJob", JobCode.Error, JobStatus.Failed, EntityType.None, responseData: JsonConvert.SerializeObject(ex).ToString());
            }
        }
    }
}
