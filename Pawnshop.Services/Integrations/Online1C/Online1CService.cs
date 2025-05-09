using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Online1C;
using RestSharp.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Services.Integrations.Online1C
{
    public class Online1CService : IOnline1CService
    {
        private readonly Online1CRepository _online1CRepository;
        private readonly OuterServiceSettingRepository _outerServiceSettingRepository;
        private readonly IEventLog _eventLog;

        public Online1CService(Online1CRepository online1CRepository, OuterServiceSettingRepository outerServiceSettingRepository, IEventLog eventLog)
        {
            _online1CRepository = online1CRepository;
            _outerServiceSettingRepository = outerServiceSettingRepository;
            _eventLog = eventLog;
        }

        public async Task<(bool, string)> SendReportManual(Online1CReportData data)
        {
            var jsonTask = data.ReportType switch
            {
                Online1CReportType.Accruals => GetAccrualsJson(data),
                Online1CReportType.Repayment => GetPaymentJson(data),
                Online1CReportType.MoneyPayments => GetIssuesJson(data),
                Online1CReportType.CashReceipts => GetPrepaymentJson(data),
                Online1CReportType.AdvanceRepayment => GetDebitDeposJson(data),
                Online1CReportType.CashOperations => GetCashFlowsJson(data),
                _ => throw new PawnshopApplicationException("Неизвестный тип отчета"),
            };

            return SendReport(data, jsonTask).Result;
        }

        public async Task<(bool, string)> SendReport(Online1CReportData data, params Task<(Online1CReportType, string)>[] jsonTasks)
        {
            Task.WaitAll(jsonTasks);

            var jsons = jsonTasks.Select(t => t.Result).ToArray();

            var uniqueTypes = new HashSet<Online1CReportType>();

            using (var client = new HttpClient())
            {
                var settings = _outerServiceSettingRepository.Find(new { Code = Constants.ONLINE_1C_INTEGRATION_SETTINGS_CODE });

                if (!settings.Login.HasValue() || !settings.Password.HasValue())
                    throw new PawnshopApplicationException("Не найден Access-Token. Обратитесь к администратору.");

                client.DefaultRequestHeaders.Add(settings.Login, settings.Password);

                var jsonData = new JObject
                {
                    { "Date", data.Date?.ToString("yyyy-MM-dd") }
                };

                for (int i = 0; i < jsons.Length; i++)
                {
                    var type = jsons[i].Item1;
                    var json = jsons[i].Item2;

                    if (!uniqueTypes.Contains(type))
                    {
                        jsonData.Add(type.ToString(), JToken.FromObject(JsonConvert.DeserializeObject<object[]>(json)));

                        uniqueTypes.Add(type);
                    }
                }

                var httpContent = new StringContent(jsonData.ToString(), Encoding.UTF8, "application/json");
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var contentString = httpContent.ReadAsStringAsync().Result;

                var requestUri = $"{settings.URL}/hs/cashflow/cashflow";
                var response = await client.PostAsync(requestUri, httpContent);

                var responseContent = await response.Content.ReadAsStringAsync();

                _eventLog.Log(EventCode.Online1CReportSending, response.IsSuccessStatusCode ? EventStatus.Success : EventStatus.Failed, EntityType.Online1C, null, jsonData.ToString(), responseContent, requestUri);

                response.EnsureSuccessStatusCode();

                return (response.IsSuccessStatusCode, responseContent);
            }
        }

        public async Task<(Online1CReportType, string)> GetAccrualsJson(Online1CReportData data)
        {
            var accruals = await _online1CRepository.GetAccurals(data);

            var accuralRows = accruals.Select(accrual => new
            {
                ВидЗалога = accrual.CollateralType,
                Подразделение = accrual.BranchName,
                Вознаграждение = accrual.InterestAmount,
                Пеня = accrual.PenaltyAmount,
                ПровизияОД = accrual.ProvisionDebt,
                ПровизияПроцент = accrual.ProvisionInterest,
                ПросроченныйПроцент = accrual.OverdueInterest,
                ПросроченныйОД = accrual.OverdueDebt
            }).ToList();

            var json = JsonConvert.SerializeObject(accuralRows);
            return (Online1CReportType.Accruals, json);

        }

        public async Task<(Online1CReportType, string)> GetPaymentJson(Online1CReportData data)
        {
            var payments = await _online1CRepository.GetPayments(data);

            var paymentsRows = payments.Select(payment => new
            {
                ВидЗалога = payment.CollateralType,
                Подразделение = payment.BranchName,
                ТекущийДолгКраткосрочный = payment.DebtAmountShort,
                ТекущийДолгДолгосрочный = payment.DebtAmountLong,
                Вознаграждение = payment.InterestAmount,
                Пеня = payment.PenaltyDebtAmount + payment.PenaltyInterestAmount,
                ПросроченныйОДКраткосрочный = payment.OverdueDebtAmountShort,
                ПросроченныйОДДолгосрочный = payment.OverdueDebtAmountLong,
                ПросроченныйПроцент = payment.OverdueInterestAmount
            }).ToList();

            return (Online1CReportType.Repayment, JsonConvert.SerializeObject(paymentsRows));
        }

        public async Task<(Online1CReportType, string)> GetIssuesJson(Online1CReportData data)
        {
            var issues = await _online1CRepository.GetIssues(data);

            var issuesRows = issues.Select(issue => new
            {
                ВидЗалога = issue.CollateralType,
                Подразделение = issue.BranchName,
                ТипОД = issue.IsShortTerm ? "Shortterm" : "Longterm",
                Сумма = issue.TotalAmount,
                СуммаКПеречислению = issue.DebtAmount,
                Страхование = issue.InsuranceAmount,
                СпособВыдачи = issue.PayTypeId,
                ПервоначальныйВзнос = issue.InitialFee,
                Автокредит = issue.IsBuyCar
            }).ToList();

            return (Online1CReportType.MoneyPayments, JsonConvert.SerializeObject(issuesRows));
        }

        public async Task<(Online1CReportType, string)> GetPrepaymentJson(Online1CReportData data)
        {
            var prepayments = await _online1CRepository.GetPrepayments(data);

            var prepaymentsRows = prepayments.Select(prepayment => new
            {
                Заемщик = prepayment.ClientName,
                ИИНЗаемщика = prepayment.IdentityNumber,
                Договор = prepayment.ContractNumber,
                Аванс = prepayment.Amount,
                ПоКассе = prepayment.CashOperation,
                ПоТерминалу = prepayment.OnlineOperation,
                ПлатежнаяСистема = prepayment.OnlineProvider,
                ВидЗалога = prepayment.CollateralType,
                Подразделение = prepayment.BranchName,
                ПодразделениеКассы = prepayment.CashOrderBranchName,
            }).ToList();

            return (Online1CReportType.CashReceipts, JsonConvert.SerializeObject(prepaymentsRows));
        }

        public async Task<(Online1CReportType, string)> GetDebitDeposJson(Online1CReportData data)
        {
            var debitDepos = await _online1CRepository.GetDebitDepos(data);

            var debitDeposRows = debitDepos.Select(debitDepo => new
            {
                Заемщик = debitDepo.ClientName,
                ИИНЗаемщика = debitDepo.IdentityNumber,
                Договор = debitDepo.ContractNumber,
                Аванс = debitDepo.Amount,
                ВидЗалога = debitDepo.CollateralType,
                Подразделение = debitDepo.BranchName,
            }).ToList();

            return (Online1CReportType.AdvanceRepayment, JsonConvert.SerializeObject(debitDeposRows));
        }

        public async Task<(Online1CReportType, string)> GetCashFlowsJson(Online1CReportData data)
        {
            var cashFlows = await _online1CRepository.GetCashFlows(data);

            var cashFlowsRows = cashFlows.Select(cashFlow => new
            {
                cashFlow.OrderDate,
                cashFlow.DT1C,
                cashFlow.DTSubkonto,
                cashFlow.OneCCodeDT,
                cashFlow.KT1C,
                cashFlow.KTSubkonto,
                cashFlow.OneCCodeKT,
                cashFlow.OrderCost,
                cashFlow.OrderBranch,
                cashFlow.Client,
                cashFlow.ClientIIN,
            }).ToList();

            return (Online1CReportType.CashOperations, JsonConvert.SerializeObject(cashFlowsRows));
        }
    }
}
