using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pawnshop.Core.Options;
using Pawnshop.Core.Utilities;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Interaction;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Services.Interactions;
using Pawnshop.Services.MessageSenders;
using Pawnshop.Services.Sms;
using Pawnshop.Web.Models.Sms;
using Pawnshop.Web.Models;
using System.Net;
using System;
using MediatR;
using Microsoft.CodeAnalysis.Operations;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/sms")]
    [ApiController]
    [Authorize]
    public class SmsController : ControllerBase
    {
        private readonly ApplicationOnlineRepository _applicationOnlineRepository;
        private readonly ClientContactRepository _clientContactRepository;
        private readonly ClientRepository _clientRepository;
        private readonly IInteractionService _interactionService;
        private readonly IKazInfoTechSmsService _kazInfoTechSmsService;
        private readonly NotificationLogRepository _notificationLogRepository;
        private readonly NotificationReceiverRepository _notificationReceiverRepository;
        private readonly NotificationRepository _notificationRepository;
        private readonly EnviromentAccessOptions _options;
        private readonly ISessionContext _sessionContext;
        private readonly UserRepository _userRepository;

        private readonly bool _sendSmsAvailable;

        public SmsController(
            ApplicationOnlineRepository applicationOnlineRepository,
            ClientContactRepository clientContactRepository,
            ClientRepository clientRepository,
            IInteractionService interactionService,
            IKazInfoTechSmsService kazInfoTechSmsService,
            NotificationLogRepository notificationLogRepository,
            NotificationReceiverRepository notificationReceiverRepository,
            NotificationRepository notificationRepository,
            IOptions<EnviromentAccessOptions> options,
            ISessionContext sessionContext,
            UserRepository userRepository)
        {
            _applicationOnlineRepository = applicationOnlineRepository;
            _clientContactRepository = clientContactRepository;
            _clientRepository = clientRepository;
            _interactionService = interactionService;
            _kazInfoTechSmsService = kazInfoTechSmsService;
            _notificationLogRepository = notificationLogRepository;
            _notificationReceiverRepository = notificationReceiverRepository;
            _notificationRepository = notificationRepository;
            _options = options.Value;
            _sessionContext = sessionContext;
            _userRepository = userRepository;

            _sendSmsAvailable = true; // TODO WARNING replace to _options.SendSmsNotifications;
        }

        // TODO: написано на коленках, переписать на что-то адекватное
        [HttpPost("send/with-entity")]
        public ActionResult<BaseResponse> SendSmsWithEntity([FromBody] SendSmsWithEntityBinding binding)
        {
            var editPhoneNumber = RegexUtilities.GetNumbers(binding.PhoneNumber);

            if (!RegexUtilities.IsValidKazakhstanPhone(editPhoneNumber))
                return BadRequest(new BaseResponse(HttpStatusCode.BadRequest, $"Номер телефона {binding.PhoneNumber} указан неправильно."));

            if (string.IsNullOrEmpty(binding.Message))
                return BadRequest(new BaseResponse(HttpStatusCode.BadRequest, $"Сообщение не может быть пустым."));

            var user = _userRepository.Get(_sessionContext.UserId);
            var resultMessage = string.Empty;
            Client client = null;
            string subject = "Смс уведомление";

            if (binding.ApplicationOnlineId.HasValue)
            {
                var application = _applicationOnlineRepository.Get(binding.ApplicationOnlineId.Value);

                if (application == null)
                    return BadRequest(new BaseResponse(HttpStatusCode.BadRequest, $"Заявка {binding.ApplicationOnlineId.Value} не найдена."));

                subject += " в рамках заявки.";
                client = _clientRepository.GetOnlyClient(application.ClientId);
                resultMessage = BuildMessageForApplication(binding.Message, application, client);
            }
            else if (binding.ClientId.HasValue && string.IsNullOrEmpty(resultMessage))
            {
                client = _clientRepository.GetOnlyClient(binding.ClientId.Value);

                if (client == null)
                    return BadRequest(new BaseResponse(HttpStatusCode.BadRequest, $"Клиент {binding.ClientId.Value} не найден."));

                subject += " в рамках клиента.";
                resultMessage = BuildMessageForClient(binding.Message, client);
            }
            else if (binding.InteractionId.HasValue && string.IsNullOrEmpty(resultMessage))
            {
                var interaction = _interactionService.Get(binding.InteractionId.Value);

                if (interaction == null)
                    return BadRequest(new BaseResponse(HttpStatusCode.BadRequest, $"Взаимодействие {binding.InteractionId.Value} не найдена."));

                subject += " в рамках взаимодействия.";
                client = _clientRepository.GetOnlyClient(binding.ClientId.Value);
                resultMessage = BuildMessageForInteraction(binding.Message, user.InternalPhoneNumber, binding.PhoneNumber);
            }
            else
            {
                var clientId = _clientContactRepository.GetClientIdByDefaultPhone(editPhoneNumber);

                if (clientId.HasValue)
                    client = _clientRepository.GetOnlyClient(clientId.Value);

                subject += " в рамках взаимодействия.";
                resultMessage = BuildMessageForInteraction(binding.Message, user.InternalPhoneNumber, binding.PhoneNumber);
            }

            if (client != null)
            {
                var smsSendResult = SendMessage(resultMessage, client.Id, binding.PhoneNumber, subject, 530);

                if (!smsSendResult.Item1)
                    return Ok(new BaseResponse(HttpStatusCode.Locked, smsSendResult.Item2));
            }
            else
            {
                var smsSendResult = SendMessageWithoutClient(resultMessage, binding.PhoneNumber, subject, 530);

                if (!smsSendResult.Item1)
                    return Ok(new BaseResponse(HttpStatusCode.Locked, smsSendResult.Item2));
            }

            return Ok(new BaseResponse(HttpStatusCode.OK, "Смс было отправлено."));
        }


        private string BuildMessageForApplication(string message, ApplicationOnline applicationOnline, Client client)
        {
            message = message.Replace("{application_id}", applicationOnline.ApplicationNumber);
            message = message.Replace("{application_value}", applicationOnline.ApplicationAmount.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
            message = message.Replace("{application_period}", applicationOnline.LoanTerm.ToString());
            message = message.Replace("{firstname}", client.Name);
            message = message.Replace("{lastname}", client.Surname);
            message = message.Replace("{middlename}", client.Patronymic);
            message = message.Replace("{date_birth}", client.BirthDay?.ToString("dd-MM-yyyy"));

            return message;
        }

        private string BuildMessageForClient(string message, Client client)
        {
            message = message.Replace("{firstname}", client.Name);
            message = message.Replace("{lastname}", client.Surname);
            message = message.Replace("{middlename}", client.Patronymic);
            message = message.Replace("{date_birth}", client.BirthDay?.ToString("dd-MM-yyyy"));

            return message;
        }

        private string BuildMessageForInteraction(string message, string userPhone, string clientPhone)
        {
            message = message.Replace("{phone_int}", userPhone);
            message = message.Replace("{phone_ext}", clientPhone);

            return message;
        }

        private (bool, string) SendMessageWithoutClient(string message, string phoneNumber, string subject, int branchId)
        {
            var messageReceiver = new MessageReceiver
            {
                ReceiverAddress = phoneNumber
            };

            NotificationStatus status = NotificationStatus.ForSend;
            string statusMessage;
            bool success = false;

            try
            {
                SMSInfoTechResponseModel responseModel = new SMSInfoTechResponseModel { message_id = -1 };

                if (_sendSmsAvailable)
                {
                    responseModel = _kazInfoTechSmsService.SendSMS(message, messageReceiver);

                    statusMessage = _kazInfoTechSmsService.GetStatusMessage(responseModel);
                    status = _kazInfoTechSmsService.GetStatus(responseModel);

                    if (status == NotificationStatus.Sent)
                        success = true;

                    using (var transaction = _applicationOnlineRepository.BeginTransaction())
                    {
                        var notification = new Notification
                        {
                            BranchId = branchId,
                            CreateDate = DateTime.Now,
                            IsPrivate = true,
                            Message = message,
                            Subject = subject,
                            IsNonSchedule = true,
                            MessageType = MessageType.Sms,
                            Status = NotificationStatus.ForSend,
                            UserId = _sessionContext.UserId,
                        };

                        _notificationRepository.Insert(notification);

                        var interaction = new Interaction(
                            _sessionContext.UserId,
                            InteractionType.SMS,
                            null,
                            phoneNumber,
                            "СМС отправлено",
                            null,
                            null,
                            null,
                            null,
                            null,
                            smsNotificationId: notification.Id
                            );

                        _interactionService.Create(interaction);

                        transaction.Commit();
                    }

                    return (success, statusMessage);
                }
                else
                {
                    status = NotificationStatus.Sent;
                    statusMessage = $"Смс сообщение не отправлено из-за false значения настройки {nameof(_options.SendSmsNotifications)}";

                    return (false, statusMessage);
                }
            }
            catch (Exception e)
            {
                return (false, $"Технические неполадки при отправке смс сообщения: {e.Message}");
            }
        }

        private (bool, string) SendMessage(string message, int clientId, string phoneNumber, string subject, int branchId)
        {
            using (var transaction = _applicationOnlineRepository.BeginTransaction())
            {
                var notification = new Notification
                {
                    BranchId = branchId,
                    CreateDate = DateTime.Now,
                    IsPrivate = true,
                    Message = message,
                    Subject = subject,
                    IsNonSchedule = true,
                    MessageType = MessageType.Sms,
                    Status = NotificationStatus.ForSend,
                    UserId = _sessionContext.UserId,
                };

                _notificationRepository.Insert(notification);

                var notificationReceiver = new NotificationReceiver
                {
                    Address = phoneNumber,
                    ClientId = clientId,
                    NotificationId = notification.Id,
                    TryCount = 0,
                    Status = NotificationStatus.ForSend
                };

                _notificationReceiverRepository.Insert(notificationReceiver);

                var messageReceiver = new MessageReceiver
                {
                    ReceiverAddress = notificationReceiver.Address,
                    ReceiverId = notificationReceiver.Id
                };

                NotificationStatus status = NotificationStatus.ForSend;
                string statusMessage;
                bool success = false;
                int messageId = -1;

                var interaction = new Interaction(
                    _sessionContext.UserId,
                    InteractionType.SMS,
                    null,
                    phoneNumber,
                    "СМС отправлено",
                    null,
                    null,
                    null,
                    null,
                    null,
                    clientId,
                    smsNotificationId: notification.Id
                    );

                _interactionService.Create(interaction);

                try
                {
                    SMSInfoTechResponseModel responseModel = new SMSInfoTechResponseModel { message_id = -1 };

                    if (_sendSmsAvailable)
                    {
                        responseModel = _kazInfoTechSmsService.SendSMS(message, messageReceiver);

                        if (responseModel.message_id.HasValue)
                        {
                            messageId = responseModel.message_id.Value;
                        }

                        statusMessage = _kazInfoTechSmsService.GetStatusMessage(responseModel);
                        status = _kazInfoTechSmsService.GetStatus(responseModel);

                        if (status == NotificationStatus.Sent)
                            success = true;

                        notificationReceiver.Status = status;
                        notificationReceiver.SentAt = DateTime.Now;
                        notificationReceiver.MessageId = messageId;

                        // заменим настоящее содержание смс на замаскированное значение
                        notificationReceiver.Status = status;
                        notificationReceiver.SentAt = DateTime.Now;
                        notificationReceiver.MessageId = messageId;
                        notificationReceiver.TryCount++;
                        notificationReceiver.Address = phoneNumber;
                        _notificationReceiverRepository.Update(notificationReceiver);

                        _notificationLogRepository.Insert(new NotificationLog
                        {
                            NotificationReceiverId = messageReceiver.ReceiverId,
                            StatusMessage = statusMessage
                        });

                        _notificationRepository.SyncWithNotificationReceiversStatus(notificationReceiver.NotificationId);
                        transaction.Commit();

                        return (success, statusMessage);
                    }
                    else
                    {
                        status = NotificationStatus.Sent;
                        statusMessage = $"Смс сообщение не отправлено из-за false значения настройки {nameof(_options.SendSmsNotifications)}";

                        return (false, statusMessage);
                    }
                }
                catch (Exception e)
                {
                    return (false, $"Технические неполадки при отправке смс сообщения: {e.Message}");
                }
            }
        }
    }
}
