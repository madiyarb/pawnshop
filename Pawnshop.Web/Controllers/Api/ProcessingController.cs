using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Data.Models.OnlinePayments;
using Pawnshop.Data.Models.Processing;
using Pawnshop.Services;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Calculation;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Middleware;
using Pawnshop.Web.Engine.Security;
using Pawnshop.Web.Models.Processing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Pawnshop.Services.CreditLines;
using DocumentFormat.OpenXml.Office2010.Excel;
using Pawnshop.Data.Models.Contracts.Expenses;
using Stimulsoft.Data.Expressions.Antlr.Runtime.Misc;
using Pawnshop.Services.Expenses;

namespace Pawnshop.Web.Controllers.Api
{
    public class ProcessingController : Controller
    {
        private readonly ISessionContext _sessionContext;
        private readonly ClientRepository _clientRepository;
        private readonly IContractService _contractService;
        private readonly ContractRepository _contractRepository;
        private readonly OrganizationRepository _organizationRepository;
        private readonly IEventLog _eventLog;
        private readonly OnlinePaymentRepository _onlinePaymentRepository;
        private readonly IContractActionRowBuilder _contractAmount;
        private readonly NotificationReceiverRepository _notificationReceiverRepository;
        private readonly NotificationRepository _notificationRepository;
        private readonly IProcessingService _processingService;
        private readonly ICreditLineService _creditLineService;
        private readonly InscriptionRepository _inscriptionRepository;
        private readonly IExpenseService _expenseService;

        public ProcessingController(ISessionContext sessionContext,
            ClientRepository clientRepository,
            IContractService contractService,
            OrganizationRepository organizationRepository,
            IEventLog eventLog,
            OnlinePaymentRepository onlinePaymentRepository,
            IContractActionRowBuilder contractAmount,
            NotificationRepository notificationRepository,
            NotificationReceiverRepository notificationReceiverRepository,
            IProcessingService processingService,
            ICreditLineService creditLineService, InscriptionRepository inscriptionRepository, ContractRepository contractRepository, IExpenseService expenseService)
        {
            _sessionContext = sessionContext;
            _clientRepository = clientRepository;
            _contractService = contractService;
            _organizationRepository = organizationRepository;
            _eventLog = eventLog;
            _onlinePaymentRepository = onlinePaymentRepository;
            _contractAmount = contractAmount;
            _notificationReceiverRepository = notificationReceiverRepository;
            _notificationRepository = notificationRepository;
            _processingService = processingService;
            _creditLineService = creditLineService;
            _inscriptionRepository = inscriptionRepository;
            _contractRepository = contractRepository;
            _expenseService = expenseService;
        }

        [HttpGet("/api/kassa24")]
        [Authorize(Permissions.OnlinePayment)]
        [Event(EventCode.OnlinePaymentTry, EventMode = EventMode.All, EntityType = EntityType.OnlinePayment, IncludeFails = true)]
        public async Task<OnlinePaymentOuterModel> Kassa24([FromQuery(Name = "action")] string action, string number, DateTime date, decimal amount = -1, Int64 receipt = -1)
        {
            var response = new OnlinePaymentOuterModel();
            switch (action)
            {
                #region Поиск клиента по ИИН
                case "check":
                    if (!string.IsNullOrEmpty(number))
                    {
                        var client = _clientRepository.FindByIdentityNumber(number);
                        if (client != null)
                        {
                            response.Code = 0;
                            response.Message = "Клиент найден";

                            var creditLines = await _contractService.FindCreditLinesForProcessing(client.Id);
                            List<object> invoicesToSend = new List<dynamic>();

                            if (creditLines.Count > 0)
                            {

                                DateTime? accrualDate = CalcAccrualDate(default);

                                foreach (var creditLine in creditLines)
                                {
                                    var organizationInfo = _organizationRepository.Get(creditLine.Branch.OrganizationId);
                                    var contract = _contractService.Get(creditLine.Id);

                                    var tranches  = await _contractService.GetAllSignedTranches(contract.Id);
                                    foreach (var tranch in tranches)
                                    {
                                        if (accrualDate.HasValue && tranch.Status == ContractStatus.Signed)
                                        {
                                            var tempContract = _contractService.Get(tranch.Id);
                                            _processingService.InitAccruals(tempContract, accrualDate.Value.Date);
                                        }
                                    }

                                    var paymentAmount = await _creditLineService.GetAmountForCurrentlyPayment(creditLine.Id, accrualDate);

                                    dynamic invoice = new ExpandoObject();
                                    invoice.fio = creditLine.ContractData.Client.FullName;
                                    invoice.contractNumber = creditLine.ContractNumber;
                                    invoice.amount = paymentAmount < 0 ? 0 : Math.Ceiling(paymentAmount);
                                    invoice.date = creditLine.ContractDate;
                                    invoice.bankAccount = organizationInfo.Configuration.BankSettings.BankAccount;
                                    invoice.bankBik = organizationInfo.Configuration.BankSettings.BankBik;
                                    invoice.companyBin = organizationInfo.Configuration.LegalSettings.BIN.Replace(" ", "");
                                    invoice.companyName = organizationInfo.Configuration.LegalSettings.LegalName;
                                    invoice.id = creditLine.Id;
                                    invoice.position = CreatePositionDescription(contract);
                                    invoice.note_kaz = Constants.PROCESSING_NOTE_KAZ;
                                    invoice.note_rus = Constants.PROCESSING_NOTE_RUS;

                                    invoicesToSend.Add(invoice);
                                }
                            }

                            var contracts = (await _contractService.FindForProcessingAsync(client.Id)).Where(contract =>
                                (contract.CreditLineId == null &&
                                contract.ContractClass != ContractClass.CreditLine) || (contract.CreditLineId != null && contract.IsOffBalance)).ToList();
                            if (contracts.Count > 0)
                            {
                                DateTime? accrualDate = CalcAccrualDate(default);

                                foreach (var c in contracts)
                                {
                                    var organizationInfo = _organizationRepository.Get(c.Branch.OrganizationId);
                                    var contract = _contractService.Get(c.Id);

                                    if (accrualDate.HasValue && c.Status == ContractStatus.Signed)
                                    {
                                        _processingService.InitAccruals(contract, accrualDate.Value.Date);
                                    }

                                    bool hasInscription = contract.InscriptionId.HasValue && contract.Inscription != null && contract.Inscription.Status != InscriptionStatus.Denied;

                                    _contractAmount.Init(contract, date: accrualDate, balanceAccountsOnly: hasInscription);

                                    dynamic invoice = new ExpandoObject();
                                    invoice.fio = c.ContractData.Client.FullName;
                                    invoice.contractNumber = c.ContractNumber;
                                    invoice.amount = _contractAmount.DisplayAmount < 0 ? 0 : Math.Ceiling(_contractAmount.DisplayAmount);
                                    invoice.date = c.ContractDate;
                                    invoice.bankAccount = organizationInfo.Configuration.BankSettings.BankAccount;
                                    invoice.bankBik = organizationInfo.Configuration.BankSettings.BankBik;
                                    invoice.companyBin = organizationInfo.Configuration.LegalSettings.BIN.Replace(" ", "");
                                    invoice.companyName = organizationInfo.Configuration.LegalSettings.LegalName;
                                    invoice.id = c.Id;
                                    invoice.position = CreatePositionDescription(contract);
                                    invoice.note_kaz = Constants.PROCESSING_NOTE_KAZ;
                                    invoice.note_rus = Constants.PROCESSING_NOTE_RUS;

                                    invoicesToSend.Add(invoice);
                                }
                            }

                            if (contracts.Any() || creditLines.Any())
                            {
                                response.invoice = invoicesToSend;
                                break;
                            }
                            response.Code = 10;
                            response.Message = "У клиента нет действующих договоров";
                            break;
                        }
                    }
                    response.Code = 2;
                    response.Message = "Клиент не найден";
                    break;

                #endregion

                #region Проведение платежа
                case "payment":
                    if (String.IsNullOrEmpty(number))
                    {
                        response.Code = 11;
                        response.Message = "Договор не найден";
                        break;
                    }

                    Regex regexAmount = new Regex(@"^\d{1,9}\.\d{2}$");
                    if (amount <= 0 || regexAmount.Matches(amount.ToString(new CultureInfo("en-US"))).Count <= 0)
                    {
                        response.Code = 3;
                        response.Message = "Неверная сумма платежа";
                        break;
                    }

                    if (date.Date != DateTime.Now.Date)
                    {
                        response.Code = 12;
                        response.Message = "Нельзя сделать оплату другим числом";
                        break;
                    }

                    Regex regexDate = new Regex(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}");
                    if (date == null || regexDate.Matches(date.ToString("s")).Count <= 0)
                    {
                        response.Code = 5;
                        response.Message = "Неверное значение даты";

                        break;
                    }

                    if (receipt <= 0)
                    {
                        response.Code = 4;
                        response.Message = "Неверное значение номера платежа";
                        break;
                    }
                    else
                    {
                        try
                        {
                            OnlinePayment onlinePayment = _onlinePaymentRepository.GetByProcessing(receipt, ProcessingType.Kassa24);
                            if (onlinePayment != null)
                            {
                                response.Code = 0;
                                response.Message = "Платеж был принят";
                                response.Date = onlinePayment.CreateDate;
                                response.Authcode = onlinePayment.Id;
                                break;
                            }
                        }
                        catch
                        {
                            response.Code = 13;
                            response.Message = "Ошибка при поиске платежа";
                            break;
                        }
                    }

                    Contract contractForAmount = new Contract();
                    try
                    {
                        contractForAmount = _contractService.Get(Convert.ToInt32(number));
                    }
                    catch
                    {
                        response.Code = 11;
                        response.Message = "Договор не найден";
                        break;
                    }

                    var processingInfo = new ProcessingInfo
                    {
                        Amount = amount,
                        Reference = receipt,
                        Type = ProcessingType.Kassa24
                    };

                    using (IDbTransaction transaction = _clientRepository.BeginTransaction())
                    {
                        OnlinePayment onlinePayment = QueueOnlinePayment(contractForAmount, processingInfo);
                        if (onlinePayment == null)
                            throw new PawnshopApplicationException($"Ожидалось что {nameof(QueueOnlinePayment)} не вернет null");

                        if (!onlinePayment.Amount.HasValue)
                            throw new PawnshopApplicationException($"Ожидалось что {nameof(onlinePayment)}.{nameof(onlinePayment.Amount)} не будет null");


                        response.Code = 0;
                        response.Message = "Оплата принята";
                        response.Date = onlinePayment.CreateDate;
                        response.Authcode = onlinePayment.Id;
                        _eventLog.Log(EventCode.Prepayment, EventStatus.Success, EntityType.Contract, contractForAmount.Id, responseData: $@"Принята оплата по кредиту: {onlinePayment.Amount.Value} KZT от KASSA 24");
                        transaction.Commit();
                    }

                    break;

                #endregion

                #region Поиск платежа

                case "status":
                    if (receipt <= 0)
                    {
                        response.Code = 4;
                        response.Message = "Неверное значение номера платежа";
                        break;
                    }
                    else
                    {
                        try
                        {
                            OnlinePayment onlinePayment = _onlinePaymentRepository.GetByProcessing(receipt, ProcessingType.Kassa24);
                            if (onlinePayment != null)
                            {
                                response.Code = 0;
                                response.Message = "Платеж был принят";
                                response.Date = onlinePayment.CreateDate;
                                response.Authcode = onlinePayment.Id;
                                break;
                            }
                            else
                            {
                                response.Code = 6;
                                response.Message = "Платеж не найден в системе";
                                break;
                            }
                        }
                        catch
                        {
                            response.Code = 13;
                            response.Message = "Ошибка при поиске платежа";
                            break;
                        }
                    }

                #endregion


                default:
                    response.Code = 1;
                    response.Message = "Неизвестный тип запроса";
                    break;
            }
            return response;
        }

        /// <summary>
        /// API для поставщика RPS Asia
        /// </summary>
        /// <param name="action">Вид действия (check, payment, status)</param>
        /// <param name="number">ИИН или идентификатор платежа</param>
        /// <param name="source">Источник оплаты(название банка)</param>
        /// <param name="net">Вид сети</param>
        /// <param name="date">Дата</param>
        /// <param name="amount">Сумма</param>
        /// <param name="receipt">Номер платежа</param>
        /// <returns></returns>
        [HttpGet("/api/rps")]
        [Authorize(Permissions.OnlinePayment)]
        [Event(EventCode.OnlinePaymentTry, EventMode = EventMode.All, EntityType = EntityType.OnlinePayment, IncludeFails = true)]
        public async Task<OnlinePaymentOuterModel> Rps([FromQuery(Name = "action")] string action, string number, string source, string net, DateTime date, decimal amount = -1, Int64 receipt = -1)
        {
            var response = new OnlinePaymentOuterModel();
            switch (action)
            {
                #region Поиск клиента по ИИН
                case "check":
                    if (number != null)
                    {
                        var clients = _clientRepository.FindAllByIdentityNumberForProcessing(number);
                        if (clients != null)
                        {
                            response.Code = 0;
                            response.Message = "Клиент найден";
                            var creditLines = new List<Contract>();
                            var contracts = new List<Contract>();
                            foreach (var client in clients)
                            {
                                creditLines.AddRange(await _contractService.FindCreditLinesForProcessing(client.Id));
                                contracts.AddRange((await _contractService.FindForProcessingAsync(client.Id)).Where(contract =>
                                    (contract.CreditLineId == null &&
                                     contract.ContractClass != ContractClass.CreditLine) || (contract.CreditLineId != null && contract.IsOffBalance)).ToList());
                            }
                            List<object> invoicesToSend = new List<dynamic>();
                            if (creditLines.Count > 0)
                            {
                                DateTime? accrualDate = CalcAccrualDate(default);

                                foreach (var creditLine in creditLines)
                                {
                                    var organizationInfo = _organizationRepository.Get(creditLine.Branch.OrganizationId);
                                    Contract contract = _contractService.Get(creditLine.Id);

                                    var tranches = await _contractService.GetAllSignedTranches(contract.Id);
                                    foreach (var tranch in tranches)
                                    {
                                        if (accrualDate.HasValue && tranch.Status == ContractStatus.Signed)
                                        {
                                            var tempContract = _contractService.Get(tranch.Id);
                                            _processingService.InitAccruals(tempContract, accrualDate.Value.Date);
                                        }
                                    }

                                    // в _contractAmount передать дату в зависимости от времени запроса + 1 или + 0
                                    var paymentAmount = await 
                                        _creditLineService.GetAmountForCurrentlyPayment(creditLine.Id, accrualDate);

                                    dynamic invoice = new ExpandoObject();
                                    invoice.fio = creditLine.ContractData.Client.FullName;
                                    invoice.contractNumber = creditLine.ContractNumber;
                                    invoice.amount = paymentAmount < 0 ? 0 : Math.Ceiling(paymentAmount);
                                    invoice.date = creditLine.ContractDate;
                                    invoice.bankAccount = organizationInfo.Configuration.BankSettings.BankAccount;
                                    invoice.bankBik = organizationInfo.Configuration.BankSettings.BankBik;
                                    invoice.companyBin = organizationInfo.Configuration.LegalSettings.BIN.Replace(" ", "");
                                    invoice.companyName = organizationInfo.Configuration.LegalSettings.LegalName;
                                    invoice.id = creditLine.Id;
                                    invoice.position = CreatePositionDescription(contract);

                                    invoicesToSend.Add(invoice);
                                }
                            }

                            if (contracts.Count > 0)
                            {
                                DateTime? accrualDate = CalcAccrualDate(default);

                                foreach (var c in contracts)
                                {
                                    var organizationInfo = _organizationRepository.Get(c.Branch.OrganizationId);
                                    Contract contract = _contractService.Get(c.Id);

                                    if (accrualDate.HasValue && c.Status == ContractStatus.Signed)
                                    {
                                        _processingService.InitAccruals(contract, accrualDate.Value.Date);
                                    }
                                    // в _contractAmount передать дату в зависимости от времени запроса + 1 или + 0
                                    bool hasInscription = contract.InscriptionId.HasValue && contract.Inscription != null && contract.Inscription.Status != InscriptionStatus.Denied;

                                    _contractAmount.Init(contract, date: accrualDate, balanceAccountsOnly: hasInscription);

                                    dynamic invoice = new ExpandoObject();
                                    invoice.fio = c.ContractData.Client.FullName;
                                    invoice.contractNumber = c.ContractNumber;
                                    invoice.amount = _contractAmount.DisplayAmount < 0 ? 0 : Math.Ceiling(_contractAmount.DisplayAmount);
                                    invoice.date = c.ContractDate;
                                    invoice.bankAccount = organizationInfo.Configuration.BankSettings.BankAccount;
                                    invoice.bankBik = organizationInfo.Configuration.BankSettings.BankBik;
                                    invoice.companyBin = organizationInfo.Configuration.LegalSettings.BIN.Replace(" ", "");
                                    invoice.companyName = organizationInfo.Configuration.LegalSettings.LegalName;
                                    invoice.id = c.Id;
                                    invoice.position = CreatePositionDescription(contract);

                                    invoicesToSend.Add(invoice);
                                }
                            }

                            if (contracts.Any() || creditLines.Any())
                            {
                                response.invoice = invoicesToSend;
                                break;
                            }
                            response.Code = 10;
                            response.Message = "У клиента нет действующих договоров";
                            break;
                        }
                    }
                    response.Code = 2;
                    response.Message = "Клиент не найден";
                    break;
                #endregion

                #region Проведение платежа
                case "payment":
                    if (String.IsNullOrEmpty(number))
                    {
                        response.Code = 11;
                        response.Message = "Договор не найден";
                        break;
                    }

                    Regex regexAmount = new Regex(@"^\d{1,9}\.\d{2}$");
                    if (amount <= 0 || regexAmount.Matches(amount.ToString(new CultureInfo("en-US"))).Count <= 0)
                    {
                        response.Code = 3;
                        response.Message = "Неверная сумма платежа";
                        break;
                    }

                    if (date.Date != DateTime.Now.Date)
                    {
                        response.Code = 12;
                        response.Message = "Нельзя сделать оплату другим числом";
                        break;
                    }

                    Regex regexDate = new Regex(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}");
                    if (date == null || regexDate.Matches(date.ToString("s")).Count <= 0)
                    {
                        response.Code = 5;
                        response.Message = "Неверное значение даты";

                        break;
                    }

                    if (receipt <= 0)
                    {
                        response.Code = 4;
                        response.Message = "Неверное значение номера платежа";
                        break;
                    }
                    else
                    {
                        try
                        {
                            OnlinePayment onlinePayment = _onlinePaymentRepository.GetByProcessing(receipt, ProcessingType.Rps);
                            if (onlinePayment != null)
                            {
                                response.Code = 0;
                                response.Message = "Платеж был принят";
                                response.Date = onlinePayment.CreateDate;
                                response.Authcode = onlinePayment.Id;
                                break;
                            }
                        }
                        catch
                        {
                            response.Code = 13;
                            response.Message = "Ошибка при поиске платежа";
                            break;
                        }
                    }

                    Contract contractForAmount = new Contract();
                    try
                    {
                        contractForAmount = _contractService.Get(Convert.ToInt32(number));
                    }
                    catch
                    {
                        response.Code = 11;
                        response.Message = "Договор не найден";
                        break;
                    }

                    var processingInfo = new ProcessingInfo
                    {
                        Amount = amount,
                        Reference = receipt,
                        Type = ProcessingType.Rps,
                        BankName = source,
                        BankNetwork = net
                    };
                    using (IDbTransaction transaction = _clientRepository.BeginTransaction())
                    {
                        OnlinePayment onlinePayment = QueueOnlinePayment(contractForAmount, processingInfo);
                        if (onlinePayment == null)
                            throw new PawnshopApplicationException($"Ожидалось что {nameof(QueueOnlinePayment)} не вернет null");

                        if (!onlinePayment.Amount.HasValue)
                            throw new PawnshopApplicationException($"Ожидалось что {nameof(onlinePayment)}.{nameof(onlinePayment.Amount)} не будет null");

                        response.Code = 0;
                        response.Message = "Оплата принята";
                        response.Date = onlinePayment.CreateDate;
                        response.Authcode = onlinePayment.Id;
                        _eventLog.Log(EventCode.Prepayment, EventStatus.Success, EntityType.Contract, contractForAmount.Id, responseData: $@"Принята оплата по кредиту: {onlinePayment.Amount.Value} KZT от RPS Asia");
                        transaction.Commit();
                    }

                    break;
                #endregion

                #region Поиск платежа
                case "status":
                    if (receipt <= 0)
                    {
                        response.Code = 4;
                        response.Message = "Неверное значение номера платежа";
                        break;
                    }
                    else
                    {
                        try
                        {
                            OnlinePayment onlinePayment = _onlinePaymentRepository.GetByProcessing(receipt, ProcessingType.Rps);
                            if (onlinePayment != null)
                            {
                                response.Code = 0;
                                response.Message = "Платеж был принят";
                                response.Date = onlinePayment.CreateDate;
                                response.Authcode = onlinePayment.Id;
                                break;
                            }
                            else
                            {
                                response.Code = 6;
                                response.Message = "Платеж не найден в системе";
                                break;
                            }
                        }
                        catch
                        {
                            response.Code = 13;
                            response.Message = "Ошибка при поиске платежа";
                            break;
                        }
                    }
                    #endregion
            }

            return response;
        }

        [HttpGet("/api/processing/getexpandoobjects")]
        public async Task<IEnumerable<ExpandoObject>> GetExpandoObjects(string number)
        {
            var creditLines = new List<Contract>();
            var contracts = new List<Contract>();
            var clients = _clientRepository.FindAllByIdentityNumberForProcessing(number);

            foreach (var client in clients)
            {
                creditLines.AddRange(await _contractService.FindCreditLinesForProcessing(client.Id));
                contracts.AddRange((await _contractService.FindForProcessingAsync(client.Id)).Where(contract =>
                    (contract.CreditLineId == null &&
                     contract.ContractClass != ContractClass.CreditLine) || (contract.CreditLineId != null && contract.IsOffBalance)).ToList());
            }
            List<ExpandoObject> invoicesToSend = new List<ExpandoObject>();
            if (creditLines.Count > 0)
            {
                DateTime? accrualDate = CalcAccrualDate(default);

                foreach (var creditLine in creditLines)
                {
                    bool isExpenses = false;

                    Contract contract = _contractService.GetOnlyContract(creditLine.Id);
                    contract.PaymentSchedule = _contractService.GetOnlyPaymentSchedule(contract.Id);
                    if (contract.SettingId.HasValue)
                    {
                        contract.Setting = _contractService.GetProductSettings(contract.SettingId.Value);
                    }

                    contract.Expenses = _contractRepository.GetContractExpenses(contract.Id).Where(x => !x.IsPayed && _expenseService.Get(x.ExpenseId).ExtraExpense).ToList();
                    if (contract.Expenses.Any())
                    {
                        isExpenses = true;
                    }//доп расход

                    var tranches = (await _contractService.GetAllSignedTranches(contract.Id)).ToList();

                    bool isIncription = false;

                    var trueInscriptions = new List<bool>();
                    foreach (var tranch in tranches)
                    {
                        if (tranch.IsOffBalance && tranch.InscriptionId.HasValue)
                        {
                            trueInscriptions.Add(true);
                        }

                        if (!isIncription && !isExpenses)
                        {
                            if (accrualDate.HasValue && tranch.Status == ContractStatus.Signed)
                            {
                                var tempContract = _contractService.GetOnlyContract(tranch.Id);
                                tempContract.PaymentSchedule = _contractService.GetOnlyPaymentSchedule(tempContract.Id);
                                if (tempContract.SettingId.HasValue)
                                {
                                    tempContract.Setting = _contractService.GetProductSettings(tempContract.SettingId.Value);
                                }

                                _processingService.InitAccruals(tempContract, accrualDate.Value.Date);
                            }
                        }
                    }

                    if (!tranches.Any())
                    {
                        continue;
                    }
                    isIncription = trueInscriptions.Count == tranches.Count();

                    var paymentAmount = await
                        _creditLineService.GetAmountForCurrentlyPayment(creditLine.Id, accrualDate);

                    dynamic invoice = new ExpandoObject();
                    invoice.contractNumber = creditLine.ContractNumber;
                    invoice.amount = paymentAmount < 0 ? 0 : paymentAmount;
                    invoice.date = creditLine.ContractDate;
                    invoice.id = creditLine.Id;
                    invoice.isExpenses = isExpenses;
                    invoice.isInscription = isIncription;

                    invoicesToSend.Add(invoice);
                }
            }

            if (contracts.Any())
            {
                DateTime? accrualDate = CalcAccrualDate(default);


                foreach (var c in contracts)
                {
                    bool isIncription = false;
                    bool isExpenses = false;
                    Contract contract = _contractService.GetOnlyContract(c.Id);
                    if (contract.IsOffBalance && contract.InscriptionId.HasValue)
                    {
                        isIncription = true;
                    }//исп надпись

                    contract.Expenses = _contractRepository.GetContractExpenses(contract.Id).Where(x => !x.IsPayed && _expenseService.Get(x.ExpenseId).ExtraExpense).ToList();

                    if (contract.Expenses.Any())
                    {
                        isExpenses = true;
                    }//доп расход

                    if (!isIncription && !isExpenses)
                    {
                        contract.PaymentSchedule = _contractService.GetOnlyPaymentSchedule(contract.Id);
                        if (contract.SettingId.HasValue)
                        {
                            contract.Setting = _contractService.GetProductSettings(contract.SettingId.Value);
                        }

                        if (accrualDate.HasValue && c.Status == ContractStatus.Signed)
                        {
                            _processingService.InitAccruals(contract, accrualDate.Value.Date);
                        }
                        bool hasInscription = contract.InscriptionId.HasValue && contract.Inscription != null && contract.Inscription.Status != InscriptionStatus.Denied;
                        _contractAmount.Init(contract, date: accrualDate, balanceAccountsOnly: hasInscription);
                    }

                    dynamic invoice = new ExpandoObject();
                    invoice.contractNumber = c.ContractNumber;
                    invoice.amount = _contractAmount.DisplayAmount < 0 ? 0 : _contractAmount.DisplayAmount;
                    invoice.date = c.ContractDate;
                    invoice.id = c.Id;
                    invoice.isExpenses = isExpenses;
                    invoice.isInscription = isIncription;

                    invoicesToSend.Add(invoice);
                }
            }

            return invoicesToSend;
        }

        [HttpGet("/api/processing/getexpandoobjectbyid")]
        public async Task<ActionResult<ExpandoObject>> GetExpandoObjectById(int id)
        {
            var contract = _contractService.GetOnlyContract(id);
            if (contract == null)
            {
                return NotFound();
            }
            if (contract.IsOffBalance && contract.InscriptionId.HasValue)//исп надпись
            {
                return NotFound();
            }
            contract.Expenses = _contractRepository.GetContractExpenses(id).Where(x => !x.IsPayed && _expenseService.Get(x.ExpenseId).ExtraExpense).ToList();

            if (contract.Expenses.Any())//доп расход
            {
                return NotFound();
            }
            contract.PaymentSchedule = _contractService.GetOnlyPaymentSchedule(contract.Id);
            if (contract.SettingId.HasValue)
            {
                contract.Setting = _contractService.GetProductSettings(contract.SettingId.Value);
            }

            DateTime? accrualDate = CalcAccrualDate(default);
            decimal paymentAmount = 0;
            var tranches = await _contractService.GetAllSignedTranches(contract.Id);
            if (tranches.Any())
            {
                var isExist = false;
                foreach (var tranch in tranches)
                {
                    tranch.Expenses = _contractRepository.GetContractExpenses(tranch.Id).Where(x => !x.IsPayed && _expenseService.Get(x.ExpenseId).ExtraExpense).ToList();
                    if (tranch.Expenses.Any())//доп расход
                    {
                       continue;
                    }
                    if (tranch.IsOffBalance && tranch.InscriptionId.HasValue)//исп надпись
                    {
                        continue;
                    }
                    if (accrualDate.HasValue && tranch.Status == ContractStatus.Signed)
                    {
                        var tempContract = _contractService.GetOnlyContract(tranch.Id);
                        tempContract.PaymentSchedule = _contractService.GetOnlyPaymentSchedule(tempContract.Id);
                        if (tempContract.SettingId.HasValue)
                        {
                            tempContract.Setting = _contractService.GetProductSettings(tempContract.SettingId.Value);
                        }
                        _processingService.InitAccruals(tempContract, accrualDate.Value.Date);
                    }

                    isExist = true;
                }

                if (isExist)
                {
                    paymentAmount =
                        await _creditLineService.GetAmountForCurrentlyPayment(contract.Id, accrualDate);
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                if (accrualDate.HasValue && contract.Status == ContractStatus.Signed)
                {
                    _processingService.InitAccruals(contract, accrualDate.Value.Date);
                }
                bool hasInscription = contract.InscriptionId.HasValue && contract.Inscription != null && contract.Inscription.Status != InscriptionStatus.Denied;
                _contractAmount.Init(contract, date: accrualDate, balanceAccountsOnly: hasInscription);
                paymentAmount = _contractAmount.DisplayAmount < 0 ? 0 : _contractAmount.DisplayAmount;
            }

            dynamic invoice = new ExpandoObject();
            invoice.clientId = contract.ClientId;
            invoice.branchId = contract.BranchId;
            invoice.contractNumber = contract.ContractNumber;
            invoice.amount = paymentAmount;
            invoice.date = contract.ContractDate;
            invoice.id = contract.Id;

            return invoice;
        }

        [HttpGet("/api/processing/getPrepaymentAmount")]
        [Event(EventCode.OnlinePaymentTry, EventMode = EventMode.All, EntityType = EntityType.OnlinePayment)]
        public IActionResult GetPrepaymentAmount(int contractId)
        {
            var contract = _contractService.GetOnlyContract(contractId);
            decimal amount;
            if (contract.ContractClass == ContractClass.CreditLine || (contract.ContractClass == ContractClass.Tranche && contract.IsOffBalance == false))
            {
                var creditLineId = contract.CreditLineId.HasValue ? contract.CreditLineId.Value : contract.Id;
                var distribution = _creditLineService.GetCurrentlyDebtForCreditLine(creditLineId).Result;
                amount = distribution.SummaryPrepaymentBalance;
            }
            else
            {
                amount = _contractService.GetPrepaymentBalance(contractId);
            }
            return Ok(amount);
        }

        [HttpGet("/api/processing")]
        [Authorize(Permissions.OnlinePayment)]
        [Event(EventCode.OnlinePaymentTry, EventMode = EventMode.All, EntityType = EntityType.OnlinePayment, IncludeFails = true)]
        public async Task<OnlinePaymentOuterModel> Processing([FromQuery(Name = "action")] string action, string number, DateTime date,
            decimal amount = -1, Int64 receipt = -1, string source = null, string net = null)
        {
            var response = new OnlinePaymentOuterModel();
            switch (action)
            {
                #region Поиск клиента по ИИН
                case "check":
                    if (number != null)
                    {
                        var client = _clientRepository.FindByIdentityNumber(number);
                        if (client != null)
                        {
                            response.Code = 0;
                            response.Message = "Клиент найден";

                            var creditLines = await _contractService.FindCreditLinesForProcessing(client.Id);
                            List<object> invoicesToSend = new List<dynamic>();
                            if (creditLines.Count > 0)
                            {

                                DateTime? accrualDate = CalcAccrualDate(default);

                                foreach (var creditLine in creditLines)
                                {
                                    var organizationInfo = _organizationRepository.Get(creditLine.Branch.OrganizationId);
                                    var contract = _contractService.Get(creditLine.Id);

                                    var tranches = await _contractService.GetAllSignedTranches(contract.Id);
                                    foreach (var tranch in tranches)
                                    {
                                        if (accrualDate.HasValue && tranch.Status == ContractStatus.Signed)
                                        {
                                            var tempContract = _contractService.Get(tranch.Id);
                                            _processingService.InitAccruals(tempContract, accrualDate.Value.Date);
                                        }
                                    }
                                    var paymentAmount =  await _creditLineService.GetAmountForCurrentlyPayment(creditLine.Id, accrualDate);

                                    dynamic invoice = new ExpandoObject();
                                    invoice.fio = creditLine.ContractData.Client.FullName;
                                    invoice.contractNumber = creditLine.ContractNumber;
                                    invoice.amount = paymentAmount < 0 ? 0 : Math.Ceiling(paymentAmount);
                                    invoice.date = creditLine.ContractDate;
                                    invoice.bankAccount = organizationInfo.Configuration.BankSettings.BankAccount;
                                    invoice.bankBik = organizationInfo.Configuration.BankSettings.BankBik;
                                    invoice.companyBin = organizationInfo.Configuration.LegalSettings.BIN.Replace(" ", "");
                                    invoice.companyName = organizationInfo.Configuration.LegalSettings.LegalName;
                                    invoice.id = creditLine.Id;
                                    invoice.position = CreatePositionDescription(contract);

                                    invoicesToSend.Add(invoice);
                                }
                            }

                            var contracts = (await _contractService.FindForProcessingAsync(client.Id)).Where(contract =>
                                (contract.CreditLineId == null &&
                                 contract.ContractClass != ContractClass.CreditLine) || (contract.CreditLineId != null && contract.IsOffBalance)).ToList();
                            if (contracts.Count > 0)
                            {

                                DateTime? accrualDate = CalcAccrualDate(default);

                                foreach (var c in contracts)
                                {
                                    var organizationInfo = _organizationRepository.Get(c.Branch.OrganizationId);
                                    var contract = _contractService.Get(c.Id);

                                    if (accrualDate.HasValue && c.Status == ContractStatus.Signed)
                                    {
                                        _processingService.InitAccruals(contract, accrualDate.Value.Date);
                                    }

                                    bool hasInscription = contract.InscriptionId.HasValue && contract.Inscription != null && contract.Inscription.Status != InscriptionStatus.Denied;

                                    _contractAmount.Init(contract, date: accrualDate, balanceAccountsOnly: hasInscription);

                                    dynamic invoice = new ExpandoObject();
                                    invoice.fio = c.ContractData.Client.FullName;
                                    invoice.contractNumber = c.ContractNumber;
                                    invoice.amount = _contractAmount.DisplayAmount < 0 ? 0 : Math.Ceiling(_contractAmount.DisplayAmount);
                                    invoice.date = c.ContractDate;
                                    invoice.bankAccount = organizationInfo.Configuration.BankSettings.BankAccount;
                                    invoice.bankBik = organizationInfo.Configuration.BankSettings.BankBik;
                                    invoice.companyBin = organizationInfo.Configuration.LegalSettings.BIN.Replace(" ", "");
                                    invoice.companyName = organizationInfo.Configuration.LegalSettings.LegalName;
                                    invoice.id = c.Id;
                                    invoice.position = CreatePositionDescription(contract);

                                    invoicesToSend.Add(invoice);
                                }
                            }

                            if (contracts.Any() || creditLines.Any())
                            {
                                response.invoice = invoicesToSend;
                                break;
                            }


                            response.Code = 10;
                            response.Message = "У клиента нет действующих договоров";
                            break;
                        }
                    }
                    response.Code = 2;
                    response.Message = "Клиент не найден";
                    break;
                #endregion

                #region Проведение платежа
                case "payment":
                    if (String.IsNullOrEmpty(number))
                    {
                        response.Code = 11;
                        response.Message = "Договор не найден";
                        break;
                    }

                    Regex regexAmount = new Regex(@"^\d{1,9}\.\d{2}$");
                    if (amount <= 0 || regexAmount.Matches(amount.ToString(new CultureInfo("en-US"))).Count <= 0)
                    {
                        response.Code = 3;
                        response.Message = "Неверная сумма платежа";
                        break;
                    }

                    if (date.Date != DateTime.Now.Date)
                    {
                        response.Code = 12;
                        response.Message = "Нельзя сделать оплату другим числом";
                        break;
                    }

                    Regex regexDate = new Regex(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}");
                    if (date == null || regexDate.Matches(date.ToString("s")).Count <= 0)
                    {
                        response.Code = 5;
                        response.Message = "Неверное значение даты";
                        break;
                    }

                    if (receipt <= 0)
                    {
                        response.Code = 4;
                        response.Message = "Неверное значение номера платежа";
                        break;
                    }
                    else
                    {
                        try
                        {
                            OnlinePayment onlinePayment = _onlinePaymentRepository.GetByProcessing(receipt, ProcessingType.Processing);
                            if (onlinePayment != null)
                            {
                                response.Code = 0;
                                response.Message = "Платеж был принят";
                                response.Date = onlinePayment.CreateDate;
                                response.Authcode = onlinePayment.Id;
                                break;
                            }
                        }
                        catch
                        {
                            response.Code = 13;
                            response.Message = "Ошибка при поиске платежа";
                            break;
                        }
                    }

                    Contract contractForAmount = new Contract();
                    try
                    {
                        contractForAmount = _contractService.Get(Convert.ToInt32(number));
                    }
                    catch
                    {
                        response.Code = 11;
                        response.Message = "Договор не найден";
                        break;
                    }

                    response.Code = 0;
                    response.Message = "Оплата принята";
                    response.Date = DateTime.Now;
                    var processingInfo = new ProcessingInfo
                    {
                        Amount = amount,
                        Reference = receipt,
                        Type = ProcessingType.Processing,
                        BankName = source,
                        BankNetwork = net
                    };
                    using (IDbTransaction transaction = _clientRepository.BeginTransaction())
                    {
                        OnlinePayment onlinePayment = QueueOnlinePayment(contractForAmount, processingInfo);
                        if (onlinePayment == null)
                            throw new PawnshopApplicationException($"Ожидалось что {nameof(QueueOnlinePayment)} не вернет null");

                        if (!onlinePayment.Amount.HasValue)
                            throw new PawnshopApplicationException($"Ожидалось что {nameof(onlinePayment)}.{nameof(onlinePayment.Amount)} не будет null");

                        response.Authcode = onlinePayment.Id;
                        _eventLog.Log(EventCode.Prepayment, EventStatus.Success, EntityType.Contract, contractForAmount.Id, responseData: $@"Принята оплата по кредиту: {onlinePayment.Amount.Value} KZT от Processing");
                        transaction.Commit();
                    }

                    break;

                #endregion

                #region Поиск платежа

                case "status":
                    if (receipt <= 0)
                    {
                        response.Code = 4;
                        response.Message = "Неверное значение номера платежа";
                        break;
                    }
                    else
                    {
                        try
                        {
                            OnlinePayment onlinePayment = _onlinePaymentRepository.GetByProcessing(receipt, ProcessingType.Processing);
                            if (onlinePayment != null)
                            {
                                response.Code = 0;
                                response.Message = "Платеж был принят";
                                response.Date = onlinePayment.CreateDate;
                                response.Authcode = onlinePayment.Id;
                                break;
                            }
                            else
                            {
                                response.Code = 6;
                                response.Message = "Платеж не найден в системе";
                                break;
                            }
                        }
                        catch
                        {
                            response.Code = 13;
                            response.Message = "Ошибка при поиске платежа";
                            break;
                        }
                    }

                #endregion


                default:
                    response.Code = 1;
                    response.Message = "Неизвестный тип запроса";
                    break;
            }
            return response;
        }

        [HttpGet("/api/qiwi")]
        [Authorize(AuthenticationSchemes = BasicAuthenticationDefaults.AuthenticationScheme, Policy = Permissions.OnlinePayment)]
        [Event(EventCode.OnlinePaymentTry, EventMode = EventMode.All, EntityType = EntityType.OnlinePayment, IncludeFails = true)]
        public async Task<string> Qiwi([FromQuery(Name = "command")] string command, string account, string txn_date, decimal sum = -1, Int64 txn_id = -1, int data1 = -1)
        {
            //Если у нас BasicAuthentication и нужен будет sessionContext, то нужно егообязательно вручную инициализировать
            var identity = (ClaimsIdentity)HttpContext.User.Identity;
            _sessionContext.InitFromClaims(identity.Claims.ToArray());

            string answer = string.Empty;
            QiwiOuterModel response = new QiwiOuterModel();
            response.Osmp_txn_id = txn_id;

            switch (command)
            {

                #region Поиск клиента по ИИН
                case "check":
                    if (!string.IsNullOrEmpty(account))
                    {
                        var client = _clientRepository.FindByIdentityNumber(account);
                        if (client == null)
                        {
                            response.Result = 5;
                            response.Comment = "Неверный формат идентификатора абонента";
                        }
                        else
                        {
                            response.Result = 0;
                            response.Comment = "ОК";

                            if (data1 > 0)
                            {
                                var contract = _contractService.Get(data1);
                                if (contract.CreditLineId != null || contract.ContractClass == ContractClass.CreditLine)
                                {
                                    int creditLineId;
                                    if (contract.ContractClass == ContractClass.CreditLine)
                                        creditLineId = contract.Id;
                                    else
                                        creditLineId = contract.CreditLineId.Value;
                                    var crediltLine = _contractService.Get(creditLineId);
                                    if (!DateTime.TryParse(txn_date, out DateTime date))
                                        date = default;

                                    DateTime? accrualDate = CalcAccrualDate(default);

                                    if (accrualDate.HasValue && contract.Status == ContractStatus.Signed)
                                    {
                                        _processingService.InitAccruals(contract, accrualDate.Value.Date);
                                    }
                                    var tranches = await _contractService.GetAllSignedTranches(contract.Id);
                                    foreach (var tranch in tranches)
                                    {
                                        if (accrualDate.HasValue && tranch.Status == ContractStatus.Signed)
                                        {
                                            var tempContract = _contractService.Get(tranch.Id);
                                            _processingService.InitAccruals(tempContract, accrualDate.Value.Date);
                                        }
                                    }

                                    var paymentAmount = await
                                        _creditLineService.GetAmountForCurrentlyPayment(crediltLine.Id, accrualDate);

                                    if (contract != null && contract.ClientId == client.Id)
                                    {
                                        Fields field = new Fields()
                                        {
                                            ContractNumber = crediltLine.ContractNumber,
                                            ClientFullName = client.FullName,
                                            AmountForPay = paymentAmount < 0 ? 0 : Math.Ceiling(paymentAmount),
                                            ContractDate = crediltLine.ContractDate.Date.ToString("dd.MM.yyyy"),
                                            ContractId = crediltLine.Id,
                                            PositionInfo = CreatePositionDescription(contract)
                                        };
                                        response.Fields = field;
                                    }
                                    else
                                    {
                                        response.Result = 5;
                                        response.Comment = "У клиента нет действующих договоров";
                                    }
                                    break;
                                }
                                else
                                {
                                    if (!DateTime.TryParse(txn_date, out DateTime date))
                                        date = default;

                                    DateTime? accrualDate = CalcAccrualDate(default);

                                    if (accrualDate.HasValue && contract.Status == ContractStatus.Signed)
                                    {
                                        _processingService.InitAccruals(contract, accrualDate.Value.Date);
                                    }

                                    bool hasInscription = contract.InscriptionId.HasValue && contract.Inscription != null && contract.Inscription.Status != InscriptionStatus.Denied;

                                    _contractAmount.Init(contract, date: accrualDate, balanceAccountsOnly: hasInscription);

                                    if (contract != null && contract.ClientId == client.Id)
                                    {
                                        Fields field = new Fields()
                                        {
                                            ContractNumber = contract.ContractNumber,
                                            ClientFullName = client.FullName,
                                            AmountForPay = _contractAmount.DisplayAmount < 0 ? 0 : Math.Ceiling(_contractAmount.DisplayAmount),
                                            ContractDate = contract.ContractDate.Date.ToString("dd.MM.yyyy"),
                                            ContractId = contract.Id,
                                            PositionInfo = CreatePositionDescription(contract)
                                        };
                                        response.Fields = field;
                                    }
                                    else
                                    {
                                        response.Result = 5;
                                        response.Comment = "У клиента нет действующих договоров";
                                    }
                                    break;
                                }

                            }
                            else
                            {
                                List<Orders> ordersToSend = new List<Orders>();
                                var creditLines = await _contractService.FindCreditLinesForProcessing(client.Id);
                                if (creditLines.Count > 0)
                                {

                                    if (!DateTime.TryParse(txn_date, out DateTime date))
                                        date = default;

                                    DateTime? accrualDate = CalcAccrualDate(default);

                                    foreach (var creditLine in creditLines)
                                    {
                                        var contract = _contractService.Get(creditLine.Id);

                                        var tranches = await _contractService.GetAllSignedTranches(contract.Id);
                                        foreach (var tranch in tranches)
                                        {
                                            if (accrualDate.HasValue && tranch.Status == ContractStatus.Signed)
                                            {
                                                var tempContract = _contractService.Get(tranch.Id);
                                                _processingService.InitAccruals(tempContract, accrualDate.Value.Date);
                                            }
                                        }

                                        var paymentAmount = await 
                                            _creditLineService.GetAmountForCurrentlyPayment(creditLine.Id, accrualDate);

                                        Orders order = new Orders()
                                        {
                                            OrderId = creditLine.Id,
                                            Text = creditLine.ContractNumber + ", " + CreatePositionDescription(contract),
                                            Amount = paymentAmount < 0 ? 0 : Math.Ceiling(paymentAmount)
                                        };

                                        ordersToSend.Add(order);
                                    }
                                }

                                var contracts = (await _contractService.FindForProcessingAsync(client.Id)).Where(contract =>
                                    (contract.CreditLineId == null &&
                                     contract.ContractClass != ContractClass.CreditLine) || (contract.CreditLineId != null && contract.IsOffBalance)).ToList();
                                if (contracts.Count > 0)
                                {

                                    if (!DateTime.TryParse(txn_date, out DateTime date))
                                        date = default;

                                    DateTime? accrualDate = CalcAccrualDate(default);

                                    foreach (var c in contracts)
                                    {
                                        var contract = _contractService.Get(c.Id);

                                        if (accrualDate.HasValue && c.Status == ContractStatus.Signed)
                                        {
                                            _processingService.InitAccruals(contract, accrualDate.Value.Date);
                                        }

                                        _contractAmount.Init(contract, date: accrualDate);

                                        Orders order = new Orders()
                                        {
                                            OrderId = c.Id,
                                            Text = c.ContractNumber + ", " + CreatePositionDescription(contract),
                                            Amount = _contractAmount.DisplayAmount < 0 ? 0 : Math.Ceiling(_contractAmount.DisplayAmount)
                                        };

                                        ordersToSend.Add(order);
                                    }
                                }

                                if (creditLines.Any() || contracts.Any())
                                {
                                    response.Orders = ordersToSend;
                                    break;
                                }

                                response.Result = 5;
                                response.Comment = "У клиента нет действующих договоров";

                                break;
                            }
                        }
                    }
                    response.Result = 4;
                    response.Comment = "Идентификатор абонента не найден (Ошиблись номером)";
                    break;
                #endregion

                #region Проведение платежа
                case "pay":
                    if (data1 <= 0)
                    {
                        response.Result = 5;
                        response.Comment = "Договор не найден";
                        break;
                    }

                    Regex regexAmount = new Regex(@"^\d{1,9}\.\d{2}$");
                    if (sum <= 0 || regexAmount.Matches(sum.ToString(new CultureInfo("en-US"))).Count <= 0)
                    {
                        response.Result = 300;
                        response.Comment = "Неверный формат суммы платежа";
                        break;
                    }

                    if (txn_date == null)
                    {
                        response.Result = 300;
                        response.Comment = "Неверное значение даты";
                        break;
                    }

                    DateTime requestDate;
                    try
                    {
                        DateTime.TryParseExact(txn_date, "yyyyMMddHHmmss",
                          CultureInfo.InvariantCulture,
                          DateTimeStyles.None, out requestDate);
                    }
                    catch
                    {
                        response.Result = 300;
                        response.Comment = "Неверный формат даты";
                        break;
                    }


                    if (txn_id <= 0)
                    {
                        response.Result = 300;
                        response.Comment = "Неверный идентификатор платеж в системе QIWI";
                        break;
                    }
                    else
                    {
                        try
                        {
                            OnlinePayment onlinePayment = _onlinePaymentRepository.GetByProcessing(txn_id, ProcessingType.Qiwi);
                            if (onlinePayment != null)
                            {
                                response.Comment = "Платеж уже был принят";
                                response.Prv_txn = onlinePayment.Id;
                                break;
                            }
                        }
                        catch
                        {
                            response.Result = 300;
                            response.Comment = "Ошибка при поиске платежа";
                            break;
                        }
                    }

                    Contract contractForAmount = null;
                    Client clientToCheck = null;
                    try
                    {
                        contractForAmount = _contractService.Get(Convert.ToInt32(data1));
                        clientToCheck = _clientRepository.Get(contractForAmount.ClientId);
                        if (clientToCheck.IdentityNumber != account) throw new PawnshopApplicationException();
                    }
                    catch
                    {
                        response.Result = 5;
                        response.Comment = "Договор не найден";
                        break;
                    }

                    var processingInfo = new ProcessingInfo
                    {
                        Amount = sum,
                        Reference = txn_id,
                        Type = ProcessingType.Qiwi
                    };

                    using (IDbTransaction transaction = _clientRepository.BeginTransaction())
                    {
                        OnlinePayment onlinePayment = QueueOnlinePayment(contractForAmount, processingInfo);
                        if (onlinePayment == null)
                            throw new PawnshopApplicationException($"Ожидалось что {nameof(QueueOnlinePayment)} не вернет null");

                        if (!onlinePayment.Amount.HasValue)
                            throw new PawnshopApplicationException($"Ожидалось что {nameof(onlinePayment)}.{nameof(onlinePayment.Amount)} не будет null");


                        response.Prv_txn = onlinePayment.Id;
                        response.Sum = onlinePayment.Amount.Value;
                        response.Result = 0;
                        response.Comment = "OK";
                        _eventLog.Log(EventCode.Prepayment, EventStatus.Success, EntityType.Contract, contractForAmount.Id, responseData: $@"Принята оплата по кредиту: {onlinePayment.Amount.Value} KZT от qiwi.kz");
                        transaction.Commit();
                    }


                    break;
                #endregion

                default:
                    response.Result = 300;
                    response.Comment = "Неизвестный тип запроса";
                    break;
            }

            //Create our own namespaces for the output
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

            //Add an empty namespace and empty value
            ns.Add("", "");

            //Create serializer
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(QiwiOuterModel));

            using (StringWriter textWriter = new Utf8StringWriter())
            {
                xmlSerializer.Serialize(textWriter, response, ns);
                answer = textWriter.ToString();
            }
            return answer;
        }

        [HttpGet("/api/jetpay")]
        [Authorize(Permissions.OnlinePayment)]
        [Event(EventCode.OnlinePaymentTry, EventMode = EventMode.All, EntityType = EntityType.OnlinePayment, IncludeFails = true)]
        public async Task<OnlinePaymentOuterModel> JetPay([FromQuery(Name = "action")] string action, string number, DateTime date,
            decimal amount = -1, Int64 receipt = -1, string source = null, string net = null)
        {
            var response = new OnlinePaymentOuterModel();
            switch (action)
            {
                #region Проведение платежа
                case "payment":
                    if (String.IsNullOrEmpty(number))
                        throw new PawnshopApplicationException($"Code = 11; Договор не найден");

                    Regex regexAmount = new Regex(@"^\d{1,9}\.\d{2}$");
                    if (amount <= 0 || regexAmount.Matches(amount.ToString(new CultureInfo("en-US"))).Count <= 0)
                        throw new PawnshopApplicationException($"Code = 3; Неверная сумма платежа");

                    if (date.Date != DateTime.Now.Date)
                        throw new PawnshopApplicationException($"Code = 12; Нельзя сделать оплату другим числом");

                    Regex regexDate = new Regex(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}");
                    if (date == null || regexDate.Matches(date.ToString("s")).Count <= 0)
                        throw new PawnshopApplicationException($"Code = 5; Неверное значение даты");

                    if (receipt <= 0)
                        throw new PawnshopApplicationException($"Code = 4; Неверное значение номера платежа");
                    else
                    {
                        try
                        {
                            OnlinePayment onlinePayment = _onlinePaymentRepository.GetByProcessing(receipt, ProcessingType.JetPay);
                            if (onlinePayment != null)
                            {
                                response.Code = 0;
                                response.Message = "Платеж был принят";
                                response.Date = onlinePayment.CreateDate;
                                response.Authcode = onlinePayment.Id;
                                break;
                            }
                        }
                        catch
                        {
                            throw new PawnshopApplicationException($"Code = 13; Ошибка при поиске платежа");
                        }
                    }

                    Contract contractForAmount = new Contract();
                    try
                    {
                        contractForAmount = _contractService.Get(Convert.ToInt32(number));
                        //TODO neeed to speak about this  
                        if (contractForAmount.CreditLineId != null)
                            contractForAmount = _contractService.Get(contractForAmount.CreditLineId.Value);
                    }
                    catch
                    {
                        throw new PawnshopApplicationException($"Code = 11; Договор не найден");
                    }

                    response.Code = 0;
                    response.Message = "Оплата принята";
                    response.Date = DateTime.Now;
                    var processingInfo = new ProcessingInfo
                    {
                        Amount = amount,
                        Reference = receipt,
                        Type = ProcessingType.JetPay,
                        BankName = source,
                        BankNetwork = net
                    };
                    using (IDbTransaction transaction = _clientRepository.BeginTransaction())
                    {
                        OnlinePayment onlinePayment = QueueOnlinePayment(contractForAmount, processingInfo);
                        if (onlinePayment == null)
                            throw new PawnshopApplicationException($"Ожидалось что {nameof(QueueOnlinePayment)} не вернет null");

                        if (!onlinePayment.Amount.HasValue)
                            throw new PawnshopApplicationException($"Ожидалось что {nameof(onlinePayment)}.{nameof(onlinePayment.Amount)} не будет null");

                        response.Authcode = onlinePayment.Id;
                        _eventLog.Log(EventCode.Prepayment, EventStatus.Success, EntityType.Contract, contractForAmount.Id, responseData: $@"Принята оплата по кредиту: {onlinePayment.Amount.Value} KZT от PayJet");
                        transaction.Commit();
                    }

                    break;

                #endregion

                default:
                    throw new PawnshopApplicationException($"{action} Неизвестный тип запроса");
            }
            return response;
        }

        [HttpGet("/api/paydala")]
        [Authorize(Permissions.OnlinePayment)]
        [Event(EventCode.OnlinePaymentTry, EventMode = EventMode.All, EntityType = EntityType.OnlinePayment, IncludeFails = true)]
        public async Task<OnlinePaymentOuterModel> PayDala([FromQuery(Name = "action")] string action, string number, DateTime date,
            decimal amount = -1, Int64 receipt = -1, string source = null, string net = null)
        {
            var response = new OnlinePaymentOuterModel();
            switch (action)
            {
                #region Поиск клиента по ИИН
                case "check":
                    if (number != null)
                    {
                        var client = _clientRepository.FindByIdentityNumber(number);
                        if (client != null)
                        {
                            response.Code = 0;
                            response.Message = "Клиент найден";
                            List<object> invoicesToSend = new List<dynamic>();
                            var creditLines = await _contractService.FindCreditLinesForProcessing(client.Id);
                            if (creditLines.Count > 0)
                            {
                                DateTime? accrualDate = CalcAccrualDate(default);

                                foreach (var creditLine in creditLines)
                                {
                                    var organizationInfo = _organizationRepository.Get(creditLine.Branch.OrganizationId);
                                    var contract = _contractService.Get(creditLine.Id);

                                    var tranches = await _contractService.GetAllSignedTranches(contract.Id);
                                    foreach (var tranch in tranches)
                                    {
                                        if (accrualDate.HasValue && tranch.Status == ContractStatus.Signed)
                                        {
                                            var tempContract = _contractService.Get(tranch.Id);
                                            _processingService.InitAccruals(tempContract, accrualDate.Value.Date);
                                        }
                                    }

                                    var paymentAmount = await 
                                        _creditLineService.GetAmountForCurrentlyPayment(creditLine.Id, accrualDate);

                                    dynamic invoice = new ExpandoObject();
                                    invoice.fio = creditLine.ContractData.Client.FullName;
                                    invoice.contractNumber = creditLine.ContractNumber;
                                    invoice.amount = paymentAmount < 0 ? 0 : Math.Ceiling(paymentAmount);
                                    invoice.date = creditLine.ContractDate;
                                    invoice.bankAccount = organizationInfo.Configuration.BankSettings.BankAccount;
                                    invoice.bankBik = organizationInfo.Configuration.BankSettings.BankBik;
                                    invoice.companyBin = organizationInfo.Configuration.LegalSettings.BIN.Replace(" ", "");
                                    invoice.companyName = organizationInfo.Configuration.LegalSettings.LegalName;
                                    invoice.id = creditLine.Id;
                                    invoice.position = CreatePositionDescription(contract);

                                    invoicesToSend.Add(invoice);
                                }
                            }

                            var contracts = (await _contractService.FindForProcessingAsync(client.Id)).Where(contract =>
                                (contract.CreditLineId == null &&
                                 contract.ContractClass != ContractClass.CreditLine) || (contract.CreditLineId != null && contract.IsOffBalance)).ToList();

                            if (contracts.Count > 0)
                            {

                                DateTime? accrualDate = CalcAccrualDate(default);

                                foreach (var c in contracts)
                                {
                                    var organizationInfo = _organizationRepository.Get(c.Branch.OrganizationId);
                                    var contract = _contractService.Get(c.Id);

                                    if (accrualDate.HasValue && c.Status == ContractStatus.Signed)
                                    {
                                        _processingService.InitAccruals(contract, accrualDate.Value.Date);
                                    }

                                    bool hasInscription = contract.InscriptionId.HasValue && contract.Inscription != null && contract.Inscription.Status != InscriptionStatus.Denied;

                                    _contractAmount.Init(contract, date: accrualDate, balanceAccountsOnly: hasInscription);

                                    dynamic invoice = new ExpandoObject();
                                    invoice.fio = c.ContractData.Client.FullName;
                                    invoice.contractNumber = c.ContractNumber;
                                    invoice.amount = _contractAmount.DisplayAmount < 0 ? 0 : Math.Ceiling(_contractAmount.DisplayAmount);
                                    invoice.date = c.ContractDate;
                                    invoice.bankAccount = organizationInfo.Configuration.BankSettings.BankAccount;
                                    invoice.bankBik = organizationInfo.Configuration.BankSettings.BankBik;
                                    invoice.companyBin = organizationInfo.Configuration.LegalSettings.BIN.Replace(" ", "");
                                    invoice.companyName = organizationInfo.Configuration.LegalSettings.LegalName;
                                    invoice.id = c.Id;
                                    invoice.position = CreatePositionDescription(contract);

                                    invoicesToSend.Add(invoice);
                                }
                            }

                            if (creditLines.Any() || contracts.Any())
                            {
                                response.invoice = invoicesToSend;
                                break;
                            }


                            response.Code = 10;
                            response.Message = "У клиента нет действующих договоров";
                            break;
                        }
                    }
                    response.Code = 2;
                    response.Message = "Клиент не найден";
                    break;
                #endregion

                #region Проведение платежа
                case "payment":
                    if (String.IsNullOrEmpty(number))
                    {
                        response.Code = 11;
                        response.Message = "Договор не найден";
                        break;
                    }

                    Regex regexAmount = new Regex(@"^\d{1,9}\.\d{2}$");
                    if (amount <= 0 || regexAmount.Matches(amount.ToString(new CultureInfo("en-US"))).Count <= 0)
                    {
                        response.Code = 3;
                        response.Message = "Неверная сумма платежа";
                        break;
                    }

                    if (date.Date != DateTime.Now.Date)
                    {
                        response.Code = 12;
                        response.Message = "Нельзя сделать оплату другим числом";
                        break;
                    }

                    Regex regexDate = new Regex(@"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}");
                    if (date == null || regexDate.Matches(date.ToString("s")).Count <= 0)
                    {
                        response.Code = 5;
                        response.Message = "Неверное значение даты";
                        break;
                    }

                    if (receipt <= 0)
                    {
                        response.Code = 4;
                        response.Message = "Неверное значение номера платежа";
                        break;
                    }
                    else
                    {
                        try
                        {
                            OnlinePayment onlinePayment = _onlinePaymentRepository.GetByProcessing(receipt, ProcessingType.PayDala);
                            if (onlinePayment != null)
                            {
                                response.Code = 0;
                                response.Message = "Платеж был принят";
                                response.Date = onlinePayment.CreateDate;
                                response.Authcode = onlinePayment.Id;
                                break;
                            }
                        }
                        catch
                        {
                            response.Code = 13;
                            response.Message = "Ошибка при поиске платежа";
                            break;
                        }
                    }

                    Contract contractForAmount = new Contract();
                    try
                    {
                        contractForAmount = _contractService.Get(Convert.ToInt32(number));
                    }
                    catch
                    {
                        response.Code = 11;
                        response.Message = "Договор не найден";
                        break;
                    }

                    response.Code = 0;
                    response.Message = "Оплата принята";
                    response.Date = DateTime.Now;
                    var processingInfo = new ProcessingInfo
                    {
                        Amount = amount,
                        Reference = receipt,
                        Type = ProcessingType.PayDala,
                        BankName = source,
                        BankNetwork = net
                    };
                    using (IDbTransaction transaction = _clientRepository.BeginTransaction())
                    {
                        OnlinePayment onlinePayment = QueueOnlinePayment(contractForAmount, processingInfo);
                        if (onlinePayment == null)
                            throw new PawnshopApplicationException($"Ожидалось что {nameof(QueueOnlinePayment)} не вернет null");

                        if (!onlinePayment.Amount.HasValue)
                            throw new PawnshopApplicationException($"Ожидалось что {nameof(onlinePayment)}.{nameof(onlinePayment.Amount)} не будет null");

                        response.Authcode = onlinePayment.Id;
                        _eventLog.Log(EventCode.Prepayment, EventStatus.Success, EntityType.Contract, contractForAmount.Id, responseData: $@"Принята оплата по кредиту: {onlinePayment.Amount.Value} KZT от PayDala");
                        transaction.Commit();
                    }

                    break;

                #endregion

                #region Поиск платежа

                case "status":
                    if (receipt <= 0)
                    {
                        response.Code = 4;
                        response.Message = "Неверное значение номера платежа";
                        break;
                    }
                    else
                    {
                        try
                        {
                            OnlinePayment onlinePayment = _onlinePaymentRepository.GetByProcessing(receipt, ProcessingType.PayDala);
                            if (onlinePayment != null)
                            {
                                response.Code = 0;
                                response.Message = "Платеж был принят";
                                response.Date = onlinePayment.CreateDate;
                                response.Authcode = onlinePayment.Id;
                                break;
                            }
                            else
                            {
                                response.Code = 6;
                                response.Message = "Платеж не найден в системе";
                                break;
                            }
                        }
                        catch
                        {
                            response.Code = 13;
                            response.Message = "Ошибка при поиске платежа";
                            break;
                        }
                    }

                #endregion


                default:
                    response.Code = 1;
                    response.Message = "Неизвестный тип запроса";
                    break;
            }
            return response;
        }

        private string CreatePositionDescription(Contract contract)
        {
            string _answer = "-";
            if (contract.CollateralType == CollateralType.Car)
            {
                var position = contract.Positions.Count() > 0 ? contract.Positions.FirstOrDefault().Position : null;
                if (position != null)
                    _answer = ((Car)position).TransportNumber + " " + ((Car)position).Mark + " " + ((Car)position).Model;

            }
            return _answer;
        }

        [HttpPost("/api/queueonlinepayment")]
        [Authorize(Permissions.OnlinePayment)]
        [Event(EventCode.OnlinePaymentTry, EventMode = EventMode.All, EntityType = EntityType.OnlinePayment, IncludeFails = true)]
        public OnlinePayment QueueOnlinePayment([FromForm] Contract contract, [FromForm] ProcessingInfo processingInfo)
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

        // проверить время запроса
        // если время с 22,30 до 10,00 заполнить дату начислений
        private DateTime? CalcAccrualDate(DateTime date)
        {
            
            DateTime? accrualDate = null;
            TimeSpan dayTime;
            if (date == default)
                dayTime = DateTime.Now.TimeOfDay;
            else
                dayTime = date.TimeOfDay;
            
            if (dayTime >= Constants.STOP_ONLINE_PAYMENTS)
                accrualDate = date == default ? DateTime.Now.Date.AddDays(1) : date.Date.AddDays(1);
            else if (dayTime < Constants.START_ONLINE_PAYMENTS)
                accrualDate = date == default ? DateTime.Now.Date : date.Date;

            return accrualDate;
        }
    }
}
