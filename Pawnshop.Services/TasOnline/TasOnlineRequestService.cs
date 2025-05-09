using System;
using Pawnshop.Core;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.TasOnline;
using Pawnshop.Services.Models.TasOnline;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Domains;

namespace Pawnshop.Services.TasOnline
{
    public class TasOnlineRequestService : BaseService<TasOnlineRequest>, ITasOnlineRequestService
    {
        private readonly ISessionContext _sessionContext;
        private readonly ITasOnlinePaymentApi _tasOnlinePaymentApi;
        private readonly ClientRepository _clientRepository;
        private readonly ClientLegalFormRepository _clientLegalFormRepository;
        private readonly CountryRepository _countryRepository;
        private readonly IDomainService _domainService;
        private readonly ClientContactRepository _clientContactRepository;
        private readonly IBusinessOperationService _businessOperationService;
        private readonly PayTypeRepository _payTypeRepository;
        private readonly TasOnlinePaymentRepository _tasOnlinePaymentRepository;
        private readonly IEventLog _eventLog;
        private readonly CashOrderRepository _cashOrderRepository;
        private readonly UserRepository _userRepository;

        public TasOnlineRequestService(IRepository<TasOnlineRequest> repository, 
            ISessionContext sessionContext,
            ITasOnlinePaymentApi tasOnlinePaymentApi, 
            ClientRepository clientRepository,
            ClientLegalFormRepository clientLegalFormRepository,
            CountryRepository countryRepository,
            IDomainService domainService,
            ClientContactRepository clientContactRepository,
            IBusinessOperationService businessOperationService,
            PayTypeRepository payTypeRepository,
            TasOnlinePaymentRepository tasOnlinePaymentRepository,
            IEventLog eventLog,
            CashOrderRepository cashOrderRepository,
            UserRepository userRepository) : base(repository)
        {
            _sessionContext = sessionContext;
            _tasOnlinePaymentApi = tasOnlinePaymentApi;
            _clientRepository = clientRepository;
            _clientLegalFormRepository = clientLegalFormRepository;
            _countryRepository = countryRepository;
            _domainService = domainService;
            _clientContactRepository = clientContactRepository;
            _businessOperationService = businessOperationService;
            _payTypeRepository = payTypeRepository;
            _tasOnlinePaymentRepository = tasOnlinePaymentRepository;
            _eventLog = eventLog;
            _cashOrderRepository = cashOrderRepository;
            _userRepository = userRepository;
        }

        private CashOrder GetCashOrder(int cashOrderId)
        {
            if (cashOrderId == 0)
                throw new PawnshopApplicationException("Кассовый ордер не выбран");

            var cashOrder = _cashOrderRepository.Get(cashOrderId);

            if (cashOrder is null)
                throw new PawnshopApplicationException($"Кассовый ордер с Id {cashOrderId} не найден");

            if (cashOrder.DeleteDate.HasValue)
                throw new PawnshopApplicationException("Кассовый ордер удален");

            var hasStorno = _cashOrderRepository.GetOrderByStornoId(cashOrderId) != null;
            if (hasStorno)
                throw new PawnshopApplicationException("Кассовый ордер сторнирован, проверка не возможна");

            return cashOrder;
        }
        public TasOnlinePayment CheckPayment(int cashOrderId)
        {
            var cashOrder = GetCashOrder(cashOrderId);

            var parameters = new
            {
                amount = cashOrder.OrderCost, 
                idTransaction = cashOrder.Id
            };

            var request = SendRequest<TasOnlinePaymentResponse>("checkpay", parameters);

            TasOnlinePayment payment = null;
            if (request.Status == TasOnlineRequestStatus.Done)
            {
                payment = _tasOnlinePaymentRepository.FindPaymentByOrderId(cashOrderId);

                if (payment is null)
                    throw new PawnshopApplicationException($"Транзакция для кассого ордера с номером {cashOrder.OrderNumber} не найдена");

                using (var transaction = _repository.BeginTransaction())
                {
                    var responseData = (TasOnlinePaymentResponse)request.ResponseDataObject;
                    payment.Status = (TasOnlinePaymentStatus)responseData.Code;
                    payment.TasOnlineDocumentId = responseData.TasOnlineDocumentId;

                    _tasOnlinePaymentRepository.Update(payment);

                    transaction.Commit();
                }
            }
            
            return payment;
        }

        public TasOnlinePayment RePayment(int cashOrderId)
        {
            var cashOrder = GetCashOrder(cashOrderId);

            var payment = _tasOnlinePaymentRepository.FindPaymentByOrderId(cashOrderId);

            if (payment is null)
                throw new PawnshopApplicationException($"Транзакция для кассого ордера с номером {cashOrder.OrderNumber} не найдена");

            var parameters = new
            {
                id = payment.TasOnlineContractId,
                amount = cashOrder.OrderCost,
                idTransaction = cashOrderId
            };

            var request = SendRequest<TasOnlinePaymentResponse>("pay", parameters, payment.Id);

            if (request.Status == TasOnlineRequestStatus.Done)
            {
                using (var transaction = _repository.BeginTransaction())
                {
                    var responseData = (TasOnlinePaymentResponse)request.ResponseDataObject;
                    payment.Status = (TasOnlinePaymentStatus)responseData.Code;
                    payment.TasOnlineDocumentId = responseData.TasOnlineDocumentId;

                    _tasOnlinePaymentRepository.Update(payment);

                    transaction.Commit();
                }
            }

            return payment;
        }

        private TasOnlineRequest SendRequest<T>(string url, object parameters, int? paymentId = null)
        {
            var request = new TasOnlineRequest()
            {
                RequestData = string.Empty,
                CreateDate = DateTime.Now,
                AuthorId = _sessionContext.UserId,
                Status = TasOnlineRequestStatus.New
            };

            Save(request);

            var response = _tasOnlinePaymentApi.Send(url, parameters);

            request.RequestData = response.Url;
            request.Status = TasOnlineRequestStatus.Done;
            request.PaymentId = paymentId;
            request.ResponseData = response.Content;

            if (!string.IsNullOrWhiteSpace(response.ErrorMessage))
            {
                request.Status = TasOnlineRequestStatus.Error;
                request.ResponseData = response.ErrorMessage;
            }

            if (request.Status == TasOnlineRequestStatus.Done)
            {
                try
                {
                    request.ResponseDataObject = JsonConvert.DeserializeObject<T>(response.Content);
                }
                catch (Exception exception)
                {
                    _eventLog.Log(EventCode.TasOnlineRequestDeserialize, EventStatus.Failed, requestData: string.Empty,
                        responseData: exception.Message, uri: request.RequestData, entityType: null);
                    request.Status = TasOnlineRequestStatus.DeserializeError;
                    Save(request);
                }
            }

            Save(request);

            if (request.Status == TasOnlineRequestStatus.Error)
                throw new PawnshopApplicationException(request.ResponseData);

            return request;
        }

        public ClientContractModel GetContractsByIdentityNumber(string identityNumber)
        {
            if (string.IsNullOrWhiteSpace(identityNumber))
                throw new PawnshopApplicationException("ИИН не введен");

            var iinRegex = new Regex(Constants.IIN_REGEX);

            if (!iinRegex.IsMatch(identityNumber))
                throw new PawnshopApplicationException("В поле ИИН должно быть 12 цифр");

            var parameters = new
            {
                identityNumber
            };

            var model = new ClientContractModel();

            var request = SendRequest<TasOnlineResponseData>("info", parameters);

            if (request.Status == TasOnlineRequestStatus.Done)
            {
                var responseData = (TasOnlineResponseData)request.ResponseDataObject;
                var code = (TasOnlinePaymentStatus) responseData.Code;

                if (code == TasOnlinePaymentStatus.ClientNotFound)
                    throw new PawnshopApplicationException(
                        $"Клиент не найден в ТасОнлайн");

                if (code == TasOnlinePaymentStatus.New && (responseData.Contracts is null || responseData.Contracts.Count == 0))
                    throw new PawnshopApplicationException(
                        $"Для клиента не найдны договора в ТасОнлайн");

                model.Contracts = responseData.Contracts;

                var client = _clientRepository.FindByIdentityNumberAndLegalFormCode(responseData.IdentityNumber, responseData.LegalForm);
                ClientContact contact = null;

                if (client is null)
                {
                    var legalForm = _clientLegalFormRepository.Find(new { Code = responseData.LegalForm });

                    if (legalForm is null)
                        throw new PawnshopApplicationException(
                            $"Не найдена правовая форма с кодом {responseData.LegalForm}");

                    var country = _countryRepository.Find(new { Code = responseData.Citizenship });

                    if (country is null)
                        throw new PawnshopApplicationException(
                            $"Не найдена страна с кодом {responseData.Citizenship}");

                    client = new Client()
                    {
                        CardType = CardType.Standard,
                        IdentityNumber = responseData.IdentityNumber,
                        Surname = responseData.Surname,
                        Name = responseData.Name,
                        Patronymic = responseData.Patronymic,
                        FullName = responseData.Fullname,
                        IsMale = responseData.IsMale,
                        BirthDay = responseData.BirthDay,
                        IsResident = responseData.IsResident,
                        LegalFormId = legalForm.Id,
                        IsPolitician = responseData.IsPEP,
                        CitizenshipId = country.Id,
                        AuthorId = _sessionContext.UserId,
                        CreateDate = DateTime.Now
                    };

                    var phoneRegex = new Regex(Constants.PHONE_REGEX);

                    if (!phoneRegex.IsMatch(responseData.MobilePhone))
                        throw new PawnshopApplicationException("В поле телефон должно быть 11 цифр");

                    var contactType = _domainService.GetDomainValue(Constants.DOMAIN_CONTACT_TYPE_CODE, Constants.DOMAIN_VALUE_MOBILE_PHONE_CODE);

                    contact = new ClientContact()
                    {
                        CreateDate = DateTime.Now,
                        AuthorId = _sessionContext.UserId,
                        Address = responseData.MobilePhone,
                        ContactTypeId = contactType.Id,
                        IsDefault = true
                    };
                }

                using (var transaction = _repository.BeginTransaction())
                {
                    if (client.Id == 0)
                    {
                        _clientRepository.Insert(client);
                        contact.ClientId = client.Id;
                        _clientContactRepository.Insert(contact);
                    }

                    transaction.Commit();
                }

                model.Client = client;
            }

            return model;
        }

        public List<CashOrder> SavePayment(PaymentModel paymentModel, int branchId)
        {
            if (paymentModel.Contract is null)
                throw new PawnshopApplicationException("Договор не выбран. Для создания платежа в ТасОнлайн выберите договор");

            if (paymentModel.Client is null)
                throw new PawnshopApplicationException("Клиент не выбран. Для создания платежа в ТасОнлайн выберите клиента");

            if (paymentModel.Cost == 0)
                throw new PawnshopApplicationException("Сумма не введена. Для создания платежа в ТасОнлайн введите сумму");

            var amounts = new Dictionary<AmountType, decimal>()
            {
                {
                    AmountType.Prepayment, 
                    paymentModel.Cost
                }
            };

            var payType = _payTypeRepository.Find(new {Code = Constants.PAY_TYPE_CASH, IsDefault = true});

            var ordersWithRecords = _businessOperationService.Register(DateTime.Now, Constants.BO_TASONLINE_PAYMENT, branchId, _sessionContext.UserId, amounts, payType.Id, note: paymentModel.Note, clientId: paymentModel.Client.Id);

            var order = ordersWithRecords.Select(t => t.Item1).FirstOrDefault();
            order.Client = _clientRepository.GetOnlyClient(order.ClientId.Value);
            
            if(order.ApprovedId.HasValue)
                order.Approved = _userRepository.Get(order.ApprovedId.Value);

            var payment = new TasOnlinePayment()
            {
                OrderId = order.Id,
                Status = TasOnlinePaymentStatus.New,
                TasOnlineContractId = paymentModel.Contract.Id
            };

            _tasOnlinePaymentRepository.Insert(payment);

            return new List<CashOrder>(){ order };
        }

        public void PrepareAndSendRequest(CashOrder cashOrder)
        {
            var tasOnlinePayment = _tasOnlinePaymentRepository.FindPaymentByOrderId(cashOrder.Id);
            var parameters = new
            {
                id = tasOnlinePayment.TasOnlineContractId,
                amount = cashOrder.OrderCost,
                idTransaction = cashOrder.Id
            };

            var request = SendRequest<TasOnlinePaymentResponse>("pay", parameters, tasOnlinePayment.Id);

            using (var transation = _repository.BeginTransaction())
            {
                if (request.Status == TasOnlineRequestStatus.Done)
                {
                    var responseData = (TasOnlinePaymentResponse)request.ResponseDataObject;

                    tasOnlinePayment.Status = (TasOnlinePaymentStatus)responseData.Code;
                    tasOnlinePayment.TasOnlineDocumentId = responseData.TasOnlineDocumentId;
                    _tasOnlinePaymentRepository.Update(tasOnlinePayment);
                }

                transation.Commit();
            }
        }
    }
}