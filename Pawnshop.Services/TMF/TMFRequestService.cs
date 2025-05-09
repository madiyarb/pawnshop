using Newtonsoft.Json;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Access.TMF;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.TasOnline;
using Pawnshop.Data.Models.TMF;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Domains;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using Pawnshop.Services.Models.TasOnline;
using Pawnshop.Services.Models.Contracts.Kdn.ContractKdnXml;
using Pawnshop.Services.Models.TMF;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Pawnshop.Services.TMF
{
    public class TMFRequestService : BaseService<TMFRequest>, ITMFRequestService
    {
        private readonly ISessionContext _sessionContext;
        //private readonly ITasOnlinePaymentApi _tasOnlinePaymentApi;
        private readonly ClientRepository _clientRepository;
        private readonly ClientLegalFormRepository _clientLegalFormRepository;
        private readonly CountryRepository _countryRepository;
        private readonly IDomainService _domainService;
        private readonly ClientContactRepository _clientContactRepository;
        private readonly IBusinessOperationService _businessOperationService;
        private readonly PayTypeRepository _payTypeRepository;
        private readonly TMFPaymentRepository _tmfPaymentRepository;
        private readonly IEventLog _eventLog;
        private readonly CashOrderRepository _cashOrderRepository;
        private readonly UserRepository _userRepository;
        private readonly TMFRequestRepository _tMFRequestRepository;
        private readonly ITMFRequestApi _tMFRequestApi;


        public TMFRequestService(IRepository<TMFRequest> repository,
            ISessionContext sessionContext,
            ClientRepository clientRepository,
            ClientLegalFormRepository clientLegalFormRepository,
            CountryRepository countryRepository,
            IDomainService domainService,
            ClientContactRepository clientContactRepository,
            IBusinessOperationService businessOperationService,
            PayTypeRepository payTypeRepository,
            TMFPaymentRepository tmFPaymentRepository,
            IEventLog eventLog,
            CashOrderRepository cashOrderRepository,
            UserRepository userRepository,
            ITMFRequestApi tMFRequestApi,
            TMFRequestRepository tMFRequestRepository) : base(repository)
        {
            _sessionContext = sessionContext;
            //_tasOnlinePaymentApi = tasOnlinePaymentApi;
            _clientRepository = clientRepository;
            _clientLegalFormRepository = clientLegalFormRepository;
            _countryRepository = countryRepository;
            _domainService = domainService;
            _clientContactRepository = clientContactRepository;
            _businessOperationService = businessOperationService;
            _payTypeRepository = payTypeRepository;
            _tmfPaymentRepository = tmFPaymentRepository;
            _eventLog = eventLog;
            _cashOrderRepository = cashOrderRepository;
            _userRepository = userRepository;
            _tMFRequestApi = tMFRequestApi;
            _tMFRequestRepository = tMFRequestRepository;
        }

        public TmfClientContractModel GetContractsByIdentityNumber(string identityNumber)
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

            var model = new TmfClientContractModel();

            var tmfRequest = new TMFBaseRequest()
            {
                MethodName = TMFOperationMethodNames.CheckAccount.GetDisplayName(),
                IIN = identityNumber
            };

            var request = SendRequest<TmfCheckAccountResultResponse>(tmfRequest, parameters);

            if(request.Status == TMFRequestStatus.Done)
            {
                var responseData = (TMFBaseResponse)request.ResponseDataObject;
                var resultData = (TmfCheckAccountResultResponse) request.ResponseDataResultObject;
                //var resultData = responseData.Result as TmfCheckAccountResultResponse;
                if(responseData.Success)
                {
                    model.Contracts = resultData.Contracts;

                    if(String.IsNullOrWhiteSpace(resultData.IdentityNumber))
                    {
                        throw new PawnshopApplicationException($"В ответе от ТМФ не был отправлен ИИН/БИН");
                    }

                    if (String.IsNullOrWhiteSpace(resultData.LegalForm))
                    {
                        throw new PawnshopApplicationException($"В ответе от ТМФ не был отправлен LegalForm");
                    }

                    var client = _clientRepository.FindByIdentityNumberAndLegalFormCode(resultData.IdentityNumber, resultData.LegalForm);
                    ClientContact contact = null;

                    if(client is null)
                    {
                        var legalForm = _clientLegalFormRepository.Find(new { Code = resultData.LegalForm });

                        if (legalForm is null)
                            throw new PawnshopApplicationException(
                                $"Не найдена правовая форма с кодом {resultData.LegalForm}");

                        var country = _countryRepository.Find(new {Code = resultData.Citizenship});

                        if (country is null)
                            throw new PawnshopApplicationException(
                                $"Не найдена страна с кодом {resultData.Citizenship}");

                        client = new Client()
                        {
                            CardType = CardType.Standard,
                            IdentityNumber = resultData.IdentityNumber,
                            Surname = resultData.Surname,
                            Name = resultData.Name,
                            Patronymic = resultData.Patronymic,
                            FullName = resultData.Fullname,
                            IsMale = resultData.IsMale,
                            BirthDay = resultData.BirthDay,
                            IsResident = resultData.IsResident ?? true,
                            LegalFormId = legalForm.Id,
                            IsPolitician = resultData.IsPEP ?? false,
                            CitizenshipId = country.Id,
                            AuthorId = _sessionContext.UserId,
                            CreateDate = DateTime.Now
                        };

                        var phoneRegex = new Regex(Constants.PHONE_REGEX);

                        if (!phoneRegex.IsMatch(resultData.MobilePhone))
                            throw new PawnshopApplicationException("В поле телефон должно быть 11 цифр");

                        var contactType = _domainService.GetDomainValue(Constants.DOMAIN_CONTACT_TYPE_CODE, Constants.DOMAIN_VALUE_MOBILE_PHONE_CODE);

                        client.LegalForm = _clientLegalFormRepository.Get(client.LegalFormId);

                        contact = new ClientContact()
                        {
                            CreateDate = DateTime.Now,
                            AuthorId = _sessionContext.UserId,
                            Address = resultData.MobilePhone,
                            ContactTypeId = contactType.Id,
                            IsDefault = true
                        };
                    }

                    using (var transaction = _tMFRequestRepository.BeginTransaction())
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
            }

            return model;
        }


        public List<CashOrder> SavePayment(TMFPaymentModel paymentModel, int branchId)
        {
            if (paymentModel.Contract is null)
                throw new PawnshopApplicationException("Договор не выбран. Для создания платежа в TMF выберите договор");

            if (paymentModel.Client is null)
                throw new PawnshopApplicationException("Клиент не выбран. Для создания платежа в TMF выберите клиента");

            if (paymentModel.Cost == 0)
                throw new PawnshopApplicationException("Сумма не введена. Для создания платежа в TMF введите сумму");

            if (paymentModel.Cost < 0)
                throw new PawnshopApplicationException("Сумма не может быть отрицательной");

            var amounts = new Dictionary<AmountType, decimal>()
            {
                {
                    AmountType.Prepayment,
                    paymentModel.Cost
                }
            };

            var payType = _payTypeRepository.Find(new { Code = Constants.PAY_TYPE_CASH, IsDefault = true });

            var ordersWithRecords = _businessOperationService.Register(DateTime.Now, Constants.BO_TMF_PAYMENT, branchId, _sessionContext.UserId, amounts, payType.Id, note: paymentModel.Note, clientId: paymentModel.Client.Id);

            var order = ordersWithRecords.Select(t => t.Item1).FirstOrDefault();
            order.Client = _clientRepository.GetOnlyClient(order.ClientId.Value);

            if (order.ApprovedId.HasValue)
                order.Approved = _userRepository.Get(order.ApprovedId.Value);

            var payment = new TMFPayment()
            {
                OrderId = order.Id,
                Status = TMFPaymentStatus.New,
                TMFContractId = paymentModel.Contract.Id
            };

            _tmfPaymentRepository.Insert(payment);

            return new List<CashOrder>() { order };
        }

        public void PrepareAndSendRequest(CashOrder cashOrder)
        {
            var tmfPayment = _tmfPaymentRepository.FindPaymentByOrderId(cashOrder.Id);
            TMFBaseRequest sendRequest = new TMFBaseRequest()
            {
                Amount = cashOrder.OrderCost,
                MethodName = TMFOperationMethodNames.Payment.GetDisplayName(),
                Contract = tmfPayment.TMFContractId
            };
            var parameters = new
            {
                id = tmfPayment.TMFContractId,
                amount = cashOrder.OrderCost,
                idTransaction = cashOrder.Id
            };

            var request = SendRequest<TMFPaymentResultResponse>(sendRequest, parameters, tmfPayment.Id);

            using (var transation = _repository.BeginTransaction())
            {
                if (request.Status == TMFRequestStatus.Done)
                {
                    var responseData = (TMFBaseResponse)request.ResponseDataObject;
                    var resultData = JsonConvert.DeserializeObject<TMFPaymentResultResponse>(responseData.Result.ToString());

                    if(responseData.Success)
                    {
                        //tmfPayment.Status = (TMFPaymentStatus)responseData.Code;
                        //tasOnlinePayment.TasOnlineDocumentId = responseData.TasOnlineDocumentId;
                        tmfPayment.Status = TMFPaymentStatus.Success;
                        tmfPayment.TmfDocumentId = resultData.TransactionId;
                    }else
                    {
                        tmfPayment.Status = TMFPaymentStatus.Failed;
                        tmfPayment.Message = responseData.Message;
                    }
                    _tmfPaymentRepository.Update(tmfPayment);
                }

                transation.Commit();
            }
        }

        private TMFRequest SendRequest<T>(TMFBaseRequest tmfRequest, object parameters, int? paymentId = null)
        {
            var request = new TMFRequest()
            {
                RequestData = string.Empty,
                CreateDate = DateTime.Now,
                AuthorId = _sessionContext.UserId,
                Status = TMFRequestStatus.New
            };

            request.RequestData = JsonConvert.SerializeObject(tmfRequest);

            Save(request);


            var response = _tMFRequestApi.Send(tmfRequest, parameters);
            //response.Result = (T)response.Result;
            request.ResponseDataObject = response;

            request.Status = TMFRequestStatus.Done;
            request.PaymentId = paymentId;
            request.ResponseData = JsonConvert.SerializeObject(response);

            if(!response.Success)
            {
                request.Status = TMFRequestStatus.Failure;
            }

            if (request.Status == TMFRequestStatus.Done)
            {
                try
                {
                    request.ResponseDataResultObject = JsonConvert.DeserializeObject<T>((string)response.Result.ToString());
                }
                catch (Exception exception)
                {
                    _eventLog.Log(EventCode.TMFRequestDeserialize, EventStatus.Failed, requestData: string.Empty,
                        responseData: exception.Message, uri: request.RequestData, entityType: null);
                    request.Status = TMFRequestStatus.Failure;
                    Save(request);
                }
            }

            Save(request);

            if (request.Status == TMFRequestStatus.Failure)
                throw new PawnshopApplicationException(request.ResponseData);

            return request;
        }

        public ListModel<TMFPayment> GetTmfPaymentList(ListQueryModel<TmfPaymentFilter> listQuery)
        {
            return  new ListModel<TMFPayment>()
            {
                List = _tmfPaymentRepository.List(listQuery, listQuery.Model),
                Count = _tmfPaymentRepository.Count(listQuery, listQuery.Model),
            };
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

        public TMFPayment RePayment(int cashOrderId)
        {
            var cashOrder = GetCashOrder(cashOrderId);

            var tmfPayment = _tmfPaymentRepository.FindPaymentByOrderId(cashOrderId);

            if (tmfPayment is null)
                throw new PawnshopApplicationException($"Транзакция для кассого ордера с номером {cashOrder.OrderNumber} не найдена");

            var parameters = new
            {
                id = tmfPayment.TMFContractId,
                amount = cashOrder.OrderCost,
                idTransaction = cashOrderId
            };

            TMFBaseRequest sendRequest = new TMFBaseRequest()
            {
                Contract = tmfPayment.TMFContractId,
                Amount = cashOrder.OrderCost,
                MethodName = TMFOperationMethodNames.Payment.GetDisplayName()
            };

            var request = SendRequest<TMFPaymentResultResponse>(sendRequest, parameters, tmfPayment.Id);

            if (request.Status == TMFRequestStatus.Done)
            {
                using (var transaction = _repository.BeginTransaction())
                {
                    var responseData = (TMFBaseResponse)request.ResponseDataObject;
                    var resultData = JsonConvert.DeserializeObject<TMFPaymentResultResponse>(responseData.Result.ToString());
                    tmfPayment.Status = TMFPaymentStatus.Success;
                    tmfPayment.TmfDocumentId = resultData.TransactionId;

                    _tmfPaymentRepository.Update(tmfPayment);

                    transaction.Commit();
                }
            }

            return tmfPayment;
        }
    }
}
