using Hangfire;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Core.Options;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.InnerNotifications;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Web.Engine.MessageSenders;
using System;
using System.Collections.Generic;
using System.Linq;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Services.Contracts;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Services;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using System.Data;
using Pawnshop.Services.Calculation;
using Pawnshop.Services.Models.Calculation;
using Pawnshop.Data.Models.Notifications.NotificationTemplates;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Services.Integrations.UKassa;
using Pawnshop.Services.Collection;
using Pawnshop.Data.Models.Collection;
using Pawnshop.Services.MessageSenders;
using Pawnshop.Services.LegalCollection.Inerfaces;
using Pawnshop.Services.Notifications;
using Serilog;
using Pawnshop.Services.ClientDeferments.Interfaces;

namespace Pawnshop.Web.Engine.Jobs
{
    public class UsePrepaymentForEndPeriodContractsJob
    {
        private readonly EventLog _eventLog;
        private readonly ContractRepository _contractRepository;
        private readonly InnerNotificationRepository _innerNotificationRepository;
        private readonly EmailSender _emailSender;
        private readonly EnviromentAccessOptions _options;
        private readonly JobLog _jobLog;
        private readonly PayTypeRepository _payTypeRepository;
        private readonly ContractActionRepository _contractActionRepository;
        private readonly IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> _accountSettingService;
        private readonly IContractDutyService _contractDutyService;
        private readonly IContractService _contractService;
        private readonly IContractActionProlongService _contractActionProlongService;
        private readonly IContractPaymentService _contractPaymentService;
        private readonly NotificationTemplateRepository _notificationTemplateRepository;
        private readonly NotificationReceiverRepository _notificationReceiverRepository;
        private readonly NotificationRepository _notificationRepository;
        private readonly INotificationTemplateService _notificationTemplateService;
        private readonly IInnerNotificationService _innerNotificationService;
        private readonly IContractActionBuyoutService _contractActionBuyoutService;
        private readonly IContractAmount _contractAmount;
        private readonly IUKassaService _uKassaService;
        private readonly ICashOrderService _cashOrderService;
        private readonly IDomainService _domainService;
        private readonly ICollectionService _collectionService;
        private readonly FunctionSettingRepository _functionSettingRepository;
        private readonly ILegalCollectionCloseService _closeLegalCaseService;
        private readonly INotificationCenterService _notificationCenter;
        private readonly ILogger _logger;
        private readonly IClientDefermentService _clientDefermentService;

        public UsePrepaymentForEndPeriodContractsJob(EventLog eventLog, ContractRepository contractRepository, InnerNotificationRepository innerNotificationRepository,
                 EmailSender emailSender, IOptions<EnviromentAccessOptions> options, JobLog jobLog,
                 PayTypeRepository payTypeRepository, INotificationTemplateService notificationTemplateService,
                 IContractService contractService, IContractActionProlongService contractActionProlongService,
                 IContractPaymentService contractPaymentService, ContractActionRepository contractActionRepository,
                 IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> accountSettingService,
                 IContractDutyService contractDutyService, NotificationTemplateRepository notificationTemplateRepository,
                 NotificationReceiverRepository notificationReceiverRepository, NotificationRepository notificationRepository,
                 IInnerNotificationService innerNotificationService, IContractActionBuyoutService contractActionBuyoutService,
                 IContractAmount contractAmount, IUKassaService uKassaService, ICashOrderService cashOrderService, IDomainService domainService,
                 ICollectionService collectionService,
                 FunctionSettingRepository functionSettingRepository,
                 ILegalCollectionCloseService closeLegalCaseService,
                 INotificationCenterService notificationCenter,
                 ILogger logger,
                 IClientDefermentService clientDefermentService
                 )
        {
            _eventLog = eventLog;
            _contractRepository = contractRepository;
            _innerNotificationRepository = innerNotificationRepository;
            _emailSender = emailSender;
            _options = options.Value;
            _jobLog = jobLog;
            _payTypeRepository = payTypeRepository;
            _contractService = contractService;
            _contractActionProlongService = contractActionProlongService;
            _contractPaymentService = contractPaymentService;
            _accountSettingService = accountSettingService;
            _contractActionRepository = contractActionRepository;
            _contractDutyService = contractDutyService;
            _notificationTemplateRepository = notificationTemplateRepository;
            _notificationReceiverRepository = notificationReceiverRepository;
            _notificationRepository = notificationRepository;
            _notificationTemplateService = notificationTemplateService;
            _innerNotificationService = innerNotificationService;
            _contractActionBuyoutService = contractActionBuyoutService;
            _contractAmount = contractAmount;
            _uKassaService = uKassaService;
            _cashOrderService = cashOrderService;
            _domainService = domainService;
            _collectionService = collectionService;
            _functionSettingRepository = functionSettingRepository;
            _closeLegalCaseService = closeLegalCaseService;
            _notificationCenter = notificationCenter;
            _logger = logger;
            _clientDefermentService = clientDefermentService;
        }

        /// <summary>
        /// Делает ежемесячное погашение по договору если хватает денег на авансе
        /// </summary>
        [Queue("payments")]
        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        public void Execute()
        {
            _jobLog.Log("UsePrepaymentForEndPeriodContractsJob", JobCode.Start, JobStatus.Success);
            if (!_options.SchedulePayments)
            {
                _jobLog.Log("UsePrepaymentForEndPeriodContractsJob", JobCode.Cancel, JobStatus.Success);
                return;
            }

            var depoMasteringSetting = _functionSettingRepository.GetByCode(Constants.FUNCTION_SETTING__DEPO_MASTERING);
            if (depoMasteringSetting != null && depoMasteringSetting.BooleanValue.HasValue && depoMasteringSetting.BooleanValue.Value)
            {
                _jobLog.Log("UsePrepaymentForEndPeriodContractsJob", JobCode.Cancel, JobStatus.Success);
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
            var contracts = _contractRepository.ListWithPrepaymentForTodayForEndPeriod(depoAccountSettingsList);
            PayType defaultPayType = _payTypeRepository.Find(new { IsDefault = true });
            if (defaultPayType == null)
                throw new InvalidOperationException($"не найден дефолтный pay type");

            if (contracts == null)
            {
                _jobLog.Log("UsePrepaymentForEndPeriodContractsJob", JobCode.Error, JobStatus.Failed, requestData: $"Ожидалось что {nameof(contracts)} не будет null");
                return;
            }

            _jobLog.Log("UsePrepaymentForEndPeriodContractsJob", JobCode.End, JobStatus.Success, requestData: $"Получено {contracts.Count} договоров");
            foreach (Contract contract in contracts)
            {
                decimal contractPrepaymentBalance = _contractService.GetPrepaymentBalance(contract.Id);
                if (contractPrepaymentBalance == 0)
                    continue;

                //ProlongContractByPrepayment(contract.Id, defaultPayType.Id);
                BackgroundJob.Enqueue<UsePrepaymentForEndPeriodContractsJob>(x => x.ProlongContractByPrepayment(contract.Id, defaultPayType.Id));
            }
        }

        [Queue("payments")]
        public void ProlongContractByPrepayment(int contractId, int payTypeId)
        {
            if (!_options.SchedulePayments)
            {
                _jobLog.Log("UsePrepaymentForEndPeriodContractsJob", JobCode.Cancel, JobStatus.Success, entityType: EntityType.Contract, entityId: contractId);
                return;
            }

            DateTime now = DateTime.Now;
            if (now.TimeOfDay >= Constants.STOP_ONLINE_PAYMENTS || now.TimeOfDay < Constants.START_ONLINE_PAYMENTS)
            {
                _jobLog.Log("UsePrepaymentForEndPeriodContractsJob", JobCode.Cancel, JobStatus.Success, entityType: EntityType.Contract, entityId: contractId);
                return;
                //throw new PawnshopApplicationException("Время освоения онлайн платежей должно быть между 10:00:00 и 22:29:59");
            }

            var depoMasteringSetting = _functionSettingRepository.GetByCode(Constants.FUNCTION_SETTING__DEPO_MASTERING);
            if (depoMasteringSetting != null && depoMasteringSetting.BooleanValue.HasValue && depoMasteringSetting.BooleanValue.Value)
            {
                _jobLog.Log("UsePrepaymentForEndPeriodContractsJob", JobCode.Cancel, JobStatus.Success, entityType: EntityType.Contract, entityId: contractId);
                return;
            }

            _jobLog.Log("UsePrepaymentForEndPeriodContractsJob", JobCode.Start, JobStatus.Success, entityType: EntityType.Contract, entityId: contractId);
            var contract = _contractRepository.Get(contractId);
            if (contract == null)
            {
                _eventLog.Log(EventCode.ContractProlong, EventStatus.Failed, EntityType.Contract, contract.Id, string.Empty, $@"Отмена автоматического освоения аванса по договору с ID - {contractId} (договор не найден)", userId: Constants.ADMINISTRATOR_IDENTITY);
                return;
            }

            var defermentInformation = _clientDefermentService.GetDefermentInformation(contractId, now);

            if (defermentInformation != null && (defermentInformation.Status == Data.Models.Restructuring.RestructuringStatusEnum.Frozen || defermentInformation.IsInDefermentPeriod))
            {
                _eventLog.Log(EventCode.ContractMonthlyPayment, EventStatus.Failed, EntityType.Contract, contract.Id, String.Empty, $@"Отмена автоматического освоения аванса по договору {contract.ContractNumber}({contract.Id}) (договор находится в период отсрочки)", userId: Constants.ADMINISTRATOR_IDENTITY);
                return;
            }

            InnerNotification innerNotification = null;
            Notification smsNotification = null;
            ContractAction lastPrepaymentAction = _contractActionRepository.GetLastContractActionByType(contract.Id, ContractActionType.Prepayment);
            if (!contract.NextPaymentDate.HasValue)
                throw new PawnshopApplicationException($"Следующая дата платежа не должен быть пустым у договора {contract.Id}");

            if (contract.LoanPeriod <= 0)
                throw new PawnshopApplicationException($"Период договора {contract.Id} должен быть положительным числом");

            if (contract.Status != ContractStatus.Signed)
            {
                _eventLog.Log(EventCode.ContractProlong, EventStatus.Failed, EntityType.Contract, contract.Id, string.Empty, $@"Отмена автоматического освоения аванса по договору {contract.ContractNumber}({contract.Id}) (договор не подписан)", userId: Constants.ADMINISTRATOR_IDENTITY);
                return;
            }

            if (contract.InscriptionId > 0 && contract.Inscription.Status != InscriptionStatus.Denied)
            {
                _eventLog.Log(EventCode.ContractProlong, EventStatus.Failed, EntityType.Contract, contract.Id, string.Empty, $@"Отмена автоматического освоения аванса по договору {contract.ContractNumber}({contract.Id}) (договор передан в ЧСИ)", userId: Constants.ADMINISTRATOR_IDENTITY);
                return;
            }

            try
            {
                ContractAction action = null;
                using (IDbTransaction transaction = _contractActionRepository.BeginTransaction())
                {
                    decimal depoBalance = _contractService.GetPrepaymentBalance(contract.Id);
                    if (depoBalance == 0)
                        return;

                    var buyoutContractDutyCheckModel = new ContractDutyCheckModel
                    {
                        ActionType = contract.ContractClass == ContractClass.Credit && contract.IsContractRestructured ? ContractActionType.BuyoutRestructuringCred : ContractActionType.Buyout,
                        ContractId = contract.Id,
                        Date = DateTime.Now,
                        PayTypeId = payTypeId
                    };
                    ContractDuty buyoutContractDuty = _contractDutyService.GetContractDuty(buyoutContractDutyCheckModel);
                    if (buyoutContractDuty == null)
                        throw new PawnshopApplicationException($"Ожидалось что {nameof(_contractDutyService)}.{nameof(_contractDutyService.GetContractDuty)} не вернет null, по действию выкупу");
                    else if (buyoutContractDuty.Rows == null)
                        throw new PawnshopApplicationException($"Ожидалось что {nameof(buyoutContractDuty)}.{nameof(buyoutContractDuty.Rows)} не будет null");

                    if (depoBalance >= buyoutContractDuty.Cost)
                    {
                        DateTime contractDate = contract.ContractDate;
                        string buyoutReason = string.Format(Constants.REASON_AUTO_BUYOUT, contract.ContractNumber, contractDate.ToString("dd.MM.yyyy"));
                        int buyoutReasonId = _domainService.GetDomainValue(Constants.BUYOUT_REASON_CODE, Constants.BUYOUT_AUTOMATIC_BUYOUT).Id;
                        List<ContractActionRow> buyoutActionRows = buyoutContractDuty.Rows.ToList();
                        var buyoutAmountsDict = new Dictionary<AmountType, decimal>();
                        foreach (ContractActionRow row in buyoutActionRows)
                        {
                            decimal amount;
                            if (buyoutAmountsDict.TryGetValue(row.PaymentType, out amount))
                            {
                                if (row.Cost != amount)
                                    throw new PawnshopApplicationException("Проводки по одинаковым типам не совпадают по сумме");
                            }

                            buyoutAmountsDict[row.PaymentType] = row.Cost;
                        }

                        decimal buyoutActionRowsCost = buyoutAmountsDict.Count > 0 ? buyoutAmountsDict.Values.Sum() : 0;
                        var buyoutContractAction = new ContractAction
                        {
                            ActionType = contract.ContractClass == ContractClass.Credit && contract.IsContractRestructured ? ContractActionType.BuyoutRestructuringCred : ContractActionType.Buyout,
                            ContractId = contract.Id,
                            CreateDate = DateTime.Now,
                            Date = buyoutContractDuty.Date,
                            AuthorId = Constants.ADMINISTRATOR_IDENTITY,
                            ExtraExpensesCost = buyoutContractDuty.ExtraExpensesCost,
                            PayTypeId = payTypeId,
                            Discount = buyoutContractDuty?.Discount,
                            Rows = buyoutContractDuty.Rows.ToArray(),
                            Cost = buyoutContractDuty.Cost,
                            TotalCost = buyoutActionRowsCost,
                            Reason = buyoutReason,
                            BuyoutReasonId = buyoutReasonId
                        };

                        action = buyoutContractAction;
                        _contractActionBuyoutService.Execute(buyoutContractAction, Constants.ADMINISTRATOR_IDENTITY, contract.BranchId, forceExpensePrepaymentReturn: true, true, null).Wait();
                        _jobLog.Log("ProlongContractByPrepayment", JobCode.Begin, JobStatus.Success, EntityType.Contract, contract.Id, $"Действие: Выкуп - {action.Id}");

                        NotificationPaymentType notificationPaymentType = NotificationPaymentType.Buyout;
                        decimal depoBalanceAfterPayment = _contractService.GetPrepaymentBalance(contract.Id);
                        if (depoBalanceAfterPayment > 0)
                            notificationPaymentType = NotificationPaymentType.BuyoutWithExcessPrepayment;

                        innerNotification = _innerNotificationService.CreateNotification(contract.Id, notificationPaymentType);

                        try
                        {
                            _notificationCenter.NotifyAboutContractBuyOuted(contract, lastPrepaymentAction).Wait();
                        }
                        catch (Exception exception)
                        {
                            _logger.Error(exception, exception.Message);
                        }
                    }
                    else
                    {
                        ContractDuty prolongContractDuty = _contractDutyService.GetContractDuty(new ContractDutyCheckModel
                        {
                            ActionType = ContractActionType.Prolong,
                            ContractId = contract.Id,
                            Date = DateTime.Now,
                            PayTypeId = payTypeId
                        });

                        List<ContractActionRow> prolongActionRows = prolongContractDuty.Rows;
                        var loanAmountTypes = new HashSet<AmountType>
                        {
                            AmountType.Loan,
                            AmountType.OverdueLoan,
                            AmountType.DebtPenalty,
                            AmountType.LoanPenalty,
                            AmountType.AmortizedLoan,
                            AmountType.AmortizedOverdueLoan,
                            AmountType.AmortizedDebtPenalty,
                            AmountType.AmortizedLoanPenalty,
                            AmountType.Receivable
                        };
                        var prolongAmountsDict = new Dictionary<AmountType, decimal>();
                        foreach (ContractActionRow row in prolongActionRows)
                        {
                            decimal amount;
                            if (prolongAmountsDict.TryGetValue(row.PaymentType, out amount))
                            {
                                if (row.Cost != amount)
                                    throw new PawnshopApplicationException("Проводки по одинаковым типам не совпадают по сумме");
                            }

                            prolongAmountsDict[row.PaymentType] = row.Cost;
                        }

                        IEnumerable<KeyValuePair<AmountType, decimal>> neededAmountTypeMappings = prolongAmountsDict.Where(k => loanAmountTypes.Contains(k.Key));
                        decimal neededActionRowsSum = 0;
                        if (neededAmountTypeMappings.Any())
                            neededActionRowsSum = neededAmountTypeMappings.Sum(kv => kv.Value);

                        decimal extraExpensesCost = _contractService.GetExtraExpensesCost(contractId);
                        decimal actionRowsAndExtraExpensesCost = neededActionRowsSum + extraExpensesCost;
                        if (neededActionRowsSum > 0 && actionRowsAndExtraExpensesCost <= depoBalance)
                        {
                            var prolongAction = new ContractAction()
                            {
                                ActionType = ContractActionType.Prolong,
                                ContractId = contract.Id,
                                CreateDate = DateTime.Now,
                                Data = new ContractActionData { ProlongPeriod = contract.LoanPeriod },
                                Date = prolongContractDuty.Date,
                                AuthorId = Constants.ADMINISTRATOR_IDENTITY,
                                ExtraExpensesCost = extraExpensesCost,
                                PayTypeId = payTypeId,
                                Discount = prolongContractDuty?.Discount,
                                Rows = prolongActionRows.ToArray(),
                                Cost = actionRowsAndExtraExpensesCost,
                                TotalCost = neededActionRowsSum,
                                Reason = prolongContractDuty.Reason
                            };

                            action = prolongAction;
                            _contractActionProlongService.Exec(prolongAction, Constants.ADMINISTRATOR_IDENTITY, contract.BranchId, forceExpensePrepaymentReturn: true, autoApprove: true);
                            _jobLog.Log("ProlongContractByPrepayment", JobCode.Begin, JobStatus.Success, EntityType.Contract, contract.Id, $"Действие: Пролонгация - {action.Id}");
                            Contract contractWithoutRelations = _contractRepository.GetOnlyContract(contract.Id);
                            if (contractWithoutRelations == null)
                                throw new PawnshopApplicationException($"Договор {contract.Id} не найден");

                            if (!contractWithoutRelations.NextPaymentDate.HasValue)
                                throw new PawnshopApplicationException($"Следующая дата платежа не должен быть пустым у договора {contract.Id}");

                            DateTime newNextPaymentDate = contractWithoutRelations.NextPaymentDate.Value;
                            DateTime contractDate = contractWithoutRelations.ContractDate;
                            string prolongReason = string.Format(Constants.REASON_AUTO_END_PERIOD_CONTRACTS_PROLONGATION, contract.ContractNumber, contractDate.ToString("dd.MM.yyyy"), newNextPaymentDate.ToString("dd.MM.yyyy"));
                            prolongAction.Reason = prolongReason;
                            _contractActionRepository.Update(prolongAction);
                            //Отключили отправку внутреннего уведомления об успешном продление договора
                            //innerNotification = _innerNotificationService.CreateNotification(contract.Id, notificationPaymentType);
                        }
                        else
                        {
                            ContractDuty paymentContractDuty = _contractDutyService.GetContractDuty(new ContractDutyCheckModel
                            {
                                ActionType = ContractActionType.Payment,
                                ContractId = contract.Id,
                                Cost = depoBalance,
                                Date = DateTime.Now,
                                PayTypeId = payTypeId,
                            });

                            List<ContractActionRow> paymentContractActionRows = paymentContractDuty.Rows;
                            List<ContractExpense> paymentContractExpenses = paymentContractDuty.ExtraContractExpenses;
                            var paymentAmountsDict = new Dictionary<AmountType, decimal>();
                            foreach (ContractActionRow row in paymentContractActionRows)
                            {
                                decimal amount;
                                if (paymentAmountsDict.TryGetValue(row.PaymentType, out amount))
                                {
                                    if (row.Cost != amount)
                                        throw new PawnshopApplicationException("Проводки по одинаковым типам не совпадают по сумме");
                                }

                                paymentAmountsDict[row.PaymentType] = row.Cost;
                            }

                            decimal paymentActionRowsSum = 0;
                            if (paymentAmountsDict.Count > 0)
                                paymentActionRowsSum = paymentAmountsDict.Values.Sum();

                            decimal paymentContractExpensesCost = 0;
                            if (paymentContractExpenses.Count > 0)
                                paymentContractExpensesCost = paymentContractExpenses.Sum(e => e.TotalCost);

                            decimal paymentTotal = paymentContractExpensesCost + paymentActionRowsSum;
                            if (paymentTotal > 0)
                            {
                                DateTime contractDate = contract.ContractDate;
                                string paymentReason = string.Format(Constants.REASON_AUTO_PAYMENT, contract.ContractNumber, contractDate.ToString("dd.MM.yyyy"));
                                var paymentAction = new ContractAction()
                                {
                                    ActionType = ContractActionType.Payment,
                                    ContractId = contract.Id,
                                    CreateDate = DateTime.Now,
                                    Date = paymentContractDuty.Date,
                                    AuthorId = Constants.ADMINISTRATOR_IDENTITY,
                                    ExtraExpensesCost = paymentContractDuty.ExtraExpensesCost,
                                    ExtraExpensesIds = paymentContractDuty.ExtraContractExpenses.Select(e => e.Id).ToList(),
                                    PayTypeId = payTypeId,
                                    Discount = paymentContractDuty?.Discount,
                                    Rows = paymentContractActionRows.ToArray(),
                                    Cost = paymentTotal,
                                    TotalCost = paymentActionRowsSum,
                                    Reason = paymentReason,
                                };

                                action = paymentAction;
                                _contractPaymentService.Payment(paymentAction, contract.BranchId, Constants.ADMINISTRATOR_IDENTITY, forceExpensePrepaymentReturn: true, autoApprove: true);
                                _jobLog.Log("ProlongContractByPrepayment", JobCode.Begin, JobStatus.Success, EntityType.Contract, contract.Id, $"Действие: Оплата - {action.Id}");
                            }

                            var notificationPaymentType = NotificationPaymentType.PartialProlong;
                            _contractAmount.Init(contract, DateTime.Now);
                            decimal prolongCost = _contractAmount.DisplayAmount;
                            decimal extraExpensesCostAfterPayment = _contractService.GetExtraExpensesCost(contractId);
                            if (extraExpensesCostAfterPayment >= depoBalance)
                                notificationPaymentType = NotificationPaymentType.NotEnoughForExpense;

                            innerNotification = _innerNotificationService.CreateNotification(contract.Id, notificationPaymentType, prolongCost);
                            smsNotification = SmsNotifications(contract, notificationPaymentType, paymentTotal, prolongCost);
                        }
                    }

                    if (innerNotification != null)
                        _innerNotificationRepository.Insert(innerNotification);

                    if (smsNotification != null && lastPrepaymentAction != null && lastPrepaymentAction.ProcessingId.HasValue)
                    {
                        if (!smsNotification.NotificationPaymentType.HasValue)
                            _jobLog.Log("UsePrepaymentForEndPeriodContractsJob", JobCode.End, JobStatus.Failed, entityType: EntityType.Contract, entityId: contractId, responseData: $"{nameof(smsNotification)} не должен содержать пустой {nameof(smsNotification.NotificationPaymentType)}");
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
                                    ClientId = contract.ClientId,
                                    CreateDate = DateTime.Now,
                                    Status = NotificationStatus.ForSend,
                                    ContractId = contract.Id
                                };

                                _notificationReceiverRepository.Insert(notificationReceiver);
                            }
                        }
                    }

                    transaction.Commit();
                    if (action != null)
                    {
                        var orderIds = _cashOrderService.GetAllRelatedOrdersByContractActionId(action.Id).Result;
                        _uKassaService.FinishRequests(orderIds);
                    }
                }

                if (action != null)
                {
                    var close = new CollectionClose()
                    {
                        ContractId = contractId,
                        ActionId = action.Id
                    };
                    _collectionService.CloseContractCollection(close);
                   _closeLegalCaseService.Close(contractId);
                }
                else
                {
                    var close = new CollectionClose()
                    {
                        ContractId = contractId,
                        ActionId = 0
                    };
                    _collectionService.CloseContractCollection(close);
                    _closeLegalCaseService.Close(contractId);
                }
                _jobLog.Log("UsePrepaymentForEndPeriodContractsJob", JobCode.End, JobStatus.Success, entityType: EntityType.Contract, entityId: contractId, $"{(action != null ? action.Id.ToString() : null)}");
            }
            catch (Exception ex)
            {
                _jobLog.Log("ProlongContractByPrepayment", JobCode.Error, JobStatus.Failed, EntityType.Contract, contractId, responseData: JsonConvert.SerializeObject(ex));
                EmailProcessingNotifications(contract);
                _eventLog.Log(EventCode.ContractProlong, EventStatus.Failed, EntityType.Contract, contract.Id, string.Empty, $@"При автоматическом продлении произошла ошибка по договору {contract.ContractNumber}({contract}) ({ex.Message})", userId: Constants.ADMINISTRATOR_IDENTITY);
                _logger.Error(ex, ex.Message);
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

        /// <summary>
        /// Email уведомление об ошибке
        /// </summary>
        private string EmailProcessingNotifications(Contract сontract)
        {
            var messageForFront = $@"ОШИБКА! Автоматическом освоение аванса";
            var message = $@"<p style=""text-align: center;""><strong>ОШИБКА! Автоматическом освоение аванса</strong></p>
                        <p><strong>ContractId = {сontract.Id}</strong></p>";

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
