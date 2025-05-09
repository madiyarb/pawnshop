using Microsoft.Extensions.Options;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Options;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ApplicationOnlineFcbKdnPayment;
using Pawnshop.Data.Models.ApplicationsOnline.Kdn;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.Clients;
using Pawnshop.Services.Gamblers;
using Pawnshop.Services.Integrations.Fcb;
using Pawnshop.Services.Models.Contracts.Kdn;
using Pawnshop.Services.Storage;
using Serilog;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System;
using Pawnshop.Services.Domains;
using System.Diagnostics.Contracts;

namespace Pawnshop.Services.ApplicationsOnline
{
    public class ApplicationOnlineKdnService : IApplicationOnlineKdnService
    {
        private readonly ApplicationOnlineCarRepository _applicationOnlineCarRepository;
        private readonly ApplicationOnlinePositionRepository _applicationOnlinePositionRepository;
        private readonly ApplicationOnlineApprovedOtherPaymentRepository _approvedOtherPaymentRepository;
        private readonly ClientExpenseRepository _clientExpenseRepository;
        private readonly NotionalRateRepository _notionalRateRepository;
        private readonly IClientIncomeService _clientIncomeService;
        private readonly IClientService _clientService;
        private readonly IFCBChecksService _gamblerService;
        private readonly ContractRepository _contractRepository;
        private readonly IFcb4Kdn _fcb4Kdn;
        private readonly ApplicationOnlineFcbKdnPaymentRepository _fcbKdnPaymentRepository;
        private readonly ApplicationOnlineKdnLogRepository _kdnLogRepository;
        private readonly ApplicationOnlineKdnPositionRepository _kdnPositionRepository;
        private readonly IManualCalculationClientExpenseService _manualCalculationClientExpenseService;
        private readonly IDomainService _domainService;
        private readonly ILogger _logger;
        private readonly EnviromentAccessOptions _options;
        private readonly IStorage _storage;
        private readonly IList<string> _appOnlineFirstStageStatuses;

        public ApplicationOnlineKdnService(
            ApplicationOnlineCarRepository applicationOnlineCarRepository,
            ApplicationOnlinePositionRepository applicationOnlinePositionRepository,
            ApplicationOnlineApprovedOtherPaymentRepository approvedOtherPaymentRepository,
            ClientExpenseRepository _clientExpenseRepository,
            NotionalRateRepository notionalRateRepository,
            IClientIncomeService clientIncomeService,
            IClientService clientService,
            IFCBChecksService gamblerService,
            ContractRepository contractRepository,
            IFcb4Kdn fcb4Kdn,
            ApplicationOnlineFcbKdnPaymentRepository fcbKdnPaymentRepository,
            ApplicationOnlineKdnLogRepository kdnLogRepository,
            ApplicationOnlineKdnPositionRepository kdnPositionRepository,
            IManualCalculationClientExpenseService manualCalculationClientExpenseService,
            IDomainService domainService,
            ILogger logger,
            IOptions<EnviromentAccessOptions> options,
            IStorage storage)
        {
            _applicationOnlineCarRepository = applicationOnlineCarRepository;
            _applicationOnlinePositionRepository = applicationOnlinePositionRepository;
            _approvedOtherPaymentRepository = approvedOtherPaymentRepository;
            this._clientExpenseRepository = _clientExpenseRepository;
            _notionalRateRepository = notionalRateRepository;
            _clientIncomeService = clientIncomeService;
            _clientService = clientService;
            _gamblerService = gamblerService;
            _contractRepository = contractRepository;
            _fcb4Kdn = fcb4Kdn;
            _fcbKdnPaymentRepository = fcbKdnPaymentRepository;
            _kdnLogRepository = kdnLogRepository;
            _kdnPositionRepository = kdnPositionRepository;
            _manualCalculationClientExpenseService = manualCalculationClientExpenseService;
            _domainService = domainService;
            _logger = logger;
            _options = options.Value;
            _storage = storage;

            _appOnlineFirstStageStatuses = new List<string> {
                nameof(ApplicationOnlineStatus.Consideration),
                nameof(ApplicationOnlineStatus.ModificationFromVerification)
            };
        }

        public ApplicationOnlineKdnLog CalculateKdn(ApplicationOnline applicationOnline, List<ContractPaymentSchedule> virtualPaymentSchedule, User author)
        {
            var kdnLog = new ApplicationOnlineKdnLog
            {
                ApplicationAmount = applicationOnline.ApplicationAmount,
                ApplicationOnlineId = applicationOnline.Id,
                ApplicationOnlineStatus = applicationOnline.Status,
                ApplicationSettingId = applicationOnline.ProductId,
                ApplicationTerm = applicationOnline.LoanTerm,
                Author = author,
                AuthorId = author.Id,
                ClientId = applicationOnline.ClientId,
                CreateDate = DateTime.Now
            };

            try
            {
                var client = _clientService.SetASPStatus(kdnLog.ClientId);
                kdnLog.Client = client;

                var calcModel = new ApplicationOnlineKdnCalculateModel();

                FillClientDetails(applicationOnline, kdnLog, calcModel, virtualPaymentSchedule);
                calcModel.IsSusn = client.ReceivesASP;

                if (calcModel.Message.Any())
                {
                    kdnLog.ResultText = string.Join("\r\n", calcModel.Message.ToArray());
                    _kdnLogRepository.Insert(kdnLog);
                    return kdnLog;
                }

                var k1 = CheckK1(applicationOnline.Status, calcModel);
                var k2 = CheckK2(calcModel);
                var k3 = CheckK3(calcModel);
                var k4 = CheckK4(calcModel);

                kdnLog.TotalIncome = calcModel.TotalIncome;
                kdnLog.AverageMonthlyPayment = calcModel.AverageMonthlyPayment;
                kdnLog.Kdn = calcModel.Kdn;
                kdnLog.ResultText = string.Join("\r\n", calcModel.Message.ToArray());
                kdnLog.AvgPaymentToday = calcModel.AvgPaymentToday;
                kdnLog.TotalIncomeK4 = calcModel.TotalIncomeK4;
                kdnLog.KdnK4 = calcModel.KdnK4;
                kdnLog.AllLoan = calcModel.AllLoan;

                if (!_appOnlineFirstStageStatuses.Contains(applicationOnline.Status))
                {
                    kdnLog.CompareIncomeAmount = calcModel.TotalFormalIncome + calcModel.TotalInformalApprovedIncome + calcModel.TotalInformalUnapprovedIncome;
                    kdnLog.CompareExpensesAmount = calcModel.FamilyDebt + calcModel.ClientExpenses - calcModel.ApprovedOtherPaymentsAmount;
                    kdnLog.OtherPaymentsAmount = calcModel.TotalFcbDebt - calcModel.ApprovedOtherPaymentsAmount < 0 ? 0 : calcModel.TotalFcbDebt - calcModel.ApprovedOtherPaymentsAmount;
                    kdnLog.IncomeConfirmed = calcModel.TotalInformalApprovedIncome != 0;
                }
                else
                {
                    kdnLog.CompareIncomeAmount = calcModel.TotalAccordingClientIncome;
                    kdnLog.CompareExpensesAmount = calcModel.FamilyDebt + calcModel.ClientExpenses + calcModel.TotalAccordingClientPaymentsExpenses;
                    kdnLog.OtherPaymentsAmount = calcModel.TotalAccordingClientPaymentsExpenses;
                    kdnLog.IncomeConfirmed = false;
                }

                if (k1 && k2 && k3 && k4 && !kdnLog.IsStopCredit)
                    kdnLog.Success = true;

                _kdnLogRepository.Insert(kdnLog);
                return kdnLog;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                kdnLog.ResultText = $"Ошибка расчета КДН.\r\n{exception.Message}";
                return kdnLog;
            }
        }

        public List<string> CheckCallCalcKdnToApprove(ApplicationOnline applicationOnline)
        {
            try
            {
                var resultList = new List<string>();

                var kdnLogs = _kdnLogRepository.List(null, new { ApplicationOnlineId = applicationOnline.Id });

                var lastKdnLogOnConsideration = kdnLogs?.OrderByDescending(x => x.CreateDate)?
                    .FirstOrDefault(x =>
                        x.Success && x.ApplicationOnlineStatus == nameof(ApplicationOnlineStatus.EstimationCompleted));

                if (!kdnLogs.Any() || lastKdnLogOnConsideration == null)
                {
                    resultList.Add("Требуется успешный расчет КДН!");
                    return resultList;
                }

                var clientExpenses = _clientIncomeService.GetClientExpenses(applicationOnline.ClientId);
                var totalFormalIncome = _clientIncomeService.GetTotalFormalIncome(applicationOnline.ClientId);
                var totalInformalApprovedIncome =
                    _clientIncomeService.GetTotalInformalApprovedIncome(applicationOnline.ClientId);
                var totalInformalUnapprovedIncome =
                    _clientIncomeService.GetTotalInformalUnapprovedIncome(applicationOnline.ClientId);
                var approvedOtherPaymentsAmount = _approvedOtherPaymentRepository
                    .List(null, new { ApplicationOnlineId = applicationOnline.Id })?.Sum(x => x.Amount) ?? 0;
                var familyDebt = _clientIncomeService.GetFamilyDebt(applicationOnline.ClientId);

                var compareIncomeAmount = totalFormalIncome + totalInformalApprovedIncome +
                                          totalInformalUnapprovedIncome;
                var compareExpensesAmount = familyDebt + clientExpenses - approvedOtherPaymentsAmount;

                if (applicationOnline.ProductId != lastKdnLogOnConsideration.ApplicationSettingId)
                    resultList.Add("Изменен способ погашения или страховая премия!");

                if (applicationOnline.ApplicationAmount != lastKdnLogOnConsideration.ApplicationAmount)
                    resultList.Add("Изменена сумма заявки!");

                if (applicationOnline.LoanTerm != lastKdnLogOnConsideration.ApplicationTerm)
                    resultList.Add("Изменен срок заявки!");

                if (compareIncomeAmount != lastKdnLogOnConsideration.CompareIncomeAmount)
                    resultList.Add("Сумма доходов изменена!");

                if (compareExpensesAmount != lastKdnLogOnConsideration.CompareExpensesAmount)
                    resultList.Add("Сумма расходов изменена!");

                //if (DateTime.Today != lastKdnLogOnConsideration.CreateDate.Date)
                //    resultList.Add("Расчет КДН не актуален!");

                return resultList;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }
        }

        public List<string> CheckCallCalcKdnToVerification(ApplicationOnline applicationOnline)
        {
            try
            {
                var resultList = new List<string>();

                var kdnLogs = _kdnLogRepository.List(null, new { ApplicationOnlineId = applicationOnline.Id });

                var lastKdnLogOnConsideration = kdnLogs?.OrderByDescending(x => x.CreateDate)?
                    .FirstOrDefault(x =>
                        x.Success && _appOnlineFirstStageStatuses.Contains(x.ApplicationOnlineStatus));

                if (!kdnLogs.Any() || lastKdnLogOnConsideration == null)
                {
                    resultList.Add("Требуется успешный расчет КДН!");
                    return resultList;
                }

                var totalPositionsIncome =
                    GetTotalPositionsIncome(applicationOnline.ClientId, applicationOnline.Id, false);
                var familyDebt = _clientIncomeService.GetFamilyDebt(applicationOnline.ClientId);
                var clientExpenses = _clientIncomeService.GetClientExpenses(applicationOnline.ClientId);

                var compareIncomeAmount = (applicationOnline.CorrectedIncomeAmount ?? 0);
                var compareExpensesAmount =
                    familyDebt + clientExpenses + (applicationOnline.CorrectedExpenseAmount ?? 0);

                if (applicationOnline.ProductId != lastKdnLogOnConsideration.ApplicationSettingId)
                    resultList.Add("Изменен способ погашения или страховая премия!");

                if (applicationOnline.ApplicationAmount != lastKdnLogOnConsideration.ApplicationAmount)
                    resultList.Add("Изменена сумма заявки!");

                if (applicationOnline.LoanTerm != lastKdnLogOnConsideration.ApplicationTerm)
                    resultList.Add("Изменен срок заявки!");

                if (compareIncomeAmount != lastKdnLogOnConsideration.CompareIncomeAmount)
                    resultList.Add("Сумма доходов изменена!");

                if (compareExpensesAmount != lastKdnLogOnConsideration.CompareExpensesAmount)
                    resultList.Add("Сумма расходов изменена!");

                //if (DateTime.Today != lastKdnLogOnConsideration.CreateDate.Date)
                //    resultList.Add("Расчет КДН не актуален!");

                return resultList;
            }
            catch (Exception exception)
            {
                _logger.Error(exception, exception.Message);
                throw;
            }
        }


        private bool CheckK1(string applicationOnlineStatus, ApplicationOnlineKdnCalculateModel calcModel)
        {
            if (!_appOnlineFirstStageStatuses.Contains(applicationOnlineStatus))
                calcModel.TotalAspIncome = (calcModel.IsSusn || calcModel.IsGambler) ? calcModel.TotalFormalIncome : calcModel.TotalFormalIncome + calcModel.TotalInformalApprovedIncome;
            else
                calcModel.TotalAspIncome = calcModel.TotalAccordingClientIncome;

            bool conditionIsKdnInvalid = true;
            if (calcModel.TotalAspIncome <= 0)
                conditionIsKdnInvalid = calcModel.FamilyDebt > 1;
            else
                conditionIsKdnInvalid = calcModel.FamilyDebt / calcModel.TotalAspIncome > 1;

            if (conditionIsKdnInvalid)
            {
                calcModel.Message.Add($"Первый шаг расчета коэффициента КДН не пройден: доход {calcModel.TotalAspIncome.ToString("N", CultureInfo.CreateSpecificCulture("ru-RU"))} меньше чем расход {calcModel.FamilyDebt.ToString("N", CultureInfo.CreateSpecificCulture("ru-RU"))} на содержание семьи. ");
                return false;
            }

            calcModel.Message.Add($"Первый шаг расчета коэффициента КДН пройден: доход {calcModel.TotalAspIncome.ToString("N", CultureInfo.CreateSpecificCulture("ru-RU"))} больше чем расход {calcModel.FamilyDebt.ToString("N", CultureInfo.CreateSpecificCulture("ru-RU"))} на содержание семьи. ");
            return true;
        }

        private bool CheckK2(ApplicationOnlineKdnCalculateModel calcModel)
        {
            if (calcModel.TotalAspIncome <= 0)
            {
                calcModel.Message.Add($"Второй шаг расчета коэффициента КДН не пройден: Общий доход {calcModel.TotalAspIncome.ToString("N", CultureInfo.CreateSpecificCulture("ru-RU"))} меньше или равно нулю. ");
                return false;
            }

            //"Изменено согласно заявки в Битриск от Коммерческого директора Анарбекова Г. Ссылка на заявку: https://bitrix.tascredit.kz/page/zapros_na_razrabotku/_zaprosy_na_razrabotku/type/134/details/537/"
            //if (calcModel.AverageMonthlyPaymentDelta < 0)
            //    calcModel.AverageMonthlyPaymentDelta = 0;

            var currentDebt = calcModel.AvgPaymentToday + calcModel.TotalFcbDebt + calcModel.AverageMonthlyPaymentDelta - calcModel.ApprovedOtherPaymentsAmount;

            if (currentDebt < 0)
                currentDebt = 0;

            if (calcModel.TotalAspIncome > 0)
                calcModel.Kdn = Math.Round(currentDebt / calcModel.TotalAspIncome, 2);
            else
                calcModel.Kdn = Math.Round(currentDebt, 2);

            decimal kdn = _options.KDN;
            var domainValue = _domainService.GetDomainValue(Constants.LOAN_PURPOSE_DOMAIN_VALUE, Constants.BUSINESS_LOAN_PURPOSE);
            var kdnBusinessMessageMinMax = "";
            if (calcModel.IsGambler)
            {
                kdn = _options.KDNLowPriority;
            }
            else if (domainValue != null && calcModel.LoanPurposeId.HasValue && calcModel.LoanPurposeId.Value == domainValue.Id)
            {
                kdn = _options.KDNBusiness;
                if (calcModel.ApplicationAmount < Constants.BUSINESS_LOAN_PURPOSE_MINIMAL_SUM)
                {
                    kdnBusinessMessageMinMax = "Минимальная сумма займа должна быть - 1 000 000 тенге";
                    kdn = _options.KDN;
                }
                var mrpRateType = _domainService.GetDomainValue(Constants.NOTIONAL_RATE_TYPES, Constants.NOTIONAL_RATE_TYPES_MRP);
                if (mrpRateType != null)
                {
                    var mrpValue = _notionalRateRepository.GetByTypeOfLastYear(mrpRateType.Id);
                    if (mrpValue != null && calcModel.ApplicationAmount > Constants.BUSINESS_LOAN_PURPOSE_MAXIMAL_MRP_MULTIPLIER * mrpValue.RateValue)
                    {
                        kdnBusinessMessageMinMax = $"Максимальная сумма займа на бизнес цели должна быть не более {Constants.BUSINESS_LOAN_PURPOSE_MAXIMAL_MRP_MULTIPLIER * mrpValue.RateValue} тенге";
                        kdn = _options.KDN;
                    }
                }
            }

            if(kdnBusinessMessageMinMax != "")
                calcModel.Message.Add(kdnBusinessMessageMinMax);

            if (calcModel.Kdn > kdn)
                calcModel.Message.Add($"Второй шаг расчета коэффициента КДН не пройден: текущее значение КДН = {calcModel.Kdn} > {kdn} . ");
            else
                calcModel.Message.Add($"Второй шаг расчета коэффициента КДН пройден: текущее значение КДН = {calcModel.Kdn} <= {kdn} . ");

            return calcModel.Kdn <= kdn;
        }

        private bool CheckK3(ApplicationOnlineKdnCalculateModel calcModel)
        {
            decimal result = 0;

            var totalDebt = calcModel.TotalDebt - calcModel.ApprovedOtherPaymentsAmount;

            if (calcModel.TotalAspIncome + calcModel.TotalInformalUnapprovedIncome <= 0)
                result = totalDebt;
            else
                result = totalDebt / (calcModel.TotalAspIncome + calcModel.TotalInformalUnapprovedIncome);

            if (result >= 1)
            {
                calcModel.Message.Add($"Третий шаг расчета коэффициента КДН не пройден: все доходы {calcModel.TotalAspIncome.ToString("N", CultureInfo.CreateSpecificCulture("ru-RU"))} < чем все расходы = {totalDebt.ToString("N", CultureInfo.CreateSpecificCulture("ru-RU"))} ");
                return false;
            }

            calcModel.Message.Add($"Третий шаг расчета коэффициента КДН пройден: все доходы {calcModel.TotalAspIncome.ToString("N", CultureInfo.CreateSpecificCulture("ru-RU"))} >= чем все расходы = {totalDebt.ToString("N", CultureInfo.CreateSpecificCulture("ru-RU"))} ");
            return true;
        }

        private bool CheckK4(ApplicationOnlineKdnCalculateModel calcModel)
        {
            if(!calcModel.AllLoan.HasValue)
            {
                calcModel.Message.Add($"Для расчета коэфицента КДД требуется заполнить в расходах Сумма задолжности по всем непогашенным кредитам у клиента с ИИН {calcModel.IIN}");
                return false;
            }

            calcModel.TotalIncomeK4 = Math.Round((calcModel.TotalFormalIncome + calcModel.TotalInformalApprovedIncome) * 12, 2);
            if (calcModel.TotalIncomeK4 <= 0)
                calcModel.KdnK4 = 10000;
            else
                calcModel.KdnK4 = Math.Round(calcModel.AllLoan.Value / calcModel.TotalIncomeK4, 2);

            if (calcModel.KdnK4 <= _options.KDNK4)
                calcModel.Message.Add($"Четвертый шаг расчета пройден: текущее значение КДД = {calcModel.KdnK4} <= {_options.KDNK4}.");
            else
                calcModel.Message.Add($"Четвертый шаг расчета не пройден: текущее значение КДД = {calcModel.KdnK4} > {_options.KDNK4}.");

            return true;
        }

        private void ConvertCarPositionToKdnPosition(List<ApplicationOnlineKdnPosition> kdnPositions, ContractPosition position, int clientId, Guid appOnlineId, int? filtredCarId)
        {
            if (!(position.Position is Car car))
                return;

            if (filtredCarId.HasValue)
            {
                if (filtredCarId == position.Position.Id)
                    return;
            }
            kdnPositions.Add(new ApplicationOnlineKdnPosition
            {
                ApplicationOnlineId = appOnlineId,
                ClientId = clientId,
                CollateralType = CollateralType.Car,
                CreateDate = DateTime.Now,
                EstimatedCost = position.EstimatedCost,
                Name = $"{car.TransportNumber} {car.Mark} {car.Model} {car.ReleaseYear}"
            });
        }

        private void FillClientDetails(
            ApplicationOnline applicationOnline,
            ApplicationOnlineKdnLog kdnLog,
            ApplicationOnlineKdnCalculateModel calcModel,
            List<ContractPaymentSchedule> virtualPaymentSchedule)
        {
            calcModel.AvgPaymentToday = _contractRepository.GetContractsByClientId(kdnLog.ClientId, new List<ContractStatus>() { ContractStatus.Signed }).Where(x => x.ContractDate == DateTime.Now.Date && x.ContractClass != ContractClass.CreditLine).Select(x => (x.LoanCost + x.LoanPercentCost) / x.LoanPeriod).Sum();
            calcModel.TotalPositionsIncome = GetTotalPositionsIncome(kdnLog.ClientId, kdnLog.ApplicationOnlineId);
            calcModel.TotalFormalIncome = _clientIncomeService.GetTotalFormalIncome(kdnLog.ClientId);
            calcModel.TotalInformalApprovedIncome = _clientIncomeService.GetTotalInformalApprovedIncome(kdnLog.ClientId);
            calcModel.TotalInformalUnapprovedIncome = _clientIncomeService.GetTotalInformalUnapprovedIncome(kdnLog.ClientId);
            calcModel.FamilyDebt = _clientIncomeService.GetFamilyDebt(kdnLog.ClientId);
            calcModel.TotalFamilyDebt = _clientIncomeService.GetTotalFamilyDebt(kdnLog.ClientId);
            calcModel.ClientExpenses = _clientIncomeService.GetClientExpenses(kdnLog.ClientId);
            calcModel.AllLoan = _clientIncomeService.GetClientFullExpenses(kdnLog.ClientId).AllLoan;
            calcModel.IIN = kdnLog.Client.IdentityNumber;
            calcModel.LoanPurposeId = applicationOnline.LoanPurposeId;
            calcModel.ApplicationAmount = applicationOnline.ApplicationAmount;

            calcModel.TotalAccordingClientIncome = applicationOnline.CorrectedIncomeAmount ?? 0;
            calcModel.TotalAccordingClientPaymentsExpenses = applicationOnline.CorrectedExpenseAmount ?? 0;

            calcModel.ApprovedOtherPaymentsAmount = _approvedOtherPaymentRepository.List(null, new { ApplicationOnlineId = applicationOnline.Id })?.Sum(x => x.Amount) ?? 0;
            calcModel.AverageMonthlyPayment = Math.Round(virtualPaymentSchedule.Select(x => x.DebtCost + x.PercentCost).Sum() / virtualPaymentSchedule.Count, 2);
            var position = _applicationOnlinePositionRepository.GetByApplicationOnlineId(applicationOnline.Id);

            // https://bitrix.tascredit.kz/crm/type/134/details/669/
            // закон принят в силу
            calcModel.AverageMonthlyPaymentDelta = ((virtualPaymentSchedule.Select(x => x.DebtCost + x.PercentCost).Sum() - position.EstimatedCost * 0.7m) / applicationOnline.LoanTerm).Value;//check is null can receive
            if (!_appOnlineFirstStageStatuses.Contains(applicationOnline.Status))
            {
                var income = calcModel.TotalFormalIncome + calcModel.TotalInformalApprovedIncome;
                var fcbDebt = GetFcbKdnResult(applicationOnline.Id, kdnLog.Client.Id, kdnLog.Author.Id, kdnLog.Author.Fullname, kdnLog.Client.IdentityNumber, income, out string errorMsg);
                var fcbChecksResult = _gamblerService.GamblerFeatureCheck(kdnLog.Client.Id, kdnLog.Author).Result;
                calcModel.IsGambler = fcbChecksResult.IsGumbler;
                kdnLog.IsGambler = fcbChecksResult.IsGumbler;
                kdnLog.IsStopCredit = calcModel.IsStopCredit;

                if (!string.IsNullOrEmpty(errorMsg))
                    calcModel.Message.AddRange(new List<string> { "Не удалось получить данные из ПКБ.", errorMsg });
                else
                    calcModel.TotalFcbDebt = fcbDebt;

                calcModel.TotalIncome = calcModel.TotalFormalIncome + calcModel.TotalInformalApprovedIncome + calcModel.TotalInformalUnapprovedIncome;
                calcModel.TotalDebt = calcModel.ClientExpenses + calcModel.TotalFamilyDebt + calcModel.AverageMonthlyPayment + calcModel.TotalFcbDebt - calcModel.ApprovedOtherPaymentsAmount;
            }
            else
            {
                calcModel.TotalIncome = calcModel.TotalAccordingClientIncome;
                calcModel.TotalDebt = calcModel.ClientExpenses + calcModel.TotalFamilyDebt + calcModel.AverageMonthlyPayment + calcModel.TotalAccordingClientPaymentsExpenses;
                calcModel.TotalFcbDebt = calcModel.TotalAccordingClientPaymentsExpenses;
            }
        }

        private decimal GetFcbKdnResult(Guid applicationOnlineId, int clientId, int authorId, string authorName, string iin, decimal income, out string errorMsg)
        {
            var fcbKdnPayments = _fcbKdnPaymentRepository.List(null, new { ApplicationOnlineId = applicationOnlineId });
            var todayRequest = fcbKdnPayments.FirstOrDefault(x => x.CreateDate.Date == DateTime.Today);
            errorMsg = string.Empty;

            if (todayRequest != null && todayRequest.Success)
            {
                return todayRequest.PaymentAmount;
            }

            var manualCalculationClientExpense = _manualCalculationClientExpenseService.GetByClientId(clientId);

            if (manualCalculationClientExpense != null)
            {
                return manualCalculationClientExpense.Debt;
            }

            var fcbKdnRequest = new FcbKdnRequest()
            {
                IIN = iin,
                Income = income,
                Author = authorName,
                OrganizationId = Constants.OrganizationId,
                RequestDate = DateTime.Now
            };

            var fcbKdnResponse = _fcb4Kdn.StorekdnReqWithIncome(fcbKdnRequest).Result;

            errorMsg = ValidateFCBResponse(fcbKdnResponse);

            _fcbKdnPaymentRepository.Insert(new ApplicationOnlineFcbKdnPayment
            {
                ApplicationOnlineId = applicationOnlineId,
                CreateBy = authorId,
                CreateDate = DateTime.Now,
                PaymentAmount = fcbKdnResponse?.Debt ?? 0,
                Success = string.IsNullOrEmpty(errorMsg),
            });

            var clientExpense = _clientExpenseRepository.Get(clientId);

            if (clientExpense == null)
            {
                clientExpense = new ClientExpense
                {
                    AuthorId = authorId,
                    ClientId = clientId,
                    Loan = Convert.ToInt32(fcbKdnResponse?.Debt ?? 0),
                };

                _clientExpenseRepository.Insert(clientExpense);
                _clientExpenseRepository.LogChanges(clientExpense, authorId, true);
            }
            else
            {
                clientExpense.Loan = Convert.ToInt32(fcbKdnResponse?.Debt ?? 0);

                _clientExpenseRepository.Update(clientExpense);
                _clientExpenseRepository.LogChanges(clientExpense, authorId);
            }

            return fcbKdnResponse?.Debt ?? 0;
        }

        private decimal GetTotalPositionsIncome(int clientId, Guid appOnlineId, bool needSavePositionLog = true)
        {
            var clientCarPositions = _contractRepository.GetByClientId(clientId, CollateralType.Car).ToList();
            var applicationPosition = _applicationOnlinePositionRepository.GetByApplicationOnlineId(appOnlineId);
            var applicationCar = _applicationOnlineCarRepository.Get(applicationPosition.Id);
            var kdnPositions = new List<ApplicationOnlineKdnPosition>();

            clientCarPositions.ForEach(x => ConvertCarPositionToKdnPosition(kdnPositions, x, clientId, appOnlineId, applicationCar.CarId));


            kdnPositions.Add(new ApplicationOnlineKdnPosition
            {
                ApplicationOnlineId = appOnlineId,
                ClientId = clientId,
                CollateralType = CollateralType.Car,
                CreateDate = DateTime.Now,
                EstimatedCost = applicationPosition.EstimatedCost ?? 0,
                Name = string.IsNullOrEmpty(applicationCar.Mark) || string.IsNullOrEmpty(applicationCar.Model) ? applicationCar.TechPassportModel
                    : $"{applicationCar.TransportNumber} {applicationCar.Mark} {applicationCar.Model} {applicationCar.ReleaseYear}"
            });

            if (needSavePositionLog)
                SaveKdnPositions(kdnPositions, clientId, appOnlineId);

            return Math.Round((decimal)kdnPositions.Sum(x => x.EstimatedCost) / 6m, 2);
        }

        private void SaveKdnPositions(List<ApplicationOnlineKdnPosition> kdnPositions, int clientId, Guid appOnlineId)
        {
            var dbKdnPositions = _kdnPositionRepository.List(null, new { ApplicationOnlineId = appOnlineId });

            var deletePositions = dbKdnPositions.Where(x => !kdnPositions.Any(k => k.Name == x.Name)).ToList();
            var insertPositions = kdnPositions.Where(x => !dbKdnPositions.Any(k => k.Name == x.Name)).ToList();
            var updatePositions = dbKdnPositions.Where(x => kdnPositions.Any(k => k.Name == x.Name)).ToList();

            updatePositions.ForEach(x =>
            {
                var item = kdnPositions.FirstOrDefault(f => f.Name == x.Name);

                if (item != null)
                    x.EstimatedCost = item.EstimatedCost;
            });

            deletePositions.ForEach(x => _kdnPositionRepository.Delete(x.Id));
            insertPositions.ForEach(x => _kdnPositionRepository.Insert(x));
            updatePositions.ForEach(x => _kdnPositionRepository.Update(x));
        }

        private string ValidateFCBResponse(FcbKdnResponse fcbKdnResponse)
        {
            if (fcbKdnResponse.ErrorCode == -2004 || fcbKdnResponse.ErrorCode == 1102 || fcbKdnResponse.ErrorCode == 2)
                fcbKdnResponse.Debt = 0;

            if (fcbKdnResponse.ErrorCode != -2004 && fcbKdnResponse.ErrorCode != 1102 && fcbKdnResponse.ErrorCode != 1 && fcbKdnResponse.ErrorCode != 2)
                return $"Ошибка при обращении в ПКБ: {fcbKdnResponse.ErrorMessage}";

            return string.Empty;
        }
    }
}
