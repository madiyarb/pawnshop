using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Web.Engine.Audit;
using System;
using Hangfire;
using Pawnshop.Core.Options;
using Microsoft.Extensions.Options;
using Pawnshop.Services.Sms;
using TimeZoneConverter;
using RestSharp;

namespace Pawnshop.Web.Engine.Jobs
{
    public class UpdateDeliveryStatusesOfSmsNotifications
    {
        private readonly IKazInfoTechSmsService _smsService;
        private readonly NotificationReceiverRepository _notificationReceiverRepository;
        private readonly NotificationRepository _notificationRepository;
        private readonly JobLog _jobLog;
        private readonly NotificationLogRepository _notificationLogRepository;
        private readonly EnviromentAccessOptions _options;
        private readonly OuterServiceSettingRepository _outerServiceSettingRepository;

        public UpdateDeliveryStatusesOfSmsNotifications(IKazInfoTechSmsService smsService, NotificationReceiverRepository notificationReceiverRepository,
                JobLog jobLog, IOptions<EnviromentAccessOptions> options, NotificationRepository notificationRepository, NotificationLogRepository notificationLogRepository, 
                OuterServiceSettingRepository outerServiceSettingRepository)
        {
            _smsService = smsService;
            _notificationReceiverRepository = notificationReceiverRepository;
            _notificationRepository = notificationRepository;
            _jobLog = jobLog;
            _notificationLogRepository = notificationLogRepository;
            _options = options.Value;
            _outerServiceSettingRepository = outerServiceSettingRepository;
        }

        [Queue("senders")]
        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        public void Execute()
        {
            _jobLog.Log("UpdateDeliveryStatusesOfSmsNotifications", JobCode.Start, JobStatus.Success);
            /*if (!_options.UpdateSmsDeliveryStatuses)
            {
                _jobLog.Log("UpdateDeliveryStatusesOfSmsNotifications", JobCode.Cancel, JobStatus.Success);
                return;
            }*/

            int iterationsCount = 0;
            int processedSmsReports = 0;

            string controllerURL = _outerServiceSettingRepository.Find(new { Code = Constants.SMS_INTEGRATION_SERVICE_SETTING_CODE }).ControllerURL;
            //TODO DI- добавлю со следующим релизом
            RestClient client = new RestClient($"{controllerURL}/api/SMS/GetReport");

            try
            {
                var forReports = _notificationReceiverRepository.GetForUpdateReport();
                foreach (NotificationReceiver receiver in forReports)
                {
                    if (!receiver.MessageId.HasValue)
                    {
                        _jobLog.Log("UpdateDeliveryStatusesOfSmsNotifications", JobCode.Error, JobStatus.Failed, requestData: $"Поле MessageId не должно быть NULL");
                        continue;
                    }

                    string serializedSmsReport = "";


                    try
                    {
                        var smsReport = _smsService.GetReport((int)receiver.MessageId, client);
                        if (smsReport == null)
                        {
                            _jobLog.Log("UpdateDeliveryStatusesOfSmsNotifications", JobCode.Error, JobStatus.Failed, requestData: $"Получение отчетов по СМС({iterationsCount} итерация): IntegrationService вернул null объект SMSReport");
                            continue;
                        }

                        serializedSmsReport = JsonConvert.SerializeObject(smsReport);

                        int notificationReceiverId = receiver.Id;

                        NotificationReceiver notificationReceiver = _notificationReceiverRepository.Get(notificationReceiverId);
                        if (notificationReceiver == null)
                        {
                            _jobLog.Log("UpdateDeliveryStatusesOfSmsNotifications", JobCode.Error, JobStatus.Failed,
                            requestData: $"NotificationReceiver не найден по Id {notificationReceiverId}",
                            responseData: serializedSmsReport);
                            continue;
                        }

                        using (var transaction = _notificationReceiverRepository.BeginTransaction())
                        {
                            NotificationStatus currentStatus = _smsService.GetReportStatus(smsReport);
                            notificationReceiver.Status = currentStatus;
                            // обнуляем deliveredAt
                            notificationReceiver.DeliveredAt = null;
                            if(currentStatus == NotificationStatus.Delivered && smsReport.done_at != null)
                            {
                                DateTime UTCDeliveredAt = DateTime.SpecifyKind(smsReport.done_at.Value, DateTimeKind.Utc);
                                TimeZoneInfo cetInfo = TZConvert.GetTimeZoneInfo("Asia/Almaty");
                                DateTime deliveredAt = TimeZoneInfo.ConvertTime(UTCDeliveredAt, cetInfo);
                                notificationReceiver.DeliveredAt = TimeZoneInfo.ConvertTime(deliveredAt, cetInfo);
                            }

                            _notificationLogRepository.Insert(new NotificationLog
                            {
                                NotificationReceiverId = notificationReceiver.Id,
                                StatusMessage = serializedSmsReport
                            });
                            _notificationReceiverRepository.Update(notificationReceiver);
                            _notificationRepository.SyncWithNotificationReceiversStatus(notificationReceiver.NotificationId);
                            transaction.Commit();
                        }

                        _jobLog.Log("UpdateDeliveryStatusesOfSmsNotifications", JobCode.Begin, JobStatus.Success, requestData: $"Получение отчетов по СМС({iterationsCount} итерация): обрабатываем отчет по messageId {smsReport.message_id} - отчет обработан", responseData: serializedSmsReport);
                        processedSmsReports++;
                    }
                    catch (Exception ex)
                    {
                        _jobLog.Log("UpdateDeliveryStatusesOfSmsNotifications", JobCode.Error, JobStatus.Failed,
                            requestData: $"Обновление NotificationReceivers не получилось, ответ от infobip {serializedSmsReport}",
                            responseData: $"{ex.Message} {ex.StackTrace}");
                    }

                }
            }
            catch (Exception ex)
            {
                _jobLog.Log("UpdateDeliveryStatusesOfSmsNotifications", JobCode.Error, JobStatus.Failed,
    requestData: "Запрашивание списка доставленных смс сообщений завершилось с ошибкой",
    responseData: $"{ex.Message} {ex.StackTrace}");
            }

        }

    }
}