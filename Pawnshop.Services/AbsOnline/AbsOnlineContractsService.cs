using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.AbsOnline;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Contracts;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace Pawnshop.Services.AbsOnline
{
    /// <summary>
    /// Реализация интерфейса обработки запросов по контрактам для AbsOnline
    /// </summary>
    public class AbsOnlineContractsService : IAbsOnlineContractsService
    {
        private readonly AccountRepository _accountRepository;
        private readonly CarRepository _carRepository;
        private readonly IContractPaymentScheduleService _contractPaymentScheduleService;
        private readonly IContractService _contractService;
        private readonly GroupRepository _groupRepository;
        private readonly InscriptionRepository _inscriptionRepository;
        private readonly LoanPercentRepository _loanPercentRepository;
        private readonly RealtyRepository _realtyRepository;

        public AbsOnlineContractsService(
            AccountRepository accountRepository,
            CarRepository carRepository,
            IContractPaymentScheduleService contractPaymentScheduleService,
            IContractService contractService,
            GroupRepository groupRepository,
            InscriptionRepository inscriptionRepository,
            LoanPercentRepository loanPercentRepository,
            RealtyRepository realtyRepository
            )
        {
            _accountRepository = accountRepository;
            _carRepository = carRepository;
            _contractPaymentScheduleService = contractPaymentScheduleService;
            _contractService = contractService;
            _groupRepository = groupRepository;
            _inscriptionRepository = inscriptionRepository;
            _loanPercentRepository = loanPercentRepository;
            _realtyRepository = realtyRepository;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Contract>> GetContractsByIdentityNumberAsync(string iin)
        {
            return await _contractService.FindListByIdentityNumberAsync(iin);
        }

        /// <inheritdoc />
        public List<AbsOnlineContractView> GetContractViewListAsync(IEnumerable<Contract> contracts)
        {
            var response = new List<AbsOnlineContractView>();
            var tasks = new List<Task>();

            foreach (var contract in contracts.Where(x => x.ContractClass != ContractClass.CreditLine))
            {
                tasks.Add(Task.Run(async () =>
                {
                    var hasPartialPayment = await _contractService.HasPartialPaymentAsync(contract.Id);
                    var balance = await _accountRepository.GetBalanceByContractIdAsync(contract.Id);
                    var paymentScheduleList = _contractPaymentScheduleService.GetListByContractId(contract.Id, true);
                    var nextPaymentAmount = contract.NextPaymentDate?.Date <= DateTime.Now.Date ? balance.CurrentDebt : 0;
                    var branch = _groupRepository.Get(contract.BranchId);
                    var car = await _carRepository.GetByContractIdAsync(contract.CreditLineId ?? contract.Id);
                    var realty = await _realtyRepository.GetByContractIdAsync(contract.CreditLineId ?? contract.Id);
                    var currentPayment = paymentScheduleList?.OrderBy(x => x.Date)
                        .FirstOrDefault(x => !x.ActionId.HasValue && !x.ActualDate.HasValue && x.Date > DateTime.Now);
                    var creditLine = contract.CreditLineId.HasValue ? _contractService.GetOnlyContract(contract.CreditLineId.Value) : null;

                    var item = new AbsOnlineContractView
                    {
                        AccountBalance = balance?.PrepaymentBalance ?? 0,
                        Balance = balance?.PrepaymentBalance ?? 0,
                        BranchCode = branch.Name,
                        BranchName = branch.DisplayName,
                        Car = car != null ? $"{car.Mark} {car.Model}" : string.Empty,
                        ContractNumber = contract.ContractNumber,
                        ContractType = contract.ContractClass switch
                        {
                            ContractClass.Tranche => "tranche",
                            _ => "simple"
                        },
                        CreditLineId = creditLine?.Id,
                        CreditLineNumber = creditLine?.ContractNumber,
                        DateClose = contract.MaturityDate.Date,
                        DateOpen = contract.ContractDate.Date,
                        ExpiredPaymentsCount = paymentScheduleList.Count(x => x.Date.Date < DateTime.Now.Date && !x.ActionId.HasValue && !x.ActualDate.HasValue),
                        HasPartialPayment = hasPartialPayment,
                        Id = contract.Id,
                        LoanCost = contract.LoanCost,
                        NextPaymentDate = contract.NextPaymentDate?.Date,
                        PaidPaymentsCount = paymentScheduleList.Count(x => x.ActionId.HasValue && x.ActualDate.HasValue),
                        PaymentsCount = paymentScheduleList.Count(),
                        Percent = Math.Round(contract.LoanPercent * 30, 2),
                        PositionType = car?.CollateralType.ToString() ?? realty?.CollateralType.ToString(),
                    };

                    if (nextPaymentAmount > 0 || item.NextPaymentDate <= DateTime.Now.Date)
                    {
                        item.NextPaymentAmount = nextPaymentAmount;
                        item.NextPaymentDate = DateTime.Now.Date;
                    }
                    else if (currentPayment != null)
                    {
                        item.NextPaymentAmount = currentPayment.DebtCost + currentPayment.PercentCost + (balance?.PenyAmount ?? 0);
                    }

                    response.Add(item);
                }));
            }

            Task.WaitAll(tasks.ToArray());
            return response;
        }

        /// <inheritdoc />
        public async Task<CreditLineParkingInscriptionStatusView> GetCreditLineParkingInscriptionStatusAsync(string creditLineNumber)
        {
            var response = new CreditLineParkingInscriptionStatusView();

            var creditLine = await _contractService.GetCreditLineByNumberAsync(creditLineNumber);

            if (creditLine == null)
                return response;

            response.CarStatus = await _carRepository.GetCarStatus(creditLine.Id);
            response.CarStatus.ContractNumber = creditLine.ContractNumber;
            response.CarStatus.Id = creditLine.Id;

            var tranches = await _contractService.GetAllSignedTranches(creditLine.Id);

            foreach (var tranche in tranches)
            {
                var inscriptions = await _inscriptionRepository.GetByContractId(tranche.Id);

                response.TrancheInscriptionStatusList.AddRange(inscriptions.Select(x =>
                    new TrancheInscriptionStatus
                    {
                        Id = tranche.Id,
                        ContractNumber = tranche.ContractNumber,
                        InscriptionStatus = x.Status.GetDisplayName(),
                        InscriptionStatusCode = x.Status.ToString(),
                        UpdateDate = x.CreateDate
                    }));
            }

            return response;
        }

        /// <inheritdoc />
        public List<AbsOnlineCreditLineView> GetCreditLineViewListAsync(IEnumerable<Contract> contracts)
        {
            var response = new List<AbsOnlineCreditLineView>();
            var tasks = new List<Task>();

            foreach (var creditLine in contracts.Where(x => x.ContractClass == ContractClass.CreditLine && x.Status == ContractStatus.Signed))
            {
                tasks.Add(Task.Run(async () =>
                {
                    var product = await _loanPercentRepository.GetOnlyAsync(creditLine.SettingId.Value);
                    var creditLineLimit = await _contractService.GetCreditLineLimit(creditLine.Id);

                    // TODO: временно не требуется
                    //if (product?.LoanCostFrom > creditLineLimit)
                    //    return;

                    var branch = _groupRepository.Get(creditLine.BranchId);
                    var car = await _carRepository.GetByContractIdAsync(creditLine.Id);
                    var balance = await _accountRepository.GetBalanceByContractIdAsync(creditLine.Id);

                    var item = new AbsOnlineCreditLineView
                    {
                        BranchCode = branch.Name,
                        BranchName = branch.DisplayName,
                        CarBrand = car.Mark,
                        CarModel = car.Model,
                        CarNumber = car.TransportNumber,
                        CarVin = car.BodyNumber,
                        ContractNumber = creditLine.ContractNumber,
                        CreditLineLimit = creditLine.LoanCost,
                        EndDate = creditLine.MaturityDate.Date,
                        Id = creditLine.Id,
                        //PenyAmount = balance.ExpenseAmount, // TODO: maybe active
                        PrepaymentBalance = balance.PrepaymentBalance,
                        ProductCode = product?.Id.ToString(),
                        ProductName = product?.Name,
                        RemainingAmount = Math.Truncate(creditLineLimit),
                        StartDate = creditLine.SignDate.Value,
                    };

                    var tranches = contracts.Where(c => c.CreditLineId == creditLine.Id);

                    if (tranches.Any())
                    {
                        var minPaymentDate = tranches.Min(t => t.NextPaymentDate);

                        if (minPaymentDate != null && minPaymentDate < DateTime.Now)
                            item.PaymentExpiredDays = Math.Abs((DateTime.Now.Date - minPaymentDate.Value).Days);

                        var tranchesBalance = _contractService.GetBalances(tranches.Select(x => x.Id).ToList());

                        item.PrincipalDebt = tranchesBalance?.Sum(x => x.AccountAmount) ?? 0;
                        item.RedemptionAmount = tranchesBalance?.Sum(x => x.TotalRedemptionAmount) ?? 0;
                        item.PenyAmount += tranchesBalance.Sum(x => x.PenyAmount);
                        item.ProfitAmount = tranchesBalance.Sum(x => x.ProfitAmount);
                    }

                    response.Add(item);
                }));
            }

            Task.WaitAll(tasks.ToArray());
            return response;
        }

        /// <inheritdoc />
        public AbsOnlineContractList GetViewListAsync(IEnumerable<Contract> contracts)
        {
            var response = new AbsOnlineContractList();

            if (contracts == null || !contracts.Any())
                return response;

            response.Contracts.AddRange(GetContractViewListAsync(contracts));
            response.CreditLines.AddRange(GetCreditLineViewListAsync(contracts));

            return response;
        }

        /// <inheritdoc />
        public async Task<MobileMainScreenView> GetMobileMainScreenView(string iin)
        {
            var contractsRaw = await _contractService.FindListByIdentityNumberAsync(iin);
            var balanceList = _contractService.GetBalances(contractsRaw.Where(x => x.CreatedInOnline).Select(x => x.Id).ToList());
            var cars = await _carRepository.GetListByContractIdsAsync(contractsRaw.Select(x => x.Id).ToList());
            var tasks = new List<Task>();

            #region fill credit lines
            var creditLinesView = new ConcurrentBag<AbsOnlineCreditLineViewMobileMainScreen>();
            var creditLines = contractsRaw.Where(x => x.CreatedInOnline && x.ContractClass == ContractClass.CreditLine);
            var creditLinesProductIds = creditLines.Where(x => x.SettingId.HasValue).Select(x => x.SettingId.Value).Distinct();
            var products = creditLinesProductIds.Select(x => _loanPercentRepository.GetOnlyAsync(x).Result);

            foreach (var creditLine in creditLines)
            {
                tasks.Add(Task.Run(async () =>
                {
                    if (!creditLine.SettingId.HasValue)
                        return;

                    var product = products.FirstOrDefault(x => x.Id == creditLine.SettingId.Value);

                    if (product == null || creditLine.MaturityDate < DateTime.Today.AddMonths(Constants.CREDIT_LINE_MIN_PERIOD_ONLINE))
                        return;

                    var creditLimit = await _contractService.GetCreditLineLimit(creditLine.Id);

                    if (creditLimit < product.LoanCostFrom)
                        return;

                    var car = cars.FirstOrDefault(x => x.ContractId == creditLine.Id);

                    creditLinesView.Add(new AbsOnlineCreditLineViewMobileMainScreen
                    {
                        ContractNumber = creditLine.ContractNumber,
                        EndDate = creditLine.MaturityDate.Date,
                        RemainingAmount = Math.Truncate(creditLimit),
                        CarNumber = car.TransportNumber,
                        CarVin = car.BodyNumber,
                    });
                }));
            }
            #endregion

            #region fill contracts
            var contractsView = new List<AbsOnlineContractMobileView>();
            var contracts = contractsRaw
                .Where(x => x.CreatedInOnline && x.ContractClass != ContractClass.CreditLine);

            var creditLinesToContracts = creditLines
                .Where(x => contracts.Any(c => c.ContractNumber == x.ContractNumber))
                .Select(x => new { contract = x, isNeedMasked = true, car = cars.FirstOrDefault(c => c.ContractId == x.Id) })
                .ToList();

            creditLinesToContracts
                .AddRange(creditLines
                    .Where(x => !creditLinesToContracts.Any(c => c.contract.Id == x.Id))
                    .Select(x => new { contract = x, isNeedMasked = false, car = cars.FirstOrDefault(c => c.ContractId == x.Id) }));

            contracts
                .GroupJoin(cars,
                    contract => contract.CreditLineId ?? contract.Id,
                    car => car.ContractId,
                    (contract, car) => new { contract, car }).ToList()
                .ForEach(item =>
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        var hasPartialPayment = await _contractService.HasPartialPaymentAsync(item.contract.Id);
                        var balance = balanceList
                            .FirstOrDefault(b => b.ContractId == item.contract.Id);
                        var paymentScheduleList = _contractPaymentScheduleService.GetListByContractId(item.contract.Id, true);

                        contractsView.Add(ToShortViewModel(
                            item.contract,
                            item.car.FirstOrDefault(),
                            hasPartialPayment,
                            balance,
                            paymentScheduleList
                        ));
                    }));
                });

            #endregion

            Task.WaitAll(tasks.ToArray());
            tasks.Clear();

            #region credit line to contract view model

            creditLinesToContracts
                .ToList()
                .ForEach(item =>
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        await _contractService.CreditLineFillConsolidateSchedule(item.contract, false);
                        var hasPartialPayment = await _contractService.HasPartialPaymentAsync(item.contract.Id);
                        var balanceContractIds = contracts
                            .Where(t => t.CreditLineId == item.contract.Id)
                            .Select(x => x.Id)
                            .ToList();

                        balanceContractIds.Add(item.contract.Id);

                        var creditLineBalanceList = balanceList
                            .Where(b => balanceContractIds.Contains(b.ContractId));

                        var balance = GetCreditLineTotalBalance(item.contract.Id, creditLineBalanceList);

                        contractsView.Add(ToShortViewModel(
                            item.contract,
                            item.car,
                            hasPartialPayment,
                            balance,
                            item.contract.PaymentSchedule,
                            item.isNeedMasked
                        ));
                    }));
                });

            Task.WaitAll(tasks.ToArray());

            #endregion

            return new MobileMainScreenView
            {
                CreditLines = creditLinesView.OrderBy(x => x.ContractNumber),
                Contracts = contractsView.OrderBy(x => x.ContractType).ThenBy(x => x.DateOpen)
            };
        }

        /// <inheritdoc />
        public async Task<AbsOnlineContractMobileView> GetMobileContractData(string contractNumber)
        {
            Contract contract;

            if (contractNumber.Contains("CL"))
                contract = await _contractService.GetCreditLineByNumberAsync(contractNumber.Replace("CL", string.Empty));
            else
                contract = await _contractService.GetNonCreditLineByNumberAsync(contractNumber);

            if (contract == null || (int)contract.Status > 30)
                contract = await _contractService.GetCreditLineByNumberAsync(contractNumber);

            if (contract == null)
                return null;

            var car = await _carRepository.GetByContractIdAsync(contract.CreditLineId ?? contract.Id);

            if (contract.ContractClass == ContractClass.CreditLine)
            {
                await _contractService.CreditLineFillConsolidateSchedule(contract, false);
                var tranches = await _contractService.GetAllSignedTranches(contract.Id);
                var balanceContractIds = tranches
                    .Where(x => x.CreatedInOnline)
                    .Select(x => x.Id)
                    .ToList();

                balanceContractIds.Add(contract.Id);

                var balances = _contractService.GetBalances(balanceContractIds);
                var creditLineBalanceList = GetCreditLineTotalBalance(contract.Id, balances);

                return ToViewModel(contract, car, false, creditLineBalanceList, contract.PaymentSchedule, contractNumber.Contains("CL"));
            }

            var schedulePayments = _contractPaymentScheduleService.GetListByContractId(contract.Id, true);
            var hasPartialPayment = await _contractService.HasPartialPaymentAsync(contract.Id);
            var balance = await _accountRepository.GetBalanceByContractIdAsync(contract.Id);

            return ToViewModel(contract, car, hasPartialPayment, balance, schedulePayments);
        }


        private AbsOnlineContractMobileView ToViewModel(
            Contract contract,
            Car car,
            bool hasPartialPayment,
            ContractBalance balance,
            List<ContractPaymentSchedule> paymentsSchedule,
            bool isNeedMasked = false
            )
        {
            var viewModel = ToShortViewModel(contract, car, hasPartialPayment, balance, paymentsSchedule, isNeedMasked);

            viewModel.DebtCurrent = balance?.CurrentDebt ?? 0;

            viewModel.OverdueAndUpcomingPayments = paymentsSchedule
                .Where(x => x.Date.Date < DateTime.Now.Date && !x.ActionId.HasValue && !x.ActualDate.HasValue)
                .Select(x => new AbsOnlinePaymentScheduleViewModel
                {
                    Date = x.Date,
                    Amount = x.DebtCost + x.PercentCost,
                    PrincipalDebt = x.DebtCost,
                    Percent = viewModel.Percent,
                    ProfitAmount = x.PercentCost,
                    Status = "Просроченный",
                    FineAmount = balance?.PenyAmount
                })
                .ToList();

            viewModel.OverdueAndUpcomingPayments.AddRange(paymentsSchedule
                .Where(x => x.Date.Date >= DateTime.Now.Date && !x.ActionId.HasValue && !x.ActualDate.HasValue)
                .Select(x => new AbsOnlinePaymentScheduleViewModel
                {
                    Date = x.Date,
                    Amount = x.DebtCost + x.PercentCost,
                    PrincipalDebt = x.DebtCost,
                    Percent = viewModel.Percent,
                    ProfitAmount = x.PercentCost,
                    Status = "Предстоящий"
                }));

            return viewModel;
        }

        private AbsOnlineContractMobileView ToShortViewModel(
            Contract contract,
            Car car,
            bool hasPartialPayment,
            ContractBalance balance,
            List<ContractPaymentSchedule> paymentsSchedule,
            bool isNeedMasked = false
            )
        {
            var viewModel = new AbsOnlineContractMobileView
            {
                ContractNumber = contract.ContractNumber,
                DateOpen = contract.ContractDate.Date,
                DateClose = contract.MaturityDate.Date,
                LoanCost = contract.LoanCost,
                Car = car != null ? $"{car.Mark} {car.Model}" : string.Empty,
                CarNumber = car?.TransportNumber,
                Percent = Math.Round(contract.LoanPercent * 30, 2),
                HasPartialPayment = hasPartialPayment,
                ContractType = GetContractTypeName(contract.ContractClass),
                PrincipalDebt = balance?.AccountAmount + balance?.OverdueAccountAmount,
                Profit = balance?.ProfitAmount + balance?.OverdueProfitAmount,
                Penalty = balance?.PenyAmount,
                NextPaymentDate = contract.NextPaymentDate?.Date ?? new DateTime(1, 1, 1),
                AccountBalance = balance?.PrepaymentBalance ?? 0,
                Balance = balance?.PrepaymentBalance ?? 0,
                PaidPaymentsCount = paymentsSchedule.Count(x => x.ActionId.HasValue && x.ActualDate.HasValue),
                ExpiredPaymentsCount = paymentsSchedule.Count(x => x.Date.Date < DateTime.Now.Date && !x.ActionId.HasValue && !x.ActualDate.HasValue),
                PaymentsCount = paymentsSchedule.Count(),
                ProductCode = contract.Setting?.Id.ToString(),
                ProductName = contract.Setting?.Name,
            };

            if (isNeedMasked)
            {
                viewModel.ContractNumber = $"CL{viewModel.ContractNumber}";
                viewModel.Car = $"(СОКЛ) {viewModel.Car}";
            }

            var lastPaymentToPay = paymentsSchedule?
                .OrderBy(x => x.Date)
                .FirstOrDefault(x => !x.ActionId.HasValue && !x.ActualDate.HasValue);
            var upcomingPayment = paymentsSchedule?
                .OrderBy(x => x.Date)
                .FirstOrDefault(x => !x.ActionId.HasValue && !x.ActualDate.HasValue && x.Date.Date >= DateTime.Now.Date);

            if (lastPaymentToPay != null && lastPaymentToPay.Date < DateTime.Today)
            {
                viewModel.NextPaymentDate = DateTime.Today;
                viewModel.NextPaymentAmount = balance?.CurrentDebt ?? 0;
            }
            else if (upcomingPayment != null)
            {
                if (upcomingPayment.Date > DateTime.Today)
                {
                    viewModel.NextPaymentDate = upcomingPayment.Date;
                    viewModel.NextPaymentAmount = upcomingPayment.DebtCost + upcomingPayment.PercentCost + (balance?.PenyAmount ?? 0);
                }
                else
                {
                    viewModel.NextPaymentDate = DateTime.Today;
                    viewModel.NextPaymentAmount = balance?.CurrentDebt ?? 0;
                }
            }

            return viewModel;
        }

        private string GetContractTypeName(ContractClass contractClass)
        {
            switch (contractClass)
            {
                case ContractClass.Credit:
                    return "simple";
                case ContractClass.CreditLine:
                    return "credit_line";
                case ContractClass.Tranche:
                    return "tranche";
                default:
                    return "simple";
            }
        }

        private ContractBalance GetCreditLineTotalBalance(int creditLineId, IEnumerable<ContractBalance> balances)
        {
            return new ContractBalance
            {
                ContractId = creditLineId,
                AccountAmount = balances.Sum(s => s.AccountAmount),
                ProfitAmount = balances.Sum(s => s.ProfitAmount),
                OverdueAccountAmount = balances.Sum(s => s.OverdueAccountAmount),
                OverdueProfitAmount = balances.Sum(s => s.OverdueProfitAmount),
                PenyAmount = balances.Sum(s => s.PenyAmount),
                TotalAcountAmount = balances.Sum(s => s.TotalAcountAmount),
                TotalProfitAmount = balances.Sum(s => s.TotalProfitAmount),
                ExpenseAmount = balances.Sum(s => s.ExpenseAmount),
                PrepaymentBalance = balances.FirstOrDefault(s => s.ContractId == creditLineId)?.PrepaymentBalance ?? 0,
                CurrentDebt = balances.Sum(s => s.CurrentDebt),
                TotalRepaymentAmount = balances.Sum(s => s.TotalRepaymentAmount),
                TotalRedemptionAmount = balances.Sum(s => s.TotalRedemptionAmount),
            };
        }
    }
}
