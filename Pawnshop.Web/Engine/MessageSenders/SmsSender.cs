using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Options;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Services.MessageSenders;
using Pawnshop.Services.Sms;
using Pawnshop.Web.Engine.Services;
using Pawnshop.Web.Engine.Services.Interfaces;
using RestSharp;

namespace Pawnshop.Web.Engine.MessageSenders
{
    public class SmsSender : IMessageSender
    {
        private readonly IKazInfoTechSmsService _smsService;
        private readonly EnviromentAccessOptions _options;
        public SmsSender(IKazInfoTechSmsService smsService, IOptions<EnviromentAccessOptions> options)
        {
            _smsService = smsService;
            _options = options.Value;
        }

        public void Send(string subject, string message, List<MessageReceiver> receivers, Action<SendResult> callback)
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException($"{nameof(message)} не должен быть пустым или содержать только пробелы");

            if (receivers == null)
                throw new ArgumentNullException(nameof(receivers));

            if (receivers.Count == 0)
                throw new ArgumentException($"{nameof(receivers)} не должен быть пустым");

            if (receivers.Any(r => r == null))
                throw new ArgumentException($"{nameof(receivers)} не должен содержать пустые элементы");

            if (callback == null)
                throw new ArgumentNullException(nameof(callback));

            //переделываем из List в IList для того чтобы избежать ошибки
            //New transaction is not allowed because there are other threads running in the session
            //https://stackoverflow.com/questions/2113498
            IList<MessageReceiver> receiverList = receivers as IList<MessageReceiver>;



            foreach (var receiver in receiverList)
            {
                try
                {
                    NotificationStatus status = NotificationStatus.ForSend;
                    string statusMessage;
                    bool success = false;
                    int messageId = -1;
                    SMSInfoTechResponseModel responseModel = new SMSInfoTechResponseModel { message_id = -1};
                    if (_options.SendSmsNotifications)
                    {
                        responseModel = _smsService.SendSMS(message,receiver);
                        if (responseModel.message_id.HasValue)
                        {
                            messageId = responseModel.message_id.Value;
                        }
                        statusMessage = _smsService.GetStatusMessage(responseModel);
                        status = _smsService.GetStatus(responseModel);
                        if (status == NotificationStatus.Sent)
                            success = true;

                    }else
                    {
                        status = NotificationStatus.Sent;
                        statusMessage = $"Смс сообщение не отправлено из-за false значения настройки {nameof(_options.SendSmsNotifications)}";
                    }

                    callback.Invoke(new SendResult { ReceiverId = receiver.ReceiverId, StatusMessage = statusMessage, Success = success, NotificationStatus = status, SendAddress = receiver.ReceiverAddress, MessageId = messageId });
                }
                catch(Exception e)
                {
                    callback.Invoke(new SendResult { ReceiverId = receiver.ReceiverId, StatusMessage = e.Message, Success = false, SendAddress = receiver.ReceiverAddress });
                }
            }


            /*foreach (var receiver in receiverList)
            {
                try
                {
                    NotificationStatus status;
                    string statusMessage;
                    if (_options.SendSmsNotifications)
                    {
                        string receiverNumberTruncated = _smsService.TruncatePhoneNumber(receiver.ReceiverAddress);
                        SMSResponse smsResponse = _smsService.SendSmsAsync(new List<(string, int)> { (receiverNumberTruncated, receiver.ReceiverId) }, message).Result;
                        SMSResponseDetails smsResponseDetails = smsResponse?.Messages?.Where(m => m.To == receiverNumberTruncated).FirstOrDefault();
                        if (smsResponseDetails == null)
                            throw new InvalidOperationException($"{nameof(smsResponseDetails)} не должен быть null, в списке messages не было найдено сообщения с получателем {receiver.ReceiverAddress}");

                        status = _smsService.GetNotificationStatusByStatus(smsResponseDetails.Status);
                        statusMessage = JsonConvert.SerializeObject(smsResponse);
                    }
                    else
                    {
                        status = NotificationStatus.Sent;
                        statusMessage = $"Смс сообщение не отправлено из-за false значения настройки {nameof(_options.SendSmsNotifications)}";
                    }

                    callback.Invoke(new SendResult { ReceiverId = receiver.ReceiverId, StatusMessage = statusMessage, Success = true, NotificationStatus = status, SendAddress = receiver.ReceiverAddress });
                }
                catch (Exception e)
                {
                    callback.Invoke(new SendResult { ReceiverId = receiver.ReceiverId, StatusMessage = e.Message, Success = false, SendAddress = receiver.ReceiverAddress });
                }
            }*/
        }
    }
}
