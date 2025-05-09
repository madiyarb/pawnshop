using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Services.MessageSenders;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.MessageSenders;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Engine.Services;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Web.Models.List;
using Pawnshop.Web.Models.Notification;

namespace Pawnshop.Web.Controllers.Api
{
    [Authorize(Permissions.NotificationView)]
    public class NotificationController : Controller
    {
        private readonly NotificationRepository _notificationRepository;
        private readonly NotificationReceiverRepository _notificationReceiverRepository;
        private readonly NotificationLogRepository _notificationLogRepository;
        private readonly ISessionContext _sessionContext;
        private readonly BranchContext _branchContext;
        private readonly EmailSender _emailSender;
        private readonly SmsSender _smsSender;
        private readonly IClientContactService _clientContactService;
        private readonly IVerificationService _verificationService;

        public NotificationController(NotificationRepository notificationRepository,
            NotificationReceiverRepository notificationReceiverRepository,
            NotificationLogRepository notificationLogRepository,
            ISessionContext sessionContext, BranchContext branchContext,
            EmailSender emailSender, SmsSender smsSender, IClientContactService clientContactService,
            IVerificationService verificationService)
        {
            _notificationRepository = notificationRepository;
            _notificationReceiverRepository = notificationReceiverRepository;
            _notificationLogRepository = notificationLogRepository;

            _sessionContext = sessionContext;
            _branchContext = branchContext;
            _emailSender = emailSender;
            _smsSender = smsSender;
            _clientContactService = clientContactService;
            _verificationService = verificationService;
        }

        [HttpPost]
        public ListModel<Notification> List([FromBody] ListQueryModel<NotificationListQueryModel> listQuery)
        {
            if (listQuery == null) listQuery = new ListQueryModel<NotificationListQueryModel>();
            if (listQuery.Model == null) listQuery.Model = new NotificationListQueryModel();

            if (listQuery.Model.EndDate.HasValue)
            {
                listQuery.Model.EndDate = listQuery.Model.EndDate.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            }

            return new ListModel<Notification>
            {
                List = _notificationRepository.List(listQuery, listQuery.Model),
                Count = _notificationRepository.Count(listQuery, listQuery.Model)
            };
        }

        [HttpPost]
        public Notification Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _notificationRepository.Get(id);
            if (model == null) throw new InvalidOperationException();

            return model;
        }

        [HttpPost, Authorize(Permissions.NotificationManage)]
        [Event(EventCode.NotificationSaved, EventMode = EventMode.Response, EntityType = EntityType.Notification)]
        public Notification Save([FromBody] Notification model)
        {
            if (model.BranchId <= 0)
            {
                model.BranchId = _branchContext.Branch.Id;
            }

            if (model.UserId <= 0)
            {
                model.UserId = _sessionContext.UserId;
            }

            ModelState.Clear();
            TryValidateModel(model);
            ModelState.Validate();

            if (model.Id > 0)
            {
                _notificationRepository.Update(model);
            }
            else
            {
                _notificationRepository.Insert(model);
            }

            return model;
        }

        [HttpPost, Authorize(Permissions.NotificationManage)]
        [Event(EventCode.NotificationDeleted, EventMode = EventMode.Request, EntityType = EntityType.Notification)]
        public IActionResult Delete([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _notificationRepository.Get(id);
            if (model == null) throw new InvalidOperationException();
            if (model.Status > NotificationStatus.Draft) throw new PawnshopApplicationException("Уведомление уже установлено для отправки");

            _notificationRepository.Delete(id);
            return Ok();
        }

        [HttpPost, Authorize(Permissions.NotificationManage)]
        [Event(EventCode.NotificationSetForSend, EventMode = EventMode.Request, EntityType = EntityType.Notification)]
        public IActionResult Send([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var model = _notificationRepository.Get(id);
            if (model == null) throw new InvalidOperationException();
            if (model.Status > NotificationStatus.Draft) throw new PawnshopApplicationException("Уведомление уже установлено для отправки");

            using (var transaction = _notificationRepository.BeginTransaction())
            {
                model.Status = NotificationStatus.ForSend;

                _notificationRepository.Update(model);
                _notificationReceiverRepository.ForSend(id);

                transaction.Commit();
            }

            return Ok();
        }

        [HttpPost, Authorize(Permissions.NotificationManage)]
        public IActionResult Resend([FromBody] int receiverId)
        {
            if (receiverId <= 0) throw new ArgumentOutOfRangeException(nameof(receiverId));

            var receiver = _notificationReceiverRepository.Get(receiverId);
            if (receiver == null) throw new InvalidOperationException();
            if (receiver.Status != NotificationStatus.Sent) throw new PawnshopApplicationException("Уведомление указанному получателю еще не отправлено");

            if (receiver.Notification.MessageType == MessageType.Email)
            {
                List<ClientContact> clientEmailAddresses = _clientContactService.GetEmailContacts(receiver.ClientId);
                if (clientEmailAddresses.Count == 0)
                    throw new PawnshopApplicationException("У получателя не заполнен email");

                foreach (ClientContact emailContact in clientEmailAddresses)
                {
                    _emailSender.Send(receiver.Notification.Subject, receiver.Notification.Message,
                        new List<MessageReceiver>
                        {
                            new MessageReceiver
                            {
                                ReceiverId = receiver.Id,
                                ReceiverName = receiver.Client.FullName,
                                ReceiverAddress = emailContact.Address
                            }
                        },
                        SenderCallback
                    );
                }

            }
            else if (receiver.Notification.MessageType == MessageType.Sms)
            {
                ClientContact clientContact = _verificationService.GetDefaultContact(receiver.ClientId);
                if (clientContact == null) throw new PawnshopApplicationException("У получателя не заполнен мобильный телефон");

                _smsSender.Send(receiver.Notification.Subject, receiver.Notification.Message,
                    new List<MessageReceiver>
                    {
                        new MessageReceiver
                        {
                            ReceiverId = receiver.Id,
                            ReceiverName = receiver.Client.FullName,
                            ReceiverAddress = clientContact.Address
                        }
                    },
                    SenderCallback
                );
            }

            return Ok();
        }

        public void SenderCallback(SendResult sendResult)
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

        [HttpPost, Authorize(Permissions.NotificationManage)]
        [Event(EventCode.NotificationSetForSend, EventMode = EventMode.Request, EntityType = EntityType.Notification)]
        public void SelectPeriod([FromBody] NotificationPeriodQueryModel model)
        {
            model.Status = NotificationStatus.ForSend;
            model.BranchId = _branchContext.Branch.Id;
            model.UserId = _sessionContext.UserId;

            ModelState.Clear();
            TryValidateModel(model);
            ModelState.Validate();

            _notificationRepository.SelectPeriod(model);
        }

        [HttpPost, Authorize(Permissions.NotificationManage)]
        [Event(EventCode.NotificationSetForSend, EventMode = EventMode.Request, EntityType = EntityType.Notification)]
        public void SelectOverdue([FromBody] NotificationOverdueQueryModel model)
        {
            model.Status = NotificationStatus.ForSend;
            model.BranchId = _branchContext.Branch.Id;
            model.UserId = _sessionContext.UserId;

            ModelState.Clear();
            TryValidateModel(model);
            ModelState.Validate();

            _notificationRepository.SelectOverdue(model);
        }
    }
}