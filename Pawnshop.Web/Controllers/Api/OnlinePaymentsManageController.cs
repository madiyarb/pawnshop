using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Options;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Files;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Engine.Audit;
using Pawnshop.Web.Engine.Export;
using Pawnshop.Web.Engine.MessageSenders;
using Pawnshop.Web.Engine.Security;
using Pawnshop.Services.Storage;
using Pawnshop.Web.Models.Processing;
using Pawnshop.Web.Models.List;
using Pawnshop.Data.Models.OnlinePayments.OnlinePaymentRevises;
using Pawnshop.Data.Models.OnlinePayments.OnlinePaymentRevises.OnlinePaymentRevisePows;
using OfficeOpenXml;
using Pawnshop.Data.Models.Insurances;
using System.Globalization;
using Pawnshop.Services.MessageSenders;
using ExcelDataReader;

namespace Pawnshop.Web.Controllers.Api
{
    public class OnlinePaymentsManageController : Controller
    {
        private readonly TokenProvider _tokenProvider;
        private readonly ISessionContext _sessionContext;
        private readonly ContractRepository _contractRepository;
        private readonly ContractActionRepository _actionRepository;
        private readonly OrganizationRepository _organizationRepository;
        private readonly GroupRepository _groupRepository;
        private readonly EventLog _eventLog;
        private readonly OnlinePaymentRepository _onlinePaymentRepository;
        private readonly CashOrderRepository _cashOrderRepository;
        private readonly CashOrderNumberCounterRepository _cashCounterRepository;
        private readonly ClientRepository _clientRepository;
        private readonly OnlinePaymentsManageExcelBuilder _excelBuilder;
        private readonly IStorage _storage;
        private readonly InnerNotificationRepository _innerNotificationRepository;
        private readonly EmailSender _emailSender;
        private readonly EnviromentAccessOptions _options;
        private readonly OnlinePaymentReviseRepository _onlinePaymentReviseRepository;
        private readonly CardTopUpTransactionRepository _cardTopUpTransactionRepository;

        public OnlinePaymentsManageController(TokenProvider tokenProvider, ISessionContext sessionContext, ContractRepository contractRepository,
            ContractActionRepository actionRepository, OrganizationRepository organizationRepository, GroupRepository groupRepository, EventLog eventLog,
            OnlinePaymentRepository onlinePaymentRepository, CashOrderRepository cashOrderRepository, CashOrderNumberCounterRepository cashCounterRepository,
            ClientRepository clientRepository, OnlinePaymentsManageExcelBuilder excelBuilder, IStorage storage, InnerNotificationRepository innerNotificationRepository,
            EmailSender emailSender, IOptions<EnviromentAccessOptions> options, OnlinePaymentReviseRepository onlinePaymentReviseRepository, CardTopUpTransactionRepository cardTopUpTransactionRepository)
        {
            _tokenProvider = tokenProvider;
            _sessionContext = sessionContext;
            _contractRepository = contractRepository;
            _actionRepository = actionRepository;
            _organizationRepository = organizationRepository;
            _groupRepository = groupRepository;
            _eventLog = eventLog;
            _onlinePaymentRepository = onlinePaymentRepository;
            _cashOrderRepository = cashOrderRepository;
            _cashCounterRepository = cashCounterRepository;
            _clientRepository = clientRepository;
            _excelBuilder = excelBuilder;
            _storage = storage;
            _innerNotificationRepository = innerNotificationRepository;
            _emailSender = emailSender;
            _options = options.Value;
            _onlinePaymentReviseRepository = onlinePaymentReviseRepository;
            _cardTopUpTransactionRepository = cardTopUpTransactionRepository;
        }

        [HttpPost]
        [Authorize(Permissions.OnlinePaymentsManage)]
        public ListModel<OnlinePaymentRevise> List([FromBody] ListQueryModel<OnlinePaymentListQueryModel> listQuery)
        {
            listQuery ??= new ListQueryModel<OnlinePaymentListQueryModel>();
            listQuery.Model ??= new OnlinePaymentListQueryModel();

            return new ListModel<OnlinePaymentRevise>
            {
                List = _onlinePaymentReviseRepository.List(listQuery, listQuery.Model),
                Count = _onlinePaymentReviseRepository.Count(listQuery, listQuery.Model)
            };
        }

        [HttpPost]
        [Authorize(Permissions.OnlinePaymentsManage)]
        public OnlinePaymentRevise Card([FromBody] int id)
        {
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            var account = _onlinePaymentReviseRepository.Get(id);
            if (account == null) throw new InvalidOperationException();

            return account;
        }

        [HttpPost]
        [Authorize(Permissions.OnlinePaymentsManage)]
        //[Event(EventCode.DictAccountSaved, EventMode = EventMode.Response)]
        public OnlinePaymentRevise Save([FromBody] OnlinePaymentRevise revise)
        {
            ModelState.Validate();

            if (revise.Id > 0)
            {
                _onlinePaymentReviseRepository.Update(revise);
            }
            else
            {
                _onlinePaymentReviseRepository.Insert(revise);
            }
            return revise;
        }

        [HttpPost]
        [Authorize(Permissions.OnlinePaymentsManage)]
        public async Task<IActionResult> Export([FromBody] OnlinePaymentRevise contracts)
        {
            using (var stream = _excelBuilder.Build(contracts))
            {
                var fileName = await _storage.Save(stream, ContainerName.Temp, "export.xlsx");
                string contentType;
                new FileExtensionContentTypeProvider().TryGetContentType(fileName, out contentType);

                var fileRow = new FileRow
                {
                    CreateDate = DateTime.Now,
                    ContentType = contentType ?? "application/octet-stream",
                    FileName = fileName,
                    FilePath = fileName
                };
                return Ok(fileRow);
            }
        }

        [HttpPost]
        public OnlinePaymentRevise UploadForKassa24(IFormCollection form)
        {
            OnlinePaymentRevise revise = new OnlinePaymentRevise();
            List<OnlinePaymentsManageModel> payments = new List<OnlinePaymentsManageModel>();
            ProcessingType processingType = ProcessingType.Kassa24;

            using (var transaction = _onlinePaymentReviseRepository.BeginTransaction())
            {
                CreatePaymentsFromFile(form, payments, processingType);

                CheckActionsByProcessingId(revise, payments, processingType);

                _onlinePaymentReviseRepository.Insert(revise);

                transaction.Commit();
            }

            return revise;
        }

        [HttpPost]
        public OnlinePaymentRevise UploadForRPS(IFormCollection form, [FromQuery(Name = "token")] string tokenString)
        {
            OnlinePaymentRevise revise = new OnlinePaymentRevise();
            List<OnlinePaymentsManageModel> payments = new List<OnlinePaymentsManageModel>();
            ProcessingType processingType = ProcessingType.Rps;

            using (var transaction = _onlinePaymentReviseRepository.BeginTransaction())
            {
                CreatePaymentsFromFile(form, payments, processingType);

                CheckActionsByProcessingId(revise, payments, processingType);

                _onlinePaymentReviseRepository.Insert(revise);

                transaction.Commit();
            }

            return revise;
        }

        [HttpPost]
        public OnlinePaymentRevise UploadForQiwi(IFormCollection form)
        {
            OnlinePaymentRevise revise = new OnlinePaymentRevise();
            List<OnlinePaymentsManageModel> payments = new List<OnlinePaymentsManageModel>();
            ProcessingType processingType = ProcessingType.Qiwi;

            using (var transaction = _onlinePaymentReviseRepository.BeginTransaction())
            {
                CreatePaymentsFromFile(form, payments, processingType);

                CheckActionsByProcessingId(revise, payments, processingType);

                _onlinePaymentReviseRepository.Insert(revise);

                transaction.Commit();
            }

            return revise;
        }

        [HttpPost]
        public OnlinePaymentRevise UploadForProcessing(IFormCollection form, [FromQuery(Name = "token")] string tokenString)
        {
            OnlinePaymentRevise revise = new OnlinePaymentRevise();
            List<OnlinePaymentsManageModel> payments = new List<OnlinePaymentsManageModel>();
            ProcessingType processingType = ProcessingType.Processing;
            using (var transaction = _contractRepository.BeginTransaction())
            {
                foreach (var file in form.Files)
                {
                    try
                    {
                        ExcelPackage.LicenseContext = LicenseContext.Commercial;
                        using (var package = new ExcelPackage(file.OpenReadStream()))
                        {
                            ExcelWorksheet report = package.Workbook.Worksheets[0];
                            int reportRowCount = report.Dimension.Rows;

                            for (int row = 10; row <= reportRowCount; row++)
                            {
                                OnlinePaymentsManageModel payment = new OnlinePaymentsManageModel();
                                var date = report.Cells[row, 2].Value?.ToString();
                                if (date == "")
                                    continue;

                                payment.Date = DateTime.Parse(date);
                                payment.Amount = Decimal.Round(decimal.Parse(report.Cells[row, 9].Value?.ToString()), 2);
                                payment.Receipt = Int64.Parse(report.Cells[row, 4].Value?.ToString());

                                var contractId = report.Cells[row, 12].Value?.ToString();
                                if (contractId == "")
                                {
                                    var contractNumber = report.Cells[row, 10].Value?.ToString();
                                    if (contractNumber == "no_AgrNum")
                                    {
                                        var cardTopUP = _cardTopUpTransactionRepository.GetByCustomerReference(payment.Receipt.ToString());
                                        payment.ContractId = cardTopUP.ContractId;
                                        payment.Receipt = cardTopUP.OrderId;
                                    }
                                    else
                                    {
                                        var contract = _contractRepository.GetNonCreditLineByNumberAsync(contractNumber).Result;
                                        if (contract == null)
                                            contract = _contractRepository.GetCreditLineByNumberAsync(contractNumber).Result;

                                        payment.ContractId = contract.Id;
                                    }
                                }
                                else
                                    payment.ContractId = Int32.Parse(report.Cells[row, 12].Value?.ToString());

                                payments.Add(payment);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw new PawnshopApplicationException($"Ошибка чтения файла, проверьте файл({e.Message})");
                    }
                }

                CheckActionsByProcessingId(revise, payments, processingType);

                _onlinePaymentReviseRepository.Insert(revise);

                transaction.Commit();
            }

            return revise;
        }

        [HttpPost]
        public OnlinePaymentRevise UploadForPayDala(IFormCollection form)
        {
            OnlinePaymentRevise revise = new OnlinePaymentRevise();
            List<OnlinePaymentsManageModel> payments = new List<OnlinePaymentsManageModel>();
            ProcessingType processingType = ProcessingType.PayDala;

            using (var transaction = _onlinePaymentReviseRepository.BeginTransaction())
            {
                CreatePaymentsFromFile(form, payments, processingType);

                CheckActionsByProcessingId(revise, payments, processingType);

                _onlinePaymentReviseRepository.Insert(revise);

                transaction.Commit();
            }

            return revise;
        }

        [HttpPost]
        public OnlinePaymentRevise UploadForJetPay(IFormCollection form)
        {
            OnlinePaymentRevise revise = new OnlinePaymentRevise();
            List<OnlinePaymentsManageModel> payments = new List<OnlinePaymentsManageModel>();
            ProcessingType processingType = ProcessingType.JetPay;

            using (var transaction = _onlinePaymentReviseRepository.BeginTransaction())
            {
                CreatePaymentsFromFile(form, payments, processingType);

                CheckActionsByProcessingId(revise, payments, processingType);

                _onlinePaymentReviseRepository.Insert(revise);

                transaction.Commit();
            }

            return revise;
        }

        [HttpPost]
        public OnlinePaymentRevise UploadForKaspi(IFormCollection form)
        {
            OnlinePaymentRevise revise = new OnlinePaymentRevise();
            List<OnlinePaymentsManageModel> payments = new List<OnlinePaymentsManageModel>();
            ProcessingType processingType = ProcessingType.Kaspi;

            using (var transaction = _onlinePaymentReviseRepository.BeginTransaction())
            {
                CreatePaymentsFromFile(form, payments, processingType);

                CheckActionsByProcessingId(revise, payments, processingType);

                _onlinePaymentReviseRepository.Insert(revise);

                transaction.Commit();
            }

            return revise;
        }

        private void CreatePaymentsFromFile(IFormCollection form, List<OnlinePaymentsManageModel> payments, ProcessingType processingType)
        {
            dynamic indexes = new ExpandoObject();

            switch (processingType)
            {
                case ProcessingType.Kassa24:
                    indexes.startLine = 0;
                    indexes.Date = 0;
                    indexes.Receipt = 1;
                    indexes.ContractId = 2;
                    indexes.Amount = 3;
                    indexes.IdentityNumber = -1;
                    indexes.Delimiter = ',';
                    break;
                case ProcessingType.Rps:
                    indexes.startLine = 0;
                    indexes.Date = 0;
                    indexes.Receipt = 1;
                    indexes.ContractId = 2;
                    indexes.Amount = 3;
                    indexes.IdentityNumber = -1;
                    indexes.Delimiter = ';';
                    break;
                case ProcessingType.Qiwi:
                    indexes.startLine = -1;
                    indexes.Date = 1;
                    indexes.Receipt = 0;
                    indexes.ContractId = 3;
                    indexes.Amount = 4;
                    indexes.IdentityNumber = -1;
                    indexes.Delimiter = ';';
                    break;
                case ProcessingType.PayDala:
                    indexes.startLine = 19;
                    indexes.Date = 1;
                    indexes.Receipt = 2;
                    indexes.ContractId = 5;
                    indexes.Amount = 8;
                    indexes.IdentityNumber = -1;
                    indexes.Delimiter = ';';
                    break;
                case ProcessingType.JetPay:
                    indexes.startLine = 0;
                    indexes.Date = 14;
                    indexes.Receipt = 2;
                    indexes.ContractId = -1;
                    indexes.Amount = 16;
                    indexes.IdentityNumber = 17;
                    indexes.Delimiter = ';';
                    break;
                case ProcessingType.Kaspi:
                    indexes.startLine = 1;
                    indexes.Date = 0;
                    indexes.Receipt = 1;
                    indexes.ContractId = 3;
                    indexes.Amount = 4;
                    indexes.IdentityNumber = 2;
                    indexes.Delimiter = ';';
                    break;
            }

            foreach (var file in form.Files)
            {
                using (var stream = file.OpenReadStream())
                    try
                    {
                        if (processingType == ProcessingType.Kaspi && (Path.GetExtension(file.FileName).Equals(".xls", StringComparison.OrdinalIgnoreCase) ||
                             Path.GetExtension(file.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase)))
                        {

                            ProcessExcel(stream, indexes, payments, processingType);
                        }
                        else
                        {
                            using (StreamReader sr = new StreamReader(stream))
                            {
                                string line;
                                int i = 0;

                                while ((line = sr.ReadLine()) != null)
                                {
                                    if (i > indexes.startLine)
                                    {
                                        if (line.Replace("\"", string.Empty).Length > 0)
                                        {
                                            OnlinePaymentsManageModel payment = new OnlinePaymentsManageModel();
                                            string[] variables = line.Replace("\"", string.Empty).Split(new char[] { indexes.Delimiter });
                                            if (variables == null)
                                            {
                                                throw new PawnshopApplicationException("В строке нету записей в файле");
                                            }

                                            if (variables.Length < 4)
                                            {
                                                throw new PawnshopApplicationException("Количество переменных в строке меньше чем должно быть в файле");
                                            }

                                            if (processingType == ProcessingType.Qiwi)
                                            {
                                                payment.Date = DateTime.ParseExact(variables[indexes.Date], "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                                            }
                                            else if (processingType == ProcessingType.Kaspi)
                                            {
                                                if (DateTime.TryParseExact(variables[indexes.Date], "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                                                    payment.Date = parsedDate;
                                                else
                                                    return;
                                            }
                                            else
                                            {
                                                payment.Date = DateTime.Parse(variables[indexes.Date]);
                                            }
                                            payment.Receipt = Int64.Parse(variables[indexes.Receipt]);
                                            payment.Amount = Convert.ToDecimal(variables[indexes.Amount].Replace(".", ","), new CultureInfo("fr-FR"));
                                            if (indexes.IdentityNumber == -1)
                                                payment.IdentityNumber = null;
                                            else
                                                payment.IdentityNumber = variables[indexes.IdentityNumber];

                                            if (indexes.ContractId == -1)
                                                payment.ContractId = null;
                                            else
                                                payment.ContractId = Int32.Parse(variables[indexes.ContractId]);

                                            payments.Add(payment);
                                        }
                                    }

                                    i++;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw new PawnshopApplicationException($"Ошибка чтения файла, проверьте файл({e.Message})");
                    }
            }

        }

        void ProcessExcel(Stream stream, dynamic indexes, List<OnlinePaymentsManageModel> payments, ProcessingType processingType)
        {
            var result = new List<string[]>();
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                while (reader.Read())
                {
                    var row = new string[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[i] = reader.GetValue(i)?.ToString() ?? string.Empty;
                    }
                    result.Add(row);
                }
            }

            for (int i = indexes.startLine + 1; i < result.Count; i++)
            {
                var variables = result[i];
                if (variables == null)
                {
                    throw new PawnshopApplicationException("В строке нету записей в файле");
                }

                if (variables.Length < 4)
                {
                    throw new PawnshopApplicationException("Количество переменных в строке меньше чем должно быть в файле");
                }

                OnlinePaymentsManageModel payment = new OnlinePaymentsManageModel();

                if (!DateTime.TryParse(variables[indexes.Date], out DateTime r))
                    break;
                payment.Date = DateTime.Parse(variables[indexes.Date]);

                payment.Receipt = Int64.Parse(variables[indexes.Receipt]);
                payment.Amount = Convert.ToDecimal(variables[indexes.Amount].Replace(".", ","), new CultureInfo("fr-FR"));
                if (indexes.IdentityNumber == -1)
                    payment.IdentityNumber = null;
                else
                    payment.IdentityNumber = variables[indexes.IdentityNumber];

                if (indexes.ContractId == -1)
                    payment.ContractId = null;
                else
                    payment.ContractId = Int32.Parse(variables[indexes.ContractId]);

                payments.Add(payment);
            }
        }


        /// <summary>
        /// Проверка платежей 
        /// </summary>
        private void CheckActionsByProcessingId(OnlinePaymentRevise revise, List<OnlinePaymentsManageModel> payments, ProcessingType processingType)
        {
            try
            {
                List<OnlinePaymentReviseRow> rows = new List<OnlinePaymentReviseRow>();
                List<ContractAction> actions = new List<ContractAction>();
                foreach (var date in payments.Select(x => x.Date.Date).Distinct())
                {
                    ListQuery listQuery = new ListQuery();
                    ObjectDateForProcessing forQuery = new ObjectDateForProcessing() { Date = date, ProcessingType = processingType };

                    object query = forQuery;
                    List<ContractAction> item = _actionRepository.ListForProcessing(listQuery, query);
                    actions.AddRange(item);
                }

                foreach (var action in actions)
                {
                    if (action == null) throw new ArgumentNullException(nameof(action));
                    if (action.ContractId <= 0) throw new ArgumentException();

                    var contract = _contractRepository.GetOnlyContract(action.ContractId);
                    contract.Client = _clientRepository.GetOnlyClient(contract.ClientId);
                    var branchInfo = _groupRepository.Get(contract.BranchId);
                    var organizationInfo = _organizationRepository.Get(branchInfo.OrganizationId);
                    var onlinePaymentInfo = _onlinePaymentRepository.GetByProcessing((Int64)action.ProcessingId, processingType);

                    OnlinePaymentReviseRow row = new OnlinePaymentReviseRow();
                    row.ReviseId = revise.Id;
                    row.ProcessingId = (Int64)action.ProcessingId;
                    row.Amount = action.TotalCost;
                    row.CompanyBin = organizationInfo.Configuration.LegalSettings.BIN;
                    row.OrganizationId = organizationInfo.Id;
                    row.ContractId = contract.Id;
                    row.Contract = contract;
                    row.ActionId = action.Id;
                    row.Action = action;

                    var payment = payments.Where(x => x.Receipt == (Int64)action.ProcessingId).FirstOrDefault();
                    if (payment == null)
                    {
                        var emailNotifications = EmailProcessingNotifications(EmailProcessingNotificationType.NotFoundInProcessing, action, payment);

                        row.Status = OnlinePaymentReviseRowStatus.Error;
                        row.Message = emailNotifications;

                        revise.Status = OnlinePaymentReviseStatus.Fail;
                    }
                    else
                    {
                        if (action.DeleteDate != null)
                        {
                            row.Status = OnlinePaymentReviseRowStatus.Fail;
                            row.Message = "Платеж был отменён. Необходимо уточнить у филиала";

                            revise.Status = OnlinePaymentReviseStatus.Fail;
                        }
                        else
                        {
                            if ((action.ContractId == payment.ContractId || contract.Client.IdentityNumber == payment.IdentityNumber) || action.Date.Date == payment.Date.Date || action.TotalCost == payment.Amount)
                            {
                                row.Status = OnlinePaymentReviseRowStatus.Success;
                                if (processingType == ProcessingType.Kaspi)
                                    row.Date = onlinePaymentInfo.CreateDate;
                                else
                                    row.Date = payment.Date;
                            }
                            else
                            {
                                var emailNotifications = EmailProcessingNotifications(EmailProcessingNotificationType.Different, action, payment);
                                row.Status = OnlinePaymentReviseRowStatus.Error;
                                row.Message = emailNotifications;
                                revise.Status = OnlinePaymentReviseStatus.Fail;
                            }
                        }
                    }
                    rows.Add(row);
                }

                foreach (var payment in payments)
                {
                    var onlinePaymentForCreatePrepayment = rows.Where(x => x.ProcessingId == payment.Receipt).FirstOrDefault();
                    if (onlinePaymentForCreatePrepayment == null)
                    {
                        Contract contract = null;
                        if (payment.ContractId != null)
                        {
                            contract = _contractRepository.GetOnlyContract((int)payment.ContractId);
                        }
                        else
                        {
                            contract = _contractRepository.GetContractsByIdentityNumber(payment.IdentityNumber).FirstOrDefault();
                        }

                        contract.Client = _clientRepository.GetOnlyClient(contract.ClientId);
                        var branchInfo = _groupRepository.Get(contract.BranchId);
                        var organizationInfo = _organizationRepository.Get(branchInfo.OrganizationId);

                        OnlinePaymentReviseRow row = new OnlinePaymentReviseRow();
                        row.ReviseId = revise.Id;
                        row.ProcessingId = payment.Receipt;
                        row.Amount = payment.Amount;
                        row.CompanyBin = organizationInfo.Configuration.LegalSettings.BIN;
                        row.OrganizationId = organizationInfo.Id;
                        row.ContractId = contract.Id;
                        row.Contract = contract;

                        var emailNotifications = EmailProcessingNotifications(EmailProcessingNotificationType.AbsentInTas, null, payment);
                        row.Status = OnlinePaymentReviseRowStatus.Error;
                        row.Message = emailNotifications;

                        revise.Status = OnlinePaymentReviseStatus.Fail;
                        rows.Add(row);
                    }
                }

                revise.Status = OnlinePaymentReviseStatus.Success;
                revise.ProcessingType = processingType;
                revise.AuthorId = _sessionContext.UserId;
                revise.Rows = rows;
                revise.TransactionDate += string.Join(",", payments.Select(x => x.Date.ToString("dd.MM.yyyy")).Distinct());
            }
            catch (Exception e)
            {
                throw new PawnshopApplicationException($"Ошибка в обработке платежей ({e.Message})");
            }
        }

        /// <summary>
        /// Email уведомление об ошибке
        /// </summary>
        private string EmailProcessingNotifications(EmailProcessingNotificationType emailType, ContractAction action,
            OnlinePaymentsManageModel payment)
        {
            var message = String.Empty;
            var messageForFront = String.Empty;
            switch (emailType)
            {
                case EmailProcessingNotificationType.NotFoundInProcessing:
                    messageForFront =
                        $@"ОШИБКА! Платеж не найден в файле сверки (у нас в базе № транзакция - {action.ProcessingId})";
                    message =
                        $@"<p style=""text-align: center;""><strong>ОШИБКА! Платеж не найден в файле сверки</strong></p>
                            <p><strong>У нас в базе:</strong></p>
                                <p>actionId = {action.Id}</p>
                                <p>Date = {action.Date}</p>
                                <p>Receipt(ProcessingId) = {action.ProcessingId}</p>
                                <p>ContractId = {action.ContractId}</p>
                                <p>Amount(TotalCost) = {action.TotalCost}</p>";
                    break;
                case EmailProcessingNotificationType.Different:
                    messageForFront =
                        $@"ОШИБКА! Платеж отличается в файле сверки (У нас в базе № транзакция - {action.ProcessingId})";
                    message =
                        $@"<p style=""text-align: center;""><strong>ОШИБКА! Платеж отличается в файле сверки</strong></p>
                            <p><strong>У нас в базе:</strong></p>
                                <p>actionId = {action.Id}</p>
                                <p>Date = {action.Date}</p>
                                <p>Receipt(ProcessingId) = {action.ProcessingId}</p>
                                <p>ContractId = {action.ContractId}</p>
                                <p>Amount(TotalCost) = {action.TotalCost}</p>
                            <p><strong>Входные данные от онлайн провайдера:</strong></p>
                                <p>Date = {payment.Date}</p>
                                <p>Receipt = {payment.Receipt}</p>
                                <p>ContractId = {payment.ContractId}</p>
                                <p>Amount = {payment.Amount}</p>";
                    break;
                case EmailProcessingNotificationType.AbsentInTas:
                    messageForFront =
                        $@"ОШИБКА! Платеж не найден в базе TasCredit (в файле сверки № транзакция - {payment.Receipt})";
                    message =
                        $@"<p style=""text-align: center;""><strong>ОШИБКА! Платеж не найден в базе TasCredit</strong></p>
                            <p><strong>Входные данные от онлайн провайдера:</strong></p>
                                <p>Date = {payment.Date}</p>
                                <p>Receipt(ProcessingId) = {payment.Receipt}</p>
                                <p>ContractId = {payment.ContractId}</p>
                                <p>Amount(TotalCost) = {payment.Amount}</p>";
                    break;
                default:
                    messageForFront = $@"Не известный тип ошибки ({emailType})";
                    message =
                        $@"<p style=""text-align: center;""><strong>Не известный тип ошибки ({emailType})</strong></p>";
                    break;
                    ;
            }

            var messageReceiver = new MessageReceiver
            {
                ReceiverAddress = _options.ErrorNotifierAddress,
                ReceiverName = _options.ErrorNotifierName
            };

            try
            {
                _emailSender.SendEmail("Ошибка сверки с онлайн провайдером", message, messageReceiver);
            }
            catch { }

            return messageForFront;
        }
    }
}
