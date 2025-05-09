using System;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.Services.CardCashOut;
using Pawnshop.Web.Models.AbsOnlineCardCashOut;
using System.Threading.Tasks;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.CardCashOutTransaction;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core;
using Pawnshop.Web.Extensions;
using Serilog;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Services.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Services.TasCore;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Web.Models.JetPay;
using Pawnshop.Services.TasCore.Models.JetPay;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AbsOnlineCardCashoutController : Controller
    {
        private readonly ICardCashOutService _cardCashOutService;
        private readonly CardCashOutTransactionRepository _cardCashOutTransactionRepository;
        private readonly ILogger _logger;
        private readonly OnlineApplicationRepository _onlineApplicationRepository;

        public AbsOnlineCardCashoutController(ICardCashOutService cardCashOutService,
            CardCashOutTransactionRepository cardCashOutTransactionRepository,
            OnlineApplicationRepository onlineApplicationRepository,
            ILogger logger)
        {
            _cardCashOutService = cardCashOutService;
            _cardCashOutTransactionRepository = cardCashOutTransactionRepository;
            _logger = logger;
            _onlineApplicationRepository = onlineApplicationRepository;
        }

        [Authorize]
        [HttpGet("cardcashouttransaction/{contractid}")]
        public async Task<IActionResult> GetCardCashoutTransaction([FromRoute] int contractid)
        {
            var transactions = _cardCashOutTransactionRepository.GetByContractId(contractid);
            if (transactions == null)
                return NotFound();

            return Ok(transactions.Select(transaction => new CardCashoutTransactionView
            {
                Id = transaction.Id,
                Amount = transaction.Amount,
                CardHolderName = transaction.CardHolderName,
                CardNumber = transaction.CardNumber,
                ClientId = transaction.ClientId,
                ContractId = transaction.ContractId,
                CreateDate = transaction.CreateDate,
                CustomerReference = transaction.CustomerReference,
                DeleteDate = transaction.DeleteDate,
                Status = transaction.Status,
                Url = transaction.Url,
                TranGUID = transaction.TranGUID
            }));
        }

        [Authorize]
        [HttpGet("cardcashouttransactions")]
        public async Task<IActionResult> GetCardCashoutTransactions([FromQuery] string status, [FromQuery] PageBinding pageBinding)
        {
            var transactions = _cardCashOutTransactionRepository.GetListByStatus(status, pageBinding.Offset, pageBinding.Limit);
            if (transactions == null)
                return NotFound();

            return Ok(new CardCashoutTransactionsView
            {
                CardCashoutTransactions = transactions.Select(transaction => new CardCashoutTransactionView
                {
                    Id = transaction.Id,
                    Amount = transaction.Amount,
                    CardHolderName = transaction.CardHolderName,
                    CardNumber = transaction.CardNumber,
                    ClientId = transaction.ClientId,
                    ContractId = transaction.ContractId,
                    CreateDate = transaction.CreateDate,
                    CustomerReference = transaction.CustomerReference,
                    DeleteDate = transaction.DeleteDate,
                    Status = transaction.Status,
                    Url = transaction.Url,
                    TranGUID = transaction.TranGUID
                })
            });
        }

        [Authorize]
        [HttpGet("cardcashouttransactions/processing/{referencenr}/status")]
        [ProducesResponseType(typeof(GetCashOutTransactionStatusView), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> GetCashOutTransactionInProcessing([FromRoute] string referencenr, CancellationToken cancellationToken)
        {
            var cardCashOutTransaction = await _cardCashOutService.GetCashOutTransactionStatus(referencenr, cancellationToken);
            return Ok(new GetCashOutTransactionStatusView
            {
                Amount = cardCashOutTransaction.Body.getCashOutTransactionStatusResponse.@return.amount,
                CardIssuerCountry = cardCashOutTransaction.Body.getCashOutTransactionStatusResponse.@return.cardIssuerCountry,
                MaskedCardNumber = cardCashOutTransaction.Body.getCashOutTransactionStatusResponse.@return.maskedCardNumber,
                MerchantLocalDateTime = cardCashOutTransaction.Body.getCashOutTransactionStatusResponse.@return.merchantLocalDateTime,
                MerchantOnlineAddress = cardCashOutTransaction.Body.getCashOutTransactionStatusResponse.@return.merchantOnlineAddress,
                ReceiverName = cardCashOutTransaction.Body.getCashOutTransactionStatusResponse.@return.receiverName,
                RspCode = cardCashOutTransaction.Body.getCashOutTransactionStatusResponse.@return.rspCode,
                SenderName = cardCashOutTransaction.Body.getCashOutTransactionStatusResponse.@return.senderName,
                Success = cardCashOutTransaction.Body.getCashOutTransactionStatusResponse.@return.success,
                TransactionCurrencyCode = cardCashOutTransaction.Body.getCashOutTransactionStatusResponse.@return.transactionCurrencyCode,
                Verified3D = cardCashOutTransaction.Body.getCashOutTransactionStatusResponse.@return.verified3D,
                TransactionStatus = cardCashOutTransaction.Body.getCashOutTransactionStatusResponse.@return.transactionStatus
            });
        }

        [Authorize(Permissions.TasOnlineCreditAdministrator)]
        [HttpPost("cardcashouttransactions/create")]
        [ProducesResponseType(typeof(CreateCardCashOutView), 200)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        public async Task<IActionResult> CreateCardCashOut(
            [FromBody] CreateCardCashOutBinding binding,
            [FromServices] ClientRepository clientRepository,
            [FromServices] CashOrderRepository cashOrderRepository,
            CancellationToken cancellationToken)
        {
            try
            {
                Regex regex = new Regex("^[0-9]{1,12}$");

                if (!regex.IsMatch(binding.CustomerReference))
                {
                    throw new PawnshopApplicationException(
                        $"Параметр CustomerReference {binding.CustomerReference} должен быть числом из 1-12 знаков");
                }

                var client = clientRepository.Get(binding.ClientId);
                var clientReq = client.Requisites.FirstOrDefault(req => req.RequisiteTypeId == 2);
                if (clientReq == null || string.IsNullOrEmpty(clientReq.Value))
                {
                    throw new PawnshopApplicationException(
                        $"Клиент {binding.ClientId} не имеет реквизитов карты невозможно осуществить вывод");
                }

                var cashOrder = cashOrderRepository.GetCashOrderByContractIdWithoutParams(binding.ContractId);
                if (cashOrder == null)
                {
                    throw new PawnshopApplicationException(
                        $"У клиента : {binding.ClientId} нет кассовых ордеров.");
                }

                decimal amount = cashOrder.OrderCost;

                var result = await _cardCashOutService.StartCashOutTransaction(clientReq.Value,
                    ((int)(amount * 100)).ToString(), binding.CustomerReference, binding.ContractId, this.BaseUrl(), cancellationToken);
                if (result.Body.StartCashOutTransactionResponse.Return.Success == "false")
                {
                    throw new PawnshopApplicationException(
                        $"Время {DateTime.Now}. Сторонний сервис отклонил перевод обратитесь к в поддержку передав следующие данные : " +
                        $" {result.Body.StartCashOutTransactionResponse.Return.ErrorDescription}");
                }

                int position = result.Body.StartCashOutTransactionResponse.Return.RedirectURL.IndexOf("=");
                Guid guid = Guid.Empty;
                Guid.TryParse(result.Body.StartCashOutTransactionResponse.Return.RedirectURL.Substring(position + 1,
                    result.Body.StartCashOutTransactionResponse.Return.RedirectURL.Length - (position + 1)), out guid);

                _cardCashOutTransactionRepository.Insert(new CardCashOutTransaction
                {
                    Amount = ((int)amount).ToString(),
                    CardHolderName = clientReq.CardHolderName,
                    CardNumber = clientReq.Value,
                    ClientId = binding.ClientId,
                    ContractId = binding.ContractId,
                    CustomerReference = binding.CustomerReference,
                    Url = result.Body.StartCashOutTransactionResponse.Return.RedirectURL,
                    TranGUID = guid.ToString(),
                    Status = CardCashOutTransactionStatus.PendingApprove.ToString()
                });

                return Ok(new CreateCardCashOutView
                {
                    CustomerReference = binding.CustomerReference,
                    Url = result.Body.StartCashOutTransactionResponse.Return.RedirectURL,
                    Amount = (int)amount,
                    CardNumberHidden = clientReq.Value,
                    CardHolderNameHidden = clientReq.CardHolderName,
                    Portal = "TAS Finance",
                    ReferenceNr = binding.CustomerReference,
                    Sender = "TOO TAS FINANCE",
                    TranGuid = guid.ToString()
                });
            }
            catch (ProcessingServiceUnavailableException exception)
            {
                _logger.Error(exception, $"Processing Kz service unavaible requested by Url {exception.Url} with details {exception.Details} ");
                throw new PawnshopApplicationException($"Сервис Processing Kz не отвечает {exception.Details} ");
            }
            catch (UnexpectedResponseException exception)
            {
                _logger.Error(exception, $"Processing Kz  вернул неожиданный ответ : {exception.Response} с сообщением {exception.ExceptionMessage} и запросом {exception.RequestBody} ");
                throw new PawnshopApplicationException($"Processing Kz вернул неожиданный ответ : {exception.Response} с сообщением {exception.ExceptionMessage} и запросом {exception.RequestBody}  ");
            }
        }

        [Authorize(Policy = Permissions.TasOnlineCreditAdministrator)]
        [HttpPost("cardcashouttransactions/accept")]
        [ProducesResponseType(204)]
        [ProducesResponseType(typeof(ProblemDetails), 400)]
        [ProducesResponseType(typeof(ProblemDetails), 503)]
        public async Task<IActionResult> CardCashOutAccept(
            [FromServices] ICardCashOutSignService cardCashOutSignService,
            [FromBody] CardCashOutTransactionAccept binding,
            CancellationToken cancellationToken)
        {
            var cardCashOutTransaction = _cardCashOutTransactionRepository.GetByCustomerReference(binding.ReferenceNr);
            if (cardCashOutTransaction == null)
                throw new PawnshopApplicationException($"Вывод с данным customer reference {binding.ReferenceNr} не найдена");

            var transaction = _cardCashOutTransactionRepository.GetByCustomerReference(binding.ReferenceNr);
            var getStatus = await _cardCashOutService.GetCashOutTransactionStatus(binding.ReferenceNr, cancellationToken);
            var checkTransactions = _cardCashOutTransactionRepository.GetListByContractId(transaction.ContractId);

            if (checkTransactions.Count(tran => tran.Status == "Processed") > 0)
                throw new PawnshopApplicationException($"По данному договору уже есть выполненый вывод на карту");

            foreach (var checkedTransaction in checkTransactions)
            {
                var status = await _cardCashOutService.GetCashOutTransactionStatus(checkedTransaction.Status, cancellationToken);
                if (status.Body.getCashOutTransactionStatusResponse.@return.transactionStatus == "PAID")
                {
                    return BadRequest(
                        $"У клиента уже есть транзакция с номером {checkedTransaction.CustomerReference} в статусе проведена." +
                        $" НЕЗАМЕДЛИТЕЛЬНО ОБРАТИТЕСЬ В ПОДДЕРЖКУ!");
                }
            }

            if (getStatus.Body.getCashOutTransactionStatusResponse.@return.transactionStatus == "PENDING_APPROVEMENT")
            {
                var result = await _cardCashOutService.CompleteCashOutTransaction(binding.ReferenceNr,
                    (int.Parse(transaction.Amount) * 100).ToString(), cancellationToken);
                if (result.Body.completeCashOutTransactionResponse.@return.transactionStatus == "PROCESSED")
                {
                    transaction.Status = CardCashOutTransactionStatus.Processed.ToString();
                    _cardCashOutTransactionRepository.Update(transaction);

                    var transactions = _cardCashOutTransactionRepository.GetListByContractId(transaction.ContractId);
                    if (transactions.Count(tran => tran.Status == "Processed") == 1)
                    {
                        bool oldApplicationLogic = false;
                        var oldapplication = await _onlineApplicationRepository.FindByContractIdAsync(new { ContractId = transaction.ContractId.ToString() });
                        if (oldapplication != null)
                            oldApplicationLogic = true;

                        await cardCashOutSignService.Sign(contractId: transaction.ContractId, oldApplicationLogic);
                    }
                    else
                    {
                        throw new PawnshopApplicationException($"По данному уже есть выполненый вывод на карту");
                    }

                }
                else
                {
                    transaction.Status = CardCashOutTransactionStatus.Failed.ToString();
                    _cardCashOutTransactionRepository.Update(transaction);
                    throw new PawnshopApplicationException($"Вывод был отклонен в стороннем сервисе обратитесь в поддержку :{binding.ReferenceNr}");
                }
            }
            else
            {
                transaction.Status = CardCashOutTransactionStatus.Failed.ToString();
                _cardCashOutTransactionRepository.Update(transaction);
                throw new PawnshopApplicationException($"Вывод был отклонен либо уже переведен : {binding.ReferenceNr}");
            }
            return NoContent();
        }

        [Authorize]
        [HttpGet("cardcashouttransactions/{contractid}/payoutsystem")]
        public async Task<IActionResult> GetPayoutSystem(
            [FromRoute] int contractid,
            [FromServices] IContractService contractService,
            [FromServices] IContractActionService contractActionService,
            [FromServices] ClientRequisitesRepository clientRequisitesRepository,
            CancellationToken cancellationToken)
        {
            var contractActionList = await contractActionService.GetContractActionsByContractId(contractid);

            if (!contractActionList.Any())
                throw new PawnshopApplicationException($"По контратку {contractid} не найдены действия.");

            var requisiteId = contractActionList.FirstOrDefault(x => x.Status == ContractActionStatus.Await && x.RequisiteId.HasValue)?.RequisiteId;

            if (!requisiteId.HasValue)
                throw new PawnshopApplicationException($"По контратку {contractid} не найдено действие с выдачей.");

            var clientRequisite = clientRequisitesRepository.Get(requisiteId.Value);

            if (clientRequisite.CardType.HasValue && clientRequisite.CardType == ClientRequisiteCardType.JetPay)
            {
                return Ok(new CardCashoutPayoutSystemView { CardType = ClientRequisiteCardType.JetPay });
            }
            else if ((clientRequisite.CardType.HasValue && clientRequisite.CardType == ClientRequisiteCardType.Processing) ||
                    (!clientRequisite.CardType.HasValue && clientRequisite.RequisiteTypeId == 2))
            {
                return Ok(new CardCashoutPayoutSystemView { CardType = ClientRequisiteCardType.Processing });
            }
            else
            {
                throw new PawnshopApplicationException($"Неизвестный тип реквизита {clientRequisite.Id} {clientRequisite.CardType}");
            }
        }

        [Authorize]
        [HttpPost("cardcashouttransactions/{contractid}/create/jetpay")]
        public async Task<IActionResult> CreateCardCashoutJetPay(
            [FromRoute] int contractid,
            [FromServices] IContractService contractService,
            [FromServices] IContractActionService contractActionService,
            [FromServices] ITasCoreJetPayService jetPayService,
            [FromServices] CashOrderRepository cashOrderRepository,
            [FromServices] ClientRequisitesRepository clientRequisitesRepository,
            [FromServices] CardCashOutJetPayTransactionRepository cardCashOutJetPayTransactionRepository,
            CancellationToken cancellationToken)
        {
            var contract = contractService.GetOnlyContract(contractid, true);

            if (contract == null)
                throw new PawnshopApplicationException($"Контракт {contractid} не найден.");

            if (!(contract.Status == ContractStatus.AwaitForMoneySend || contract.Status == ContractStatus.InsuranceApproved))
                throw new PawnshopApplicationException($"Статус контракта {contractid} не соответствует действию.");

            var cashOrder = cashOrderRepository.GetCashOrderByContractIdWithoutParams(contractid);

            if (cashOrder == null)
                throw new PawnshopApplicationException($"У клиента : {contract.ClientId} нет кассовых ордеров.");

            var jetPayCardCashoutTransaction = await cardCashOutJetPayTransactionRepository.GetLastByContractIdAsync(contractid);

            if (jetPayCardCashoutTransaction != null && jetPayCardCashoutTransaction.Status != CardCashOutTransactionStatus.Failed)
                throw new PawnshopApplicationException($"Выдача на карту по договору {contractid} уже существует, проверьте статус операции!");

            var cashoutAction = await contractActionService.GetAsync(cashOrder.ContractActionId.Value);

            var clientRequisite = clientRequisitesRepository.Get(cashoutAction.RequisiteId.Value);

            if (clientRequisite.CardType != ClientRequisiteCardType.JetPay)
                throw new PawnshopApplicationException($"По контракту {contractid} не возможно выдача на карту JetPay.");

            var paymentId = Guid.NewGuid();

            var entityTransaction = new CardCashOutJetPayTransaction(
                contract.ClientId,
                contract.Id,
                clientRequisite.JetPayCardInfo.Id,
                paymentId,
                cashOrder.OrderCost);

            await cardCashOutJetPayTransactionRepository.InsertAsync(entityTransaction);

            var cardCashoutRequest = new TasCoreJetPayCardCashoutRequest(
                contract.ClientId,
                contract.Id,
                Convert.ToInt32(cashOrder.OrderCost),
                "KZT",
                clientRequisite.JetPayCardInfo.Token,
                clientRequisite.JetPayCardInfo.CustomerIp,
                clientRequisite.JetPayCardInfo.CustomerId,
                paymentId);

            var createResult = await jetPayService.CreateCardCashoutAsync(cardCashoutRequest, cancellationToken);

            if (!createResult.Success)
            {
                entityTransaction.Status = CardCashOutTransactionStatus.Failed;
                await cardCashOutJetPayTransactionRepository.UpdateAsync(entityTransaction);
                throw new PawnshopApplicationException($"Ошибка выполнения действия {createResult.Message}");
            }

            return NoContent();
        }

        [Authorize]
        [HttpGet("cardcashouttransactions/{contractid}/jetpay/status")]
        public async Task<IActionResult> GetJetPayStatus(
            [FromRoute] int contractid,
            [FromServices] IContractService contractService,
            [FromServices] CardCashOutJetPayTransactionRepository cardCashOutJetPayTransactionRepository,
            [FromServices] ITasCoreJetPayService jetPayService,
            CancellationToken cancellationToken)
        {
            var contract = contractService.GetOnlyContract(contractid, true);

            if (contract == null)
                throw new PawnshopApplicationException($"Договор {contractid} не найден.");

            if (contract.Status == ContractStatus.Signed || contract.Status == ContractStatus.BoughtOut)
                throw new PawnshopApplicationException($"Договор {contractid} находится в некорректном статусе.");

            var jetPayCardCashoutTransaction = await cardCashOutJetPayTransactionRepository.GetLastByContractIdAsync(contract.Id);

            if (jetPayCardCashoutTransaction == null)
                throw new PawnshopApplicationException($"Договор {contractid} не имеет выдачи в JetPay.");

            var status = await jetPayService.GetStatusAsync(jetPayCardCashoutTransaction.PaymentId, cancellationToken);
            return Ok(new { status });
        }

        [Authorize]
        [HttpGet("cardcashouttransactions/{contractid}/jetpay/card")]
        public async Task<IActionResult> GetJetPayCardInfo(
            [FromRoute] int contractid,
            [FromServices] IContractService contractService,
            [FromServices] IContractActionService contractActionService,
            [FromServices] CashOrderRepository cashOrderRepository,
            [FromServices] ClientRequisitesRepository clientRequisitesRepository)
        {
            var contract = contractService.GetOnlyContract(contractid, true);

            if (contract == null)
                throw new PawnshopApplicationException($"Контракт {contractid} не найден.");

            if (!(contract.Status == ContractStatus.AwaitForMoneySend || contract.Status == ContractStatus.InsuranceApproved))
                throw new PawnshopApplicationException($"Статус контракта {contractid} не соответствует действию.");

            var cashOrder = cashOrderRepository.GetCashOrderByContractIdWithoutParams(contractid);

            if (cashOrder == null)
                throw new PawnshopApplicationException($"У клиента : {contract.ClientId} нет кассовых ордеров.");

            var cashoutAction = await contractActionService.GetAsync(cashOrder.ContractActionId.Value);

            var clientRequisite = clientRequisitesRepository.Get(cashoutAction.RequisiteId.Value);

            if (clientRequisite.CardType != ClientRequisiteCardType.JetPay)
                throw new PawnshopApplicationException($"По контракту {contractid} не возможно выдача на карту JetPay.");

            return Ok(new JetPayCardInfoView
            {
                Amount = (int)cashOrder.OrderCost,
                CardHolderNameHidden = clientRequisite.CardHolderName,
                CardNumberHidden = clientRequisite.Value,
                Portal = "TAS Finance",
                Sender = "TOO TAS FINANCE",
            });
        }

        [AllowAnonymous]
        [HttpPost("cardcashouttransactions/jetpay/status")]
        public async Task<IActionResult> SetJetPayStatus(
            [FromBody] JetPayCardCashoutStatusBinding binding,
            [FromServices] IContractService contractService,
            [FromServices] ITasCoreJetPayService jetPayService,
            [FromServices] CardCashOutJetPayTransactionRepository cardCashOutJetPayTransactionRepository,
            [FromServices] ICardCashOutSignService cardCashOutSignService,
            CancellationToken cancellationToken)
        {
            var contract = contractService.GetOnlyContract(binding.ContractId, true);

            if (contract == null)
                throw new PawnshopApplicationException($"Контракт {binding.ContractId} не найден.");

            if (!(contract.Status == ContractStatus.AwaitForMoneySend || contract.Status == ContractStatus.InsuranceApproved))
                throw new PawnshopApplicationException($"Статус контракта {binding.ContractId} не соответствует действию.");

            var status = (CardCashOutTransactionStatus)binding.Status;

            var cashoutTransaction = await cardCashOutJetPayTransactionRepository.GetByPaymentIdAsync(binding.PaymentId);

            if (cashoutTransaction == null)
                throw new PawnshopApplicationException($"Платеж {binding.PaymentId} не найден.");

            if (cashoutTransaction.Status != CardCashOutTransactionStatus.Created)
                throw new PawnshopApplicationException($"Статус платежа {binding.PaymentId} не возможно обновить с {cashoutTransaction.Status.ToString()} на {status.ToString()}.");

            cashoutTransaction.Status = status;
            cashoutTransaction.Message = binding.Message;
            await cardCashOutJetPayTransactionRepository.UpdateAsync(cashoutTransaction);

            if (status == CardCashOutTransactionStatus.PendingApprove)
                await cardCashOutSignService.Sign(binding.ContractId);

            return NoContent();
        }
    }
}
