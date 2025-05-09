using Newtonsoft.Json;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.MobileApp.HardCollection.Models;
using System;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.Notifications
{
    public class GetLogNotification
    {
        public LogNotification GetLogNotifications(string responseMessage = null, Exception exception = null, EventStatus eventStatus = EventStatus.Success, bool WriteTelegramLog = true, bool WriteEventLog = false)
        {
            var logNotification = new LogNotification()
            {
                FileLogNotification = GetFileLogNotification(responseMessage, exception, eventStatus)
            };

            if(WriteTelegramLog)
                logNotification.TelegaLogNotification = GetTelegramLogNotification(responseMessage, exception, eventStatus);

            if(WriteEventLog)
                logNotification.EventLogNotification = GetEventLogNotification(responseMessage, exception, eventStatus);

            return logNotification;
        }
        private TelegramLogNotification GetTelegramLogNotification(string responseMessage = null, Exception exception = null, EventStatus eventStatus = EventStatus.Success)
        {
            if (eventStatus == EventStatus.Success)
            {
                var objectData = JsonConvert.SerializeObject(MemberwiseClone());

                return new TelegramLogNotification()
                {
                    LogNotificationText = @$"{GetType().Name}
IsSucceeded={true}
{responseMessage}
Data:" + objectData
                };
            }
            else
            {
                return new TelegramLogNotification()
                {
                    LogNotificationText = @$"{GetType().Name}
IsSucceeded={false}
{responseMessage}
ExceptionMessage:" + exception.Message
                };
            }
        }

        private EventLogNotification GetEventLogNotification(string responseMessage = null, Exception exception = null, EventStatus eventStatus = EventStatus.Success)
        {
            if(eventStatus == EventStatus.Success)
            {
                var objectData = JsonConvert.SerializeObject(MemberwiseClone());
                return new EventLogNotification()
                {
                    RequestData = @$"{GetType().Name} Data:" + objectData,
                    ResponseData = responseMessage,
                    EventStatus = eventStatus
                };
            }
            else
            {
                var objectData = JsonConvert.SerializeObject(MemberwiseClone());
                return new EventLogNotification()
                {
                    RequestData = @$"{GetType().Name} Data:" + objectData,
                    ResponseData = exception.Message,
                    EventStatus = eventStatus
                };
            }
        }

        private FileLogNotification GetFileLogNotification(string responseMessage = null, Exception exception = null, EventStatus eventStatus = EventStatus.Success)
        {
            if(eventStatus == EventStatus.Success)
            {
                var objectData = JsonConvert.SerializeObject(MemberwiseClone());
                return new FileLogNotification()
                {
                    RequestData = @$"{GetType().Name} Data:" + objectData,
                    ResponseData = responseMessage,
                    EventStatus = eventStatus
                };
            }
            else
            {
                var objectData = JsonConvert.SerializeObject(MemberwiseClone());
                var responseData = JsonConvert.SerializeObject(exception.Message);
                return new FileLogNotification()
                {
                    RequestData = @$"{GetType().Name} Data:" + objectData,
                    ResponseData = responseData,
                    EventStatus = eventStatus
                };
            }
        }
    }
}
