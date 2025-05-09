using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.TasOnline;
using Pawnshop.Data.Models.UKassa;
using Pawnshop.Services.Models.UKassa;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pawnshop.Services.Integrations.UKassa
{
    public class UKassaService : IUKassaService
    {
        private readonly UKassaRepository _uKassaRepository;
        private readonly IUKassaHttpService _httpService;
        private readonly UKassaAccountSettingsRepository _uKassaAccountsRepository;
        private readonly CashOrderRepository _cashOrderRepository;
        private readonly UKassaBOSettingsRepository _uKassaBOSettingsRepository;
        private readonly DomainValueRepository _domainValueRepository;
        private readonly ClientContactRepository _clientContactRepository;
        private readonly AccountRepository _accountRepository;
        private readonly ILogger<UKassaService> _logger;

        public UKassaService(UKassaRepository uKassaRepository, IUKassaHttpService httpService,
            UKassaAccountSettingsRepository uKassaAccountsRepository, CashOrderRepository cashOrderRepository,
            UKassaBOSettingsRepository uKassaBOSettingsRepository,
            DomainValueRepository domainValueRepository,
            ClientContactRepository clientContactRepository,
            AccountRepository accountRepository,
            ILogger<UKassaService> logger)
        {
            _uKassaRepository = uKassaRepository;
            _httpService = httpService;
            _uKassaAccountsRepository = uKassaAccountsRepository;
            _cashOrderRepository = cashOrderRepository;
            _uKassaBOSettingsRepository = uKassaBOSettingsRepository;
            _domainValueRepository = domainValueRepository;
            _clientContactRepository = clientContactRepository;
            _accountRepository = accountRepository;
            _logger = logger;
        }

        public IDbTransaction BeginUKassaTransaction()
        {
            return _uKassaRepository.BeginTransaction();
        }

        public void CreateCheckRequest(CashOrder order)
        {
            var uKassaBOSettings = _uKassaBOSettingsRepository.GetByBOId(order.BusinessOperationSettingId.Value);
            if (uKassaBOSettings != null)
            {
                var account = _accountRepository.GetBranchMainAccount(order.BranchId);
                int accountId = account != null ? account.Id : 0;

                if (uKassaBOSettings.CheckOperationType != null || uKassaBOSettings.CheckStornoOperationType != null)
                {
                    var operationType = order.StornoId.HasValue ? uKassaBOSettings.CheckStornoOperationType : uKassaBOSettings.CheckOperationType;
                    var domainValue = _domainValueRepository.Get(uKassaBOSettings.NomenclatureId.HasValue ? uKassaBOSettings.NomenclatureId.Value : 0);
                    order.Language = _cashOrderRepository.GetCashOrderPrintLanguageByOrderId(order.Id).Result;
                    string domainValueName = domainValue.Name;
                    if(order.Language != null)
                    {
                        switch(order.Language.Code)
                        {
                            case Constants.KZ_LANGUAGE_CODE:
                                domainValueName = domainValue.NameAlt;
                                break;
                            case Constants.RU_LANGUAGE_CODE:
                                domainValueName = domainValue.Name;
                                break;
                            default: 
                                domainValueName = domainValue.Name;
                                break;
                        }
                    }
                    GenerateCheck(order.ApprovedId.Value, accountId, order.Id, order.ClientId, operationType.Value,
                        uKassaBOSettings.PaymentType, domainValueName, 1, order.OrderCost);
                }
                else if (uKassaBOSettings.CashOperationType != null || uKassaBOSettings.CashStornoOperationType != null)
                {
                    var operationType = order.StornoId.HasValue ? uKassaBOSettings.CashStornoOperationType : uKassaBOSettings.CashOperationType;
                    CashOperation(order.ApprovedId.Value, accountId, order.Id, operationType.Value, order.OrderCost);
                }
            }
        }

        public void GenerateCheck(int authorId, int accountId, int cashOrderId, int? clientId, int operationType, int paymentType, string itemName, int quantity, decimal itemPrice)
        {
            _logger.LogInformation($"Start GenerateCheck. CashOrderId: {cashOrderId}, AuthorId: {authorId}, AccountId: {accountId}");
            var request = _uKassaRepository.GetByOrderId(cashOrderId);
            if (request != null)
            {
                _logger.LogInformation($"GenerateCheck double cashOrderId: {cashOrderId}. Return");
                return;
            }

            var uKassaAccount = _uKassaAccountsRepository.GetByAccountId(accountId);
            if (uKassaAccount == null)
                return;

            var payments = new List<Payment>();
            payments.Add(new Payment() { payment_type = paymentType, total = quantity * itemPrice });
            var items = new List<Item>();
            items.Add(new Item()
            {
                name = itemName,
                section = uKassaAccount.SectionId,
                quantity = quantity,
                price = itemPrice,
                sum = quantity * itemPrice,
                total = quantity * itemPrice,
                nds_percent = 12,
                quantity_type = 796,
                excise_type = 1
            });
            var requestModel = new UKassaGenerateCheckRequest()
            {
                operation_type = operationType,
                kassa = uKassaAccount.KassaId,
                total_amount = quantity * itemPrice,
                change = 0,
                currency = false,
                check_type = 0,
                tax = 0,
                payments = payments,
                items = items
            };

            if (clientId.HasValue)
            {
                var emailContact = _clientContactRepository.Find(new { ClientId = clientId, SendCheck = true, ContactType = 4 });
                requestModel.email = emailContact != null ? emailContact.Address : null;
                var phoneContact = _clientContactRepository.Find(new { ClientId = clientId, SendCheck = true, ContactType = 1 });
                requestModel.phone = phoneContact != null ? phoneContact.Address : null;
            }

            var newRequest = new UKassaRequest()
            {
                RequestId = Guid.NewGuid(),
                CashOrderId = cashOrderId,
                RequestData = JsonConvert.SerializeObject(requestModel),
                AuthorId = authorId,
                KassaId = uKassaAccount.KassaId,
                SectionId = uKassaAccount.SectionId,
                TotalAmount = quantity * itemPrice,
                Status = TasOnlineRequestStatus.New,
                OperationType = operationType,
                RequestUrl = "generate_check"
            };
            _uKassaRepository.Insert(newRequest);
        }

        public void CashOperation(int authorId, int accountId, int cashOrderId, int operationType, decimal amount)
        {
            _logger.LogInformation($"Start CashOperation. CashOrderId: {cashOrderId}, AuthorId: {authorId}, AccountId: {accountId}");
            var request = _uKassaRepository.GetByOrderId(cashOrderId);
            if (request != null)
            {
                _logger.LogInformation($"CashOperation double cashOrderId: {cashOrderId}. Return");
                return;
            }

            var uKassaAccount = _uKassaAccountsRepository.GetByAccountId(accountId);
            if (uKassaAccount == null)
                return;
            var requestModel = new UKassaCashOperationRequest()
            {
                amount = amount,
                kassa = uKassaAccount.KassaId,
                operation_type = operationType
            };
            var newRequest = new UKassaRequest()
            {
                RequestId = Guid.NewGuid(),
                CashOrderId = cashOrderId,
                RequestData = JsonConvert.SerializeObject(requestModel),
                AuthorId = authorId,
                KassaId = uKassaAccount.KassaId,
                SectionId = uKassaAccount.SectionId,
                TotalAmount = amount,
                Status = TasOnlineRequestStatus.New,
                RequestUrl = "cash_operation"
            };
            _uKassaRepository.Insert(newRequest);
        }

        public void ResendRequest(int orderId)
        {
            _logger.LogInformation($"Start ResendRequest. OrderId: {orderId}");
            var order = _cashOrderRepository.Get(orderId);
            if (order != null)
            {
                var request = _uKassaRepository.GetByOrderId(orderId);
                if (request != null && (request.Status == TasOnlineRequestStatus.Error || request.Status == TasOnlineRequestStatus.New))
                {
                    using (var transaction = _cashOrderRepository.BeginTransaction())
                    {
                        _uKassaRepository.UpdateStatus(request.Id, TasOnlineRequestStatus.New);
                        transaction.Commit();
                        FinishRequests(new List<int> { orderId });
                    }
                }
                else if (request == null)
                {
                    using (var transaction = _cashOrderRepository.BeginTransaction())
                    {
                        CreateCheckRequest(order);
                        transaction.Commit();
                        FinishRequests(new List<int> { orderId });
                    }
                }
                else
                {
                    throw new PawnshopApplicationException($"Запрос уже был успешно отправлен в UKassa.");
                }
            }
            else
            {
                throw new PawnshopApplicationException($"Запрос уже был успешно отправлен в UKassa.");
            }
        }

        public void FinishRequests(List<int> orders)
        {
            _logger.LogInformation($"Start FinishRequests. Orders: {string.Join(',', orders)}");
            Thread.Sleep(1000);
            using (IDbTransaction transaction = BeginUKassaTransaction())
            {
                var requests = _uKassaRepository.GetNewRequests(orders);
                _logger.LogInformation($"FinishRequests. Found {requests.Count} waiting requests.");
                foreach (var request in requests.Distinct())
                {
                    Thread.Sleep(1000);
                    _logger.LogInformation($"FinishRequests. Sending CashOrderId: {request.CashOrderId}, RequestId: {request.RequestId}");
                    try
                    {
                        request.RequestDate = DateTime.Now;
                        var response = _httpService.SendRequest(request.RequestId.ToString(), request.RequestUrl, request.RequestData);
                        request.ResponseDate = DateTime.Now;
                        if (response.Item1)
                        {
                            request.ResponseData = response.Item2;
                            var obj = JsonConvert.DeserializeObject<UKassaGenerateCheckResponse>(response.Item2);
                            request.ResponseCheckNumber = obj.fixed_check;
                            request.ShiftNumber = obj.shift;
                            request.Status = TasOnlineRequestStatus.Done;
                            _uKassaRepository.Update(request);
                        }
                        else if (!response.Item1 && response.Item2 != "Already_sent")
                        {
                            request.Status = TasOnlineRequestStatus.Error;
                            request.ResponseData = response.Item2;
                            _uKassaRepository.Update(request);
                        }
                        _logger.LogInformation($"FinishRequests. Sent {request.CashOrderId}. Result {request.Status}");
                    }
                    catch (Exception ex)
                    {
                        request.Status = TasOnlineRequestStatus.Error;
                        _uKassaRepository.Update(request);
                        _logger.LogError($"FinishRequests. Sent {request.CashOrderId}. Result {ex.Message}.");
                    }
                }
                var ids = requests.Select(x => x.Id).ToList();
                _uKassaRepository.ReturnToNewState(ids);
                transaction.Commit();
            }
        }
    }
}