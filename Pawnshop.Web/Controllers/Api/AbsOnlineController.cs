using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Extensions;
using Pawnshop.Core.Options;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Access.AccountingCore;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.AccountRecords;
using Pawnshop.Data.Models.Base;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.LoanSettings;
using Pawnshop.Data.Models.OnlineApplications;
using Pawnshop.Data.Models.PayOperations;
using Pawnshop.Services.AbsOnline;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Calculation;
using Pawnshop.Services.Cars;
using Pawnshop.Services.Clients;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Insurance;
using Pawnshop.Services.Models.Calculation;
using Pawnshop.Services.OnlineApplications;
using Pawnshop.Services.Refinance;
using Pawnshop.Web.Converters;
using Pawnshop.Web.Engine.MessageSenders;
using Pawnshop.Web.Engine.Services.Interfaces;
using Pawnshop.Web.Models.AbsOnline;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System;
using Pawnshop.Services.PaymentSchedules;

namespace Pawnshop.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class AbsOnlineController : Controller
    {
        private const int VEFIFICATION_PERIOD = 90;

        private readonly AccountRepository _accountRepository;
        private readonly CarRepository _carRepository;
        private readonly ClientDocumentProviderRepository _clientDocumentProviderRepository;
        private readonly ClientDocumentTypeRepository _clientDocumentTypeRepository;
        private readonly ClientRepository _clientRepository;
        private readonly GroupRepository _groupRepository;
        private readonly IAbsOnlineService _absOnlineService;
        private readonly ICarService _carService;
        private readonly IClientContactService _clientContactService;
        private readonly IClientService _clientService;
        private readonly IContractDutyService _contractDutyService;
        private readonly IContractPaymentScheduleService _contractPaymentScheduleService;
        private readonly IContractService _contractService;
        private readonly IInsurancePremiumCalculator _insurancePremiumCalculator;
        private readonly IOnlineApplicationCarService _onlineApplicationCarService;
        private readonly IOnlineApplicationService _onlineApplicationService;
        private readonly LoanPercentRepository _loanPercentRepository;
        private readonly OnlineApplicationRepository _onlineApplicationRepository;
        private readonly TypeRepository _typeRepository;
        private readonly IContractActionService _contractActionService;
        private readonly EnviromentAccessOptions _options;
        private readonly EmailSender _emailSender;
        private readonly IRefinanceService _refinanceService;
        private readonly ContractCheckRepository _contractCheckRepository;
        private readonly ContractCheckValueRepository _contractCheckValueRepository;
        private readonly IPaymentScheduleService _paymentScheduleService;

        public AbsOnlineController(
            AccountRepository accountRepository,
            CarRepository carRepository,
            ClientDocumentProviderRepository clientDocumentProviderRepository,
            ClientDocumentTypeRepository clientDocumentTypeRepository,
            ClientRepository clientRepository,
            GroupRepository groupRepository,
            IAbsOnlineService absOnlineService,
            ICarService carService,
            IClientContactService clientContactService,
            IClientService clientService,
            IContractDutyService contractDutyService,
            IContractPaymentScheduleService contractPaymentScheduleService,
            IContractService contractService,
            IInsurancePremiumCalculator insurancePremiumCalculator,
            IOnlineApplicationCarService onlineApplicationCarService,
            IOnlineApplicationService onlineApplicationService,
            LoanPercentRepository loanPercentRepository,
            OnlineApplicationRepository onlineApplicationRepository,
            TypeRepository typeRepository,
            IContractActionService contractActionService,
            IOptions<EnviromentAccessOptions> options,
            EmailSender emailSender,
            IRefinanceService refinanceService,
            ContractCheckRepository contractCheckRepository,
            ContractCheckValueRepository contractCheckValueRepository,
            IPaymentScheduleService paymentScheduleService)
        {
            _accountRepository = accountRepository;
            _carRepository = carRepository;
            _clientDocumentProviderRepository = clientDocumentProviderRepository;
            _clientDocumentTypeRepository = clientDocumentTypeRepository;
            _clientRepository = clientRepository;
            _groupRepository = groupRepository;
            _absOnlineService = absOnlineService;
            _carService = carService;
            _clientContactService = clientContactService;
            _clientService = clientService;
            _contractDutyService = contractDutyService;
            _contractPaymentScheduleService = contractPaymentScheduleService;
            _contractService = contractService;
            _insurancePremiumCalculator = insurancePremiumCalculator;
            _onlineApplicationCarService = onlineApplicationCarService;
            _onlineApplicationService = onlineApplicationService;
            _loanPercentRepository = loanPercentRepository;
            _onlineApplicationRepository = onlineApplicationRepository;
            _typeRepository = typeRepository;
            _contractActionService = contractActionService;
            _options = options.Value;
            _emailSender = emailSender;
            _refinanceService = refinanceService;
            _contractCheckRepository = contractCheckRepository;
            _contractCheckValueRepository = contractCheckValueRepository;
            _paymentScheduleService = paymentScheduleService;
        }


        /// <summary>
        /// Метод шины get_product_list
        /// </summary>
        [HttpGet("products")]
        public async Task<IActionResult> GetProducts()
        {
            var rawProductList = await _loanPercentRepository.GetOnlyListAsync(new { UseSystemType = UseSystemType.ONLINE });

            var groupProductList = rawProductList
                .Where(x => x.IsActual)
                .GroupBy(x => new { x.ParentId, x.ScheduleType, x.IsFloatingDiscrete })
                .ToList();

            var productListView = new List<LoanPercentSettingViewModel>();

            foreach (var products in groupProductList)
            {
                var bProduct = products.OrderBy(x => x.IsInsuranceAvailable).FirstOrDefault();
                var minPercent = products.OrderBy(x => x.LoanPercent).First();
                var maxPercnet = products.OrderByDescending(x => x.LoanPercent).First();

                var insuranceType = 0;

                if (bProduct.ContractClass == ContractClass.Credit)
                    insuranceType = bProduct.IsInsuranceAvailable ? 2 : 0;

                else if (products.Any(x => x.IsInsuranceAvailable) && products.Any(x => !x.IsInsuranceAvailable))
                    insuranceType = 2;

                else if (products.Any(x => x.IsInsuranceAvailable))
                    insuranceType = 1;
                else
                    insuranceType = 0;

                productListView.Add(new LoanPercentSettingViewModel
                {
                    Name = bProduct.Name,
                    NameKz = bProduct.NameAlt,
                    //Description = bProduct.Description,
                    //DescriptionKz = bProduct.DescriptionKz,
                    ProductId = bProduct.Id,
                    InsuranceType = insuranceType,
                    Percent = Math.Round(minPercent.LoanPercent * ((int?)minPercent.ContractPeriodToType ?? 0), 2),
                    MinPercent = Math.Round(minPercent.LoanPercent * ((int?)minPercent.ContractPeriodToType ?? 0), 2),
                    MaxPercent = Math.Round(maxPercnet.LoanPercent * ((int?)maxPercnet.ContractPeriodToType ?? 0), 2),
                    IsActive = bProduct.IsActual,
                    MinValue = bProduct.LoanCostFrom,
                    MaxValue = bProduct.LoanCostTo,
                    MinMonth = (bProduct.ContractPeriodFrom * (int?)bProduct.ContractPeriodFromType) / (int)bProduct.PaymentPeriodType ?? 0,
                    MaxMonth = (bProduct.ContractPeriodTo * (int?)bProduct.ContractPeriodToType) / (int)bProduct.PaymentPeriodType ?? 0,
                    MinAge = 18,
                    MaxAge = 62
                });
            }

            return Ok(productListView);
        }

        /// <summary>
        /// Метод шины payment_shedule / get_pay_shedule_data 
        /// </summary>
        [HttpGet("calculate-schedule")]
        public async Task<IActionResult> GetCalculateSchedule([FromQuery] CalculateScheduleRequest rq)
        {
            try
            {
                if (!int.TryParse(rq.ProductId, out int productId))
                    return NotFound($"Продукт {rq.ProductId} не найден.");

                var maturityDate = DateTime.Today.AddMonths(rq.Period);
                var product = _loanPercentRepository.Get(productId);
                decimal? totalDebtWithoutInsurance = null;
                decimal? loanCostForCreditLine = null;
                Contract creditLine = null;

                if (rq.Tranche && !string.IsNullOrEmpty(rq.BaseContractNumber))
                {
                    creditLine = await _contractService.GetCreditLineByNumberAsync(rq.BaseContractNumber.Trim());

                    if (creditLine == null || creditLine.Status != ContractStatus.Signed)
                        return NotFound($"Кредитная линия {rq.BaseContractNumber} не найдена.");

                    if (rq.Period == 0 || maturityDate > creditLine.MaturityDate)
                        maturityDate = creditLine.MaturityDate;
                }
                else if (rq.Period == 0)
                    return BadRequest("Срок займа должен быть больше 0.");

                if (product.ContractClass == ContractClass.Tranche)
                {
                    if (creditLine != null && product.ParentId != creditLine.SettingId.Value)
                        return BadRequest($"Продукт кредитной линии транша {product.ParentId} не равен продукту кредитной линии {creditLine.SettingId.Value}");

                    var childs = await _loanPercentRepository.GetChild(product.ParentId.Value);
                    product = childs.FirstOrDefault(x =>
                        x.IsActual &&
                        x.IsInsuranceAvailable == rq.Insurance &&
                        x.ScheduleType == product.ScheduleType &&
                        x.UseSystemType != UseSystemType.OFFLINE);

                    var creditLimit = creditLine != null ? await _contractService.GetCreditLineLimit(creditLine.Id) : 0;
                    creditLimit = Math.Truncate(creditLimit);

                    if (creditLine != null && rq.LoanCost > creditLimit)
                        loanCostForCreditLine = creditLimit;

                    if (rq.Insurance)
                    {
                        var preInsuranceResult = GetInsuranceAmount(rq.Insurance, loanCostForCreditLine ?? rq.LoanCost, product);

                        if (creditLine != null && creditLimit < preInsuranceResult.Item1 + preInsuranceResult.Item2)
                            loanCostForCreditLine = _insurancePremiumCalculator.GetLoanCostWithoutInsurancePremium(creditLimit);

                        var productWithoutInsurance = childs.FirstOrDefault(x =>
                            x.IsActual &&
                            x.IsInsuranceAvailable == false &&
                            x.ScheduleType == product.ScheduleType &&
                            x.UseSystemType != UseSystemType.OFFLINE);

                        if (productWithoutInsurance != null)
                        {
                            var scheduleWithoutInsurance = _paymentScheduleService.Build(productWithoutInsurance.ScheduleType.Value, loanCostForCreditLine ?? rq.LoanCost, productWithoutInsurance.LoanPercent, DateTime.Today, maturityDate);
                            totalDebtWithoutInsurance = scheduleWithoutInsurance.Sum(x => x.DebtCost) + scheduleWithoutInsurance.Sum(x => x.PercentCost);
                        }
                    }
                }

                var insuranceResult = GetInsuranceAmount(rq.Insurance, loanCostForCreditLine ?? rq.LoanCost, product);
                var schedule = _paymentScheduleService.Build(product.ScheduleType.Value, insuranceResult.Item1 + insuranceResult.Item2, product.LoanPercent, DateTime.Today, maturityDate);

                var response = new CalculateScheduleViewModel
                {
                    ScheduleList = schedule.OrderBy(x => x.Date).Select((x, i) => x.ToOnlineViewModel(i + 1)).ToList(),
                    ClientAmount = insuranceResult.Item1,
                    TotalAmount = insuranceResult.Item1 + insuranceResult.Item2,
                    Percent = Math.Round(product.LoanPercent * 30, 2),
                    InsuredAmount = insuranceResult.Item2,
                    Insurance = rq.Insurance,
                };

                if (totalDebtWithoutInsurance.HasValue)
                {
                    response.InsuranceEconomy = totalDebtWithoutInsurance.Value -
                        (schedule.Sum(x => x.DebtCost) + schedule.Sum(x => x.PercentCost));
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Метод шины crm_get_vin_data
        /// </summary>
        [HttpGet("vin/{vin}")]
        public async Task<IActionResult> GetVinData(string vin)
        {
            var contracts = await _contractService.GetContractsByVinAsync(vin);

            var contractIds = contracts.Select(x => x.Id).ToList();

            foreach (var creditLine in contracts.Where(x => x.ContractClass == ContractClass.CreditLine))
            {
                var tranches = await _contractService.GetAllSignedTranches(creditLine.Id);

                if (tranches.Any())
                    contractIds.AddRange(tranches.Select(x => x.Id));
            }

            var balances = _contractService.GetBalances(contractIds);

            var response = new VinDataViewModel
            {
                Vin = vin,
                PrincipalDebt = balances.Sum(x => x.TotalAcountAmount),
                Percent = balances.Sum(x => x.TotalProfitAmount),
                Penalty = balances.Sum(x => x.PenyAmount)
            };

            return Ok(response);
        }

        /// <summary>
        /// Метод шины get_pkd_unsent_contracts
        /// </summary>
        [HttpGet("contracts/fcb-unset")]
        public async Task<IActionResult> GetUnsetFcbContracts([FromQuery] string iin)
        {
            var response = new UnsetFcbContractsResponse { IIN = iin };

            var client = _clientRepository.FindByIdentityNumber(iin);

            if (client == null)
            {
                response.Error = 1;
                response.Message = "По данному ИИН заемщик не найден.";
                return Ok(response);
            }

            var contracts = await _contractService.GetListForOnlineByIinAsync(iin);

            var unsetContracts = contracts.Where(x => x.ContractDate.Date == DateTime.Today)
                .Select(x => new UnsetFcbContractViewModel
                {
                    ContractNumber = x.ContractNumber,
                    Date = x.ContractDate,
                    LoanCost = Math.Round(_contractPaymentScheduleService.GetAverageMonthlyPaymentAsync(x.Id).Result, 2)
                })
                .ToList();

            if (!unsetContracts?.Any() ?? true)
                response.Message = "Договоров не обнаружено.";
            else
                response.Message = "Договора отправлены.";

            response.Contracts.AddRange(unsetContracts);
            return Ok(response);
        }

        /// <summary>
        /// Метод шины mobile_main_screen
        /// </summary>
        [HttpGet("contracts")]
        public async Task<IActionResult> GetContracts([FromQuery] string iin, [FromQuery] string phone)
        {
            if (string.IsNullOrEmpty(iin))
                return BadRequest("ИИН обязателен к заполнению.");

            var contractsRaw = await _contractService.GetListForOnlineByIinAsync(iin);

            var cars = await _carRepository.GetListByContractIdsAsync(contractsRaw.Select(x => x.Id).ToList());

            var contracts = contractsRaw.Where(x => x.ContractClass != ContractClass.CreditLine).ToList();

            var creditLines = contractsRaw.Where(x => x.ContractClass == ContractClass.CreditLine && x.MaturityDate > DateTime.Now.AddMonths(3))
                .Select(x => new
                {
                    creditLine = x,
                    creditLimit = _contractService.GetCreditLineLimit(x.Id).Result,
                    product = _loanPercentRepository.GetOnlyAsync(x.SettingId.Value).Result,
                    car = cars.FirstOrDefault(c => c.ContractId == x.Id)
                })
                .Where(x => x.creditLimit > x.product.LoanCostFrom)
                .ToList();

            creditLines.ForEach(x =>
            {
                if (!contracts.Any(t => t.ContractNumber == x.creditLine.ContractNumber))
                {
                    var firstTranche = _contractService.GetNonCreditLineByNumberAsync(x.creditLine.ContractNumber).Result;

                    if (firstTranche != null)
                    {
                        contracts.Add(firstTranche);
                    }
                }
            });

            var contractsViewModel = new List<ContractViewModel>();
            var tasks = new List<Task>();

            contracts
                .GroupJoin(cars,
                    contract => contract.CreditLineId ?? contract.Id,
                    car => car.ContractId,
                    (contract, car) => new { contract, car }).ToList()
                .ForEach(x =>
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        var hasPartialPayment = await _contractService.HasPartialPaymentAsync(x.contract.Id);
                        var balance = await _accountRepository.GetBalanceByContractIdAsync(x.contract.Id);
                        var paymentScheduleList = _contractPaymentScheduleService.GetListByContractId(x.contract.Id, true);
                        var nextPaymentAmount = x.contract.NextPaymentDate?.Date <= DateTime.Now.Date ? balance.CurrentDebt : 0;

                        contractsViewModel.Add(x.contract.ToShortOnlineViewModel(
                            x.car.FirstOrDefault(),
                            hasPartialPayment,
                            balance,
                            paymentScheduleList,
                            nextPaymentAmount
                        ));
                    }));
                });

            Task.WaitAll(tasks.ToArray());

            var creditLinesViewModel = creditLines.Select(x => x.creditLine.ToCreditLineOnlineViewModel(x.car, x.creditLimit));

            return Ok(new { contracts = contractsViewModel.OrderBy(x => x.DateOpen), creditLines = creditLinesViewModel.OrderBy(x => x.ContractNumber) });
        }

        /// <summary>
        /// Метод шины mobile_history
        /// </summary>
        [HttpGet("contracts/history")]
        public async Task<IActionResult> GetContractsHistory([FromQuery] string iin)
        {
            if (string.IsNullOrEmpty(iin))
                return BadRequest("ИИН обязателен к заполнению.");

            var contractsRaw = await _contractService.GetHistoryForOnlineByIinAsync(iin);

            var cars = await _carRepository.GetListByContractIdsAsync(contractsRaw.Select(x => x.Id).ToList());

            var otherContractsRaw = contractsRaw.Where(x => x.ContractClass != ContractClass.CreditLine);

            var creditLinesRaw = contractsRaw.Where(x => x.ContractClass == ContractClass.CreditLine);

            var contracts = otherContractsRaw
                .GroupJoin(cars,
                    contract => contract.CreditLineId ?? contract.Id,
                    car => car.ContractId,
                    (contract, car) => new { contract, car })
                .Select(x => x.contract.ToShortOnlineViewModel(
                    x.car.FirstOrDefault(),
                    _contractService.HasPartialPaymentAsync(x.contract.Id).Result,
                    _accountRepository.GetBalanceByContractIdAsync(x.contract.Id).Result,
                    _contractPaymentScheduleService.GetListByContractId(x.contract.Id, true)
                    ));

            var creditLines = creditLinesRaw
                .GroupJoin(cars, contract => contract.Id, car => car.ContractId, (contract, car) => new { contract, car })
                .Select(x => x.contract.ToCreditLineOnlineViewModel(
                    x.car.FirstOrDefault(),
                    _contractService.GetCreditLineLimit(x.contract.Id).Result
                    ));

            return Ok(new { contracts, creditLines });
        }

        /// <summary>
        /// Метод шины mobile_contract_data
        /// </summary>
        [HttpGet("contracts/{contractnumber}")]
        public async Task<IActionResult> GetContractByContractNumber(string contractnumber, [FromQuery] bool isCollecting)
        {
            var contract = await _contractService.GetNonCreditLineByNumberAsync(contractnumber);

            if (contract == null)
                return NotFound($"Займ с указанным номером {contractnumber} не найден.");

            if (contract.SettingId.HasValue)
                contract.Setting = await _loanPercentRepository.GetOnlyAsync(contract.SettingId.Value);

            var car = await _carRepository.GetByContractIdAsync(contract.CreditLineId ?? contract.Id);
            var balance = await _accountRepository.GetBalanceByContractIdAsync(contract.Id);
            var schedulePayments = _contractPaymentScheduleService.GetListByContractId(contract.Id, true);
            var hasPartialPayment = await _contractService.HasPartialPaymentAsync(contract.Id);
            var nextPaymentAmount = contract.NextPaymentDate.HasValue && contract.NextPaymentDate <= DateTime.Now.Date ? balance.CurrentDebt : 0;
            var paidPayments = isCollecting ? await _accountRepository.GetMovementsOfDepoAccountAsync(contract.Id) : new List<MovementsOfDepoAccount>();
            return Ok(contract.ToOnlineViewModel(car, hasPartialPayment, balance, schedulePayments, nextPaymentAmount, isCollecting, paidPayments.ToList()));
        }

        /// <summary>
        /// Метод шины <b><u>crm_get_panel</u></b>
        /// </summary>
        // TODO: change fill response object
        [HttpGet("contracts/crm-menu/{contractnumber}")]
        public async Task<IActionResult> GetContractInfoForCrmMenu(string contractnumber)
        {
            var contract = await _contractService.GetNonCreditLineByNumberAsync(contractnumber);

            if (contract == null || (int)contract.Status < (int)ContractStatus.Signed)
                return BadRequest();

            var product = await _loanPercentRepository.GetOnlyAsync(contract.SettingId.Value);
            var balance = await _accountRepository.GetBalanceByContractIdAsync(contract.Id);
            var paymentsSchedule = _contractPaymentScheduleService.GetListByContractId(contract.Id, true);

            if (balance == null)
                balance = new ContractBalance();

            if (paymentsSchedule == null || !paymentsSchedule.Any())
                paymentsSchedule = new List<ContractPaymentSchedule>();

            var paidPayments = paymentsSchedule.Where(x => x.ActionId.HasValue && x.ActualDate.HasValue);
            var remainingPayments = paymentsSchedule.Where(x => !x.ActionId.HasValue && !x.ActualDate.HasValue);
            var expiredPayments = paymentsSchedule.Where(x => x.Date.Date < DateTime.Now.Date && !x.ActionId.HasValue && !x.ActualDate.HasValue);

            var firstExpiredPayment = expiredPayments.OrderBy(x => x.Date).FirstOrDefault();
            var currentPayment = paymentsSchedule.OrderBy(x => x.Date).FirstOrDefault(x => !x.ActionId.HasValue && !x.ActualDate.HasValue && x.Date > DateTime.Now);
            var lastPayment = paymentsSchedule.OrderByDescending(x => x.Date).FirstOrDefault(x => !x.ActionId.HasValue && !x.ActualDate.HasValue && x.Date > DateTime.Now);
            var fineAmount = balance.OverdueAccountAmount + balance.OverdueProfitAmount + balance.PenyAmount;


            var response = new ContractInfoForCrmMenuResponse();
            response.ContractNumber = contract.ContractNumber;
            response.ContractDate = contract.ContractDate;
            response.PrincipalDebt = balance.AccountAmount;
            response.PrincipalDebtSchedule = paymentsSchedule.Sum(x => x.DebtCost);
            response.Profit = balance.ProfitAmount;
            //response.Commission = 0;
            //response.MembershipFees = 0;
            response.Fine = fineAmount;
            response.Penalty = balance.PenyAmount;
            //response.StateDuty = 0;
            response.FirstPaymentExpiredDate = firstExpiredPayment?.Date ?? response.FirstPaymentExpiredDate;
            response.PaymentExpiredDays = firstExpiredPayment == null ? 0 : Math.Abs((DateTime.Now - firstExpiredPayment.Date).Days);
            response.PaymentExpiredCount = expiredPayments.Count();
            response.OverduePrincipalDebt = balance.OverdueAccountAmount;
            response.OverdueProfit = balance.OverdueProfitAmount;
            //response.OverdueCommission = 0;
            //response.OverdueMembershipFees = 0;
            response.OverduePenalty = balance.PenyAmount;
            response.OverdueFine = fineAmount;
            //response.OverdueOtherIncome = qwer;
            response.UrgentPrincipalDebt = balance.AccountAmount;
            response.UrgentProfit = balance.ProfitAmount;
            //response.UrgenCommission = qwer;
            //response.UrgenMembershipFees = qwer;
            //response.UrgenPenalty = qwer;
            response.UrgenFine = fineAmount;
            //response.UrgenOtherIncome = qwer;
            response.NextPaymentDate = currentPayment == null ? response.NextPaymentDate : currentPayment.Date;
            response.NextPaymentDays = currentPayment == null ? 0 : Math.Abs((DateTime.Now - contract.NextPaymentDate.Value).Days);
            response.NextPaymentPrincipalDebt = currentPayment?.DebtCost ?? 0;
            response.NextPaymentProfit = currentPayment?.PercentCost ?? 0;
            //response.NextPaymentCommission = 0;
            //response.NextPaymentMembershipFees = 0;
            response.PaymentScheduleDate = paymentsSchedule.FirstOrDefault()?.CreateDate ?? response.PaymentScheduleDate;
            response.Period = contract.ContractDate;
            response.ContractViewName = $"{contract.ContractNumber} от {contract.ContractDate.ToString("dd.MM.yyyy")} г.";
            response.SignDate = contract.SignDate.HasValue ? contract.SignDate.Value.Date : response.SignDate;
            response.MaturityDate = contract.MaturityDate;
            response.ActualContractNumber = contract.ContractNumber;
            response.ActualContractDate = contract.ContractDate;
            response.ActualContractRunDate = contract.ContractDate;
            response.YearPercent = contract.Note == "Миграция TasOnline" ? product.LoanPercent * 365 : product.LoanPercent * 360;
            response.LastPaymentDate = lastPayment == null ? response.LastPaymentDate : lastPayment.Date;
            response.LastPaymentPrincipalDebt = lastPayment?.DebtCost ?? 0;
            response.LastPaymentProfit = lastPayment?.PercentCost ?? 0;
            //response.LastPaymentCommission = 0;
            //response.LastPaymentMembershipFees = 0;
            response.LasyPaymentPenalty = lastPayment?.PenaltyCost ?? 0;
            //response.LasyPaymentFine = 0;
            response.PaidPrincipalDebt = paidPayments?.Sum(x => x.DebtCost) ?? 0;
            response.PaidProfit = paidPayments?.Sum(x => x.PercentCost) ?? 0;
            //response.PaidCommission = 0;
            //response.PaidMembershipFees = 0;
            response.PaidPenalty = paidPayments?.Sum(x => x.PenaltyCost ?? 0) ?? 0;
            response.PaidFine = 0; // TODO: abs-migration узнать как получить всего погашенных штрафов
            response.ProductName = product.Name;
            response.RemnantContract = remainingPayments.Sum(x => x.DebtCost);
            response.RemnantCommonContract = response.RemnantContract;
            response.ContractCloseDate = contract.BuyoutDate.HasValue ? contract.BuyoutDate.Value : response.ContractCloseDate;
            response.IsClosed = contract.BuyoutDate.HasValue;
            response.RemainingPaymentsCount = remainingPayments.Count();
            //response.ProblemStatusDate = new DateTime();
            //response.ProblemContractNumber = string.Empty;
            //response.ProblemContractDate = new DateTime();
            //response.ProblemContractStatusMessage = string.Empty;
            //response.CourtDecisionNumber = string.Empty;

            return Ok(response);
        }

        /// <summary>
        /// Метод шины <b><u>get_late_payments</u></b>
        /// </summary>
        [HttpGet("contracts/overdue/{contractnumber}")]
        public async Task<IActionResult> GetContractOverdue(string contractnumber)
        {
            //var overdueContract = await _contractService.GetOverdueForCrmAsync(contractnumber);

            //return Ok(overdueContract);
            return Ok();
        }

        /// <summary>
        /// Метод шины <b><u>get_default_date_client</u></b>
        /// </summary>
        [HttpGet("contracts/overdue-list")]
        public async Task<IActionResult> GetOverdueContractList()
        {
            //var overdueContracts = await _contractService.GetOverdueListForCrmAsync();

            //var notMobilePhoneList = overdueContracts.Where(x => string.IsNullOrEmpty(x.MobilePhone)).ToList();
            //var sendList = overdueContracts.Except(notMobilePhoneList).ToList();

            //return Ok(overdueContracts);
            return Ok();
        }

        /// <summary>
        /// Метод шины <b><u>crm_payment_information</u></b>
        /// </summary>
        [HttpGet("client/arrears")]
        public async Task<IActionResult> GetClientArrears(string iin)
        {
            var client = _clientRepository.FindByIdentityNumber(iin);

            if (client == null)
                return NotFound(new ClientArrearsResponse { Code = 2, Message = "По данному ИИН заемщик не найден." });

            var clientPhone = _clientContactService.GetMobilePhoneContacts(client.Id);

            var contractsRaw = await _contractService.GetListForOnlineByIinAsync(iin);
            var contracts = new List<ContractArrears>();

            foreach (var contract in contractsRaw)
            {
                if ((int)contract.Status < (int)ContractStatus.Signed || contract.ContractClass == ContractClass.CreditLine)
                    continue;

                var balance = await _accountRepository.GetBalanceByContractIdAsync(contract.Id);

                if (balance == null)
                    balance = new ContractBalance();

                var paymentsSchedule = _contractPaymentScheduleService.GetListByContractId(contract.Id, true);

                var firstExpiredPayments = paymentsSchedule?
                    .OrderBy(x => x.Date)
                    .FirstOrDefault(x => !x.ActionId.HasValue && !x.ActualDate.HasValue && x.Date.Date < DateTime.Now.Date);

                var todayPayment = paymentsSchedule?
                    .OrderBy(x => x.Date)
                    .FirstOrDefault(x => !x.ActionId.HasValue && !x.ActualDate.HasValue && x.Date == DateTime.Now.Date);

                var nextPayment = paymentsSchedule?
                    .OrderBy(x => x.Date)
                    .FirstOrDefault(x => !x.ActionId.HasValue && !x.ActualDate.HasValue && x.Date >= DateTime.Now.Date);

                var amount = 0M;
                var payDayExpired = 0;

                if (firstExpiredPayments != null)
                {
                    amount = balance.OverdueAccountAmount + balance.OverdueProfitAmount + balance.PenyAmount;
                    payDayExpired = Math.Abs((DateTime.Now - firstExpiredPayments.Date).Days);

                    if (todayPayment != null)
                        amount += todayPayment.DebtCost + todayPayment.PercentCost + (todayPayment.PenaltyCost ?? 0);
                }
                else if (nextPayment != null)
                    amount = nextPayment.DebtCost + nextPayment.PercentCost + (nextPayment.PenaltyCost ?? 0);

                contracts.Add(new ContractArrears
                {
                    ContractNumber = contract.ContractNumber,
                    Number = contract.ContractNumber,
                    Amount = amount,
                    Totalamount = balance.TotalRedemptionAmount + balance.PrepaymentBalance,
                    CurrentAmount = balance.PrepaymentBalance,
                    DebtMain = balance.AccountAmount + balance.OverdueAccountAmount,
                    DebtPercent = balance.ProfitAmount + balance.OverdueProfitAmount,
                    Penalties = balance.PenyAmount,
                    PayDayExpired = payDayExpired,
                });
            }

            var response = new ClientArrearsResponse
            {
                Code = 0,
                Message = "Заемщик найден.",
                IdentityNumber = client.IdentityNumber,
                Surname = client.Surname,
                Name = client.Name,
                Patronymic = client.Patronymic,
                Fullname = client.FullName,
                IsMale = client.IsMale ?? false,
                BirthDay = client.BirthDay ?? new DateTime(),
                MobilePhone = clientPhone?.FirstOrDefault(x => x.IsDefault)?.Address,
                LegalForm = client.LegalForm?.Code,
                IsResident = client.IsResident,
                IsPEP = client.IsPolitician,
                Citizenship = client.Citizenship?.Code,
                ClientId = client.Id.ToString(),
                Contracts = contracts
            };

            return Ok(response);
        }

        /// <summary>
        /// Метод шины <b><u>crm_update_tel</u></b>
        /// </summary>
        [HttpPut("client/phone")]
        public IActionResult ChangeClientPhone([FromBody] ChangeClientPhoneRequest rq)
        {
            var client = _clientRepository.FindByIdentityNumber(rq.IIN);

            if (client == null)
                return BadRequest(new ChangeResponse { Message = $"Ошибка! Клиент с указанным ИИН {rq.IIN} не найден." });

            var mp = _clientContactService.Find(new { Address = rq.NewPhone, IsDefault = true });

            if (mp != null && mp.ClientId != client.Id && mp.IsDefault && mp.VerificationExpireDate > DateTime.Now)
                return BadRequest(new ChangeResponse { Message = $"Ошибка! Номер телефона {rq.NewPhone} привязан к другому клиенту." });

            var clientContacts = _clientContactService.GetMobilePhoneContacts(client.Id);
            var defaultContacts = clientContacts.Where(x => x.IsDefault).ToList();

            defaultContacts.ForEach(x =>
            {
                x.IsDefault = false;
                _clientContactService.SaveWithoutChecks(x);
            });

            client.MobilePhone = rq.NewPhone;
            _clientRepository.Update(client);

            var newClientContract = clientContacts?.FirstOrDefault(x => x.Address == rq.NewPhone) ??
                new ClientContact
                {
                    Address = rq.NewPhone,
                    AuthorId = 1,
                    ClientId = client.Id,
                    ContactTypeId = 1,
                    CreateDate = DateTime.Now,
                };

            _clientContactService.SaveWithoutChecks(newClientContract);

            newClientContract.IsDefault = true;
            newClientContract.VerificationExpireDate = DateTime.Now.AddDays(VEFIFICATION_PERIOD);

            _clientContactService.SaveWithoutChecks(newClientContract);

            return Ok(new ChangeResponse { Message = $"В АБС номера клиента с ИИН {rq.IIN} изменен на {rq.NewPhone}.", Result = true });
        }

        /// <summary>
        /// Метод шины <b><u>crm_change_client_uin</u></b>
        /// </summary>
        [HttpPut("client/iin")]
        public IActionResult ChangeClientIin([FromBody] ChangeClientIinRequest rq)
        {
            var client = _clientRepository.FindByIdentityNumber(rq.IIN);

            if (client == null)
                return BadRequest(new ChangeResponse { Message = $"Клиент с указанным ИИН {rq.IIN} не найден." });

            var anyClient = _clientRepository.FindByIdentityNumber(rq.NewIIN);

            if (anyClient != null)
                return BadRequest(new ChangeResponse { Message = $"Указанный ИИН {rq.NewIIN} привязан к другому клиенту." });

            client.IdentityNumber = rq.NewIIN;
            _clientService.Save(client);

            return Ok(new ChangeResponse { Message = $"В АБС изменен ИИН {rq.IIN} клиента на {rq.NewIIN}.", Result = true });
        }

        /// <summary>
        /// Метод шины <b><u>get_contract_pdf</u></b>
        /// </summary>
        [HttpGet("getcontractpdf/{contractnumber}/{format?}")]
        public async Task<IActionResult> GetContractPdf(
            [FromServices] ContractAdditionalInfoRepository contractAdditionalInfoRepository,
            [FromServices] ApplicationOnlineRepository applicationOnlineRepository,
            string contractnumber,
            string format = "pdf")
        {
            var contract = await _contractService.GetNonCreditLineByNumberAsync(contractnumber);

            if (contract == null)
                return NotFound($"Не найден контракт по заявке: {contractnumber}.");

            var applicationOnline = applicationOnlineRepository.GetByContractId(contract.Id);

            using (var http = new HttpClient())
            {
                var pdfApiUrl = _absOnlineService.GetUrlPdfApi();

                var requestParam = $"contractId={contract.Id}";

                if (format == "html")
                    requestParam += $"&format=html";
                if (contract.ContractClass == ContractClass.Tranche)
                    requestParam += $"&creditLineId={contract.CreditLineId}";

                var additionalInfo = contractAdditionalInfoRepository.Get(contract.Id);

                if (additionalInfo != null && !string.IsNullOrEmpty(additionalInfo.SmsCode))
                    requestParam += $"&code={additionalInfo.SmsCode}";
                else if (applicationOnline != null && applicationOnline.SignType == Data.Models.ApplicationsOnline.ApplicationOnlineSignType.NPCK &&
                    additionalInfo != null && additionalInfo.LoanStorageFileId.HasValue)
                    requestParam += $"&fileGuid={additionalInfo.LoanStorageFileId}";

                var request = await http.GetAsync($"{pdfApiUrl}/sendPdf?{requestParam}");

                var response = await request.Content.ReadAsStringAsync();

                return Ok(JsonConvert.DeserializeObject<PostContractPdfModel>(response));
            }
        }

        /// <summary>
        /// Метод шины <b><u>get_loan_req_data</u></b>
        /// </summary>
        [HttpGet("applications/{contractnumber}")]
        public async Task<IActionResult> GetApplication(string contractnumber)
        {
            var application = await _onlineApplicationService.FindByContractNumberAsync(contractnumber);

            if (application.SettingId != 0)
            {
                var product = await _loanPercentRepository.GetOnlyAsync(application.SettingId);

                if (product != null & product.ParentId.HasValue)
                {
                    var products = await _loanPercentRepository.GetChild(product.ParentId.Value);
                    product = products.FirstOrDefault(x =>
                        x.IsActual &&
                        x.IsInsuranceAvailable == false &&
                        x.ScheduleType == product.ScheduleType &&
                        x.UseSystemType != UseSystemType.OFFLINE);

                    if (product != null)
                        application.SettingId = product.Id;
                }
            }

            if (application == null)
                return NotFound($"Заявка не найдена по контракту!: {contractnumber}.");

            var client = _clientRepository.Get(application.ClientId);
            var clientRequisite = client.Requisites?.FirstOrDefault(x => x.IsDefault);
            var bankInfo = _clientRepository.GetOnlyClient(clientRequisite?.BankId ?? 0);

            var response = application.ToApplicationResponse(client, clientRequisite, bankInfo);

            return Ok(response);
        }

        /// <summary>
        /// Метод который возвращает статус online заявки
        /// </summary>
        /// <param name="contractnumber">Номер договора</param>
        /// <returns></returns>

        [HttpGet("applications/{contractnumber}/status")]
        public async Task<IActionResult> GetApplicationStatus(string contractnumber)
        {
            var application = await _onlineApplicationService.FindByContractNumberAsync(contractnumber);

            if (application == null)
                return NotFound($"Заявка не найдена по контракту!: {contractnumber}.");

            return Ok(new OnlineApplicationStatusViewModel
            {
                ContractNumber = application.ContractNumber,
                Status = application.Status.ToString(),
                CreateDate = application.CreateDate,
                Id = application.Id,
            });
        }

        /// <summary>
        /// Метод шины <b><u>mobile_application_1</u></b>
        /// </summary>
        [HttpPost("applications")]
        public async Task<IActionResult> CreateApplication([FromBody] CreateApplicationRequest rq)
        {
            var contract = await _contractService.GetNonCreditLineByNumberAsync(rq.ContractNumber);

            if (contract != null)
                // TODO: add log $"По заявке {rq.ContractNumber} есть созданный займ."
                return BadRequest();

            var application = await _onlineApplicationService.FindByContractNumberAsync(rq.ContractNumber);

            if (application != null && application.Status != OnlineApplicationStatusType.Draft)
                // TODO: add log $"Займ по заявке {rq.ContractNumber} был создан ранее."
                return BadRequest();

            if (!int.TryParse(rq.ProductId, out int productId))
                return NotFound($"Продукт {rq.ProductId} не найден.");

            LoanPercentSetting product = _loanPercentRepository.Get(productId);
            LoanPercentSetting creditLineProduct = null;

            if (product != null && product.ContractClass == ContractClass.Tranche)
            {
                creditLineProduct = _loanPercentRepository.Get(product.ParentId.Value);
                var childs = await _loanPercentRepository.GetChild(product.ParentId.Value);
                product = childs.FirstOrDefault(x =>
                    x.IsActual &&
                    x.IsInsuranceAvailable == rq.Insurance &&
                    x.ScheduleType == product.ScheduleType &&
                    x.UseSystemType != UseSystemType.OFFLINE);
            }

            if (product == null)
                // TODO: add log eror find product
                return BadRequest();

            var client = _clientRepository.FindByIdentityNumber(rq.IIN);
            client = rq.ToDomainModel(client);
            _clientService.Save(client);

            UpdateClientMobilePhone(client, rq.MobilePhone);

            application = rq.ToDomainModel(application, product.Id, client.Id, creditLineProduct?.Id, isOpeningCreditLine: creditLineProduct != null);

            if (application.Position != null)
                application.Position.Car = await _onlineApplicationCarService.GetEntityForCreateAsync(application.Position.Car);

            _onlineApplicationService.Save(application);

            return Ok(CreateResponse(rq.IIN, rq.ContractNumber, rq.LoanCost, product, rq.Insurance));
        }

        /// <summary>
        /// Метод шины <b><u>mobile_tranche_application</u></b>
        /// </summary>
        [HttpPost("applications/tranche")]
        public async Task<IActionResult> CreateTranche([FromBody] CreateTrancheRequest rq)
        {
            var creditLine = await _contractService.GetCreditLineByNumberAsync(rq.BaseContractNumber.Trim());

            if (creditLine == null || creditLine.Status != ContractStatus.Signed || creditLine.MaturityDate < rq.MaturityDate)
                // TODO: add log error find creditLine
                return BadRequest();

            if (rq.LoanCost > _contractService.GetCreditLineLimit(creditLine.Id).Result)
                // TODO: add log error "Превышена максимальная сумма транша."
                return BadRequest();

            var application = await _onlineApplicationService.FindByContractNumberAsync(rq.ContractNumber);

            if (application != null && application.Status != OnlineApplicationStatusType.Draft)
                return BadRequest($"Займ по заявке {rq.ContractNumber} был создан ранее.");

            if (!int.TryParse(rq.ProductId, out int productId))
                return NotFound($"Продукт {rq.ProductId} не найден.");

            LoanPercentSetting product = _loanPercentRepository.Get(productId);

            if (product == null || !product.ParentId.HasValue)
                // TODO: add log eror find product
                return BadRequest();

            var childs = await _loanPercentRepository.GetChild(product.ParentId.Value);
            product = childs.FirstOrDefault(x =>
                x.IsActual &&
                x.IsInsuranceAvailable == rq.Insurance &&
                x.ScheduleType == product.ScheduleType &&
                x.UseSystemType != UseSystemType.OFFLINE);

            var client = _clientRepository.FindByIdentityNumber(rq.IIN);

            if (client == null || client.Id != creditLine.ClientId)
                return BadRequest($"Нельзя открыть транш другому клиенту.");

            client = rq.ToDomainModel(client);
            _clientService.Save(client);

            UpdateClientMobilePhone(client, rq.MobilePhone);

            var maturityDate = rq.Period > 0 ? DateTime.Now.AddMonths(rq.Period) : creditLine.MaturityDate;

            application = rq.ToDomainModel(application, product.Id, client.Id, creditLine.SettingId, creditLine.Id, maturityDate);
            application.BranchId = creditLine.BranchId;

            _onlineApplicationService.Save(application);

            return Ok(GetCrmStateViewModel(rq.ContractNumber, rq.LoanCost, product, rq.Insurance, application.CreditLineId));
        }

        /// <summary>
        /// Метод шины <b><u>crm_update_contractdata</u></b>
        /// </summary>
        [HttpPut("applications/{contractnumber}")]
        public async Task<IActionResult> UpdateApplication(string contractnumber, [FromBody] UpdateApplicationRequest rq)
        {
            rq.ContractNumber = contractnumber;
            var application = await _onlineApplicationService.FindByContractNumberAsync(rq.ContractNumber);

            if (application == null)
                return NotFound($"Заявка не найдена по контракту!: {contractnumber}.");

            if (!ContractCanEdit(application.ContractId))
                return BadRequest($"Заявка {contractnumber} обработана.");

            LoanPercentSetting product = null;
            LoanPercentSetting creditLineProduct = null;

            if (!string.IsNullOrEmpty(rq.ProductId))
            {
                if (!int.TryParse(rq.ProductId, out int productId))
                    return NotFound($"Продукт {rq.ProductId} не найден.");

                product = _loanPercentRepository.Get(productId);

                if (product != null && product.ContractClass == ContractClass.Tranche)
                {
                    var childs = await _loanPercentRepository.GetChild(product.ParentId.Value);
                    product = childs.FirstOrDefault(x =>
                        x.IsActual &&
                        x.IsInsuranceAvailable == application.WithInsurance &&
                        x.ScheduleType == product.ScheduleType &&
                        x.UseSystemType != UseSystemType.OFFLINE);
                }
            }

            product ??= _loanPercentRepository.Get(application.SettingId);

            if (product.ParentId.HasValue)
                creditLineProduct = _loanPercentRepository.Get(product.ParentId.Value);

            var client = _clientRepository.Get(application.ClientId);
            rq.ToDomainModel(client);
            _clientRepository.Update(client);

            application.ClientDocumentId = client.Documents?.FirstOrDefault(x => x.Number == rq.PassportNumber)?.Id ?? application.ClientDocumentId;

            if (rq.Period.HasValue)
            {
                application.Period = rq.Period.Value;
                application.MaturityDate = DateTime.Today.AddMonths(rq.Period.Value);
            }

            if (!application.IsOpeningCreditLine && application.CreditLineId.HasValue)
            {
                var creditLine = _contractService.GetOnlyContract(application.CreditLineId.Value);

                if (!rq.Period.HasValue || rq.Period.Value == 0 || application.MaturityDate > creditLine.MaturityDate)
                    application.MaturityDate = creditLine.MaturityDate;
            }

            application = rq.ToDomainModel(application, product.Id, creditLineProduct?.Id);

            if (application.Position != null)
                application.Position.Car = await _onlineApplicationCarService.GetEntityForCreateAsync(application.Position.Car);

            _onlineApplicationService.Save(application);

            if (application.ContractId.HasValue)
            {
                await SaveContract(application, client, product, creditLineProduct);
                _absOnlineService.SaveAdditionalInfo(application.ContractId.Value, string.Empty, application.BranchId);
            }

            return Ok();
        }

        /// <summary>
        /// Метод шины <b><u>application_update_data</u></b>
        /// </summary>
        [HttpPut("applications/{contractnumber}/fields")]
        public async Task<IActionResult> UpdateApplicationFields(string contractnumber, [FromBody] UpdateApplicationFieldsRequest rq)
        {
            try
            {
                rq.ContractNumber = contractnumber;

                var branchInfo = rq.Fields.FirstOrDefault(x => x.Name.Equals("city"));

                if (branchInfo == null)
                    throw new Exception("Не удалось найти филиал в параметрах.");

                var branch = await _groupRepository.GetByDisplayName(branchInfo.Data);

                if (branch == null)
                    throw new Exception($"Указанный филиал {branchInfo.Data} не найден.");

                var application = await _onlineApplicationService.FindByContractNumberAsync(rq.ContractNumber);
                var contract = await _contractService.GetNonCreditLineByNumberAsync(rq.ContractNumber);

                if (application == null && contract == null)
                    throw new Exception($"Заявка не найдена по контракту!: {contractnumber}.");

                if (application != null)
                {
                    application.BranchId = branch.Id;
                    _onlineApplicationService.Save(application);
                }

                if (contract != null)
                    _absOnlineService.SaveAdditionalInfo(contract.Id, string.Empty, branch.Id);

                return Ok(new UpdateApplicationFieldsResponse
                {
                    ContractNumber = rq.ContractNumber,
                    InstanceId = rq.InstanceId,
                    Message = "Новые данные установлены.",
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new UpdateApplicationFieldsResponse
                {
                    ContractNumber = contractnumber,
                    InstanceId = rq.InstanceId,
                    Message = ex.Message,
                    Error = 1,
                });
            }
        }

        /// <summary>
        /// Метод шины <b><u>change_pay_date</u></b>
        /// </summary>
        [HttpPut("applications/{contractnumber}/pay-day")]
        public async Task<IActionResult> SetApplicationPaymentDay(string contractnumber, [FromBody] ApplicationPaymentDayRequest rq)
        {
            rq.ContractNumber = contractnumber;
            var application = await _onlineApplicationService.FindByContractNumberAsync(rq.ContractNumber);

            if (application == null)
                return NotFound($"Заявка не найдена по контракту!: {contractnumber}.");

            if (!ContractCanEdit(application.ContractId, true))
            {
                rq.Message = "Нельзя изменить дату!";
                rq.Error = 1;
                return BadRequest(rq);
            }

            var currentDate = DateTime.Today;
            DateTime firstPaymentDate;

            try
            {
                firstPaymentDate = new DateTime(currentDate.Year, currentDate.Month, rq.PayDay);
            }
            catch
            {
                rq.Message = $"Ошибка формирования даты {currentDate.ToString("yyyy-MM")}-{rq.PayDay}.";
                rq.Error = 1;
                return BadRequest(rq);
            }

            var minDate = currentDate.AddDays(15);
            var maxDate = currentDate.AddDays(45);

            if (firstPaymentDate < currentDate)
            {
                firstPaymentDate = firstPaymentDate.AddMonths(1);
            }

            if (firstPaymentDate < minDate)
            {
                firstPaymentDate = firstPaymentDate.AddMonths(1);
            }

            if (firstPaymentDate > maxDate)
            {
                rq.Message = $"Ошибка выбора даты, диапазон даты велик.";
                rq.Error = 1;
                return BadRequest(rq);
            }

            application.PayDay = rq.PayDay;
            application.FirstPaymentDate = firstPaymentDate;

            _onlineApplicationService.Save(application);

            if (application.ContractId.HasValue)
            {
                var client = _clientRepository.Get(application.ClientId);
                var product = _loanPercentRepository.Get(application.SettingId);
                var creditLineProduct = application.CreditLineSettingId.HasValue ?
                    _loanPercentRepository.Get(application.CreditLineSettingId.Value) : null;

                await SaveContract(application, client, product, creditLineProduct);
                _absOnlineService.SaveAdditionalInfo(application.ContractId.Value, string.Empty, application.BranchId);
            }

            return Ok(rq);
        }

        /// <summary>
        /// Метод шины <b><u>mobile_payment_data</u></b>
        /// </summary>
        [HttpPut("applications/{contractnumber}/receiver-account")]
        public async Task<IActionResult> UpdateApplicationReceiverAccount(string contractnumber, [FromBody] ApplicationReceiverAccountRequest rq)
        {
            rq.ContractNumber = contractnumber;
            var application = await _onlineApplicationService.FindByContractNumberAsync(rq.ContractNumber);

            if (application == null)
                return NotFound($"Заявка не найдена по контракту!: {contractnumber}.");

            if (!ContractCanEdit(application.ContractId))
                return BadRequest($"Заявка {contractnumber} обработана.");

            var client = _clientRepository.Get(application.ClientId);

            if (client == null)
                return NotFound($"Ошибка поиска клиента по контракту!: {contractnumber}");

            var type = rq.PaymentType switch
            {
                0 => 1, // счет
                1 => 2, // карта
                _ => 0
            };

            if (type == 0)
                return BadRequest("Не удалось определить тип реквизита.");

            var value = rq.Iban.IsNullOrEmpty(rq.CardNumber);

            if (!string.IsNullOrEmpty(rq.Iban) && !string.IsNullOrEmpty(rq.CardNumber))
            {
                if (type == 1)
                    value = rq.Iban;
                if (type == 2)
                    value = rq.CardNumber;
            }

            if (string.IsNullOrEmpty(value))
                return BadRequest("Не указаны номер реквизита.");

            if (client.Requisites.Any())
            {
                client.Requisites.ForEach(x => x.IsDefault = false);
                _clientRepository.Update(client);
            }

            var clientRequisite = client.Requisites?.FirstOrDefault(x => x.Value.Equals(value))
                ?? new ClientRequisite
                {
                    AuthorId = 1,
                    ClientId = application.ClientId,
                    CreateDate = DateTime.Today,
                    RequisiteTypeId = type,
                    Value = value,
                };

            clientRequisite.CardExpiryDate = rq.CardDate.IsNullOrEmpty(clientRequisite.CardExpiryDate);
            clientRequisite.CardHolderName = rq.CardHolderName.IsNullOrEmpty(clientRequisite.CardHolderName);
            clientRequisite.IsDefault = true;

            if (!string.IsNullOrEmpty(rq.Bank))
            {
                var bank = _clientService.GetBankByName(rq.Bank);

                if (bank != null)
                    clientRequisite.BankId = bank.Id;
            }

            client.Requisites = new List<ClientRequisite> { clientRequisite };
            _clientRepository.Update(client);
            if (!string.IsNullOrEmpty(rq.MobilePhone))
                UpdateClientMobilePhone(client, rq.MobilePhone);

            _onlineApplicationService.Save(application);

            return Ok();
        }

        /// <summary>
        /// Метод шины <b><u>crm_verification_result</u></b>
        /// </summary>
        [HttpPut("applications/{contractnumber}/verification-result")]
        public async Task<IActionResult> SetApplicationVerificationResult(string contractnumber, [FromBody] ApplicationVerificationResultRequest rq)
        {
            rq.ContractNumber = contractnumber;
            var application = await _onlineApplicationService.FindByContractNumberAsync(rq.ContractNumber);

            if (application == null)
                return NotFound($"Заявка не найдена по контракту!: {contractnumber}.");

            if (!ContractCanEdit(application.ContractId))
                return BadRequest($"Заявка {contractnumber} обработана.");

            if (rq.VerificationResult.Equals("decline"))
            {
                application.Status = OnlineApplicationStatusType.Reject;
                _onlineApplicationService.Save(application);

                if (application.ContractId.HasValue)
                    _absOnlineService.DeleteInsuranceRecords(application.ContractId.Value);

                return NoContent();
            }

            var docProviderId = await _clientDocumentProviderRepository.GetMapProviderIdAsync(rq.PassportIssuer);

            var client = _clientRepository.Get(application.ClientId);
            var clientDocTypes = _clientDocumentTypeRepository.List(null);
            var branch = _groupRepository.Get(application.BranchId ?? 0);

            var clientRequisite = client.Requisites.FirstOrDefault(x => x.IsDefault);

            if (clientRequisite != null)
            {
                clientRequisite.CardHolderName = rq.CardholderName.IsNullOrEmpty(clientRequisite.CardHolderName);

                if (!string.IsNullOrEmpty(rq.Bank))
                {
                    var bank = _clientService.GetBankByName(rq.Bank);

                    if (bank != null)
                        clientRequisite.BankId = bank.Id;
                }
            }

            var completedClient = rq.ToDomainModel(client, clientDocTypes, docProviderId, branch?.ATEId);
            _clientRepository.Update(completedClient);

            UpdateClientMobilePhone(client, rq.MobilePhone);

            application.ClientDocumentId = completedClient.Documents?.FirstOrDefault(x => x.Number.Equals(rq.PassportNumber))?.Id ?? application.ClientDocumentId;

            if (rq.Insurance.HasValue && rq.Insurance.Value != application.WithInsurance && application.CreditLineSettingId.HasValue)
            {
                var oldProduct = _loanPercentRepository.Get(application.SettingId);
                var childs = await _loanPercentRepository.GetChild(oldProduct.ParentId.Value);
                var product = childs.FirstOrDefault(x =>
                    x.IsActual &&
                    x.IsInsuranceAvailable == rq.Insurance &&
                    x.ScheduleType == oldProduct.ScheduleType &&
                    x.UseSystemType != UseSystemType.OFFLINE);

                if (product == null)
                    return BadRequest("Продукт с указанными параметрами не найден!");

                application.SettingId = product.Id;
            }

            if (rq.RefinanceList.Count != 0)
            {
                for (int i = 0; i < rq.RefinanceList.Count; i++)
                {
                    var indx = application.OnlineApplicationRefinances
                        .FindIndex(oap => oap.RefinancedContractNumber == rq.RefinanceList[i]);
                    if (indx == -1)
                    {
                        var contract = await _contractService.GetNonCreditLineByNumberAsync(rq.RefinanceList[i]);
                        if (contract != null)
                        {
                            application.OnlineApplicationRefinances.Add(new OnlineApplicationRefinance
                            {
                                ContractId = application.ContractId,
                                ContractNumber = application.ContractNumber,
                                RefinancedContractId = contract.Id,
                                RefinancedContractNumber = contract.ContractNumber
                            });
                        }
                    }
                    else
                    {
                        var contract = await _contractService.GetNonCreditLineByNumberAsync(rq.RefinanceList[i]);
                        if (contract != null)
                        {
                            application.OnlineApplicationRefinances[i].ContractId = application.ContractId;
                            application.OnlineApplicationRefinances[i].ContractNumber = application.ContractNumber;
                            application.OnlineApplicationRefinances[i].RefinancedContractId = contract.Id;
                            application.OnlineApplicationRefinances[i].RefinancedContractNumber = contract.ContractNumber;
                        }
                    }
                }

                for (int i = 0; i < application.OnlineApplicationRefinances.Count; i++)
                {
                    if (!rq.RefinanceList.Contains(application.OnlineApplicationRefinances[i].RefinancedContractNumber))
                    {
                        application.OnlineApplicationRefinances[i].DeleteDate = DateTime.Now;
                    }
                }
            }

            decimal refinanceAmount = 0;
            decimal sumForClient = 0;
            bool needChangeBySmallRefinance = false;
            bool impossibleСoncludeСontract = false;

            if (application.OnlineApplicationRefinances.Count != 0 && application.ContractId != null)
            {
                refinanceAmount =
                    await _refinanceService.CalculateRefinanceAmountForContract(application.ContractId.Value);

                if (application.IsOpeningCreditLine && application.CreditLineId != null) // Нельзя выдать рефинанс в рамках существующей кредитной линии 
                {
                    var creditLineLimit = _contractService.GetCreditLineLimit(application.CreditLineId.Value).Result;

                    if (creditLineLimit < refinanceAmount)
                    {
                        impossibleСoncludeСontract = true;// Нельзя выдать рефинанс если лимит КЛ меньше суммы рефинансирования
                    }
                    if (application.WithInsurance)
                    {
                        var sumPure = _insurancePremiumCalculator.GetLoanCostWithoutInsurancePremium(rq.LoanCost);

                        if ((sumPure - refinanceAmount > (decimal)0.01 && sumPure - refinanceAmount <= 1000) || refinanceAmount > sumPure)
                        {
                            needChangeBySmallRefinance = true;
                        }
                    }
                    else
                    {
                        sumForClient = rq.LoanCost;
                        if ((sumForClient - refinanceAmount > (decimal)0.01 &&
                             sumForClient - refinanceAmount <= 1000) || refinanceAmount > sumForClient)
                        {
                            needChangeBySmallRefinance = true;
                        }
                    }
                }
                else
                {
                    impossibleСoncludeСontract = true;// Нельзя выдать рефинанс в рамках существующей кредитной линии 
                }
            }

            if (needChangeBySmallRefinance)
            {
                if (application.WithInsurance)
                {
                    var product = _loanPercentRepository.Get(application.SettingId);
                    var sumWithInsurance = GetInsuranceAmount(true, refinanceAmount, product);
                    rq.LoanCost = sumWithInsurance.Item1 + sumWithInsurance.Item2;
                    application.LoanCost = sumWithInsurance.Item1 + sumWithInsurance.Item2;
                }
                else
                {
                    rq.LoanCost = refinanceAmount;
                    application.LoanCost = refinanceAmount;
                }
            }

            application = rq.ToDomainModel(application);

            if (application.Position != null)
                application.Position.Car = await _onlineApplicationCarService.GetEntityForCreateAsync(application.Position.Car);


            _onlineApplicationService.Save(application);

            if (rq.ApprovedStage >= 3)
            {
                var product = _loanPercentRepository.Get(application.SettingId);
                var creditLineProduct = application.CreditLineSettingId.HasValue ?
                    _loanPercentRepository.Get(application.CreditLineSettingId.Value) : null;
                await SaveContract(application, client, product, creditLineProduct);
                _absOnlineService.SaveAdditionalInfo(application.ContractId.Value, string.Empty, application.BranchId);

                if (application.ContractId.HasValue)
                {
                    _absOnlineService.CreateInsurancePolicy(application.ContractId.Value);

                    var contract = _contractService.Get(application.ContractId.Value);

                    if (contract.Status != ContractStatus.Draft)
                    {
                        contract.Status = ContractStatus.Draft;
                        _contractService.Save(contract);
                    }
                }

                if (rq.ApprovedStage == 3)
                {
                    var loanCost = !application.WithInsurance ? application.LoanCost : _insurancePremiumCalculator.GetLoanCostWithoutInsurancePremium(application.LoanCost);
                    return Ok(GetCrmStateViewModel(application.ContractNumber, loanCost, product, application.WithInsurance, application.IsOpeningCreditLine ? null : application.CreditLineId, impossibleСoncludeСontract));
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Метод шины <b><u>check_contract_signature_sms</u></b>
        /// </summary>
        [HttpPost("applications/{contractnumber}/sign")]
        public async Task<IActionResult> SignApplication([FromServices] IContractActionSignService contractActionSignService,
            [FromServices] PayTypeRepository payTypeRepository, string contractnumber, [FromBody] SignApplicationRequest rq,
            [FromServices] PayOperationRepository _payOperationRepository,
            [FromServices] ICashOrderService _cashOrderService,
            [FromServices] ContractRepository _contractRepository,
            [FromServices] PayOperationActionRepository _payOperationActionRepository,
            [FromServices] IRefinanceBuyOutService refinanceBuyOutService
            )
        {
            rq.ContractNumber = contractnumber;
            var application = await _onlineApplicationService.FindByContractNumberAsync(rq.ContractNumber);

            if (application == null)
                return NotFound($"Заявка не найдена по контракту!: {rq.ContractNumber}.");

            if (!application.ContractId.HasValue)
                return BadRequest($"Займ по заявке {rq.ContractNumber} не найден.");

            if (application.Status == OnlineApplicationStatusType.Sign)
                return Ok();

            _absOnlineService.SaveAdditionalInfo(application.ContractId.Value, rq.SmsCode, application.BranchId.Value, application.PartnerCode);

            var client = _clientRepository.Get(application.ClientId);
            var branch = _groupRepository.Get(application.BranchId.Value);

            client.MobilePhone = rq.MobilePhone.IsNullOrEmpty(client.MobilePhone);
            client.Addresses.FirstOrDefault(x => x.IsActual && x.AddressTypeId == 5).ATEId = branch.ATEId;

            client.Documents.ForEach(x =>
            {
                if (x.TypeId == 1 || !x.ProviderId.HasValue)
                    _clientRepository.DeleteClientDocument(x.Id);
            });

            _clientRepository.Update(client);
            _onlineApplicationService.Save(application);

            UpdateClientMobilePhone(client, rq.MobilePhone);

            var clientRequisite = client.Requisites.FirstOrDefault(x => x.IsDefault);
            var payType = await payTypeRepository.GetByRequisiteType(clientRequisite.RequisiteTypeId);

            using (var transaction = _accountRepository.BeginTransaction())
            {
                // если это не открытие кредитной линии, то открытие транша или обычного займа
                // иначе открытие кредитной линии с первым траншем
                if (!application.IsOpeningCreditLine)
                {
                    var contract = _contractService.Get(application.ContractId.Value);
                    OpenAccountsAndBO(contractActionSignService, contract, clientRequisite.Id, payType.Id);
                }
                else
                {
                    var creditLine = _contractService.Get(application.CreditLineId.Value);

                    if (creditLine.Status != ContractStatus.Signed)
                    {
                        OpenAccountsAndBO(contractActionSignService, creditLine, clientRequisite.Id, payType.Id);

                        creditLine.SignDate = DateTime.Now;
                        creditLine.Status = ContractStatus.Signed;
                        _contractService.Save(creditLine);
                    }

                    var tranche = _contractService.Get(application.ContractId.Value);
                    OpenAccountsAndBO(contractActionSignService, tranche, clientRequisite.Id, payType.Id);
                }

                application.Status = OnlineApplicationStatusType.Sign;
                _onlineApplicationService.Save(application);

                _absOnlineService.DeleteInsuranceRecords(application.ContractId.Value);

                transaction.Commit();
            }


            if (application.OnlineApplicationRefinances.Count > 0)
            {
                decimal amount = await _refinanceService.CalculateRefinanceAmountForContract(application.ContractId.Value);

                var contract = _contractService.Get(application.ContractId.Value);

                decimal refSum;
                if (application.WithInsurance)
                    refSum = _insurancePremiumCalculator.GetLoanCostWithoutInsurancePremium(application.LoanCost);
                else
                    refSum = application.LoanCost;


                if (refSum - amount < (decimal)0.01)
                {
                    await _refinanceService.RefinanceAllAssociatedContracts(contract.Id);

                    #region ChangeStatusAndAddOnlinePayment

                    var payOperationId = _payOperationRepository.GetPayOperationByContractIdWithoutCashOrders(contract.Id);
                    var payOperation = _payOperationRepository.Get(payOperationId.Id);
                    PayOperationAction action = new PayOperationAction()
                    {
                        ActionType = PayOperationActionType.Execute,
                        AuthorId = Constants.ADMINISTRATOR_IDENTITY,
                        CreateDate = DateTime.Now,
                        Date = DateTime.Now,
                        OperationId = payOperation.Id

                    };

                    var policyResult = _absOnlineService.RegisterPolicy(contract.Id, contract);

                    if (!string.IsNullOrEmpty(policyResult))
                        _absOnlineService.SaveRetrySendInsurance(contract.Id);

                    ContractAction operactionAction = payOperation.Action;

                    if (payOperation.Action.ActionType == ContractActionType.Sign)
                    {
                        contract.SignDate = DateTime.Now;
                        contract.Status = ContractStatus.Signed;
                        payOperation.Status = PayOperationStatus.Executed;
                        operactionAction.Status = ContractActionStatus.Approved;
                        _contractActionService.Save(operactionAction);
                        _contractPaymentScheduleService.UpdateFirstPaymentInfo(contract.Id, contract);
                    }

                    var cashOrders = await _cashOrderService.GetAllRelatedOrdersByContractActionId(operactionAction.Id);

                    for (int i = 0; i < cashOrders.Count; i++)
                    {
                        CashOrder order = await _cashOrderService.GetAsync(cashOrders[i]);
                        if (order.OrderDate.Date != DateTime.Now.Date)
                            order.OrderDate = DateTime.Now;

                        order.ApproveStatus = OrderStatus.Approved;
                        _cashOrderService.Register(order, branch);
                    }
                    _payOperationRepository.Update(payOperation);
                    _contractRepository.Update(contract);
                    _payOperationActionRepository.Insert(action);
                    #endregion

                    await refinanceBuyOutService.BuyOutAllRefinancedContracts(contract.Id);
                }
            }

            return Ok();
        }



        [HttpGet("clients/{iin}/contracts/prepayment-balances")]
        public async Task<IActionResult> GetContracts(string iin)
        {
            var contractsRaw = await _contractService.GetListForOnlineByIinAsync(iin);

            if (contractsRaw == null)
                return NotFound($"Договора для клиента с ИИН : {iin} не найден");
            List<ContractsPrepaymentBalanceViewModel> contractsPrepaymentBalances = new List<ContractsPrepaymentBalanceViewModel>();
            foreach (var contract in contractsRaw)
            {
                var contractBalance = await _accountRepository.GetBalanceByContractIdAsync(contract.Id);
                contractsPrepaymentBalances.Add(new ContractsPrepaymentBalanceViewModel
                {
                    ContractNumber = contract.ContractNumber,
                    PrepaymentBalance = contractBalance.PrepaymentBalance
                });
            }
            return Ok(contractsPrepaymentBalances);
        }


        [HttpPost("contracts/movePrepayment")]
        public async Task<IActionResult> MovePrepayment(
            [FromServices] IContractActionPrepaymentService _contractActionPrepaymentService, [FromBody] ContractMovePrepaymentRequest request)
        {
            var sourceContract = await _contractService.GetNonCreditLineByNumberAsync(request.SourceContractNumber);
            var recipientContract = await _contractService.GetNonCreditLineByNumberAsync(request.RecipientContractNumber);

            if (sourceContract.ClientId != recipientContract.ClientId)
            {
                return BadRequest("Контракты относятся к разным пользователям");
            }

            decimal amount;

            if (!decimal.TryParse(request.Amount, out amount))
            {
                return BadRequest($"Не могу преобразовать строку {request.Amount} в число");
            }

            var branch = _groupRepository.Get(recipientContract.BranchId);

            var prepaymentModel = new MovePrepayment
            {
                SourceContractId = sourceContract.Id,
                Date = DateTime.Now.Date,
                Amount = amount,
                RecipientContractId = recipientContract.Id,
                Note = request.Note
            };

            var incompleteExists = await _contractActionService.IncopleteActionExists(sourceContract.Id);
            if (incompleteExists)
                return BadRequest("В контракте есть неподтвержденное действие");

            _contractActionPrepaymentService.MovePrepayment(prepaymentModel, 1, branch);
            return Ok(new ContractMovePrepaymentViewModel
            {
                Error = 0,
                Message = "Перемещение ДС проведено"
            });
        }


        [HttpPost("applications/todraft")]
        public async Task<IActionResult> ToDraft([FromBody] string applicationNumber)
        {
            var application = await _onlineApplicationService.FindByContractNumberAsync(applicationNumber);
            if (application.Status != OnlineApplicationStatusType.Sign)
                throw new PawnshopApplicationException(
                    $"Заявка не в статусе подписан. Текущий статус : {application.Status}");
            application.Status = OnlineApplicationStatusType.Draft;
            _onlineApplicationService.Save(application);
            return Ok();
        }

        [HttpGet("applications/{applicationnumber}/dataforresign")]
        public async Task<IActionResult> DataForResign([FromRoute] string applicationnumber)
        {
            var application = await _onlineApplicationService.FindByContractNumberAsync(applicationnumber);
            var client = _clientRepository.Get(application.ClientId);
            var smsCode = _absOnlineService.GetSmsCode(application.ContractId.Value);

            return Ok(new SignApplicationRequest
            {
                ContractNumber = applicationnumber,
                IIN = client.IdentityNumber,
                MobilePhone = client.MobilePhone,
                SmsCode = smsCode
            });
        }


        private LoanPercentSetting GetProductByStringId(string productIdStr)
        {
            if (!int.TryParse(productIdStr, out int productId))
                return null;

            try
            {
                var product = _loanPercentRepository.Get(productId);

                if (product.ContractClass == ContractClass.Tranche)
                    // TODO: add log error use tranche product
                    return null;

                return product;
            }
            catch
            {
                return null;
            }
        }

        private async Task<LoanPercentSetting> GetTrancheProduct(int creditLineProductId, bool insurance)
        {
            var childs = await _loanPercentRepository.GetChild(creditLineProductId);
            var child = childs.FirstOrDefault(x => x.IsActual == true && x.IsInsuranceAvailable == insurance);

            if (child != null)
                return _loanPercentRepository.Get(child.Id);

            return child;
        }

        private (decimal, decimal) GetInsuranceAmount(bool withInsurance, decimal loanCost, LoanPercentSetting product)
        {
            if (!withInsurance)
                return (loanCost, 0);

            var insurance = _insurancePremiumCalculator.GetInsuranceDataV2(loanCost, product.InsuranceCompanies.FirstOrDefault().InsuranceCompanyId, product.Id);

            return (insurance.Eds == 0 || loanCost > 3909999 ? loanCost : insurance.Eds, insurance.InsurancePremium);
        }

        private object CreateResponse(string iin, string contractNumber, decimal loanCost, LoanPercentSetting product, bool insurance)
        {
            var contracts = _contractService.GetListForOnlineByIinAsync(iin).Result;
            var lastContract = contracts.OrderByDescending(x => x.Id)
                .FirstOrDefault(x => x.Status == ContractStatus.Signed || x.Status == ContractStatus.SoldOut);

            var response = new
            {
                crmState = GetCrmStateViewModel(contractNumber, loanCost, product, insurance),
                suggestPayDay = lastContract == null ? null : new SuggestPayDayViewModel
                {
                    ContractNumber = contractNumber,
                    Day = lastContract.SignDate.HasValue ? lastContract.SignDate.Value.Day : lastContract.ContractDate.Day
                }
            };

            return response;
        }

        private CrmStateViewModel GetCrmStateViewModel(string contractNumber, decimal loanCost,
            LoanPercentSetting product, bool insurance, int? creditLineId = null,
            bool contractImpossible = false)
        {
            var insuranceResult = GetInsuranceAmount(insurance, loanCost, product);

            if (creditLineId.HasValue)
            {
                var creditLimit = _contractService.GetCreditLineLimit(creditLineId.Value).Result;
                creditLimit = Math.Truncate(creditLimit);

                if (creditLimit < insuranceResult.Item1 + insuranceResult.Item2)
                {
                    var loanCostWitoutInsurancePremium = _insurancePremiumCalculator.GetLoanCostWithoutInsurancePremium(creditLimit);
                    insuranceResult = GetInsuranceAmount(insurance, loanCostWitoutInsurancePremium, product);
                }
            }

            if (contractImpossible)
            {
                return new CrmStateViewModel
                {
                    ContractNumber = contractNumber,
                    From = "ABS",
                    Services = new List<ServiceViewModel>
                    {
                        new ServiceViewModel
                        {
                            Insurance = insurance,
                            LoanCost = 0,
                            Name = "Контракт невозможно заключить лимит кл < суммы рефинансирования",
                            PayServiceValue = 0,
                            PayTotalValue = 0,
                            Percent = 0,
                        }
                    }
                };
            }


            return new CrmStateViewModel
            {
                ContractNumber = contractNumber,
                From = "ABS",
                Services = new List<ServiceViewModel>
                    {
                        new ServiceViewModel
                        {
                            Insurance = insurance,
                            LoanCost = insuranceResult.Item1,
                            Name = "insurance",
                            PayServiceValue = insuranceResult.Item2,
                            PayTotalValue = insuranceResult.Item1 + insuranceResult.Item2,
                            Percent = Math.Round(product.LoanPercent*30 , 2),
                        }
                    }
            };
        }

        private void UpdateClientMobilePhone(Client client, string mobilePhone)
        {
            if (string.IsNullOrEmpty(mobilePhone))
                return;

            var clientContacts = _clientContactService.GetMobilePhoneContacts(client.Id);

            clientContacts.ForEach(x =>
            {
                x.IsDefault = false;
                _clientContactService.SaveWithoutChecks(x);
            });

            var clientContact = clientContacts?.FirstOrDefault(x => x.Address == mobilePhone);

            if (clientContact == null)
            {
                clientContact = new ClientContact
                {
                    Address = mobilePhone,
                    AuthorId = 1,
                    ClientId = client.Id,
                    ContactTypeId = 1,
                    CreateDate = DateTime.Now,
                    IsDefault = true,
                };
            }

            if (!clientContact.IsDefault)
                clientContact.IsDefault = true;

            if (!clientContact.VerificationExpireDate.HasValue || clientContact.VerificationExpireDate.Value < DateTime.Now)
                clientContact.VerificationExpireDate = DateTime.Now.AddDays(VEFIFICATION_PERIOD);

            _clientContactService.SaveWithoutChecks(clientContact);
        }

        private bool ContractCanEdit(int? contractId, bool isChangePayDay = false)
        {
            if (contractId.HasValue)
            {
                var contract = _contractService.GetOnlyContract(contractId.Value, true);

                if (contract != null && (int)contract.Status > 20 && (int)contract.Status != 27)
                    return false;

                if (isChangePayDay && contract.ContractClass == ContractClass.Tranche)
                {
                    var tranches = _contractService.GetActiveTranchesCount(contract.CreditLineId.Value).Result;

                    if (tranches > 0)
                        return false;
                }
            }

            return true;
        }

        private async Task SaveContract(OnlineApplication application, Client client, LoanPercentSetting product, LoanPercentSetting creditLineProduct)
        {
            var tsoBranch = _groupRepository.Find(new { Name = "TSO" });

            using (var transaction = _accountRepository.BeginTransaction())
            {
                var periodTypeId = 0;

                if (Math.Abs((DateTime.Now - application.MaturityDate).Days) <= (int)PeriodType.Year)
                {
                    periodTypeId = _typeRepository.FindAsync(new { Code = Constants.PERIOD_TYPE_TERMS_SHORT }).Result.Id;
                }
                else
                {
                    periodTypeId = _typeRepository.FindAsync(new { Code = Constants.PERIOD_TYPE_TERMS_LONG }).Result.Id;
                }

                if (!application.IsOpeningCreditLine)
                {
                    var contract = application.ContractId.HasValue ?
                        _contractService.Get(application.ContractId.Value) : null;

                    if (product.ContractClass == ContractClass.Credit)
                        contract = application.ToContractDomainModel(contract, product, client, periodTypeId, tsoBranch.Id, SaveCar(application, client.Id));
                    else
                        contract = application.ToContractDomainModel(contract, product, client, periodTypeId, tsoBranch.Id, 0, application.CreditLineId);

                    await SetAnyContractParameters(contract);
                    SetContractChecks(contract);
                    _contractService.Save(contract);

                    application.ContractId = contract.Id;

                    if (_contractPaymentScheduleService.IsNeedUpdatePaymentSchedule(contract.PaymentSchedule, contract.Id))
                        _contractPaymentScheduleService.Save(contract.PaymentSchedule, contract.Id, 1);
                }
                else
                {
                    var cretiLine = application.CreditLineId.HasValue ?
                        _contractService.Get(application.CreditLineId.Value) : null;

                    cretiLine = application.ToContractDomainModel(cretiLine, creditLineProduct, client, periodTypeId, tsoBranch.Id, SaveCar(application, client.Id));
                    SetContractChecks(cretiLine);
                    _contractService.Save(cretiLine);

                    var tranche = application.ContractId.HasValue ?
                        _contractService.Get(application.ContractId.Value) : null;

                    tranche = application.ToContractDomainModel(tranche, product, client, periodTypeId, tsoBranch.Id, 0, cretiLine.Id);

                    await SetAnyContractParameters(tranche, application.IsOpeningCreditLine);
                    SetContractChecks(tranche);
                    _contractService.Save(tranche);

                    if (_contractPaymentScheduleService.IsNeedUpdatePaymentSchedule(tranche.PaymentSchedule, tranche.Id))
                        _contractPaymentScheduleService.Save(tranche.PaymentSchedule, tranche.Id, 1);

                    application.ContractId = tranche.Id;
                    application.CreditLineId = cretiLine.Id;
                }

                _onlineApplicationService.Save(application);
                transaction.Commit();
            }
        }

        private int SaveCar(OnlineApplication application, int clientId)
        {
            var car = _carService.Find(new { application.Position.Car.TechPassportNumber });
            car = application.Position.Car.ToCarDomainModel(car, clientId);
            _carService.Save(car);
            return car.Id;
        }

        private async Task SetAnyContractParameters(Contract contract, bool isFirstTranche = false)
        {
            _paymentScheduleService.BuildWithContract(contract);

            if (!contract.FirstPaymentDate.HasValue && isFirstTranche)
                contract.FirstPaymentDate = contract.PaymentSchedule.FirstOrDefault().Date;

            contract.NextPaymentDate = contract.PaymentSchedule.FirstOrDefault().Date;
            contract.AnnuityType = _contractService.GetAnnuityType(contract);
            await _contractService.CalculateAPR(contract);
        }

        private void OpenAccountsAndBO(IContractActionSignService contractActionSignService, Contract contract, int requisiteId, int payTypeId)
        {
            var onlineApplication = _onlineApplicationRepository.FindByContractIdAsync(new { ContractId = contract.Id.ToString() }).Result;

            decimal amount = 0;

            if (contract.ContractClass == ContractClass.Tranche && onlineApplication.OnlineApplicationRefinances.Count > 0)
            {
                amount = _refinanceService.CalculateRefinanceAmountForContract(onlineApplication.ContractId.Value).Result;
            }

            if (contract.ContractClass != ContractClass.CreditLine)
                _absOnlineService.CreateInsurancePolicy(contract.Id);

            ContractDutyCheckModel checkModel = new ContractDutyCheckModel
            {
                ActionType = ContractActionType.Sign,
                ContractId = contract.Id,
                Cost = contract.LoanCost,// in test use amount check this 
                Date = contract.ContractDate,
                PayTypeId = payTypeId,
                Refinance = amount
            };
            var contractDuty = _contractDutyService.GetContractDuty(checkModel);

            var action = new ContractAction
            {
                ActionType = ContractActionType.Sign,
                Checks = contractDuty.Checks.Select((x, i) => new ContractActionCheckValue { Check = x, CheckId = x.Id, Value = true }).ToList(),
                ContractId = contract.Id,
                Date = contractDuty.Date,
                Discount = contractDuty.Discount,
                Expense = contractDuty.ExtraContractExpenses?.FirstOrDefault(),
                ExtraExpensesCost = contractDuty.ExtraExpensesCost,
                PayTypeId = payTypeId,
                Reason = contractDuty.Reason,
                RequisiteCost = contract.ContractClass == ContractClass.CreditLine ? (int?)0 : null,
                RequisiteId = requisiteId,
                Rows = contractDuty.Rows.ToArray(),
                TotalCost = contractDuty.Cost,
            };

            contractActionSignService.Exec(action, 1, contract.BranchId, false, ignoreCheckQuestionnaireFilledStatus: true,
                orderStatus: contract.ContractClass == ContractClass.CreditLine ? (OrderStatus?)OrderStatus.Approved : null);
        }

        private void SetContractChecks(Contract contract)
        {
            if (contract.Id != 0)
                contract.Checks = _contractCheckValueRepository.List(new ListQuery(), new { ContractId = contract.Id });

            if (contract.Checks.Any())
                return;

            var contractChecks = _contractCheckRepository.List(new ListQuery() { Page = null });

            contract.Checks.AddRange(contractChecks.Select(c => new ContractCheckValue()
            {
                AuthorId = Constants.ADMINISTRATOR_IDENTITY,
                CreateDate = DateTime.Now,
                BeginDate = c.PeriodRequired ? contract.ContractDate : default,
                EndDate = c.PeriodRequired ? contract.ContractDate.AddYears(c.DefaultPeriodAddedInYears ?? 0) : default,
                Value = true,
                CheckId = c.Id,
                Check = c
            }).ToList());
        }
    }
}
