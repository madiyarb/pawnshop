using Hangfire;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Options;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models._1c;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.PayOperations;
using Pawnshop.Services.AbsOnline;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Contracts;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Web.Models.AccountantIntegration;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Xml.Serialization;
using System;
using Pawnshop.Services.Refinance;
using Pawnshop.Web.Engine.MessageSenders;
using Pawnshop.Services.Positions;
using Pawnshop.Core.Queries;
using Pawnshop.Services.ApplicationOnlineRefinances;
using Pawnshop.Services.MessageSenders;
using Serilog;

namespace Pawnshop.Web.Engine.Jobs
{
    public class AccountantIntegrationJob
    {
        private readonly IAbsOnlineService _absOnlineService;
        private readonly ICashOrderService _cashOrderService;
        private readonly ClientRepository _clientRepository;
        private readonly IContractActionService _contractActionService;
        private readonly ContractRepository _contractRepository;
        private readonly EventLog _eventLog;
        private readonly GroupRepository _groupRepository;
        private readonly JobLog _jobLog;
        private readonly EnviromentAccessOptions _options;
        private readonly OuterServiceSettingRepository _outerServiceSettingRepository;
        private readonly PayOperationActionRepository _payOperationActionRepository;
        private readonly PayOperationQueryRepository _payOperationQueryRepository;
        private readonly PayOperationRepository _payOperationRepository;
        private readonly IContractPaymentScheduleService _contractPaymentScheduleService;
        private readonly IApplicationOnlineRefinancesService _refinanceService;
        private readonly EmailSender _emailSender;
        private readonly IPositionEstimateHistoryService _positionEstimateHistoryService;
        private string _url = string.Empty;
        private readonly IRefinanceBuyOutService _refinanceBuyOutService;
        private readonly IPositionSubjectService _positionSubjectService;
        private readonly HttpClient _http;
        private readonly LoanPercentRepository _loanPercentRepository;
        private readonly ContractPaymentScheduleRepository _contractPaymentScheduleRepository;
        private readonly ContractService _contractService;
        private readonly ContractCheckValueRepository _contractCheckValueRepository;
        private readonly ContractRateRepository _contractRateRepository;
        private readonly LoanProductTypeRepository _loanProductTypeRepository;
        private readonly ILogger _logger;
        private readonly OnlineApplicationRepository _onlineApplicationRepository;
        private readonly IRefinanceService _oldRefinanceService;

        public AccountantIntegrationJob(
            IAbsOnlineService absOnlineService,
            ICashOrderService cashOrderService,
            ClientRepository clientRepository,
            IContractActionService contractActionService,
            ContractRepository contractRepository,
            EventLog eventLog,
            GroupRepository groupRepository,
            JobLog jobLog,
            IOptions<EnviromentAccessOptions> options,
            OuterServiceSettingRepository outerServiceSettingRepository,
            PayOperationActionRepository payOperationActionRepository,
            PayOperationQueryRepository payOperationQueryRepository,
            PayOperationRepository payOperationRepository,
            IContractPaymentScheduleService contractPaymentScheduleService,
            IApplicationOnlineRefinancesService refinanceService,
            EmailSender emailSender,
            IRefinanceBuyOutService refinanceBuyOutService,
            IPositionSubjectService positionSubjectService,
            IPositionEstimateHistoryService positionEstimateHistoryService,
            HttpClient http,
            LoanPercentRepository loanPercentRepository,
            ContractPaymentScheduleRepository contractPaymentScheduleRepository,
            ContractService contractService,
            ContractCheckValueRepository contractCheckValueRepository,
            ContractRateRepository contractRateRepository,
            LoanProductTypeRepository loanProductTypeRepository,
            ILogger logger,
            OnlineApplicationRepository onlineApplicationRepository,
            IRefinanceService oldRefinanceService
        )
        {
            _absOnlineService = absOnlineService;
            _cashOrderService = cashOrderService;
            _clientRepository = clientRepository;
            _contractActionService = contractActionService;
            _contractRepository = contractRepository;
            _eventLog = eventLog;
            _groupRepository = groupRepository;
            _jobLog = jobLog;
            _options = options.Value;
            _outerServiceSettingRepository = outerServiceSettingRepository;
            _payOperationActionRepository = payOperationActionRepository;
            _payOperationQueryRepository = payOperationQueryRepository;
            _payOperationRepository = payOperationRepository;
            _contractPaymentScheduleService = contractPaymentScheduleService;
            _refinanceService = refinanceService;
            _emailSender = emailSender;
            _refinanceBuyOutService = refinanceBuyOutService;
            _positionSubjectService = positionSubjectService;
            _positionEstimateHistoryService = positionEstimateHistoryService;
            _http = http;
            _loanPercentRepository = loanPercentRepository;
            _contractPaymentScheduleRepository = contractPaymentScheduleRepository;
            _contractService = contractService;
            _contractCheckValueRepository = contractCheckValueRepository;
            _contractRateRepository = contractRateRepository;
            _loanProductTypeRepository = loanProductTypeRepository;
            _logger = logger;
            _onlineApplicationRepository = onlineApplicationRepository;
            _oldRefinanceService = oldRefinanceService;
        }

        public void Execute()
        {
            if (!_options.AccountantUpload)
                return;

            List<PayOperationQuery> queue = _payOperationQueryRepository.Find();
            if (queue == null || queue.Count == 0)
                return;

            foreach (var item in queue)
            {
                Query(item.Id);
            }
        }

        [Queue("1c")]
        public void Query(int id) // Вся логика выгрузки в 1C
        {
            if (!_options.AccountantUpload)
                throw new PawnshopApplicationException("Недостаточно прав для запуска");

            int payOperationId = 0;
            try
            {
                var _query = _payOperationQueryRepository.Get(id);
                if (_query == null)
                    throw new PawnshopApplicationException($"Очередь для платежной операции {id} не найдена");

                var operation = _payOperationRepository.Get(_query.OperationId);
                if (operation == null)
                    throw new PawnshopApplicationException($"Платежная операция {_query.OperationId} не найдена");

                if (!operation.ClientId.HasValue)
                    throw new PawnshopApplicationException($"Операция {operation.Id} должен иметь клиента({nameof(operation.ClientId)})");

                if (!operation.ContractId.HasValue)
                    throw new PawnshopApplicationException($"Операция {operation.Id} должен иметь контракт({nameof(operation.ContractId)})");

                if (operation.ActionId.HasValue && operation.Action == null)
                    throw new PawnshopApplicationException($"Операция {operation.Id} должен подгружать действие({nameof(operation.Action)}), так как к него есть действие");

                ContractAction operactionAction = operation.Action;
                payOperationId = operation.Id;
                if (!operation.PayType.AccountantUploadRequired)
                    throw new PawnshopApplicationException("Операция не требует выгрузки в 1с");

                _jobLog.Log("AccountantIntegrationJob", JobCode.Start, JobStatus.Success, EntityType.PayOperation, payOperationId, requestData: id.ToString());

                var contract = _contractRepository.GetContractWithSubject(operation.ContractId.Value);
                contract.Setting = _loanPercentRepository.Get(contract.SettingId.Value);
                contract.Branch = _groupRepository.Get(contract.BranchId);
                contract.Client = _clientRepository.Get(contract.ClientId);
                contract.PaymentSchedule = _contractPaymentScheduleRepository.GetListByContractId(contract.Id);
                contract.Positions = _contractService.GetPositionsByContractId(contract.Id);
                contract.Checks = _contractCheckValueRepository.List(new ListQuery(), new { ContractId = contract.Id });
                contract.ContractRates = _contractRateRepository.List(new ListQuery(), new { ContractId = contract.Id });
                contract.ProductType = _loanProductTypeRepository.Get(contract.ProductTypeId.Value);


                if (contract.Client == null)
                    throw new PawnshopApplicationException($"Клиент {operation.ClientId.Value} не найден");

                if (contract.Branch == null)
                    throw new PawnshopApplicationException($"Филиал {contract.BranchId} не найден");

                var config = _outerServiceSettingRepository.Find(new { Code = Constants.PAY_OPERATION_INTEGRATION_SETTINGS_CODE });

                if (config == null)
                    throw new PawnshopApplicationException($"Настройки не найдены");

                if (string.IsNullOrEmpty(config.Login)
                    || string.IsNullOrEmpty(config.Password)
                    || string.IsNullOrEmpty(config.URL))
                    throw new PawnshopApplicationException("Некорректные настройки для выгрузки платежных поручений в 1с");

                StringBuilder xml = new StringBuilder();
                _url = config.URL;
                if (_query.QueryType == QueryType.Upload)
                {
                    string totalCostString = operation.TotalCost.ToString(CultureInfo.InvariantCulture);
                    var buycar = contract.ProductTypeId.HasValue && contract.ProductType.Code == Constants.PRODUCT_BUYCAR;
                    var damu = contract.ProductTypeId.HasValue && contract.ProductType.Code == Constants.PRODUCT_DAMU;

                    string appointmentText = buycar ? $"Перечисление денег по договору займа №{contract.ContractNumber} заемщика {contract.Client.FullName}(ИИН:{contract.Client.IdentityNumber})" : $"Предоставление займа по договору №{contract.ContractNumber}";

                    if (damu)
                        appointmentText = $"Предоставление займа по программе ДАМУ МИКРО по договору №{contract.ContractNumber}";

                    if (operactionAction != null && operactionAction.ActionType == ContractActionType.PrepaymentReturn)
                        appointmentText = $"Возрат излишне оплаченных денег клиенту Сумма {totalCostString} тенге без НДС";

                    xml.Append("<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:wsp=\"http://localhost/wsPayment\">");
                    xml.Append("<soap:Header/>");
                    xml.Append("<soap:Body>");
                    xml.Append("<wsp:Create>");
                    xml.Append($"<wsp:Data>{operation.Date:yyyy-MM-dd}</wsp:Data>");
                    xml.Append($"<wsp:Summ>{totalCostString}</wsp:Summ>");
                    xml.Append($"<wsp:NameClient>{contract.Client.FullName}</wsp:NameClient>");
                    xml.Append($"<wsp:IIN>{contract.Client.IdentityNumber}</wsp:IIN>");
                    xml.Append($"<wsp:KBE>{contract.Client.BeneficiaryCode}</wsp:KBE>");
                    xml.Append($"<wsp:IBAN>{operation.Requisite.Value}</wsp:IBAN>");
                    xml.Append($"<wsp:NameBank>{operation.Requisite.Bank.FullName}</wsp:NameBank>");
                    xml.Append($"<wsp:BIKBank>{operation.Requisite.Bank.BankIdentifierCode}</wsp:BIKBank>");
                    xml.Append(
                        $"<wsp:Appointment>{appointmentText}</wsp:Appointment>");
                    xml.Append($"<wsp:Login>{config.Login}</wsp:Login>");
                    xml.Append($"<wsp:Password>{config.Password}</wsp:Password>");
                    xml.Append($"<wsp:ID>{operation.Id}</wsp:ID>");
                    xml.Append("</wsp:Create>");
                    xml.Append("</soap:Body>");
                    xml.Append("</soap:Envelope>");

                    _url += "/Create";
                }
                else if (_query.QueryType == QueryType.Check)
                {
                    xml.Append("<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:wsp=\"http://localhost/wsPayment\">");
                    xml.Append("<soap:Header/>");
                    xml.Append("<soap:Body>");
                    xml.Append("<wsp:GetStatus>");
                    xml.Append($"<wsp:Login>{config.Login}</wsp:Login>");
                    xml.Append($"<wsp:Password>{config.Password}</wsp:Password>");
                    xml.Append($"<wsp:ID>{_query.OperationId}</wsp:ID>");
                    xml.Append("</wsp:GetStatus>");
                    xml.Append("</soap:Body>");
                    xml.Append("</soap:Envelope>");

                    _url += "/GetStatus";
                }
                else
                    throw new ArgumentOutOfRangeException(_query.GetType().Name);

                var stringContent = new StringContent(xml.ToString(), Encoding.UTF8, "text/xml");
                using (var response = _http.PostAsync(_url, stringContent).Result)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var result = response.Content.ReadAsStringAsync().Result;
                        var oldApplication = _onlineApplicationRepository.FindByContractIdAsync(new { ContractId = contract.Id.ToString() }).Result;
                        bool isOldApplication = oldApplication != null;
                        if (_query.QueryType == QueryType.Check)
                        {
                            AccountantIntegrationCheckStatusAnswer resultModel;
                            using (TextReader reader = new StringReader(result))
                            {
                                XmlSerializer serializer = new XmlSerializer(typeof(AccountantIntegrationCheckStatusAnswer));
                                resultModel = (AccountantIntegrationCheckStatusAnswer)serializer.Deserialize(reader);
                            }
                            if (bool.Parse(resultModel.Body.GetStatusResponse.Return.Text))
                            {
                                if (operation.Status == PayOperationStatus.Checked)
                                {
                                    _logger.Information($"Start Refinancing trying {contract.Id}");
                                    var refinanceResult = false;
                                    if (isOldApplication) //TODO эта вилка должна уйти вместе после успешного релиза оставив только else
                                        refinanceResult = _oldRefinanceService.RefinanceAllAssociatedContracts(contract.Id).Result;
                                    else
                                        refinanceResult = _refinanceService.RefinanceAllAssociatedContracts(contract.Id).Result;

                                    if (!refinanceResult)
                                        EmailProcessingNotifications(contract.Id, "Ошибка рефинансирования займов!", 
                                            "Не удалось рефинансировать займы!");

                                    if (operation.Action.ActionType == ContractActionType.Sign)
                                    {
                                        var policyResult = _absOnlineService.RegisterPolicy(contract.Id, contract);

                                        if (!string.IsNullOrEmpty(policyResult))
                                            _absOnlineService.SaveRetrySendInsurance(contract.Id);
                                    }
                                }
                            }
                        }

                        using (var transaction = _payOperationQueryRepository.BeginTransaction())
                        {
                            if (_query.QueryType == QueryType.Upload)
                            {
                                // При выгрузке платежного поручения в 1С
                            }
                            else if (_query.QueryType == QueryType.Check)
                            {
                                // При проверке статуса платежного поручения в 1С
                                AccountantIntegrationCheckStatusAnswer resultModel;
                                using (TextReader reader = new StringReader(result))
                                {
                                    XmlSerializer serializer = new XmlSerializer(typeof(AccountantIntegrationCheckStatusAnswer));
                                    resultModel = (AccountantIntegrationCheckStatusAnswer)serializer.Deserialize(reader);
                                }

                                if (bool.Parse(resultModel.Body.GetStatusResponse.Return.Text))
                                {
                                    if (operation.Status == PayOperationStatus.Checked)
                                    {
                                        operation.Status = PayOperationStatus.Executed;
                                        operation.ExecuteDate = DateTime.Now;

                                        PayOperationAction action = new PayOperationAction()
                                        {
                                            ActionType = PayOperationActionType.Execute,
                                            AuthorId = _query.AuthorId,
                                            CreateDate = DateTime.Now,
                                            Date = DateTime.Now,
                                            OperationId = operation.Id
                                        };

                                        if (operation.Action.ActionType == ContractActionType.Sign)
                                        {
                                            contract.SignDate = DateTime.Now;
                                            contract.Status = ContractStatus.Signed;
                                            operactionAction.Status = ContractActionStatus.Approved;
                                            _contractActionService.Save(operactionAction);

                                            _contractPaymentScheduleService.UpdateFirstPaymentInfo(contract.Id, contract);
                                        }

                                        var orders = operation.Orders.Count > 0 ? _cashOrderService.GetCashOrdersForApprove(operation.Orders) : null;

                                        foreach (var order in orders)
                                        {
                                            if (order.OrderDate.Date != DateTime.Now.Date)
                                                order.OrderDate = DateTime.Now;

                                            order.ApproveStatus = OrderStatus.Approved;
                                            _cashOrderService.Register(order, contract.Branch);
                                        }

                                        if (contract.CollateralType == CollateralType.Realty && contract.ContractClass != Data.Models.Contracts.ContractClass.Tranche)
                                        {
                                            _positionEstimateHistoryService.SavePositionEstimateToHistoryForContract(contract);
                                            _positionSubjectService.SavePositionSubjectsToHistoryForContract(contract);
                                        }

                                        _payOperationRepository.Update(operation);
                                        _contractRepository.Update(contract);
                                        _payOperationActionRepository.Insert(action);
                                    }
                                }
                            }
                            else
                                throw new ArgumentOutOfRangeException(_query.GetType().Name);

                            _query.Status = QueryStatus.Success;
                            _query.QueryDate = DateTime.Now;
                            _payOperationQueryRepository.Update(_query);

                            transaction.Commit();
                        }

                        if (_query.QueryType == QueryType.Check)
                        {
                            AccountantIntegrationCheckStatusAnswer resultModel;
                            using (TextReader reader = new StringReader(result))
                            {
                                XmlSerializer serializer = new XmlSerializer(typeof(AccountantIntegrationCheckStatusAnswer));
                                resultModel = (AccountantIntegrationCheckStatusAnswer)serializer.Deserialize(reader);
                            }

                            if (bool.Parse(resultModel.Body.GetStatusResponse.Return.Text))
                            {
                                if (operation.Status == PayOperationStatus.Executed)
                                {
                                    _logger.Information($"Try to refinance buyout {contract.Id}");
                                    var refinanceBuyoutResult = false;
                                    if (isOldApplication) //TODO эта вилка должна уйти вместе после успешного релиза оставив только else
                                        refinanceBuyoutResult =  _refinanceBuyOutService.BuyOutAllRefinancedContracts(contract.Id).Result;
                                    else
                                        refinanceBuyoutResult = _refinanceBuyOutService.BuyOutAllRefinancedContractsForApplicationsOnline(contract.Id).Result;

                                    if (!refinanceBuyoutResult)
                                        EmailProcessingNotifications(contract.Id, "Ошибка выкупа займов!", "Не удалось выкупить займы!");
                                }
                            }
                        }

                        _eventLog.Log(_query.QueryType == QueryType.Upload ? EventCode.AccountantUpload : EventCode.AccountantStatusCheck, EventStatus.Success, EntityType.PayOperation, operation.Id, xml.ToString(), result, uri: _url, userId: _query.AuthorId);
                    }
                    else
                    {
                        _eventLog.Log(_query.QueryType == QueryType.Upload ? EventCode.AccountantUpload : EventCode.AccountantStatusCheck, EventStatus.Failed, EntityType.PayOperation, operation.Id, xml.ToString(), response.Content.ReadAsStringAsync().Result, uri: _url, userId: _query.AuthorId);
                    }

                    response.EnsureSuccessStatusCode();
                    _jobLog.Log("AccountantIntegrationJob", JobCode.End, JobStatus.Success, EntityType.PayOperation, payOperationId, requestData: id.ToString());
                }
            }
            catch (Exception ex)
            {
                _jobLog.Log("AccountantIntegrationJob", JobCode.Error, JobStatus.Failed, EntityType.PayOperation, payOperationId, requestData: id.ToString(), responseData: JsonConvert.SerializeObject(ex));
            }
        }

        /// <summary>
        /// Email уведомление об ошибке
        /// </summary>
        private void EmailProcessingNotifications(int contractId, string messageTitle, string details)
        {
            try
            {
                var message = $@"<p style=""text-align: center;""><strong>{messageTitle}</strong></p>
                        <p><strong>ContractId = {contractId}</strong></p>
<p>{details}<p>";

                var messageReceiver = new MessageReceiver
                {
                    ReceiverAddress = _options.ErrorNotifierAddress,
                    ReceiverName = _options.ErrorNotifierName
                };

                _emailSender.SendEmail(messageTitle, message, messageReceiver);
            }
            catch
            {
                _jobLog.Log("OnlinePaymentJob", JobCode.Error, JobStatus.Failed, responseData: $"Не удалось отправить уведомление об ошибке на емейл {_options.ErrorNotifierAddress}");
            }
        }
    }
}