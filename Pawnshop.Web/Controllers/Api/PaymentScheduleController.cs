using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Data.Models.Base;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Data.Models.SchedulePayments;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Insurance;
using Pawnshop.Services.LoanPercent;
using Pawnshop.Services.PaymentSchedules;
using Pawnshop.Web.Engine;
using Pawnshop.Web.Models.SchedulePayments;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.ContractPaymentSchedules;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/payment-schedule")]
    [ApiController]
    public class PaymentScheduleController : Controller
    {
        private readonly IContractService _contractService;
        private readonly IInsurancePremiumCalculator _insurancePremiumCalculator;
        private readonly LoanPercentService _loanPercentService;
        private readonly IPaymentScheduleService _paymentScheduleService;

        public PaymentScheduleController(
            IContractService contractService,
            IInsurancePremiumCalculator insurancePremiumCalculator,
            LoanPercentService loanPercentService,
            IPaymentScheduleService paymentScheduleService)
        {
            _contractService = contractService;
            _insurancePremiumCalculator = insurancePremiumCalculator;
            _loanPercentService = loanPercentService;
            _paymentScheduleService = paymentScheduleService;
        }

        [HttpGet("calculation")]
        public async Task<IActionResult> CalculateFromMobile([FromQuery] CalculateFromMobileQuery query)
        {
            try
            {
                var errorMsg = new List<string>();

                if (query.FirstPaymentDate.HasValue)
                {
                    var minDate = DateTime.Today.AddDays(Constants.PAYMENT_RANGE_MIN_DAYS);
                    var maxDate = DateTime.Today.AddDays(Constants.PAYMENT_RANGE_MAX_DAYS);

                    if (query.FirstPaymentDate.Value.Date < minDate || query.FirstPaymentDate.Value.Date > maxDate)
                        errorMsg.Add($"Дата первого платежа должна быть в диапазоне между {minDate.ToString("yyyy-MM-dd")} - {maxDate.ToString("yyyy-MM-dd")}.");

                    _paymentScheduleService.CheckPayDay(query.FirstPaymentDate.Value.Day);
                }

                if (query.ProductId == 0)
                    errorMsg.Add("Идентификатор продукта указан неправильно.");

                var maturityDate = DateTime.Today.AddMonths(query.Period);
                var product = _loanPercentService.Get(query.ProductId);
                var scheduleType = product?.ScheduleType;
                LoanPercentSetting altProduct = null;

                if (product != null && product.ContractClass == ContractClass.Tranche && product.UseSystemType == UseSystemType.ONLINE)
                {
                    var childs = await _loanPercentService.GetChild(product.ParentId.Value);
                    product = childs.FirstOrDefault(x =>
                        x.IsActual &&
                        x.IsInsuranceAvailable == query.Insurance &&
                        x.ScheduleType == scheduleType &&
                        x.UseSystemType != UseSystemType.OFFLINE);

                    altProduct = childs.FirstOrDefault(x =>
                        x.IsActual &&
                        x.IsInsuranceAvailable == !query.Insurance &&
                        x.ScheduleType == scheduleType &&
                        x.UseSystemType != UseSystemType.OFFLINE);
                }

                if (product == null)
                    errorMsg.Add($"Не найден продукт {query.ProductId}.");

                CorrectTrancheDetails(query, ref maturityDate, product, errorMsg);

                var minPeriod = product?.ContractPeriodFrom;
                var maxPeriod = product?.ContractPeriodTo;

                if (minPeriod.HasValue && query.Period < minPeriod)
                    errorMsg.Add($"Срок займа должен быть больше {minPeriod}.");
                if (maxPeriod.HasValue && query.Period > maxPeriod)
                    errorMsg.Add($"Срок займа должен быть меньше {maxPeriod}.");

                var minLoanCost = product?.LoanCostFrom;
                var maxLoanCost = product?.LoanCostTo;

                if (!query.Insurance)
                {
                    if (minLoanCost.HasValue && query.LoanCost < minLoanCost)
                        errorMsg.Add($"Сумма для калькуляции должна быть больше {minLoanCost}.");
                    if (maxLoanCost.HasValue && query.LoanCost > maxLoanCost)
                        errorMsg.Add($"Сумма для калькуляции должна быть меньше {maxLoanCost}.");
                }
                else
                {
                    minLoanCost = minLoanCost.HasValue ? Convert.ToInt32(_insurancePremiumCalculator.GetLoanCostWithoutInsurancePremium(minLoanCost.Value)) : default(int?);
                    maxLoanCost = maxLoanCost.HasValue ? Convert.ToInt32(_insurancePremiumCalculator.GetLoanCostWithoutInsurancePremium(maxLoanCost.Value)) : default(int?);

                    if (minLoanCost.HasValue && query.LoanCost < minLoanCost)
                        errorMsg.Add($"Сумма для калькуляции должна быть больше {minLoanCost}.");
                    if (maxLoanCost.HasValue && query.LoanCost > maxLoanCost)
                        errorMsg.Add($"Сумма для калькуляции должна быть меньше {maxLoanCost}.");
                }

                var hasInsurance = product?.IsInsuranceAvailable;

                if (hasInsurance.HasValue && !hasInsurance.Value && query.Insurance)
                    errorMsg.Add($"Продукт не имеет возможности страхования.");

                if (errorMsg.Any())
                    return BadRequest(string.Join(" ", errorMsg.ToArray()));

                var fpd = query.FirstPaymentDate.HasValue ? query.FirstPaymentDate.Value.DateTime : (DateTime?)default;
                var insuranceResult = GetInsuranceAmount(query.Insurance, query.LoanCost, product);

                if (product.IsInsuranceAvailable && insuranceResult.Item1 + insuranceResult.Item2 > product.LoanCostTo)
                {
                    var loanCost = _insurancePremiumCalculator.GetLoanCostWithoutInsurancePremium(product.LoanCostTo);
                    insuranceResult = GetInsuranceAmount(query.Insurance, loanCost, product);
                }

                var schedule = _paymentScheduleService.Build(product.ScheduleType.Value, insuranceResult.Item1 + insuranceResult.Item2, product.LoanPercent, DateTime.Today, maturityDate, fpd);

                var scheduleList = schedule
                    .OrderBy(x => x.Date)
                    .Select((x, i) => new SchedulePaymentView
                    {
                        Date = x.Date,
                        Number = i + 1,
                        Amount = x.DebtCost + x.PercentCost,
                        PrincipalDebtLeft = x.DebtLeft,
                        PrincipalDebt = x.DebtCost,
                        ProfitAmount = x.PercentCost,
                    })
                    .ToList();

                var response = new ScheduleInfoView
                {
                    ClientAmount = insuranceResult.Item1,
                    FirstPaymentDate = schedule.Min(x => x.Date),
                    Insurance = query.Insurance,
                    InsuredAmount = insuranceResult.Item2,
                    Percent = Math.Round(product.LoanPercent * 30, 2),
                    TotalAmount = insuranceResult.Item1 + insuranceResult.Item2,
                    ScheduleList = scheduleList,
                    PaymentAmount = scheduleList.FirstOrDefault(x => x.Number == 1).Amount,
                };

                if (altProduct != null)
                {
                    var insuranceResultAlt = GetInsuranceAmount(!query.Insurance, query.LoanCost, altProduct);

                    if (altProduct.IsInsuranceAvailable && insuranceResultAlt.Item1 + insuranceResultAlt.Item2 > altProduct.LoanCostTo)
                    {
                        var loanCost = _insurancePremiumCalculator.GetLoanCostWithoutInsurancePremium(altProduct.LoanCostTo);
                        insuranceResultAlt = GetInsuranceAmount(!query.Insurance, loanCost, altProduct);
                    }

                    var scheduleEconomy = _paymentScheduleService.Build(altProduct.ScheduleType.Value, insuranceResultAlt.Item1 + insuranceResultAlt.Item2, altProduct.LoanPercent, DateTime.Today, maturityDate, fpd);
                    var totalDebt = schedule.Sum(x => x.DebtCost) + schedule.Sum(x => x.PercentCost);
                    var totalDebtAlt = scheduleEconomy.Sum(x => x.DebtCost) + scheduleEconomy.Sum(x => x.PercentCost);

                    response.InsuranceEconomy = altProduct.IsInsuranceAvailable ?
                        totalDebt - totalDebtAlt :
                        totalDebtAlt - totalDebt;
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


        [HttpGet("/api/contracts/{id}/payment-schedules")]
        public async Task<IActionResult> GetPaymentSchedules([FromRoute] int id,
            [FromServices] ContractPaymentScheduleRepository repository,
            [FromServices] ClientDefermentRepository clientDefermentRepository,
            [FromServices] RestructuredContractPaymentScheduleRepository restructuredContractPaymentScheduleRepository)
        {
            var contract = await _contractService.GetOnlyContractAsync(id);
            if (contract == null)
            {
                return NotFound();
            }
            var isContractRestructured = clientDefermentRepository.IsClientDeffered(contract.ClientId, contract.Id);
            if (contract.ContractClass == ContractClass.CreditLine)
            {
                if (isContractRestructured)
                {
                    return Ok(new ContractPaymentScheduleOnlineView());
                }

                await _contractService.CreditLineFillConsolidateSchedule(contract);

                var tranches = await _contractService.GetAllSignedTranches(contract.Id);
                List<ContractPaymentScheduleOnlineView> contractPaymentSchedules =
                    new List<ContractPaymentScheduleOnlineView>();
                for (int i = 0; i < tranches.Count; i++)
                {
                    var paymentSchedule = await repository.GetContractPaymentSchedules(tranches[i].Id);
                    contractPaymentSchedules.AddRange(paymentSchedule.Select((ps, index) => new ContractPaymentScheduleOnlineView
                    {
                        Number = index,
                        Date = ps.Date.ToString("yyyy-MM-dd"),
                        ContractNumber = tranches[i].ContractNumber,
                        ActualDate = ps.ActualDate?.ToString("yyyy-MM-dd"),
                        DebtLeft = ps.DebtLeft,
                        DebtCost = ps.DebtCost,
                        PercentCost = ps.PercentCost,
                        PenaltyCost = ps.PenaltyCost ?? 0,
                        MonthlyPayment = ps.DebtCost + ps.PercentCost + (ps.PenaltyCost ?? 0),
                        TotalMonthlyPayment = ps.DebtCost + ps.PercentCost + (ps.PenaltyCost ?? 0),
                        Status = (int)ps.Status,
                        DaysOverdue = ps.ActualDate == null
                            ? (DateTime.Now - ps.Date).Days < 0 ? 0 : (DateTime.Now - ps.Date).Days
                            : (ps.ActualDate - ps.Date).Value.Days
                    }));
                }
                return Ok(contract.PaymentSchedule.Select(ps => new CreditLinePaymentScheduleOnlineView
                    {
                        Number = ps.Id,
                        Date = ps.Date.ToString("yyyy-MM-dd"),
                        ContractNumber = contract.ContractNumber,
                        ActualDate = ps.ActualDate?.ToString("yyyy-MM-dd"),
                        DebtLeft = ps.DebtLeft,
                        DebtCost = ps.DebtCost,
                        PercentCost = ps.PercentCost,
                        PenaltyCost = ps.PenaltyCost ?? 0,
                        MonthlyPayment = ps.DebtCost + ps.PercentCost + (ps.PenaltyCost ?? 0),
                        TotalMonthlyPayment = ps.DebtCost + ps.PercentCost + (ps.PenaltyCost ?? 0),
                        Status = (int)ps.Status,
                        DaysOverdue = ps.ActualDate == null ? (DateTime.Now - ps.Date).Days < 0 ? 0 : (DateTime.Now - ps.Date).Days : (ps.ActualDate - ps.Date).Value.Days,
                        TranchesPaymentSchedules = contractPaymentSchedules
                            .Where(cps => cps.Date == ps.Date.ToString("yyyy-MM-dd"))
                            .Select((cps, index) => new ContractPaymentScheduleOnlineView
                        {
                            Number = index+1,
                            Date = cps.Date,
                            ContractNumber = cps.ContractNumber,
                            ActualDate = cps.ActualDate,
                            DebtLeft = cps.DebtLeft,
                            DebtCost = cps.DebtCost,
                            PercentCost = cps.PercentCost,
                            PenaltyCost = cps.PenaltyCost ,
                            MonthlyPayment = cps.MonthlyPayment,
                            TotalMonthlyPayment = cps.TotalMonthlyPayment,
                            Status = (int)cps.Status,
                            DaysOverdue = cps.DaysOverdue
                        })
                    }));
            }

            if (contract.ContractClass == ContractClass.Credit || contract.ContractClass == ContractClass.Tranche)
            {
                if (isContractRestructured)
                {
                    var restructuredPaymentSchedule = 
                        await restructuredContractPaymentScheduleRepository.GetListByContractId(contract.Id);
                    return Ok(restructuredPaymentSchedule.Select((ps, index) => new ContractPaymentScheduleOnlineView
                    {
                        Number = index,
                        Date = ps.Date.ToString("yyyy-MM-dd"),
                        ContractNumber = contract.ContractNumber,
                        ActualDate = ps.ActualDate?.ToString("yyyy-MM-dd"),
                        DebtLeft = ps.DebtLeft,
                        DebtCost = ps.DebtCost,
                        PercentCost = ps.PercentCost,
                        PenaltyCost = ps.PenaltyCost ?? 0,
                        MonthlyPayment = ps.DebtCost + ps.PercentCost + (ps.PenaltyCost ?? 0),
                        TotalMonthlyPayment = ps.DebtCost + ps.PercentCost + (ps.PenaltyCost ?? 0) 
                            + (ps.PaymentBalanceOfDefferedPercent ?? 0) + (ps.PaymentBalanceOfOverduePercent ?? 0) + (ps.PaymentPenaltyOfOverdueDebt ?? 0) 
                            + (ps.PaymentPenaltyOfOverduePercent ?? 0),
                        Status = (int)ps.Status,
                        DaysOverdue = ps.ActualDate == null ? (DateTime.Now - ps.Date).Days < 0 ? 0 : (DateTime.Now - ps.Date).Days : (ps.ActualDate - ps.Date).Value.Days
                    }));
                }
                var paymentSchedule = await repository.GetContractPaymentSchedules(contract.Id);
                return Ok(paymentSchedule.Select((ps, index) => new ContractPaymentScheduleOnlineView
                {
                    Number = index+1,
                    Date = ps.Date.ToString("yyyy-MM-dd"),
                    ContractNumber = contract.ContractNumber,
                    ActualDate = ps.ActualDate?.ToString("yyyy-MM-dd"),
                    DebtLeft = ps.DebtLeft,
                    DebtCost = ps.DebtCost,
                    PercentCost = ps.PercentCost,
                    PenaltyCost = ps.PenaltyCost ?? 0,
                    MonthlyPayment = ps.DebtCost + ps.PercentCost + (ps.PenaltyCost ?? 0),
                    TotalMonthlyPayment = ps.DebtCost + ps.PercentCost + (ps.PenaltyCost ?? 0),
                    Status = (int)ps.Status,
                    DaysOverdue = ps.ActualDate == null ? (DateTime.Now - ps.Date).Days < 0 ? 0 : (DateTime.Now - ps.Date).Days : (ps.ActualDate - ps.Date).Value.Days
                }));
            }

            throw new NotImplementedException();
        }


        private void CorrectTrancheDetails(CalculateFromMobileQuery query, ref DateTime maturityDate, LoanPercentSetting product, List<string> errorMsg)
        {
            if (!query.CreditLineId.HasValue || product == null)
                return;

            var creditLine = _contractService.GetOnlyContract(query.CreditLineId.Value);

            if (creditLine == null || creditLine.Status != ContractStatus.Signed)
            {
                errorMsg.Add($"Кредитная линия {query.CreditLineId.Value} не найдена.");
                return;
            }

            if (creditLine.SettingId.Value != product.ParentId.Value)
            {
                errorMsg.Add($"Неправильно подобранный продукт для транша [{product.Name}] в рамках кредитной линии .");
                return;
            }

            if (query.Period == 0 || maturityDate > creditLine.MaturityDate)
            {
                query.Period = creditLine.MaturityDate.MonthDifference(DateTime.Today);
                maturityDate = creditLine.MaturityDate;
            }

            var creditLineLimit = _contractService.GetCreditLineLimit(creditLine.Id).Result;
            creditLineLimit = Math.Truncate(creditLineLimit);

            if (query.Insurance)
            {
                var preInsuranceResult = GetInsuranceAmount(query.Insurance, query.LoanCost, product);

                if (preInsuranceResult.Item1 + preInsuranceResult.Item2 > creditLineLimit)
                    query.LoanCost = _insurancePremiumCalculator.GetLoanCostWithoutInsurancePremium(creditLineLimit);
            }
            else if (query.LoanCost > creditLineLimit)
            {
                query.LoanCost = creditLineLimit;
            }

            var firstPaymentDate = _contractService.GetNextPaymentDateByCreditLineId(query.CreditLineId.Value);

            if (firstPaymentDate.HasValue)
            {
                query.FirstPaymentDate = firstPaymentDate;
            }
        }

        private (decimal, decimal) GetInsuranceAmount(bool withInsurance, decimal loanCost, LoanPercentSetting product)
        {
            if (!withInsurance)
                return (loanCost, 0);

            var insurance = _insurancePremiumCalculator.GetInsuranceDataV2(loanCost, product.InsuranceCompanies.FirstOrDefault().InsuranceCompanyId, product.Id);

            return (insurance.Eds == 0 || loanCost > 3909999 ? loanCost : insurance.Eds, insurance.InsurancePremium);
        }
    }
}
