using Microsoft.Extensions.Options;
using Pawnshop.Core;
using Pawnshop.Core.Options;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationOnlineSignOtpVerifications;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Services.MessageSenders;
using Pawnshop.Services.Notifications;
using Pawnshop.Services.OTP;
using Pawnshop.Services.Sms;
using Serilog;
using System;

namespace Pawnshop.Services.ApplicationOnlineSms
{
    public sealed class ApplicationOnlineSmsService : IApplicationOnlineSmsService
    {
        private readonly IKazInfoTechSmsService _kazInfoTechSmsService;
        private readonly INotificationTemplateService _templateService;
        private readonly EnviromentAccessOptions _options;
        private readonly ApplicationOnlineRepository _applicationOnlineRepository;
        private readonly OTPCodeGeneratorService _otpCodeGeneratorService;
        private readonly NotificationRepository _notificationRepository;
        private readonly NotificationReceiverRepository _notificationReceiverRepository;
        private readonly ClientContactRepository _clientContactRepository;
        private readonly NotificationLogRepository _notificationLogRepository;
        private readonly ApplicationOnlineSignOtpVerificationRepository _applicationOnlineSignOtpVerificationRepository;
        private readonly ILogger _logger;
        public ApplicationOnlineSmsService(
            IKazInfoTechSmsService kazInfoTechSmsService,
            INotificationTemplateService templateService,
            IOptions<EnviromentAccessOptions> options,
            ApplicationOnlineRepository applicationOnlineRepository,
            OTPCodeGeneratorService otpCodeGeneratorService,
            NotificationRepository notificationRepository,
            NotificationReceiverRepository notificationReceiverRepository,
            ClientContactRepository clientContactRepository,
            NotificationLogRepository notificationLogRepository,
            ApplicationOnlineSignOtpVerificationRepository applicationOnlineSignOtpVerificationRepository,
            ILogger logger
            )
        {
            _kazInfoTechSmsService =
                kazInfoTechSmsService ?? throw new ArgumentNullException(nameof(kazInfoTechSmsService));
            _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
            _options = options.Value ?? throw new ArgumentNullException(nameof(options.Value));
            _applicationOnlineRepository = applicationOnlineRepository ??
                                           throw new ArgumentNullException(nameof(applicationOnlineRepository));
            _otpCodeGeneratorService = otpCodeGeneratorService ??
                                       throw new ArgumentNullException(nameof(otpCodeGeneratorService));
            _notificationReceiverRepository = notificationReceiverRepository ??
                                              throw new ArgumentNullException(nameof(notificationReceiverRepository));
            _clientContactRepository = clientContactRepository ??
                                       throw new ArgumentNullException(nameof(clientContactRepository));
            _notificationRepository =
                notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
            _notificationLogRepository = notificationLogRepository ??
                                         throw new ArgumentNullException(nameof(notificationLogRepository));
            _applicationOnlineSignOtpVerificationRepository = applicationOnlineSignOtpVerificationRepository ??
                                                              throw new ArgumentNullException(
                                                                  nameof(
                                                                      applicationOnlineSignOtpVerificationRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public ApplicationOnlineSignOtpVerification SendSmsForSign(Guid applicationOnlineId, string? phoneNumber = null)
        {
            var application = _applicationOnlineRepository.Get(applicationOnlineId);
            string otp = _otpCodeGeneratorService.GenerateRandomOTP(6);
            var template = _templateService.GetTemplate(Constants.APPLICATION_ONLINE_SIGN);
            string verificationMessage = string.Format(template.Message, otp);
            string numberWhereSmsSend = SendSms(verificationMessage, application.ClientId, template.Subject, application.BranchId, phoneNumber);
            ApplicationOnlineSignOtpVerification verification =
                new ApplicationOnlineSignOtpVerification(Guid.NewGuid(), application.Id, otp, 5, 0, numberWhereSmsSend);
            _applicationOnlineSignOtpVerificationRepository.Insert(verification);
            
            return verification;
        }


        public string SendSms(string message, int clientId, string subject, int? branchId, string? phoneNumber = null, int userId = Constants.ADMINISTRATOR_IDENTITY)
        {
            if (branchId == null)
            {
                branchId = 530;//TODO sms branch is TSO or not? 
            }
            ClientContact defaultContact = _clientContactRepository.Find(new { IsDefault = true, ClientId = clientId });
            string numberForSend = !string.IsNullOrEmpty(phoneNumber) ? phoneNumber : defaultContact.Address;

            var notification = new Notification
            {
                BranchId = branchId.Value,
                CreateDate = DateTime.Now,
                IsPrivate = true,
                Message = message,
                Subject = subject,
                IsNonSchedule = true,
                MessageType = MessageType.Sms,
                Status = NotificationStatus.ForSend,
                UserId = userId
            };
            _notificationRepository.Insert(notification);
            var notificationReceiver = new NotificationReceiver
            {
                Address = numberForSend,
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

            try
            {
                SMSInfoTechResponseModel responseModel = new SMSInfoTechResponseModel { message_id = -1 };
                if (_options.SendSmsNotifications)
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

                    using (var transaction = _notificationReceiverRepository.BeginTransaction())
                    {
                        // заменим настоящее содержание смс на замаскированное значение
                        notificationReceiver.Status = status;
                        notificationReceiver.SentAt = DateTime.Now;
                        notificationReceiver.MessageId = messageId;
                        notificationReceiver.TryCount++;
                        notificationReceiver.Address = numberForSend;
                        _notificationReceiverRepository.Update(notificationReceiver);
                        _notificationLogRepository.Insert(new NotificationLog
                        {
                            NotificationReceiverId = messageReceiver.ReceiverId,
                            StatusMessage = statusMessage
                        });

                        _notificationRepository.SyncWithNotificationReceiversStatus(notificationReceiver.NotificationId);
                        transaction.Commit();
                    }

                }
                else
                {
                    status = NotificationStatus.Sent;
                    statusMessage = $"Смс сообщение не отправлено из-за false значения настройки {nameof(_options.SendSmsNotifications)}";
                }

                return numberForSend;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }
        }
    }
}
