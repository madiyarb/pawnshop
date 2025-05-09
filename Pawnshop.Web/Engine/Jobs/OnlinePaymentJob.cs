using System;
using System.Collections.Generic;
using Hangfire;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.InnerNotifications;
using Pawnshop.Data.Models.Notifications.NotificationTemplates;
using Pawnshop.Data.Models.OnlinePayments;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Web.Engine.MessageSenders;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Options;
using Microsoft.Extensions.Options;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Services.Calculation;
using Pawnshop.Web.Engine.Services.Interfaces;
using System.Threading;
using Pawnshop.Services.Models.Calculation;
using Pawnshop.Services.Contracts;
using System.Data;
using System.Linq;
using Pawnshop.Data.Models.Processing;
using Pawnshop.Services.CreditLines;
using Pawnshop.Services.MessageSenders;
using Pawnshop.Services.Notifications;
using Serilog;

namespace Pawnshop.Web.Engine.Jobs
{
    public class OnlinePaymentJob
    {
        private static readonly object _object = new object();
        private readonly OnlinePaymentRepository _onlinePaymentRepository;
        private readonly ContractRepository _contractRepository;
        private readonly NotificationRepository _notificationRepository;
        private readonly NotificationReceiverRepository _notificationReceiverRepository;
        private readonly EnviromentAccessOptions _options;
        private readonly JobLog _jobLog;
        private readonly PayTypeRepository _payTypeRepository;
        private readonly InnerNotificationRepository _innerNotificationRepository;
        private readonly IContractActionPrepaymentService _contractActionPrepaymentService;
        private readonly IContractAmount _contractAmount;
        private readonly IInnerNotificationService _innerNotificationService;
        private readonly EmailSender _emailSender;
        private readonly IContractDutyService _contractDutyService;
        private readonly IContractService _contractService;
        private readonly INotificationTemplateService _notificationTemplateService;
        private readonly ICreditLineService _creditLineService;
        private readonly INotificationCenterService _notificationCenter;
        private readonly CreditLineRepository _creditLineRepository;
        private readonly ILogger _logger;

        public OnlinePaymentJob(OnlinePaymentRepository onlinePaymentRepository,
            ContractRepository contractRepository, 
            JobLog jobLog,
            IContractActionPrepaymentService contractActionPrepaymentService,
            NotificationRepository notificationRepository,
            NotificationReceiverRepository notificationReceiverRepository,
            IOptions<EnviromentAccessOptions> options,
            PayTypeRepository payTypeRepository,
            IContractAmount contractAmount,
            IInnerNotificationService innerNotificationService, 
            InnerNotificationRepository innerNotificationRepository,
            EmailSender emailSender,
            IContractDutyService contractDutyService,
            IContractService contractService,
            INotificationTemplateService notificationTemplateService,
            ICreditLineService creditLineService,
            INotificationCenterService notificationCenter,
            CreditLineRepository creditLineRepository,
            ILogger logger)
        {
            _onlinePaymentRepository = onlinePaymentRepository;
            _contractRepository = contractRepository;
            _notificationReceiverRepository = notificationReceiverRepository;
            _notificationRepository = notificationRepository;
            _jobLog = jobLog;
            _options = options.Value;
            _contractActionPrepaymentService = contractActionPrepaymentService;
            _payTypeRepository = payTypeRepository;
            _contractAmount = contractAmount;
            _innerNotificationService = innerNotificationService;
            _innerNotificationRepository = innerNotificationRepository;
            _emailSender = emailSender;
            _contractDutyService = contractDutyService;
            _contractService = contractService;
            _notificationTemplateService = notificationTemplateService;
            _creditLineService = creditLineService;
            _notificationCenter = notificationCenter;
            _creditLineRepository = creditLineRepository;
            _logger = logger;
        }

        [DisableConcurrentExecution(10 * 60)]
        public void Execute()
        {
            if (!_options.OnlinePayment)
                return;

            bool tryEnter = false;
            if (tryEnter = Monitor.TryEnter(_object, new TimeSpan(1, 0, 0)))
            {
                try
                {
                    if (DateTime.Now.TimeOfDay >= Constants.STOP_ONLINE_PAYMENTS || DateTime.Now.TimeOfDay < Constants.START_ONLINE_PAYMENTS)
                    {
                        return;
                    }

                    List<OnlinePayment> onlinePayments = _onlinePaymentRepository.Select();
                    if (onlinePayments.Count == 0)
                        return;

                    string onlinePayTypeCode = Constants.PAY_TYPE_ONLINE;
                    var onlinePayType = _payTypeRepository.Find(new { Code = onlinePayTypeCode });
                    if (onlinePayType == null)
                        throw new PawnshopApplicationException($"Вид оплаты {onlinePayTypeCode} не найден");

                    foreach (OnlinePayment onlinePayment in onlinePayments)
                    {
                        try
                        {
                            _jobLog.Log("OnlinePaymentJob", JobCode.Begin, JobStatus.Success, EntityType.Contract, onlinePayment.ContractId, JsonConvert.SerializeObject(onlinePayment));
                            if (!onlinePayment.ProcessingId.HasValue)
                                throw new PawnshopApplicationException($"{nameof(onlinePayment)}.{nameof(onlinePayment.ProcessingId)} обязателен, onlinePaymentId = {onlinePayment.Id}");

                            if (!onlinePayment.ProcessingType.HasValue)
                                throw new PawnshopApplicationException($"{nameof(onlinePayment)}.{nameof(onlinePayment.ProcessingType)} обязателен, onlinePaymentId = {onlinePayment.Id}");

                            if (!onlinePayment.ProcessingStatus.HasValue)
                                throw new PawnshopApplicationException($"{nameof(onlinePayment)}.{nameof(onlinePayment.ProcessingStatus)} обязателен, onlinePaymentId = {onlinePayment.Id}");

                            if (onlinePayment.ProcessingStatus.Value == ProcessingStatus.Processed)
                                throw new PawnshopApplicationException($"Ожидалось что {nameof(onlinePayment)}.{nameof(onlinePayment.ProcessingStatus)} не будет {ProcessingStatus.Processed}, onlinePaymentId = {onlinePayment.Id}");

                            if (!onlinePayment.Amount.HasValue)
                                throw new PawnshopApplicationException($"{nameof(onlinePayment)}.{nameof(onlinePayment.Amount)} обязателен, onlinePaymentId = {onlinePayment.Id}");

                            if (onlinePayment.ContractActionId.HasValue)
                                throw new PawnshopApplicationException($"Ожидалось что {nameof(onlinePayment)}.{nameof(onlinePayment.ContractActionId)} будет null, onlinePaymentId = {onlinePayment.Id}");

                            Contract contract = _contractRepository.Get(onlinePayment.ContractId);
                            if (contract == null)
                                throw new PawnshopApplicationException($"Договор {onlinePayment.ContractId} не найден, onlinePaymentId = {onlinePayment.Id}");

                            if (contract.Status != ContractStatus.Signed && contract.Status != ContractStatus.AwaitForInitialFee)
                                throw new PawnshopApplicationException($"Договор {onlinePayment.ContractId} должен иметь статус 'Подписан' или 'Ожидает первоначального взноса', onlinePaymentId = {onlinePayment.Id}");
                            ContractAction prepaymentAction = null;
                            _contractAmount.Init(contract, DateTime.Now);
                            try
                            {
                                using (IDbTransaction transaction = _onlinePaymentRepository.BeginTransaction())
                                {
                                    var processingInfo = new ProcessingInfo
                                    {
                                        Amount = onlinePayment.Amount.Value,
                                        BankName = onlinePayment.ProcessingBankName,
                                        BankNetwork = onlinePayment.ProcessingBankNetwork,
                                        Reference = onlinePayment.ProcessingId.Value,
                                        Type = onlinePayment.ProcessingType.Value
                                    };
                                    prepaymentAction = _contractActionPrepaymentService.Exec(contract.Id, onlinePayment.Amount.Value, onlinePayType.Id, contract.BranchId, Constants.ADMINISTRATOR_IDENTITY, date: DateTime.Now, processingInfo: processingInfo, orderStatus: OrderStatus.Approved);
                                    if (prepaymentAction == null)
                                        throw new PawnshopApplicationException($"Ожидалось что {nameof(_contractActionPrepaymentService)}.{nameof(_contractActionPrepaymentService.Exec)} не вернет null");

                                    onlinePayment.ContractActionId = prepaymentAction.Id;
                                    onlinePayment.FinishDate = DateTime.Now;
                                    onlinePayment.ProcessingStatus = ProcessingStatus.Processed;
                                    _onlinePaymentRepository.Update(onlinePayment);
                                    if (_contractAmount.DisplayAmountWithoutPrepayment == 0)
                                        CreatePrepaymentNotification(contract, onlinePayment.Amount.Value, _contractAmount.DisplayAmount);
                                    else if (_contractAmount.DisplayAmount < 0)
                                        throw new PawnshopApplicationException($"{nameof(IContractAmount)} вернул отрицательный {nameof(_contractAmount.DisplayAmount)} после метода {nameof(_contractAmount.Init)}");

                                    transaction.Commit();
                                }
                            }
                            catch
                            {
                                onlinePayment.ProcessingStatus = ProcessingStatus.Failed;
                                onlinePayment.FinishDate = null;
                                _onlinePaymentRepository.Update(onlinePayment);
                                throw;
                            }

                            Contract contractAfterPrepayment = _contractRepository.GetOnlyContract(contract.Id);
                            if (contractAfterPrepayment == null)
                                throw new PawnshopApplicationException($"Договор {contract.Id} не найден");

                            if (contractAfterPrepayment.Status != ContractStatus.Signed)
                            {
                                string logText = $"Договор {contract.Id} не имеет статус {ContractStatus.Signed}, не будет осваивать аванс";
                                _jobLog.Log("OnlinePaymentJob", JobCode.End, JobStatus.Success, EntityType.Contract, onlinePayment.ContractId, requestData: logText, responseData: JsonConvert.SerializeObject(onlinePayment));
                                continue;
                            }

                            if (contract.ContractClass == ContractClass.CreditLine)
                            {

                                try 
                                { 
                                    var tranchesCount = _creditLineRepository.GetActiveTranchesCount(contract.Id).Result;

                                    if (tranchesCount > 1)
                                    {
                                        var balance = _creditLineService.GetCurrentlyDebtForCreditLine(contract.Id).Result;
                                        if (balance.SummaryPrepaymentBalance < balance.SummaryTotalRedemptionAmount
                                            && balance.ContractsBalances.Any(cb =>
                                                cb.TotalRedemptionAmount < balance.SummaryPrepaymentBalance))
                                        {
                                            _notificationCenter
                                                .NotifyAboutSomeContractReadyToBuyOut(balance.ContractsBalances
                                                    .FirstOrDefault(cb => cb.TotalRedemptionAmount <= balance.SummaryPrepaymentBalance 
                                                                          && cb.ContractId != contract.Id)
                                                    .ContractId, prepaymentAction).Wait();
                                        }
                                    }
                                }
                                catch (Exception exception)
                                {
                                    _logger.Error(exception, exception.Message);
                                }
                                BackgroundJob.Enqueue<UsePrepaymentForCreditLineForMonthlyPaymentJob>
                                    (x => x.UsePrepaymentForCreditLine(contract.Id, onlinePayType.Id));
                            }

                            _contractAmount.Init(contract, DateTime.Now);
                            if (_contractAmount.DisplayAmountWithoutPrepayment > 0)
                            {
                                if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
                                    BackgroundJob.Enqueue<UsePrepaymentForEndPeriodContractsJob>(x => x.ProlongContractByPrepayment(contract.Id, onlinePayType.Id));
                                else
                                {
                                    if (DateTime.Now.Date >= contract.NextPaymentDate.Value.Date || _contractAmount.PrepaymentCost >= _contractAmount.BuyoutAmount)
                                    {
                                        BackgroundJob.Enqueue<UsePrepaymentForMonthlyPaymentJob>(x => x.UsePrepaymentForAnnuityContract(contract.Id, onlinePayType.Id));
                                        //_usePrepaymentForMonthlyPaymentJob.UsePrepaymentForAnnuityContract(contract.Id, onlinePayType.Id);
                                    }
                                    else
                                    {
                                        CreatePrepaymentNotification(contract, onlinePayment.Amount.Value, _contractAmount.DisplayAmount);
                                    }
                                }
                                //if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
                                //    _usePrepaymentForEndPeriodContractsJob.ProlongContractByPrepayment(contract.Id, onlinePayType.Id);
                                //else
                                //    _usePrepaymentForMonthlyPaymentJob.UsePrepaymentForAnnuityContract(contract.Id, onlinePayType.Id);
                            }
                            else if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
                            {
                                var buyoutContractDutyCheckModel = new ContractDutyCheckModel
                                {
                                    ActionType = contract.ContractClass == ContractClass.Credit && contract.IsContractRestructured ? ContractActionType.BuyoutRestructuringCred : ContractActionType.Buyout,
                                    ContractId = contract.Id,
                                    Date = DateTime.Now,
                                    PayTypeId = onlinePayType.Id
                                };
                                ContractDuty buyoutContractDuty = _contractDutyService.GetContractDuty(buyoutContractDutyCheckModel);
                                decimal depoBalance = _contractService.GetPrepaymentBalance(contract.Id);
                                if (buyoutContractDuty == null)
                                    throw new PawnshopApplicationException($"Ожидалось что {nameof(_contractDutyService)}.{nameof(_contractDutyService.GetContractDuty)} не вернет null, по действию выкупу");
                                else if (buyoutContractDuty.Rows == null)
                                    throw new PawnshopApplicationException($"Ожидалось что {nameof(buyoutContractDuty)}.{nameof(buyoutContractDuty.Rows)} не будет null");

                                decimal buyoutCost = buyoutContractDuty.Cost;
                                if (depoBalance >= buyoutCost)
                                {
                                    BackgroundJob.Enqueue<UsePrepaymentForEndPeriodContractsJob>(x => x.ProlongContractByPrepayment(contract.Id, onlinePayType.Id));
                                    //_usePrepaymentForEndPeriodContractsJob.ProlongContractByPrepayment(contract.Id, onlinePayType.Id);
                                }
                            }

                            _jobLog.Log("OnlinePaymentJob", JobCode.End, JobStatus.Success, EntityType.Contract, onlinePayment.ContractId, responseData: JsonConvert.SerializeObject(onlinePayment));
                        }
                        catch (Exception ex)
                        {
                            _jobLog.Log("OnlinePaymentJob", JobCode.Error, JobStatus.Failed, responseData: JsonConvert.SerializeObject(ex).ToString());
                            EmailProcessingNotifications(onlinePayment.ContractId);
                            _logger.Error(ex, ex.Message);
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(_object);
                }
            }

            if (!tryEnter)
                _jobLog.Log("OnlinePaymentJob", JobCode.Error, JobStatus.Failed, EntityType.None, responseData: "Процесс не долждался своей очереди");
        }

        private void CreatePrepaymentNotification(Contract contract, decimal prepaymentCost, decimal displayAmount = 0)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            using (IDbTransaction transaction = _notificationReceiverRepository.BeginTransaction())
            {
                if (contract.Status != ContractStatus.AwaitForInitialFee)
                {
                    var notificationPaymentType = NotificationPaymentType.PaymentAccepted;
                    InnerNotification innerNotification = _innerNotificationService.CreateNotification(contract.Id, notificationPaymentType);
                    _innerNotificationRepository.Insert(innerNotification);
                }

                transaction.Commit();
            }
        }

        /// <summary>
        /// Email уведомление об ошибке
        /// </summary>
        private void EmailProcessingNotifications(int contractId)
        {
            try
            {
                var messageForFront = $@"ОШИБКА! Произошла ошибка при принятии аванса";
                var message = $@"<p style=""text-align: center;""><strong>ОШИБКА! Произошла ошибка при принятии аванса</strong></p>
                        <p><strong>ContractId = {contractId}</strong></p>";

                var messageReceiver = new MessageReceiver
                {
                    ReceiverAddress = _options.ErrorNotifierAddress,
                    ReceiverName = _options.ErrorNotifierName
                };

                _emailSender.SendEmail("Ошибка в автоматическом освоение аванса", message, messageReceiver);
            }
            catch
            {
                _jobLog.Log("OnlinePaymentJob", JobCode.Error, JobStatus.Failed, responseData: $"Не удалось отправить уведомление об ошибке на емейл {_options.ErrorNotifierAddress}");
            }
        }
    }
}
