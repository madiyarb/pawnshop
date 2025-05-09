using Pawnshop.Data.Access;
using Pawnshop.Web.Engine.Audit;
using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire;
using Pawnshop.Core;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.InnerNotifications;
using Pawnshop.Data.Models.Notifications.NotificationTemplates;
using Pawnshop.Web.Engine.MessageSenders;
using Pawnshop.Core.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Mintos;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Contracts;
using System.Data;
using Pawnshop.Services.Models.List;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services;
using Pawnshop.Services.Models.Calculation;
using Pawnshop.Services.Calculation;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Data.Models.Dictionaries;
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
    public class UsePrepaymentForMonthlyPaymentJob
    {
        private readonly EventLog _eventLog;
        private readonly ContractRepository _contractRepository;
        private readonly GroupRepository _groupRepository;
        private readonly OrganizationRepository _organizationRepository;
        private readonly InnerNotificationRepository _innerNotificationRepository;
        private readonly EmailSender _emailSender;
        private readonly EnviromentAccessOptions _options;
        private readonly JobLog _jobLog;
        private readonly MintosContractRepository _mintosContractRepository;
        private readonly MintosContractActionRepository _mintosContractActionRepository;
        private readonly NotificationRepository _notificationRepository;
        private readonly NotificationReceiverRepository _notificationReceiverRepository;
        private readonly ContractActionRepository _contractActionRepository;
        private readonly PayTypeRepository _payTypeRepository;
        private readonly IInnerNotificationService _innerNotificationService;
        private readonly IContractDutyService _contractDutyService;
        private readonly IContractService _contractService;
        private readonly IContractPaymentService _contractPaymentService;
        private readonly IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> _accountSettingService;
        private readonly INotificationTemplateService _notificationTemplateService;
        private readonly IContractAmount _contractAmount;
        private readonly IUKassaService _uKassaService;
        private readonly ICashOrderService _cashOrderService;
        private readonly IDomainService _domainService;
        private readonly IContractActionBuyoutService _contractActionBuyoutService;
        private readonly ICollectionService _collectionService;
        private readonly FunctionSettingRepository _functionSettingRepository;
        private readonly ILegalCollectionCloseService _legalCollectionCloseService;
        private readonly INotificationCenterService _notificationCenter;
        private readonly ILogger _logger;
        private readonly IClientDefermentService _clientDefermentService;

        public UsePrepaymentForMonthlyPaymentJob(
            EventLog eventLog, ContractRepository contractRepository, GroupRepository groupRepository,
            OrganizationRepository organizationRepository,
            InnerNotificationRepository innerNotificationRepository,
            EmailSender emailSender, IOptions<EnviromentAccessOptions> options, JobLog jobLog, MintosContractRepository mintosContractRepository,
            MintosContractActionRepository mintosContractActionRepository,
            IContractService contractService, IContractPaymentService contractPaymentService,
            IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> accountSettingService,
            IContractDutyService contractDutyService, INotificationTemplateService notificationTemplateService,
            NotificationRepository notificationRepository, NotificationReceiverRepository notificationReceiverRepository,
            ContractActionRepository contractActionRepository, IInnerNotificationService innerNotificationService,
            PayTypeRepository payTypeRepository, IContractAmount contractAmounty,
            IUKassaService uKassaService, ICashOrderService cashOrderService,
            IDomainService domainService,
            IContractActionBuyoutService contractActionBuyoutService,
            ICollectionService collectionService,
            ILegalCollectionCloseService legalCollectionCloseService,
            FunctionSettingRepository functionSettingRepository,
            INotificationCenterService notificationCenter,
            ILogger logger,
            IClientDefermentService clientDefermentService)
        {
            _eventLog = eventLog;
            _contractRepository = contractRepository;
            _groupRepository = groupRepository;
            _organizationRepository = organizationRepository;
            _innerNotificationRepository = innerNotificationRepository;
            _emailSender = emailSender;
            _options = options.Value;
            _jobLog = jobLog;
            _mintosContractRepository = mintosContractRepository;
            _mintosContractActionRepository = mintosContractActionRepository;
            _contractService = contractService;
            _contractPaymentService = contractPaymentService;
            _accountSettingService = accountSettingService;
            _contractDutyService = contractDutyService;
            _notificationTemplateService = notificationTemplateService;
            _notificationRepository = notificationRepository;
            _notificationReceiverRepository = notificationReceiverRepository;
            _contractActionRepository = contractActionRepository;
            _innerNotificationService = innerNotificationService;
            _payTypeRepository = payTypeRepository;
            _contractAmount = contractAmounty;
            _uKassaService = uKassaService;
            _cashOrderService = cashOrderService;
            _domainService = domainService;
            _contractActionBuyoutService = contractActionBuyoutService;
            _collectionService = collectionService;
            _functionSettingRepository = functionSettingRepository;
            _notificationCenter = notificationCenter;
            _logger = logger;
            _legalCollectionCloseService = legalCollectionCloseService;
            _clientDefermentService = clientDefermentService;
        }

        /// <summary>
        /// Делает ежемесячное погашение по договору если хватает денег на авансе
        /// </summary>
        [Queue("payments")]
        [DisableConcurrentExecution(timeoutInSeconds: 10 * 60)]
        public void Execute()
        {
            _jobLog.Log("UsePrepaymentForMonthlyPaymentJob", JobCode.Start, JobStatus.Success);
            if (!_options.SchedulePayments)
            {
                _jobLog.Log("UsePrepaymentForMonthlyPaymentJob", JobCode.Cancel, JobStatus.Success);
                return;
            }

            var depoMasteringSetting = _functionSettingRepository.GetByCode(Constants.FUNCTION_SETTING__DEPO_MASTERING);
            if (depoMasteringSetting != null && depoMasteringSetting.BooleanValue.HasValue && depoMasteringSetting.BooleanValue.Value)
            {
                _jobLog.Log("UsePrepaymentForMonthlyPaymentJob", JobCode.Cancel, JobStatus.Success);
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

            ListModel<AccountSetting> penyAccountSettings = _accountSettingService.List(new ListQueryModel<AccountSettingFilter>
            {
                Page = null,
                Model = new AccountSettingFilter
                {
                    Code = Constants.ACCOUNT_SETTING_PENY_ACCOUNT
                }
            });

            ListModel<AccountSetting> penyProfitAccountSettings = _accountSettingService.List(new ListQueryModel<AccountSettingFilter>
            {
                Page = null,
                Model = new AccountSettingFilter
                {
                    Code = Constants.ACCOUNT_SETTING_PENY_PROFIT
                }
            });

            ListModel<AccountSetting> accountSettings = _accountSettingService.List(new ListQueryModel<AccountSettingFilter>
            {
                Page = null,
                Model = new AccountSettingFilter
                {
                    Code = Constants.ACCOUNT_SETTING_ACCOUNT
                }
            });

            ListModel<AccountSetting> profitSettings = _accountSettingService.List(new ListQueryModel<AccountSettingFilter>
            {
                Page = null,
                Model = new AccountSettingFilter
                {
                    Code = Constants.ACCOUNT_SETTING_PROFIT
                }
            });

            if (depoAccountSettings.List.Count == 0)
                throw new PawnshopApplicationException($"Настройка счета {Constants.ACCOUNT_SETTING_DEPO} не найдена");
            if (penyAccountSettings.List.Count == 0)
                throw new PawnshopApplicationException($"Настройка счета {Constants.ACCOUNT_SETTING_PENY_ACCOUNT} не найдена");
            if (penyProfitAccountSettings.List.Count == 0)
                throw new PawnshopApplicationException($"Настройка счета {Constants.ACCOUNT_SETTING_PENY_PROFIT} не найдена");

            var depoAccountSettingsList = depoAccountSettings.List.Select(x => x.Id).ToList();
            var penyAccountSettingsList = penyAccountSettings.List.Select(x => x.Id).ToList();
            var penyProfitAccountSettingsList = penyProfitAccountSettings.List.Select(x => x.Id).ToList();
            var accountSettingsList = accountSettings.List.Select(x => x.Id).ToList();
            var profitSettingId = profitSettings.List.Select(x => x.Id).ToList();

            var contracts = _contractRepository.ListWithPrepaymentForTodayForAnnuity(depoAccountSettingsList, penyAccountSettingsList, penyProfitAccountSettingsList, accountSettingsList, profitSettingId);
            if (contracts == null)
            {
                _jobLog.Log("UsePrepaymentForMonthlyPaymentJob", JobCode.End, JobStatus.Success);
                return;
            }

            if (contracts.Count == 0)
            {
                _jobLog.Log("UsePrepaymentForMonthlyPaymentJob", JobCode.End, JobStatus.Success);
                return;
            }

            PayType defaultPayType = _payTypeRepository.Find(new { IsDefault = true });
            if (defaultPayType == null)
                throw new InvalidOperationException($"не найден дефолтный pay type");

            foreach (var c in contracts)
            {
                BackgroundJob.Enqueue<UsePrepaymentForMonthlyPaymentJob>(x => x.UsePrepaymentForAnnuityContract(c.Id, defaultPayType.Id));
                //UsePrepaymentForAnnuityContract(c.Id, defaultPayType.Id);
            }
        }

        [Queue("payments")]
        public void UsePrepaymentForAnnuityContract(int contractId, int payTypeId)
        {
            if (!_options.SchedulePayments)
            {
                _jobLog.Log("UsePrepaymentForMonthlyPaymentJob", JobCode.Cancel, JobStatus.Success, entityType: EntityType.Contract, entityId: contractId);
                return;
            }

            DateTime now = DateTime.Now;
            if (now.TimeOfDay >= Constants.STOP_ONLINE_PAYMENTS || now.TimeOfDay < Constants.START_ONLINE_PAYMENTS)
            {
                _jobLog.Log("UsePrepaymentForMonthlyPaymentJob", JobCode.Cancel, JobStatus.Success, entityType: EntityType.Contract, entityId: contractId);
                return;
                //throw new PawnshopApplicationException("Время освоения онлайн платежей должно быть между 10:00:00 и 22:31:00");
            }

            var depoMasteringSetting = _functionSettingRepository.GetByCode(Constants.FUNCTION_SETTING__DEPO_MASTERING);
            if (depoMasteringSetting != null && depoMasteringSetting.BooleanValue.HasValue && depoMasteringSetting.BooleanValue.Value)
            {
                _jobLog.Log("UsePrepaymentForMonthlyPaymentJob", JobCode.Cancel, JobStatus.Success, entityType: EntityType.Contract, entityId: contractId);
                return;
            }

            _jobLog.Log("UsePrepaymentForMonthlyPaymentJob", JobCode.Cancel, JobStatus.Success, entityType: EntityType.Contract, entityId: contractId);
            Contract contract = null;
            try
            {
                contract = _contractRepository.Get(contractId);
                if (contract == null)
                    throw new PawnshopApplicationException($"Договор {contractId} не найден");

                if (contract.Id != contractId)
                    throw new PawnshopApplicationException($"Идентификаторы {nameof(contractId)} и {nameof(contract)} не совпадают");

                var defermentInformation = _clientDefermentService.GetDefermentInformation(contractId, now);

                if (defermentInformation != null && (defermentInformation.Status == Data.Models.Restructuring.RestructuringStatusEnum.Frozen || defermentInformation.IsInDefermentPeriod))
                {
                    _eventLog.Log(EventCode.ContractMonthlyPayment, EventStatus.Failed, EntityType.Contract, contract.Id, String.Empty, $@"Отмена автоматического освоения аванса по договору {contract.ContractNumber}({contract.Id}) (договор находится в период отсрочки)", userId: Constants.ADMINISTRATOR_IDENTITY);
                    return;
                }

                if (contract.Status != ContractStatus.Signed)
                {
                    _eventLog.Log(EventCode.ContractMonthlyPayment, EventStatus.Failed, EntityType.Contract, contract.Id, String.Empty, $@"Отмена автоматического освоения аванса по договору {contract.ContractNumber}({contract.Id}) (договор не подписан)", userId: Constants.ADMINISTRATOR_IDENTITY);
                    return;
                }

                InnerNotification innerNotification = null;
                Notification smsNotification = null;
                ContractAction lastPrepaymentAction = _contractActionRepository.GetLastContractActionByType(contractId, ContractActionType.Prepayment);
                //bool doHasUnpayedScheduleItems =
                //    contract.PaymentSchedule.Any(
                //        x => !x.Canceled.HasValue
                //        && !x.ActionId.HasValue
                //        && x.Date.Date <= DateTime.Now.Date
                //        && !x.DeleteDate.HasValue);

                //if (!doHasUnpayedScheduleItems)
                //{
                //    _eventLog.Log(EventCode.ContractMonthlyPayment, EventStatus.Failed, EntityType.Contract, contract.Id, string.Empty, $@"Отмена автоматического освоения аванса по договору {contract.ContractNumber}({contract.Id}) (по договору нет актиного платежа)");
                //    return;
                //}

                if (contract.InscriptionId > 0 && contract.Inscription.Status != InscriptionStatus.Denied)
                {
                    _eventLog.Log(EventCode.ContractMonthlyPayment, EventStatus.Failed, EntityType.Contract, contract.Id, string.Empty, $@"Отмена автоматического освоения аванса по договору {contract.ContractNumber}({contract.Id}) (договор передан в ЧСИ)", userId: Constants.ADMINISTRATOR_IDENTITY);
                    return;
                }

                var branch = _groupRepository.Get(contract.BranchId);
                var organization = _organizationRepository.Get(branch.OrganizationId);
                decimal contractPrepaymentBalance = _contractService.GetPrepaymentBalance(contract.Id);
                if (contractPrepaymentBalance == 0)
                    return;

                var contractDutyModel = new ContractDutyCheckModel
                {
                    ActionType = ContractActionType.Payment,
                    ContractId = contract.Id,
                    Cost = contractPrepaymentBalance,
                    Date = DateTime.Now.Date,
                    PayTypeId = payTypeId
                };

                ContractDuty contractDuty = _contractDutyService.GetContractDuty(contractDutyModel);
                if (contractDuty == null)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(contractDuty)} не будет null");

                if (contractDuty.Rows == null)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(contractDuty)}.{nameof(contractDuty.Rows)} не будет null");

                if (contractDuty.ExtraContractExpenses == null)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(contractDuty)}.{nameof(contractDuty.ExtraContractExpenses)} не будет null");

                decimal payableExtraExpensesCost = 0;
                List<ContractExpense> payableExtraExpenses = contractDuty.ExtraContractExpenses;
                if (payableExtraExpenses.Count > 0)
                {
                    payableExtraExpensesCost = contractDuty.ExtraContractExpenses.Sum(e => e.TotalCost);
                }

                List<ContractActionRow> contractActionRows = contractDuty.Rows;
                var contractActionAmountTypeDict = new Dictionary<AmountType, decimal>();
                foreach (ContractActionRow contractActionRow in contractActionRows)
                {
                    contractActionAmountTypeDict[contractActionRow.PaymentType] = contractActionRow.Cost;
                }

                decimal actionRowsCost = contractActionAmountTypeDict.Values.Sum();
                decimal expensesAndActionRowsCost = payableExtraExpensesCost + actionRowsCost;
                List<int> payableExtraExpenseIds = payableExtraExpenses.Select(e => e.Id).ToList();
                ContractAction action = null;
                _contractAmount.Init(contract, DateTime.Now);
                using (IDbTransaction transaction = _contractRepository.BeginTransaction())
                {
                    if (_contractAmount.PrepaymentCost >= _contractAmount.BuyoutAmount)
                    {
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
                    }
                    else if (contractDuty.ExtraContractExpenses.Count > 0 || contractDuty.Rows.Count > 0)
                    {
                        action = new ContractAction()
                        {
                            ContractId = contract.Id,
                            CreateDate = DateTime.Now,
                            Data = new ContractActionData(),
                            Date = DateTime.Now.Date,
                            AuthorId = 1,
                            PayTypeId = payTypeId,
                            TotalCost = actionRowsCost,
                            ExtraExpensesIds = payableExtraExpenseIds,
                            Cost = expensesAndActionRowsCost,
                            Rows = contractDuty.Rows.ToArray(),
                            Reason = contractDuty.Reason,
                            ActionType = ContractActionType.Payment,
                            Discount = contractDuty.Discount,
                            ExtraExpensesCost = payableExtraExpensesCost,
                        };
                        _contractPaymentService.Payment(action, branch.Id, Constants.ADMINISTRATOR_IDENTITY, forceExpensePrepaymentReturn: true, autoApprove: true);
                    }
                    Contract contractAfterPayment = _contractRepository.GetOnlyContract(contract.Id);
                    if (contractAfterPayment == null)
                        throw new PawnshopApplicationException($"Договор {contract.Id} не найден");

                    if (contractAfterPayment.Status == ContractStatus.BoughtOut)
                    {
                        NotificationPaymentType notificationPaymentType = NotificationPaymentType.Buyout;
                        decimal depoBalanceAfterPayment = _contractService.GetPrepaymentBalance(contract.Id);
                        if (depoBalanceAfterPayment > 0)
                            notificationPaymentType = NotificationPaymentType.BuyoutWithExcessPrepayment;

                        try
                        {
                            _notificationCenter.NotifyAboutContractBuyOuted(contract, lastPrepaymentAction).Wait();
                        }
                        catch (Exception exception)
                        {
                            _logger.Error(exception, exception.Message);
                        }
                        innerNotification = _innerNotificationService.CreateNotification(contract.Id, notificationPaymentType);
                    }
                    else
                    {
                        decimal monthlyPaymentAmount = _contractAmount.DisplayAmount;
                        decimal expensesBalance = _contractService.GetExtraExpensesCost(contractId);
                        if (expensesBalance > 0)
                        {
                            var notificationPaymentType = NotificationPaymentType.NotEnoughForExpense;
                            decimal notEnoughAmount = monthlyPaymentAmount;
                            smsNotification = SmsNotifications(contract, notificationPaymentType, failPaymentCost: notEnoughAmount);
                            innerNotification = _innerNotificationService.CreateNotification(contract.Id, notificationPaymentType, notEnoughAmount);
                        }
                        else if (monthlyPaymentAmount > 0)
                        {
                            var notificationPaymentType = NotificationPaymentType.PartialMonthlyPayment;
                            decimal notEnoughAmount = monthlyPaymentAmount;
                            smsNotification = SmsNotifications(contract, notificationPaymentType, successPaymentCost: expensesAndActionRowsCost, failPaymentCost: notEnoughAmount); ;
                            innerNotification = _innerNotificationService.CreateNotification(contract.Id, notificationPaymentType, notEnoughAmount);
                        }
                        else if (monthlyPaymentAmount < 0)
                        {
                            throw new PawnshopApplicationException($"{_contractAmount}.{nameof(_contractAmount.DisplayAmount)} не должен быть меньше нуля");
                        }
                    }

                    if (innerNotification != null)
                        _innerNotificationRepository.Insert(innerNotification);

                    if (smsNotification != null && lastPrepaymentAction != null && lastPrepaymentAction.ProcessingId.HasValue)
                    {
                        if (!smsNotification.NotificationPaymentType.HasValue)
                            _jobLog.Log("UsePrepaymentForAnnuityContract", JobCode.End, JobStatus.Failed, entityType: EntityType.Contract, entityId: contractId, responseData: $"{nameof(smsNotification)} не должен содержать пустой {nameof(smsNotification.NotificationPaymentType)}");
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

                    _jobLog.Log("UsePrepaymentForAnnuityContract", JobCode.End, JobStatus.Success, EntityType.Contract, contract?.Id, responseData: $"{(action != null ? action.Id.ToString() : null)}");
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
                    _legalCollectionCloseService.Close(close.ContractId);
                }
                else
                {
                    var close = new CollectionClose()
                    {
                        ContractId = contractId,
                        ActionId = 0
                    };
                    _collectionService.CloseContractCollection(close);
                    _legalCollectionCloseService.Close(close.ContractId);
                }
            }
            catch (Exception e)
            {
                _jobLog.Log("UsePrepaymentForAnnuityContract", JobCode.Error, JobStatus.Failed, EntityType.Contract, contractId, responseData: JsonConvert.SerializeObject(e));
                FailureInnerNotification(contract);
                EmailProcessingNotifications(contract);
                _eventLog.Log(EventCode.ContractMonthlyPayment, EventStatus.Failed, EntityType.Contract, contract.Id, string.Empty, $@"При автоматическом освоении аванса произошла ошибка по договору {contract.ContractNumber}({contract.Id}) ({e.Message})", userId: Constants.ADMINISTRATOR_IDENTITY);
                _logger.Error(e, e.Message);
            }
        }


        /// <summary>
        /// Внутренние уведомления для филиала
        /// </summary>
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

        /// <summary>
        /// Email уведомление об ошибке
        /// </summary>
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

        private void ScheduleMintosPaymentUpload(Contract contract, ContractAction action)
        {
            var mintosContracts = _mintosContractRepository.GetByContractId(contract.Id);
            MintosContract mintosContract = null;
            if (mintosContracts.Count > 1)
            {
                mintosContract = mintosContracts.Where(x => x.MintosStatus.Contains("active")).FirstOrDefault();
                if (mintosContracts.Where(x => x.MintosStatus.Contains("active")).Count() > 1)
                {
                    _eventLog.Log(
                        EventCode.MintosContractUpdate,
                        EventStatus.Failed,
                        EntityType.MintosContract,
                        mintosContract.Id,
                        requestData: JsonConvert.SerializeObject(mintosContracts),
                        responseData: $"Договор {contract.ContractNumber}({contract.Id}) выгружен в Mintos {mintosContracts.Where(x => x.MintosStatus.Contains("active")).Count()} раз(а)", userId: Constants.ADMINISTRATOR_IDENTITY);
                }
            }
            else if (mintosContracts.Count == 1)
            {
                mintosContract = mintosContracts.FirstOrDefault();
                if (mintosContract != null && mintosContracts.Where(x => x.MintosStatus.Contains("active")).Count() == 0)
                {
                    _eventLog.Log(
                        EventCode.MintosContractUpdate,
                        EventStatus.Failed,
                        EntityType.MintosContract,
                        mintosContract.Id,
                        requestData: JsonConvert.SerializeObject(mintosContracts),
                        responseData: $"Договор {contract.ContractNumber}({contract.Id}) выгружен в Mintos, но не проверен/утвержден модерацией Mintos)", userId: Constants.ADMINISTRATOR_IDENTITY);
                }
            }
            else
            {
                return;
            }
            if (mintosContracts == null) return;
            if (!mintosContract.MintosStatus.Contains("active")) return;

            try
            {
                MintosContractAction mintosAction = new MintosContractAction(action);

                using (var transaction = _mintosContractRepository.BeginTransaction())
                {
                    mintosAction.MintosContractId = mintosContract.Id;

                    _mintosContractActionRepository.Insert(mintosAction);

                    transaction.Commit();
                }
            }
            catch (Exception e)
            {
                _eventLog.Log(
                    EventCode.MintosContractUpdate,
                    EventStatus.Failed,
                    EntityType.MintosContract,
                    mintosContract.Id,
                    responseData: e.Message, userId: Constants.ADMINISTRATOR_IDENTITY);
            }
        }
    }
}
