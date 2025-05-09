using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.CardTopUpTransaction;
using Pawnshop.Services.CardTopUp;
using Pawnshop.Web.Models.AbsOnlineCardTopUp;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.OnlinePayments;
using Pawnshop.Data.Models.Processing;
using System.Data;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Services.Contracts;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Services;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AbsOnlineCardTopUpController : Controller
    {
        private readonly ICardTopUpService _cardTopUpService;
        private readonly CardTopUpTransactionRepository _cardTopUpTransactionRepository;
        private readonly OnlinePaymentRepository _onlinePaymentRepository;
        private readonly IContractService _contractService;
        private readonly ClientRepository _clientRepository;
        private readonly NotificationRepository _notificationRepository;
        private readonly NotificationReceiverRepository _notificationReceiverRepository;
        private readonly IEventLog _eventLog;
        public AbsOnlineCardTopUpController(ICardTopUpService cardTopUpService,
            CardTopUpTransactionRepository cardTopUpTransactionRepository,
            OnlinePaymentRepository onlinePaymentRepository, IContractService contractService,
            ClientRepository clientRepository, NotificationRepository notificationRepository,
            NotificationReceiverRepository notificationReceiverRepository,
            IEventLog eventLog)
        {
            _cardTopUpService = cardTopUpService;
            _cardTopUpTransactionRepository = cardTopUpTransactionRepository;
            _onlinePaymentRepository = onlinePaymentRepository;
            _contractService = contractService;
            _clientRepository = clientRepository;
            _notificationRepository = notificationRepository;
            _notificationReceiverRepository = notificationReceiverRepository;
            _eventLog = eventLog;
        }

        [HttpPost("cardtopuptransactions/create")]
        [ProducesResponseType(typeof(MobilePayView), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> StartTransaction([FromBody] MobilePayBinding binding, CancellationToken cancellationToken)
        {
            Contract contract = null;

            if (binding.contract_id.Contains("CL"))
                contract = await _contractService.GetCreditLineByNumberAsync(binding.contract_id.Replace("CL", string.Empty));
            else
                contract = await _contractService.GetNonCreditLineByNumberAsync(binding.contract_id);

            if (contract == null)
                return BadRequest();

            if (contract.CreditLineId == null && contract.ContractClass != ContractClass.CreditLine)
                return BadRequest($"CreditLine for contract {contract.ContractNumber} not found ");

            var creditLine = contract.ContractClass == ContractClass.CreditLine ? contract : _contractService.Get(contract.CreditLineId.Value, DateTime.Now);

            int topUpedContractId = creditLine.Id;

            if (contract.IsOffBalance)
                topUpedContractId = contract.Id;

            if (contract.Status != ContractStatus.Signed)
                return BadRequest();

            string chars = "123456789";
            Random random = new Random();
            string referenceNr = new string(Enumerable.Repeat(chars, 12)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            string orderId = new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());

            var response = await _cardTopUpService.StartTransaction(referenceNr, (int.Parse(binding.totalAmount)).ToString(),
                int.Parse(orderId), cancellationToken);

            _cardTopUpTransactionRepository.Insert(new CardTopUpTransaction
            {
                Amount = binding.totalAmount,
                ClientId = contract.ClientId,
                ContractId = topUpedContractId,
                CreateDate = DateTime.Now,
                CustomerReference = referenceNr,
                OrderId = int.Parse(orderId),// TOdo long int
                Status = "Created",
                UpdateDate = DateTime.Now,
                Url = response.Body.startTransactionResponse.@return.redirectURL
            });

            return Ok(new MobilePayView
            {
                contract_id = binding.contract_id,
                redirectURL = response.Body.startTransactionResponse.@return.redirectURL,
                totalAmount = binding.totalAmount
            });
        }

        [HttpGet("cardtopuptransactions/complete")]

        [ProducesResponseType(typeof(MobilePayView), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> CompleteTransaction([FromQuery] string customerReference,
            CancellationToken cancellationToken)
        {
            var cardTopUpTransaction = _cardTopUpTransactionRepository.GetByCustomerReference(customerReference);
            if (cardTopUpTransaction == null)
            {
                return BadRequest();
            }

            var transactionStatus = await _cardTopUpService.GetTransactionStatusCode(customerReference, cancellationToken);
            if (transactionStatus == null)
                return BadRequest();
            if (transactionStatus.Body.getTransactionStatusCodeResponse.@return.transactionStatus != "AUTHORISED")
            {
                return BadRequest();
            }

            var complete = await _cardTopUpService.CompleteTransaction(customerReference, cancellationToken);

            if (complete.Body.completeTransactionResponse.@return == false)
            {
                cardTopUpTransaction.Status = "Failed";
                cardTopUpTransaction.UpdateDate = DateTime.Now;
                _cardTopUpTransactionRepository.Update(cardTopUpTransaction);
                return BadRequest();
            }

            cardTopUpTransaction.Status = "Paid";
            cardTopUpTransaction.UpdateDate = DateTime.Now;
            _cardTopUpTransactionRepository.Update(cardTopUpTransaction);


            try
            {
                OnlinePayment onlinePayment = _onlinePaymentRepository.GetByProcessing(cardTopUpTransaction.OrderId, ProcessingType.Processing);
                if (onlinePayment != null)
                {
                    return BadRequest();
                }
            }
            catch
            {
                return BadRequest();
            }


            Contract contractForAmount = new Contract();
            try
            {
                contractForAmount = _contractService.Get(Convert.ToInt32(cardTopUpTransaction.ContractId));
            }
            catch
            {
                return BadRequest();
            }

            var processingInfo = new ProcessingInfo
            {
                Amount = decimal.Parse(cardTopUpTransaction.Amount) / 100, // Сумма которая в базе в тиынах у процессинга в тенге 
                Reference = cardTopUpTransaction.OrderId,
                Type = ProcessingType.Processing,
                BankName = null,
                BankNetwork = null
            };
            using (IDbTransaction transaction = _clientRepository.BeginTransaction())
            {
                OnlinePayment onlinePayment = QueueOnlinePayment(contractForAmount, processingInfo);
                if (onlinePayment == null)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(QueueOnlinePayment)} не вернет null");

                if (!onlinePayment.Amount.HasValue)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(onlinePayment)}.{nameof(onlinePayment.Amount)} не будет null");

                _eventLog.Log(EventCode.Prepayment, EventStatus.Success, EntityType.Contract, contractForAmount.Id, responseData: $@"Принята оплата по кредиту: {onlinePayment.Amount.Value} KZT от KASSA 24");
                transaction.Commit();
            }
            return Ok();
        }

        [HttpGet("cardtopuptransactions/get")]
        [ProducesResponseType(typeof(GetTransactionStatusView), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> GetTransactionStatus([FromQuery] GetTransactionStatusBinding binding,
            CancellationToken cancellationToken)
        {
            return Ok();
        }

        [HttpPost("cardtopuptransactions/checkall")]
        [ProducesResponseType(typeof(GetTransactionStatusView), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> CheckAll(CancellationToken cancellationToken)
        {
            DateTime from = DateTime.Now.AddDays(-3);
            DateTime to = DateTime.Now.AddDays(1);
            var transactions = _cardTopUpTransactionRepository.GetTopUpTransactionsForPeriod(from, to);
            for (int i = 0; i < transactions.Count; i++)
            {
                var transactionStatus = await _cardTopUpService.GetTransactionStatusCode(transactions[i].CustomerReference, cancellationToken);
                if (transactionStatus != null)
                {
                    if (transactionStatus.Body.getTransactionStatusCodeResponse.@return.transactionStatus ==
                        "AUTHORISED" || transactionStatus.Body.getTransactionStatusCodeResponse.@return.transactionStatus == "PAID")
                    {
                        try
                        {
                            if (transactionStatus.Body.getTransactionStatusCodeResponse.@return.transactionStatus ==
                                "AUTHORISED")
                            {
                                var complete =
                                    await _cardTopUpService.CompleteTransaction(transactions[i].CustomerReference,
                                        cancellationToken);
                                if (complete.Body.completeTransactionResponse.@return == false)
                                {
                                    transactions[i].Status = "Failed";
                                    transactions[i].UpdateDate = DateTime.Now;
                                    _cardTopUpTransactionRepository.Update(transactions[i]);
                                    break;
                                }
                            }

                            transactions[i].Status = "Paid";
                            transactions[i].UpdateDate = DateTime.Now;
                            _cardTopUpTransactionRepository.Update(transactions[i]);
                            var contractForAmount = _contractService.Get(Convert.ToInt32(transactions[i].ContractId));
                            var processingInfo = new ProcessingInfo
                            {
                                Amount = decimal.Parse(transactions[i].Amount) /
                                         100, // Сумма которая в базе в тиынах
                                Reference = transactions[i].OrderId,
                                Type = ProcessingType.Processing,
                                BankName = null,
                                BankNetwork = null
                            };
                            using (IDbTransaction transaction = _clientRepository.BeginTransaction())
                            {
                                OnlinePayment onlinePayment = QueueOnlinePayment(contractForAmount, processingInfo);
                                if (onlinePayment == null)
                                    throw new PawnshopApplicationException(
                                        $"Ожидалось что {nameof(QueueOnlinePayment)} не вернет null");

                                if (!onlinePayment.Amount.HasValue)
                                    throw new PawnshopApplicationException(
                                        $"Ожидалось что {nameof(onlinePayment)}.{nameof(onlinePayment.Amount)} не будет null");

                                _eventLog.Log(EventCode.Prepayment, EventStatus.Success, EntityType.Contract,
                                    contractForAmount.Id,
                                    responseData:
                                    $@"Принята оплата по кредиту: {onlinePayment.Amount.Value} KZT от KASSA 24");
                                transaction.Commit();
                            }
                        }
                        catch (Exception ex)
                        {
                            transactions[i].Status = "Failed";
                            transactions[i].UpdateDate = DateTime.Now;
                            _cardTopUpTransactionRepository.Update(transactions[i]);

                            throw new PawnshopApplicationException(ex.Message);
                            break;
                        }
                    }
                }
            }
            return Ok();
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
