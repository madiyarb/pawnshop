using System;
using System.Collections.Generic;
using Hangfire;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Web.Engine.MessageSenders;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Web.Engine.Audit;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Web.Engine.Services;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Services.MessageSenders;
using Pawnshop.Web.Engine.Services.Interfaces;

namespace Pawnshop.Web.Engine.Jobs
{
    public class MessageSenderJob
    {
        private readonly NotificationRepository _notificationRepository;
        private readonly NotificationReceiverRepository _notificationReceiverRepository;
        private readonly NotificationLogRepository _notificationLogRepository;
        private readonly EmailSender _emailSender;
        private readonly SmsSender _smsSender;
        private readonly JobLog _jobLog;
        private readonly IClientContactService _clientContactService;
        private readonly IVerificationService _verificationService;
        public MessageSenderJob(NotificationRepository notificationRepository,
            NotificationReceiverRepository notificationReceiverRepository,
            NotificationLogRepository notificationLogRepository, IClientContactService clientContactService,
            EmailSender emailSender, SmsSender smsSender, JobLog jobLog, IVerificationService verificationService)
        {
            _notificationRepository = notificationRepository;
            _notificationReceiverRepository = notificationReceiverRepository;
            _notificationLogRepository = notificationLogRepository;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _jobLog = jobLog;
            _clientContactService = clientContactService;
            _verificationService = verificationService;
        }

        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        public void Execute()
        {
            while (true)
            {
                try
                {
                    _jobLog.Log("MessageSenderJob", JobCode.Start, JobStatus.Success);

                    var receiver = _notificationReceiverRepository.Find(null);
                    if (receiver == null)
                    {
                        _jobLog.Log("MessageSenderJob", JobCode.End, JobStatus.Success);
                        break;
                    }

                    _notificationReceiverRepository.UpdateStatus(receiver.Id, NotificationStatus.Sending);

                    _jobLog.Log("MessageSenderJob", JobCode.Begin, JobStatus.Success, EntityType.Notification, receiver.NotificationId, JsonConvert.SerializeObject(receiver));
                    if (receiver.Notification.MessageType == MessageType.Email)
                    {
                        List<ClientContact> clientEmailAddresses = _clientContactService.GetEmailContacts(receiver.ClientId);
                        if (clientEmailAddresses.Count == 0)
                        {
                            Callback(new SendResult { ReceiverId = receiver.Id, StatusMessage = "У получателя не заполнен email", Success = false });
                            continue;
                        }

                        foreach (ClientContact clientEmailContact in clientEmailAddresses)
                        {
                            _emailSender.Send(receiver.Notification.Subject, receiver.Notification.Message,
                                new List<MessageReceiver>
                                {
                                    new MessageReceiver
                                    {
                                        ReceiverId = receiver.Id,
                                        ReceiverName = receiver.Client.FullName,
                                        ReceiverAddress = clientEmailContact.Address
                                    }
                                }, Callback
                            );
                        }


                    }
                    else if (receiver.Notification.MessageType == MessageType.Sms)
                    {
                        ClientContact defaultContact = _verificationService.GetDefaultContact(receiver.ClientId, false);
                        if (defaultContact == null)
                        {
                            Callback(new SendResult { ReceiverId = receiver.Id, StatusMessage = "У получателя не заполнен основной телефон", Success = false });
                            continue;
                        }

                        _smsSender.Send(receiver.Notification.Subject, receiver.Notification.Message,
                            new List<MessageReceiver>
                            {
                            new MessageReceiver
                            {
                                ReceiverId = receiver.Id,
                                ReceiverName = receiver.Client.FullName,
                                ReceiverAddress = defaultContact.Address
                            }
                            }, Callback
                        );
                    }
                    _jobLog.Log("MessageSenderJob", JobCode.End, JobStatus.Success, EntityType.Notification, receiver.NotificationId, responseData: JsonConvert.SerializeObject(receiver));
                }
                catch (Exception ex)
                {
                    _jobLog.Log("MessageSenderJob", JobCode.Error, JobStatus.Failed, responseData: JsonConvert.SerializeObject(ex));
                }
            }
        }

        [Queue("senders")]
        public void SendMessage(int id)
        {
            var receiver = _notificationReceiverRepository.Get(id);

            if (receiver.Status == NotificationStatus.Sent) return;


            if (receiver.Notification.MessageType == MessageType.Email)
            {
                List<ClientContact> clientEmailAddresses = _clientContactService.GetEmailContacts(receiver.ClientId);
                if (clientEmailAddresses.Count == 0)
                {
                    Callback(new SendResult { ReceiverId = receiver.Id, StatusMessage = "У получателя не заполнен email", Success = false });
                    return;
                }

                foreach (ClientContact clientEmailContact in clientEmailAddresses)
                {
                    _emailSender.Send(receiver.Notification.Subject, receiver.Notification.Message,
                    new List<MessageReceiver>
                    {
                        new MessageReceiver
                        {
                            ReceiverId = receiver.Id,
                            ReceiverName = receiver.Client.FullName,
                            ReceiverAddress = clientEmailContact.Address
                        }
                    }, Callback);
                }
            }
            if (receiver.Notification.MessageType == MessageType.Sms)
            {
                ClientContact defaultContact = _verificationService.GetDefaultContact(receiver.ClientId, false);
                if (defaultContact == null)
                {
                    Callback(new SendResult { ReceiverId = receiver.Id, StatusMessage = "У получателя не заполнен основной телефон", Success = false });
                    return;
                }

                _smsSender.Send(receiver.Notification.Subject, receiver.Notification.Message,
                    new List<MessageReceiver>
                    {
                            new MessageReceiver
                            {
                                ReceiverId = receiver.Id,
                                ReceiverName = receiver.Client.FullName,
                                ReceiverAddress = defaultContact.Address
                            }
                    }, Callback
                );
            }
        }

        private void Callback(SendResult sendResult)
        {
            if (sendResult == null)
                throw new ArgumentNullException(nameof(sendResult));

            var receiver = _notificationReceiverRepository.Get(sendResult.ReceiverId);
            if (receiver == null) throw new InvalidOperationException();

            using (var transaction = _notificationReceiverRepository.BeginTransaction())
            {
                if (sendResult.NotificationStatus.HasValue)
                    receiver.Status = sendResult.NotificationStatus.Value;
                else if (sendResult.Success)
                    receiver.Status = NotificationStatus.Sent;

                if (sendResult.NotificationStatus == NotificationStatus.Sent)
                {
                    receiver.SentAt = DateTime.Now;
                    receiver.MessageId = sendResult.MessageId;
                }
                    

                receiver.TryCount++;
                receiver.Address = sendResult.SendAddress;
                _notificationReceiverRepository.Update(receiver);

                _notificationLogRepository.Insert(new NotificationLog
                {
                    NotificationReceiverId = sendResult.ReceiverId,
                    StatusMessage = sendResult.StatusMessage
                });

                _notificationRepository.SyncWithNotificationReceiversStatus(receiver.NotificationId);

                transaction.Commit();
            }
        }
    }
}
