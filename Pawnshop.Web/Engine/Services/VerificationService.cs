using Microsoft.Extensions.Options;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Options;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Data.Models.Notifications.NotificationTemplates;
using Pawnshop.Data.Models.Verifications;
using Pawnshop.Services.Clients;
using Pawnshop.Services.MessageSenders;
using Pawnshop.Services.Notifications;
using Pawnshop.Services.OTP;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Web.Engine.MessageSenders;
using Pawnshop.Web.Engine.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pawnshop.Web.Engine.Services
{
    public class VerificationService : IVerificationService
    {
        private readonly ClientRepository _clientRepository;
        private readonly VerificationRepository _verificationRepository;
        private readonly BranchContext _branchContext;
        private readonly NotificationReceiverRepository _notificationReceiverRepository;
        private readonly NotificationRepository _notificationRepository;
        private readonly ISessionContext _sessionContext;
        private readonly SmsSender _smsSender;
        private readonly NotificationLogRepository _notificationLogRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ClientContactRepository _clientContactRepository;
        private readonly NotificationTemplateRepository _notificationTemplateRepository;
        private readonly EventLog _eventLog;
        private readonly IDomainService _domainService;
        private const int VEFIFICATION_PERIOD = 90;
        private readonly IClientBlackListService _clientBlackListService;
        private readonly VehicleBlackListRepository _vehicleBlackListRepository;
        private readonly ContractRepository _contractRepository;
        private readonly IClientQuestionnaireService _clientQuestionnaireService;
        private readonly IClientModelValidateService _clientModelValidateService;
        private readonly EnviromentAccessOptions _options;
        private readonly IOTPCodeGeneratorService _otpCodeGeneratorService;
        private readonly INotificationTemplateService _notificationTemplateService;

        public VerificationService(ClientRepository clientRepository, 
            VerificationRepository verificationRepository,
            BranchContext branchContext, 
            ISessionContext sessionContext, 
            SmsSender smsSender,
            NotificationLogRepository notificationLogRepository, 
            IUnitOfWork unitOfWork,
            NotificationReceiverRepository notificationReceiverRepository, 
            NotificationRepository notificationRepository,
            ClientContactRepository clientContactRepository,
            EventLog eventLog, IDomainService domainService,
            IClientBlackListService clientBlackListService,
            NotificationTemplateRepository notificationTemplateRepository,
            VehicleBlackListRepository vehicleBlackListRepository,
            ContractRepository contractRepository,
            IClientQuestionnaireService clientQuestionnaireService,
            IClientModelValidateService clientModelValidateService,
            IOptions<EnviromentAccessOptions> options,
            IOTPCodeGeneratorService otpCodeGeneratorService,
            INotificationTemplateService notificationTemplateService
            )
        {
            _clientRepository = clientRepository;
            _verificationRepository = verificationRepository;
            _branchContext = branchContext;
            _notificationReceiverRepository = notificationReceiverRepository;
            _notificationRepository = notificationRepository;
            _sessionContext = sessionContext;
            _smsSender = smsSender;
            _notificationLogRepository = notificationLogRepository;
            _unitOfWork = unitOfWork;
            _clientContactRepository = clientContactRepository;
            _eventLog = eventLog;
            _domainService = domainService;
            _clientBlackListService = clientBlackListService;
            _notificationTemplateRepository = notificationTemplateRepository;
            _vehicleBlackListRepository = vehicleBlackListRepository;
            _contractRepository = contractRepository;
            _clientQuestionnaireService = clientQuestionnaireService;
            _clientModelValidateService = clientModelValidateService;
            _options = options.Value;
            _otpCodeGeneratorService = otpCodeGeneratorService ??
                                       throw new ArgumentNullException(nameof(otpCodeGeneratorService));
            _notificationTemplateService = notificationTemplateService;
        }

        public (int, DateTime) Get(int clientId, string phoneNumber = null, bool sendToDefaultPhoneNumber = true)
        {
            GetClient(clientId);
            ClientContact defaultContact = GetDefaultContact(clientId, false);
            if (phoneNumber != null)
            {
                ClientContact defaultClientContactFromDB = _clientContactRepository.Find(new { IsDefault = true, Address = phoneNumber });
                //if (defaultClientContactFromDB != null && defaultClientContactFromDB.ClientId != clientId)
                //    throw new PawnshopApplicationException($"Номер {phoneNumber} уже пренадлежит другому клиенту");
            }

            if (defaultContact == null)
            {
                if (phoneNumber == null)
                    throw new PawnshopApplicationException($"Не найден основной номер для верификации или не передан {nameof(phoneNumber)}");
            }
            else if (sendToDefaultPhoneNumber &&
                defaultContact.VerificationExpireDate > DateTime.Now &&
                defaultContact.Address == phoneNumber)
                throw new PawnshopApplicationException($"Основной номер уже верифицирован");

            string sendSmsTo = phoneNumber != null ? phoneNumber : defaultContact.Address;
            // генерируем 6 значный код
            string otp = _otpCodeGeneratorService.GenerateRandomOTP(6);
            _eventLog.Log(EventCode.VerifyRequest, EventStatus.Success, EntityType.Verification, requestData: $"Клиент: {clientId}");
            using (var transaction = _unitOfWork.BeginTransaction())
            {
                var verificationEntity = new Verification
                {
                    Address = sendSmsTo,
                    OTP = otp,
                    ClientId = clientId,
                    ExpireDate = DateTime.Now.AddMinutes(1),
                    MaxTryCount = 5,
                    AuthorId = _sessionContext.UserId
                };

                var template = _notificationTemplateService.GetTemplate(Constants.MANAGER_CODE_APPROVE);
                string verificationMessage = string.Format(template.Message, otp);
                // создадим сообщение вне очереди
                var notification = new Notification
                {
                    BranchId = _branchContext.Branch.Id,
                    CreateDate = DateTime.Now,
                    IsPrivate = true,
                    Message = verificationMessage,
                    Subject = template.Subject ?? string.Empty,
                    IsNonSchedule = true,
                    MessageType = MessageType.Sms,
                    Status = NotificationStatus.ForSend,
                    UserId = _sessionContext.UserId
                };
                _notificationRepository.Insert(notification);
                var notificationReceiver = new NotificationReceiver
                {
                    Address = verificationEntity.Address,
                    ClientId = clientId,
                    NotificationId = notification.Id,
                    TryCount = 0,
                    Status = NotificationStatus.ForSend
                };
                _notificationReceiverRepository.Insert(notificationReceiver);
                _verificationRepository.Insert(verificationEntity);
                var messageReceiver = new MessageReceiver
                {
                    ReceiverAddress = notificationReceiver.Address,
                    ReceiverId = notificationReceiver.Id
                };
                // отправляем смс мне очереди
                _smsSender.Send(notification.Message, notification.Message, new List<MessageReceiver> { messageReceiver }, SenderCallback);
                _eventLog.Log(EventCode.VerifyRequest, EventStatus.Success, EntityType.Verification, requestData: $"Клиент: {clientId}", responseData: "Верификация прошла успешно");
                transaction.Commit();
                return (verificationEntity.Id, verificationEntity.ExpireDate);
            }
        }

        public void Verify(string otp, int clientId)
        {
            if (string.IsNullOrWhiteSpace(otp))
                throw new ArgumentException($"{nameof(otp)} не должен быть пустым");

            Verification verification = _verificationRepository.GetLastVerification(clientId);
            if (verification == null)
                throw new PawnshopApplicationException("Верификация не найдена");

            ClientContact defaultContact = GetDefaultContact(clientId);
            if (defaultContact.VerificationExpireDate > DateTime.Now)
                throw new PawnshopApplicationException("Данный номер уже верифицирован");
            if (defaultContact.Address != verification.Address)
                throw new PawnshopApplicationException("Номер верификации не совпадает с основным номером клиента");
            if (verification.ExpireDate <= DateTime.Now)
                throw new PawnshopApplicationException("Верификация истекла, создайте новую");
            if (verification.ActivationDate.HasValue)
                throw new PawnshopApplicationException("Верификация уже активирована, создайте новую");
            if (verification.TryCount >= verification.MaxTryCount)
                throw new PawnshopApplicationException("Достигнуто максимальное количество попыток ввода кода СМС, создайте новую верификацию");

            verification.TryCount++;
            _verificationRepository.Update(verification);

            var needOTP = _options.SendStandardCode ? "0000" : verification.OTP;
            if (otp != needOTP)
            {
                throw new PawnshopApplicationException("Не совпадает код верификации");
            }

            using (var transaction = _unitOfWork.BeginTransaction())
            {
                verification.ActivationDate = DateTime.Now;
                _verificationRepository.Update(verification);
                defaultContact.VerificationExpireDate = DateTime.Now.AddDays(VEFIFICATION_PERIOD);
                _clientContactRepository.Update(defaultContact);
                transaction.Commit();
            }
        }

        public bool DoNeedVerification(int clientId, int? contractId = null)
        {
            if (contractId.HasValue)
                _vehicleBlackListRepository.CheckContractPositionsInBlackList(contractId.Value);

            var needVerification = _clientBlackListService.CheckClientIsInBlackList(clientId, ContractActionType.Sign, contractId);

            if (needVerification)
            {
                ClientContact defaultContact = GetDefaultContact(clientId);
                if (!(defaultContact.VerificationExpireDate >= DateTime.Now))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Получить основной контакт
        /// </summary>
        /// <param name="clientId">Идентификатор клиента</param>
        /// <param name="throwExceptionOnNotFound">Вызвать исключение при отсутствии основого контакта</param>
        /// <returns></returns>
        public ClientContact GetDefaultContact(int clientId, bool throwExceptionOnNotFound = true)
        {
            var client = GetClient(clientId);
            ClientContact defaultNumber = _clientContactRepository.Find(new { IsDefault = true, ClientId = clientId });
            if (defaultNumber == null)
            {
                if (throwExceptionOnNotFound)
                    throw new PawnshopApplicationException($"У клиента {client.FullName} нет основного номера");
            }

            return defaultNumber;
        }

        /// <summary>
        /// Получает клиента, при отсутствии клиента в базе отдает исключение
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        private Client GetClient(int clientId)
        {
            Client client = _clientRepository.GetOnlyClient(clientId);
            if (client == null)
                throw new PawnshopApplicationException($"Клиент {clientId} не найден");

            return client;
        }


        private void SenderCallback(SendResult sendResult)
        {
            if (sendResult == null)
                throw new ArgumentNullException(nameof(sendResult));

            var receiver = _notificationReceiverRepository.Get(sendResult.ReceiverId);
            if (receiver == null) throw new InvalidOperationException();

            Notification notification = _notificationRepository.Get(receiver.NotificationId);
            if (notification == null) throw new InvalidOperationException();

            using (var transaction = _notificationReceiverRepository.BeginTransaction())
            {
                // заменим настоящее содержание смс на замаскированное значение
                notification.Message = Regex.Replace(notification.Message, @"\d", "*");
                _notificationRepository.Update(notification);
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

        public void CheckVerification(int contractId)
        {
            Contract contract = _contractRepository.GetContractWithSubject(contractId);
            if (contract == null)
                throw new PawnshopApplicationException($"Договор {contractId} не найден");

            bool needVerification = DoNeedVerification(contract.ClientId);
            if (needVerification)
                throw new PawnshopApplicationException($"Клиенту {contract.Client.FullName} необходимо пройти верификацию основного номера");

            if (contract.Subjects.Any())
                contract.Subjects.ForEach(sub =>
                {
                    if (sub.Subject.Code == Constants.COBORROWER_CODE || sub.Subject.Code == Constants.GUARANTOR_CODE)
                    {
                        bool needVerification = DoNeedVerification(sub.ClientId);

                        if (needVerification)
                            throw new PawnshopApplicationException($"Клиенту {sub.Client.FullName} необходимо пройти верификацию основного номера");
                    }
                });
        }

        public void CheckClientQuestionnaireFilledStatus(int contractId, Contract contract = null)
        {
            contract ??= _contractRepository.GetContractWithSubject(contractId);

            if (contract == null)
                throw new PawnshopApplicationException($"Договор {contractId} не найден");

            bool filled = _clientQuestionnaireService.IsClientHasFilledQuestionnaire(contract.ClientId);
            if (!filled)
                throw new PawnshopApplicationException($"У клиента {contract.Client.FullName} не заполнена анкета");

            if (contract.Subjects != null && contract.Subjects.Any())
            {
                contract.Subjects.ForEach(sub =>
                {
                    if (sub.Subject.Code == Constants.COBORROWER_CODE || sub.Subject.Code == Constants.GUARANTOR_CODE)
                    {
                        sub.Client = _clientRepository.Get(sub.ClientId);
                        _clientModelValidateService.ValidateClientModel(sub.Client);

                        bool filledSubjects = _clientQuestionnaireService.IsClientHasFilledQuestionnaire(sub.ClientId);

                        if (!filledSubjects)
                            throw new PawnshopApplicationException(
                                $"У клиента {sub.Client.FullName} не заполнена анкета");
                    }
                });
            }
        }
    }
}
