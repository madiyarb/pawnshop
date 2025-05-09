using Infobip.Api.Client;
using Infobip.Api.Config;
using Infobip.Api.Model;
using Infobip.Api.Model.Sms.Mt.Reports;
using Infobip.Api.Model.Sms.Mt.Send;
using Infobip.Api.Model.Sms.Mt.Send.Textual;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Options;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Web.Engine.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pawnshop.Web.Engine.Services
{
    public class InfobipSmsService : IInfobipSmsService
    {
        private readonly string _sendSmsFrom;
        private readonly SendMultipleTextualSmsAdvanced _sendSmsClient;
        private readonly GetSentSmsDeliveryReports _smsDeviveryReportsClient;
        private const string DELIVERED_STATUS = "DELIVERED_TO_HANDSET";
        private const string REJECTED_STATUS_GROUP = "REJECTED";
        private const string UNDELIVERABLE_STATUS_GROUP = "UNDELIVERABLE";
        private const string EXPIRED_STATUS_GROUP = "EXPIRED";

        public InfobipSmsService(IOptions<EnviromentAccessOptions> options)
        {
            EnviromentAccessOptions envOptions = options.Value;
            if (string.IsNullOrWhiteSpace(envOptions.InfobipFrom) || string.IsNullOrWhiteSpace(envOptions.InfobipPassword)
            ||  string.IsNullOrWhiteSpace(envOptions.InfobipUser))
                throw new ArgumentException($"Свойства аргрумента {nameof(options)}: {nameof(envOptions.InfobipFrom)} or {nameof(envOptions)} or {nameof(envOptions.InfobipUser)} должны быть непустыми");

            Configuration basicAuthConfig = new BasicAuthConfiguration(envOptions.InfobipUser, envOptions.InfobipPassword);
            _sendSmsClient = new SendMultipleTextualSmsAdvanced(basicAuthConfig);
            _smsDeviveryReportsClient = new GetSentSmsDeliveryReports(basicAuthConfig);
            _sendSmsFrom = envOptions.InfobipFrom;
        }
        public Task<SMSResponse> SendSmsAsync(IEnumerable<(string, int)> recepientsWithNotificationReceiverId, string message)
        {
            if (recepientsWithNotificationReceiverId == null)
                throw new ArgumentNullException(nameof(recepientsWithNotificationReceiverId));

            if (!recepientsWithNotificationReceiverId.Any())
                throw new ArgumentException($"{nameof(recepientsWithNotificationReceiverId)} не должен быть пустым");

            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException($"{nameof(message)} не должен быть пустым или состоять только из пробелов");

            SMSAdvancedTextualRequest smsRequest = CreateSMSMultiTextualRequest(_sendSmsFrom, message, recepientsWithNotificationReceiverId);
            return _sendSmsClient.ExecuteAsync(smsRequest);
        }

        public Task<SMSReportResponse> GetSmsDeliveryReportsAsync(int limit = 1000, string messageId = null, string bulkId = null)
        {
            if (limit <= 0)
                throw new ArgumentOutOfRangeException(nameof(limit));

            var request = new GetSentSmsDeliveryReportsExecuteContext { Limit = limit, MessageId = messageId, BulkId = bulkId };
            return _smsDeviveryReportsClient.ExecuteAsync(request);
        }

        public NotificationStatus GetNotificationStatusByStatus(Status smsStatus)
        {
            if (smsStatus == null)
                throw new ArgumentNullException(nameof(smsStatus));

            string statusName = smsStatus.Name;
            if (statusName == DELIVERED_STATUS)
                return NotificationStatus.Delivered;

            string statusGroupName = smsStatus.GroupName;
            if (statusGroupName == EXPIRED_STATUS_GROUP)
                return NotificationStatus.Expired;

            if (statusGroupName == REJECTED_STATUS_GROUP || statusGroupName == UNDELIVERABLE_STATUS_GROUP)
                return NotificationStatus.NotDelivered;

            return NotificationStatus.Sent;
        }

        public string TruncatePhoneNumber(string number)
        {
            if (string.IsNullOrWhiteSpace(number))
                throw new ArgumentException($"{nameof(number)} не должен быть Null или пустым");

            return Regex.Replace(number, "\\D", string.Empty);
        }

        private SMSAdvancedTextualRequest CreateSMSMultiTextualRequest(string from, string message, IEnumerable<(string, int)> recepientsWithCallbackData, string bulkId = null)
        {
            return new SMSAdvancedTextualRequest
            {
                Messages = recepientsWithCallbackData.Select(r => new Message
                {
                    Destinations = new List<Destination> { new Destination { To = r.Item1, MessageId = r.Item2.ToString() } },
                    From = from,
                    Text = message
                }
                ).ToList(),
                BulkId = bulkId
            };
        }
    }
}
