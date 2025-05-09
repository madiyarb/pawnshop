using MediatR;
using Pawnshop.Core;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using System.Threading.Tasks;
using System.Threading;
using Pawnshop.Services.HardCollection.HttpClientService.Interfaces;
using Pawnshop.Data.Models.Audit;
using Microsoft.Extensions.Logging;
using Pawnshop.Data.Models.MobileApp.HardCollection.Models;

namespace Pawnshop.Services.HardCollection.Notifications
{
    public class LogHandler : INotificationHandler<HardCollectionNotification>
    {
        private readonly ITelegramHttpSender _telegramSender;
        private readonly IEventLog _eventLog;
        private readonly ILogger<FileLogNotification> _logger;
        public LogHandler(ITelegramHttpSender telegramSender, IEventLog eventLog, ILogger<FileLogNotification> logger) 
        {
            _telegramSender = telegramSender;
            _eventLog = eventLog;
            _logger = logger;
        }

        public async Task Handle(HardCollectionNotification request, CancellationToken cancellationToken = default)
        {
            if (request.LogNotification is null)
                await Task.CompletedTask;

            if(request.LogNotification.TelegaLogNotification != null)
            {
                var message = request.LogNotification.TelegaLogNotification.LogNotificationText;
                if (message.ToCharArray().Length > 4096)
                {
                    message = message.Substring(0, 4095);
                }
                await _telegramSender.SendLog(message);
            }

            if(request.LogNotification.EventLogNotification != null)
            {
                _eventLog.Log(EventCode.MobileAppHardCollectionSendPortfel, 
                    request.LogNotification.EventLogNotification.EventStatus, 
                    EntityType.HardCollection, 
                    requestData: request.LogNotification.EventLogNotification.RequestData, 
                    responseData: request.LogNotification.EventLogNotification.ResponseData, 
                    userId: Constants.ADMINISTRATOR_IDENTITY);
            }

            if(request.LogNotification!=null && request.LogNotification.FileLogNotification!=null)
            {
                if(request.LogNotification.FileLogNotification.EventStatus == EventStatus.Success)
                {
                    _logger.Log(LogLevel.Information, request.LogNotification.FileLogNotification.RequestData);
                }
                else
                {
                    _logger.Log(LogLevel.Error, request.LogNotification.FileLogNotification.RequestData);
                }
            }
        }
    }
}
