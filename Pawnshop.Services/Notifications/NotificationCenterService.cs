using System;
using System.Threading.Tasks;
using KafkaFlow.Producers;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Events;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Data.Models.Notifications.NotificationTemplates;
using Pawnshop.Services.Contracts;
using Serilog;

namespace Pawnshop.Services.Notifications
{
    public sealed class NotificationCenterService : INotificationCenterService
    {
        private const string ContractTopic = "Contract";
        private const string NotificationTopic = "Notification";
        private readonly IProducerAccessor _producers;
        private readonly ILogger _logger;
        private readonly IContractService _contractService;
        private readonly INotificationTemplateService _notificationTemplateService;
        private readonly NotificationReceiverRepository _notificationReceiverRepository;
        private readonly NotificationRepository _notificationRepository;
        private readonly ClientRepository _clientRepository;
        private readonly ContractRepository _contractRepository;
        public NotificationCenterService(IProducerAccessor producers,
            IContractService contractService,
            ILogger logger, ClientRepository clientRepository,
            NotificationRepository notificationRepository,
            NotificationReceiverRepository notificationReceiverRepository,
            INotificationTemplateService notificationTemplateService,
            ContractRepository contractRepository)
        {
            _contractService = contractService;
            _producers = producers;
            _logger = logger;
            _clientRepository = clientRepository;
            _notificationRepository = notificationRepository;
            _notificationReceiverRepository = notificationReceiverRepository;
            _notificationTemplateService = notificationTemplateService;
            _contractRepository = contractRepository;
        }
        public async Task NotifyAboutContractBuyOuted(Contract contract, ContractAction contractAction)
        {
            var client = await _clientRepository.GetOnlyClientAsync(contract.ClientId);
            ContractBuyOuted notificationEvent = new ContractBuyOuted
            {
                BuyOutDate = DateTime.Now,
                ClientId = contract.ClientId,
                ClientIdentityNumber = client.IdentityNumber,
                ContractId = contract.Id,
                ContractNumber = contract.ContractNumber
            };
            try
            {
                await SentToKafka(ContractTopic, notificationEvent, notificationEvent.ContractId);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
            }
        }

        public async Task NotifyAboutSomeContractReadyToBuyOut(int contractWhatCanBeBuyout, ContractAction contractAction)
        {
            var tranche = await _contractRepository.GetOnlyContractAsync(contractWhatCanBeBuyout);
            var client = await _clientRepository.GetOnlyClientAsync(tranche.ClientId);
            try
            {
                SomeContractReadyToBuyOut notificationEvent = new SomeContractReadyToBuyOut
                {
                    ClientId = tranche.ClientId,
                    ClientIdentityNumber = client.IdentityNumber,
                    ContractId = tranche.Id,
                    ContractNumber = tranche.ContractNumber,
                    CreateDate = DateTime.Now
                };

                await SentToKafka(NotificationTopic, notificationEvent, notificationEvent.ContractId);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
            }

            try
            {
                await NotifyBySms(tranche, contractAction, NotificationPaymentType.MoneyEnoughForOneTranche);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
            }
        }

        private async Task SentToKafka<T, TK>(string topic, T message, TK key)
        {
            //JsonConvert.SerializeObject(message)
            await _producers[topic].ProduceAsync(key.ToString(), message);
        }

        private async Task NotifyBySms(Contract contract, ContractAction contractAction, NotificationPaymentType notificationPaymentType)
        {
            Notification smsNotification = SmsNotifications(contract.Id, contract.BranchId, notificationPaymentType);
            smsNotification.ContractActionId = contractAction.Id;
            NotificationPaymentType notificationPayment = smsNotification.NotificationPaymentType.Value;
            Notification existingNotification = _notificationRepository.GetNotificationByActionIdAndType(contractAction.Id, notificationPayment);
            if (existingNotification == null)
            {
                _notificationRepository.Insert(smsNotification);
                NotificationReceiver notificationReceiver = new NotificationReceiver()
                {
                    NotificationId = smsNotification.Id,
                    ClientId = contract.ClientId,
                    CreateDate = DateTime.Now,
                    Status = NotificationStatus.ForSend,
                    ContractId = contract.Id
                };

                _notificationReceiverRepository.Insert(notificationReceiver);
            }
        }

        private Notification SmsNotifications(int contractId, int branchId, NotificationPaymentType type)
        {
            string text = _notificationTemplateService.GetNotificationTextByFilters(contractId, MessageType.Sms, type, -1, -1);
            if (string.IsNullOrWhiteSpace(text))
                throw new PawnshopApplicationException($"Ожидалось что {nameof(_notificationTemplateService)}.{nameof(_notificationTemplateService.GetNotificationTextByFilters)} не вернет пустой текст");

            Notification notification = new Notification()
            {
                BranchId = branchId,
                CreateDate = DateTime.Now,
                Subject = "Уведомление о оплате",
                Message = text,
                MessageType = MessageType.Sms,
                NotificationPaymentType = type,
                Status = NotificationStatus.ForSend,
                IsPrivate = true,
                UserId = Constants.ADMINISTRATOR_IDENTITY,
            };

            return notification;
        }
    }
}
