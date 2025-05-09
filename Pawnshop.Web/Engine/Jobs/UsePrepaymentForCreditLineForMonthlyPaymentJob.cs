using Hangfire;
using Microsoft.Extensions.Options;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Options;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using Pawnshop.Web.Engine.Audit;
using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.InnerNotifications;
using Pawnshop.Data.Models.Notifications;
using Newtonsoft.Json;
using Pawnshop.Data.Models.Notifications.NotificationTemplates;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.CreditLines;
using Pawnshop.Services.CreditLines.Buyout;
using Pawnshop.Services.CreditLines.Payment;
using Pawnshop.Services.Integrations.UKassa;
using Pawnshop.Services.MessageSenders;
using Contract = Pawnshop.Data.Models.Contracts.Contract;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Web.Engine.MessageSenders;
using Pawnshop.Services.Notifications;
using ILogger = Serilog.ILogger;
using Pawnshop.Services.ClientDeferments.Interfaces;

namespace Pawnshop.Web.Engine.Jobs
{
    public class UsePrepaymentForCreditLineForMonthlyPaymentJob
    {
        private readonly JobLog _jobLog;
        private readonly EnviromentAccessOptions _options;
        private readonly IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> _accountSettingService;
        private readonly ContractRepository _contractRepository;
        private readonly PayTypeRepository _payTypeRepository;
        private readonly EventLog _eventLog;
        private readonly ContractActionRepository _contractActionRepository;
        private readonly GroupRepository _groupRepository;
        private readonly OrganizationRepository _organizationRepository;
        private readonly INotificationTemplateService _notificationTemplateService;
        private readonly IInnerNotificationService _innerNotificationService;
        private readonly InnerNotificationRepository _innerNotificationRepository;
        private readonly NotificationRepository _notificationRepository;
        private readonly NotificationReceiverRepository _notificationReceiverRepository;
        private readonly NotificationTemplateRepository _notificationTemplateRepository;
        private readonly EmailSender _emailSender;
        private readonly ICreditLineService _creditLineService;
        private readonly ICreditLinePaymentService _creditLinePaymentService;
        private readonly ICreditLinesBuyoutService _creditLinesBuyoutService;
        private readonly IDomainService _domainService;
        private readonly ICashOrderService _cashOrderService;
        private readonly IUKassaService _uKassaService;
        private readonly FunctionSettingRepository _functionSettingRepository;
        private readonly INotificationCenterService _notificationCenter;
        private readonly ILogger<UsePrepaymentForCreditLineForMonthlyPaymentJob> _logger;
        private readonly IClientDefermentService _clientDefermentService;

        public UsePrepaymentForCreditLineForMonthlyPaymentJob(
            JobLog jobLog,
            IOptions<EnviromentAccessOptions> options,
            IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> accountSettingService,
            ContractRepository contractRepository,
            PayTypeRepository payTypeRepository,
            EventLog eventLog,
            ContractActionRepository contractActionRepository,
            GroupRepository groupRepository,
            OrganizationRepository organizationRepository,
            INotificationTemplateService notificationTemplateService,
            IInnerNotificationService innerNotificationService,
            InnerNotificationRepository innerNotificationRepository,
            NotificationRepository notificationRepository,
            NotificationReceiverRepository notificationReceiverRepository,
            NotificationTemplateRepository notificationTemplateRepository,
            EmailSender emailSender,
            ICreditLineService creditLineService,
            ICreditLinePaymentService creditLinePaymentService,
            ICreditLinesBuyoutService creditLinesBuyoutService,
            IDomainService domainService,
            ICashOrderService cashOrderService,
            IUKassaService uKassaService,
            FunctionSettingRepository functionSettingRepository,
            INotificationCenterService notificationCenter,
            ILogger<UsePrepaymentForCreditLineForMonthlyPaymentJob> logger,
            IClientDefermentService clientDefermentService)
        {
            _jobLog = jobLog;
            _options = options.Value;
            _accountSettingService = accountSettingService;
            _contractRepository = contractRepository;
            _payTypeRepository = payTypeRepository;
            _eventLog = eventLog;
            _contractActionRepository = contractActionRepository;
            _groupRepository = groupRepository;
            _organizationRepository = organizationRepository;
            _notificationTemplateService = notificationTemplateService;
            _innerNotificationService = innerNotificationService;
            _innerNotificationRepository = innerNotificationRepository;
            _notificationRepository = notificationRepository;
            _notificationReceiverRepository = notificationReceiverRepository;
            _notificationTemplateRepository = notificationTemplateRepository;
            _emailSender = emailSender;
            _creditLineService = creditLineService;
            _creditLinePaymentService = creditLinePaymentService;
            _creditLinesBuyoutService = creditLinesBuyoutService;
            _domainService = domainService;
            _cashOrderService = cashOrderService;
            _uKassaService = uKassaService;
            _functionSettingRepository = functionSettingRepository;
            _notificationCenter = notificationCenter;
            _logger = logger;
            _clientDefermentService = clientDefermentService;
        }

        [Queue("payments")]
        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        public void Execute()
        {
            _jobLog.Log("UsePrepaymentForCreditLineForMonthlyPaymentJob", JobCode.Start, JobStatus.Success);
            if (!_options.SchedulePayments)
            {
                _jobLog.Log("UsePrepaymentForCreditLineForMonthlyPaymentJob", JobCode.Cancel, JobStatus.Success);
                return;
            }

            var depoMasteringSetting = _functionSettingRepository.GetByCode(Constants.FUNCTION_SETTING__DEPO_MASTERING);
            if (depoMasteringSetting != null && depoMasteringSetting.BooleanValue.HasValue && depoMasteringSetting.BooleanValue.Value)
            {
                _jobLog.Log("UsePrepaymentForCreditLineForMonthlyPaymentJob", JobCode.Cancel, JobStatus.Success);
                return;
            }

            ListModel<AccountSetting> depoAccountSettings = _accountSettingService.List(new ListQueryModel<AccountSettingFilter>
            {
                Page = null,
                Model = new AccountSettingFilter
                {
                    Code = Constants.ACCOUNT_SETTING_DEPO
                }
            });
            if (depoAccountSettings.List.Count == 0)
                throw new PawnshopApplicationException($"Настройка счета {Constants.ACCOUNT_SETTING_DEPO} не найдена");

            var depoAccountSettingsList = depoAccountSettings.List.Select(x => x.Id).ToList();

            var creditLinesIds = _contractRepository.GetCreditLinesWhatCanPaySomethingNow();
            var crediLinesIdsForBuyOut = _contractRepository.GetCreditLinesWhatCanBuyOutAllTranchesNow();
            creditLinesIds.AddRange(crediLinesIdsForBuyOut);
            if (creditLinesIds.Count == 0)
            {
                _jobLog.Log("UsePrepaymentForCreditLineForMonthlyPaymentJob", JobCode.End, JobStatus.Success);
                return;
            }

            PayType defaultPayType = _payTypeRepository.Find(new { IsDefault = true });
            if (defaultPayType == null)
                throw new InvalidOperationException($"не найден дефолтный pay type");
            foreach (var id in creditLinesIds)
            {
                BackgroundJob.Enqueue<UsePrepaymentForCreditLineForMonthlyPaymentJob>(
                    x => x.UsePrepaymentForCreditLine(id, defaultPayType.Id));
                //UsePrepaymentForCreditLine(id, defaultPayType.Id);
            }
        }

        [Queue("payments")]
        public void UsePrepaymentForCreditLine(int creditLineId, int payTypeId)
        {
            if (!_options.SchedulePayments)
            {
                _jobLog.Log("UsePrepaymentForCreditLineForMonthlyPaymentJob", JobCode.Cancel, JobStatus.Success, entityType: EntityType.Contract, entityId: creditLineId);
                return;
            }

            DateTime now = DateTime.Now;
            if (now.TimeOfDay >= Constants.STOP_ONLINE_PAYMENTS || now.TimeOfDay < Constants.START_ONLINE_PAYMENTS)
            {
                _jobLog.Log("UsePrepaymentForCreditLineForMonthlyPaymentJob", JobCode.Cancel, JobStatus.Success, entityType: EntityType.Contract, entityId: creditLineId);
                return;
                //throw new PawnshopApplicationException("Время освоения онлайн платежей должно быть между 10:00:00 и 22:31:00");
            }

            var depoMasteringSetting = _functionSettingRepository.GetByCode(Constants.FUNCTION_SETTING__DEPO_MASTERING);
            if (depoMasteringSetting != null && depoMasteringSetting.BooleanValue.HasValue && depoMasteringSetting.BooleanValue.Value)
            {
                _jobLog.Log("UsePrepaymentForCreditLineForMonthlyPaymentJob", JobCode.Cancel, JobStatus.Success, entityType: EntityType.Contract, entityId: creditLineId);
                return;
            }

            Contract creditLine = null;
            try
            {
                creditLine = _contractRepository.Get(creditLineId);
                if (creditLine == null)
                    throw new PawnshopApplicationException($"Договор {creditLineId} не найден");

                var creditLineDeferments = _clientDefermentService.GetCreditLineDefermentInformation(creditLineId, now);

                if (creditLineDeferments != null && creditLineDeferments.Any(x => x.IsInDefermentPeriod || x.Status == Data.Models.Restructuring.RestructuringStatusEnum.Frozen))
                {
                    _eventLog.Log(EventCode.ContractMonthlyPayment, EventStatus.Failed, EntityType.Contract, creditLine.Id, String.Empty, $@"Отмена автоматического освоения аванса по договору {creditLine.ContractNumber}({creditLine.Id}) (договор находится в период отсрочки)", userId: Constants.ADMINISTRATOR_IDENTITY);
                    return;
                }

                if (creditLine.Status != ContractStatus.Signed)
                {
                    _eventLog.Log(EventCode.ContractMonthlyPayment, EventStatus.Failed, EntityType.Contract, creditLine.Id, String.Empty, $@"Отмена автоматического освоения аванса по договору {creditLine.ContractNumber}({creditLine.Id}) (договор не подписан)", userId: Constants.ADMINISTRATOR_IDENTITY);
                    return;
                }
                InnerNotification innerNotification = null;
                Notification smsNotification = null;
                ContractAction lastPrepaymentAction = _contractActionRepository.GetLastContractActionByType(creditLineId, ContractActionType.Prepayment); // ??

                var branch = _groupRepository.Get(creditLine.BranchId);
                var organization = _organizationRepository.Get(branch.OrganizationId);
                var distribution = _creditLineService.GetCurrentlyDebtForCreditLine(creditLineId).Result;
                var paymentAmount = distribution.SummaryPrepaymentBalance;
                if (paymentAmount == 0)//нечем платить
                {
                    return;
                }

                int? actionId = null;
                if (distribution.SummaryPrepaymentBalance >= distribution.SummaryTotalRedemptionAmount)
                {
                    actionId = _creditLinesBuyoutService.TransferPrepaymentAndBuyBack(creditLineId,
                        Constants.ADMINISTRATOR_IDENTITY, payTypeId, branch.Id,
                        distribution.ContractsBalances.Where(cb => cb.ContractId != creditLineId)
                            .Select(cb => cb.ContractId).ToList(),
                        _domainService.GetDomainValue(Constants.BUYOUT_REASON_CODE, Constants.BUYOUT_AUTOMATIC_BUYOUT)
                            .Id, buyoutCreditLine: false, DateTime.Now, autoApprove: true).Result;
                }
                else
                {
                    actionId = _creditLinePaymentService.TransferPrepaymentAndPayment(creditLineId,
                        Constants.ADMINISTRATOR_IDENTITY, payTypeId, branch.Id, autoApprove: true).Result;
                }

                Contract contractAfterPayment = _contractRepository.GetOnlyContract(creditLine.Id);
                distribution = _creditLineService.GetCurrentlyDebtForCreditLine(creditLineId).Result;
                if (contractAfterPayment == null)
                    throw new PawnshopApplicationException($"Договор {creditLine.Id} не найден");

                bool allTranchesBoughtOut = true;
                var tranchesid = _contractRepository.GetTrancheIdsByCreditLine(creditLineId).Result;
                for (int i = 0; i < tranchesid.Count; i++)
                {
                    var contract = _contractRepository.GetOnlyContract(tranchesid[i]);
                    if (contract != null)
                    {
                        if (contract.Status != ContractStatus.BoughtOut)
                        {
                            allTranchesBoughtOut = false;
                            break;
                        }
                    }
                }
                if (allTranchesBoughtOut)
                {
                    NotificationPaymentType notificationPaymentType = NotificationPaymentType.Buyout;
                    innerNotification = _innerNotificationService.CreateNotification(creditLine.Id, notificationPaymentType);
                    try
                    {
                        _notificationCenter.NotifyAboutContractBuyOuted(creditLine, lastPrepaymentAction).Wait();
                    }
                    catch (Exception exception)
                    {
                        _logger.LogError(exception, exception.Message);
                    }
                }
                else
                {
                    if (distribution.SummaryExpenseAmount > 0)
                    {
                        var notificationPaymentType = NotificationPaymentType.NotEnoughForExpense;
                        decimal notEnoughAmount = distribution.SummaryRepaymentAccountAmount;
                        smsNotification = SmsNotifications(creditLine, notificationPaymentType, failPaymentCost: notEnoughAmount);
                        innerNotification = _innerNotificationService.CreateNotification(creditLine.Id, notificationPaymentType, notEnoughAmount);
                    }
                    else if (distribution.SummaryCurrentDebt > 0)
                    {
                        var notificationPaymentType = NotificationPaymentType.PartialMonthlyPayment;
                        decimal notEnoughAmount = distribution.SummaryCurrentDebt;
                        smsNotification = SmsNotifications(creditLine, notificationPaymentType, paymentAmount, failPaymentCost: notEnoughAmount); ;
                        innerNotification = _innerNotificationService.CreateNotification(creditLine.Id, notificationPaymentType, notEnoughAmount);
                    }
                    else if (distribution.SummaryRepaymentAccountAmount == 0)
                    {
                        var notificationPaymentType = NotificationPaymentType.MonthlyPayment;
                        innerNotification = _innerNotificationService.CreateNotification(creditLineId, notificationPaymentType);
                    }
                    else
                    {
                        throw new PawnshopApplicationException($"{distribution.SummaryRepaymentAccountAmount}.{nameof(distribution.SummaryRepaymentAccountAmount)} не должен быть меньше нуля");
                    }
                }

                if (innerNotification != null)
                    _innerNotificationRepository.Insert(innerNotification);

                if (smsNotification != null && lastPrepaymentAction != null && lastPrepaymentAction.ProcessingId.HasValue)
                {
                    if (!smsNotification.NotificationPaymentType.HasValue)
                        _jobLog.Log("UsePrepaymentForCreditLineForMonthlyPaymentJob", JobCode.End, JobStatus.Failed, entityType: EntityType.Contract, entityId: creditLineId, responseData: $"{nameof(smsNotification)} не должен содержать пустой {nameof(smsNotification.NotificationPaymentType)}");
                    else
                    {
                        smsNotification.ContractActionId = lastPrepaymentAction.Id;
                        NotificationPaymentType notificationPayment = smsNotification.NotificationPaymentType.Value;
                        Notification existingNotification = _notificationRepository.GetNotificationByActionIdAndType(lastPrepaymentAction.Id, notificationPayment);
                        if (existingNotification == null)
                        {
                            _notificationRepository.Insert(smsNotification);
                            NotificationReceiver notificationReceiver = new NotificationReceiver()
                            {
                                NotificationId = smsNotification.Id,
                                ClientId = creditLine.ClientId,
                                CreateDate = DateTime.Now,
                                Status = NotificationStatus.ForSend,
                                ContractId = creditLine.Id
                            };

                            _notificationReceiverRepository.Insert(notificationReceiver);
                        }
                    }
                }
                _jobLog.Log("UsePrepaymentForCreditLineForMonthlyPaymentJob", JobCode.End, JobStatus.Success, EntityType.Contract, creditLine.Id, responseData: $"");
                if (actionId != null)
                {
                    var orderIds = _cashOrderService.GetAllRelatedOrdersByContractActionId(actionId.Value).Result;
                    if (orderIds.Any())
                    {
                        _uKassaService.FinishRequests(orderIds);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                _jobLog.Log("UsePrepaymentForCreditLineForMonthlyPaymentJob", JobCode.Error, JobStatus.Failed, EntityType.Contract, creditLineId, responseData: JsonConvert.SerializeObject(e));
                FailureInnerNotification(creditLine);
                EmailProcessingNotifications(creditLine);
                _eventLog.Log(EventCode.ContractMonthlyPayment, EventStatus.Failed, EntityType.Contract, creditLine.Id, string.Empty,
                    $@"При автоматическом освоении аванса произошла ошибка по договору {creditLine.ContractNumber}({creditLine.Id}) ({e.Message})",
                    userId: Constants.ADMINISTRATOR_IDENTITY);
            }
        }


        /// <summary>
        /// Смс уведомления для клиента
        /// </summary>
        private Notification SmsNotifications(Contract contract, NotificationPaymentType type, decimal successPaymentCost = -1, decimal failPaymentCost = -1)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            string text = _notificationTemplateService.GetNotificationTextByFilters(contract.Id, MessageType.Sms, type, successPaymentCost, failPaymentCost);
            if (string.IsNullOrWhiteSpace(text))
                throw new PawnshopApplicationException($"Ожидалось что {nameof(_notificationTemplateService)}.{nameof(_notificationTemplateService.GetNotificationTextByFilters)} не вернет пустой текст");

            Notification notification = new Notification()
            {
                BranchId = contract.BranchId,
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


        private InnerNotification FailureInnerNotification(Contract contract)
        {
            string message = $@"Автоматическое освоение аванса по договору займа №{contract.ContractNumber} не произошло. Имеется аванс {(int)contract.ContractData.PrepaymentCost} KZT. Свяжитесь с клиентом для уточнения!";
            var innerNotification = new InnerNotification()
            {
                Message = message,
                CreateDate = DateTime.Now,
                CreatedBy = 1,
                ReceiveBranchId = contract.BranchId,
                EntityType = EntityType.Contract,
                EntityId = contract.Id,
                Status = InnerNotificationStatus.Sent
            };
            using (var transaction = _innerNotificationRepository.BeginTransaction())
            {
                _innerNotificationRepository.Insert(innerNotification);
                transaction.Commit();
            }
            return innerNotification;
        }

        private string EmailProcessingNotifications(Contract сontract)
        {
            var messageForFront = $@"ОШИБКА! Автоматическом освоение аванса";
            var message = $@"<p style=""text-align: center;""><strong>ОШИБКА! Автоматическом освоение аванса</strong></p>
                        <p><strong>ContractId = {сontract.Id.ToString()}</strong></p>";

            var messageReceiver = new MessageReceiver
            {
                ReceiverAddress = _options.ErrorNotifierAddress,
                ReceiverName = _options.ErrorNotifierName
            };

            _emailSender.SendEmail("Ошибка в автоматическом освоение аванса", message, messageReceiver);

            return messageForFront;
        }
    }
}
