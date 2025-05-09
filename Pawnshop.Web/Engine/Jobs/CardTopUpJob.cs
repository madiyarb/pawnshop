using System;
using Hangfire;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Data.Models.OnlinePayments;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Core.Exceptions;
using System.Threading;
using Pawnshop.Services.Contracts;
using System.Data;
using Pawnshop.Data.Models.Processing;
using Pawnshop.Services.CardTopUp;


namespace Pawnshop.Web.Engine.Jobs
{
    public class CardTopUpJob
    {
        private static readonly object _object = new object();
        private readonly OnlinePaymentRepository _onlinePaymentRepository;
        private readonly NotificationRepository _notificationRepository;
        private readonly NotificationReceiverRepository _notificationReceiverRepository;
        private readonly EventLog _eventLog;
        private readonly JobLog _jobLog;
        private readonly IContractService _contractService;
        private readonly ICardTopUpService _cardTopUpService;
        private readonly CardTopUpTransactionRepository _cardTopUpTransactionRepository;
        private readonly ClientRepository _clientRepository;
        public CardTopUpJob(OnlinePaymentRepository onlinePaymentRepository,
            EventLog eventLog, JobLog jobLog, 
            NotificationRepository notificationRepository, NotificationReceiverRepository notificationReceiverRepository,
            IContractService contractService, ICardTopUpService cardTopUpService,
            CardTopUpTransactionRepository cardTopUpTransactionRepository,
            ClientRepository clientRepository)
        {
            _onlinePaymentRepository = onlinePaymentRepository;
            _notificationReceiverRepository = notificationReceiverRepository;
            _notificationRepository = notificationRepository;
            _eventLog = eventLog;
            _jobLog = jobLog;
            _contractService = contractService;
            _cardTopUpService = cardTopUpService;
            _cardTopUpTransactionRepository = cardTopUpTransactionRepository;
            _clientRepository = clientRepository;
        }

        [DisableConcurrentExecution(10 * 60)]
        public void Execute()
        {
            bool tryEnter = false;
            if (tryEnter = Monitor.TryEnter(_object, new TimeSpan(1, 0, 0)))
            {
                _jobLog.Log("CardTopUpJob", JobCode.Start, JobStatus.Success, EntityType.CardTopUp);
                try
                {
                    DateTime from = DateTime.Now.AddDays(-3);
                    DateTime to = DateTime.Now.AddDays(1);
                    var transactions = _cardTopUpTransactionRepository.GetTopUpTransactionsForPeriod(from, to);
                    for (int i = 0; i < transactions.Count; i++)
                    {
                        var transactionStatus = _cardTopUpService
                            .GetTransactionStatusCode(transactions[i].CustomerReference, CancellationToken.None).Result;
                        if (transactionStatus != null)
                        {
                            if (transactionStatus.Body.getTransactionStatusCodeResponse.@return.transactionStatus == "PAID")
                            {
                                using (IDbTransaction transaction = _cardTopUpTransactionRepository.BeginTransaction())
                                {
                                    try
                                    {
                                        transactions[i].Status = "Paid";
                                        transactions[i].UpdateDate = DateTime.Now;
                                        var contractForAmount =
                                            _contractService.Get(Convert.ToInt32(transactions[i].ContractId));
                                        var processingInfo = new ProcessingInfo
                                        {
                                            Amount = decimal.Parse(transactions[i].Amount) /
                                                     100, // Сумма которая в базе в тиынах у процессинга в тенге 
                                            Reference = transactions[i].OrderId,
                                            Type = ProcessingType.Processing,
                                            BankName = null,
                                            BankNetwork = null
                                        };
                                        _cardTopUpTransactionRepository.Update(transactions[i]);
                                        OnlinePayment onlinePayment =
                                            QueueOnlinePayment(contractForAmount, processingInfo);
                                        if (onlinePayment == null)
                                            throw new PawnshopApplicationException(
                                                $"Ожидалось что {nameof(QueueOnlinePayment)} не вернет null");

                                        if (!onlinePayment.Amount.HasValue)
                                            throw new PawnshopApplicationException(
                                                $"Ожидалось что {nameof(onlinePayment)}.{nameof(onlinePayment.Amount)} не будет null");

                                        _eventLog.Log(EventCode.Prepayment, EventStatus.Success, EntityType.Contract,
                                            contractForAmount.Id,
                                            responseData:
                                            $@"Принята оплата по кредиту: {onlinePayment.Amount.Value} KZT от PROCESSING.KZ");
                                        transaction.Commit();
                                    }
                                    catch (Exception exception)
                                    {
                                        transaction.Rollback();
                                        _jobLog.Log("CardTopUpJob", JobCode.Error, JobStatus.Failed, EntityType.CardTopUp,
                                            responseData: exception.Message);
                                    }
                                }
                            }
                        }
                    }
                }
                finally
                {
                    Monitor.Exit(_object);
                }
            }
            if (!tryEnter)
                _jobLog.Log("CardTopUpJob", JobCode.Error, JobStatus.Failed, EntityType.CardTopUp, responseData: "Процесс не долждался своей очереди");
        }

        private OnlinePayment QueueOnlinePayment(Contract contract, ProcessingInfo processingInfo)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            if (processingInfo == null)
                throw new ArgumentNullException(nameof(processingInfo));

            if (processingInfo.Amount <= 0)
                throw new ArgumentException($"{processingInfo.Amount} должен быть больше нуля", nameof(processingInfo));

            if (processingInfo.Reference <= 0)
                throw new ArgumentException($"{processingInfo.Reference} должен быть больше нуля", nameof(processingInfo));

            using (IDbTransaction transaciton = _notificationRepository.BeginTransaction())
            {
                DateTime now = DateTime.Now;
                var onlinePayment = new OnlinePayment
                {
                    ContractId = contract.Id,
                    CreateDate = DateTime.Now,
                    Amount = processingInfo.Amount,
                    ProcessingBankName = processingInfo.BankName,
                    ProcessingBankNetwork = processingInfo.BankNetwork,
                    ProcessingId = processingInfo.Reference,
                    ProcessingStatus = ProcessingStatus.Created,
                    ProcessingType = processingInfo.Type,
                };
                _onlinePaymentRepository.Insert(onlinePayment);
                transaciton.Commit();
                return onlinePayment;
            }
        }
    }
}
