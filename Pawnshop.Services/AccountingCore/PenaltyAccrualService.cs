using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Access.AccountingCore;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Penalty;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.ClientDeferments.Interfaces;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.AccountingCore
{
    public class PenaltyAccrualService : IPenaltyAccrual
    {
        private readonly IDictionaryWithSearchService<Holiday, HolidayFilter> _holydayService;
        private readonly IAccountService _accountService;
        private readonly IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> _accountSettingService;
        private readonly IBusinessOperationService _businessOperationService;
        private readonly IDictionaryWithSearchService<Group, BranchFilter> _branchService;
        private readonly IDictionaryWithSearchService<Blackout, BlackoutFilter> _blackoutService;
        private readonly IEventLog _eventLog;
        private List<AccrualBase> _accrualSetting;
        private readonly IContractActionService _contractActionService;
        private readonly IContractActionOperationService _contractActionOperationService;
        private readonly CashOrderRepository _cashOrderRepository;
        private readonly IBusinessOperationSettingService _businessOperationSettingService;
        private readonly PenaltyAccrualRepository _penaltyAccrualRespoitory;
        private readonly IContractService _contractService;
        private readonly IClientDefermentService _clientDefermentService;

        public PenaltyAccrualService(IDictionaryWithSearchService<Holiday, HolidayFilter> holydayService,
            IAccountService accountService,
            IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> accountSettingService,
            IBusinessOperationService businessOperationService,
            IDictionaryWithSearchService<Group, BranchFilter> branchService,
            IDictionaryWithSearchService<Blackout, BlackoutFilter> blackoutService,
            IDictionaryWithSearchService<AccrualBase, AccrualBaseFilter> accrualBaseService,
            IEventLog eventLog, CashOrderRepository cashOrderRepository,
            IContractActionService contractActionService,
            IContractActionOperationService contractActionOperationService,
            IBusinessOperationSettingService businessOperationSettingService,
            PenaltyAccrualRepository penaltyAccrualRepository,
            IContractService contractService,
            IClientDefermentService clientDefermentService)
        {
            _holydayService = holydayService;
            _accountService = accountService;
            _accountSettingService = accountSettingService;
            _businessOperationService = businessOperationService;
            _branchService = branchService;
            _blackoutService = blackoutService;
            _eventLog = eventLog;
            _contractActionService = contractActionService;
            _contractActionOperationService = contractActionOperationService;
            _accrualSetting = accrualBaseService.List(new ListQueryModel<AccrualBaseFilter> { Page = null, Model = new AccrualBaseFilter { AccrualType = AccrualType.PenaltyAccrual, IsActive = true } }).List;
            _cashOrderRepository = cashOrderRepository;
            _businessOperationSettingService = businessOperationSettingService;
            _penaltyAccrualRespoitory = penaltyAccrualRepository;
            _contractService = contractService;
            _clientDefermentService = clientDefermentService;
        }
        /*
1. В автомате по начислению пени для каждого договора просматриваешь все настройки с типом начисления = 10(AccrualType.PenaltyAccrual)
2. Для каждой найденной строки из пункта 1, находишь счет с BaseSettingId на договоре, по этому счету формируешь выписку для начисления пени через  GetRecordsForPenaltyAccrual
3. Рассчитываешь пеню по сформированной выписке
4. Сверяешь с фактическими начислениями
5. Если есть разница, которую нужно начислить, то находишь БО с кодом "PENALTY."+ BaseSettingId.Code
6. Зовешь метод, который формирует проводку
         */

        public void Execute(IContract contract, DateTime accrualDate, int authorId, IPaymentScheduleItem? lastScheduleItem = null,
            DateTime? startDate = null)
        {
            if (contract == null) throw new ArgumentNullException(nameof(contract), "Договор не найден");
            
            var defermentInformation = _clientDefermentService.GetDefermentInformation(contract.Id, accrualDate);
            if (defermentInformation != null &&
                ((defermentInformation.IsInDefermentPeriod && defermentInformation.Status == Data.Models.Restructuring.RestructuringStatusEnum.Restructured) ||
                defermentInformation.Status == Data.Models.Restructuring.RestructuringStatusEnum.Frozen))
            {
                return;
            }

            IDictionary<AmountType, decimal> amounts = new ConcurrentDictionary<AmountType, decimal>();
            IDictionary<AmountType, (decimal, string)> amountsBlackout = new ConcurrentDictionary<AmountType, (decimal, string)>();

            var branch = _branchService.GetAsync(contract.BranchId).Result;

            List<Account> accounts = _accountService.List(new ListQueryModel<AccountFilter>
            {
                Page = null,
                Model = new AccountFilter
                {
                    ContractId = contract.Id
                }
            }).List;

            (bool, Blackout) blackoutCheck = IsBlackoutDay(accrualDate);

            foreach (var accrualSetting in _accrualSetting)
            {
                List<Account> overdueAccounts = accounts.Where(x => x.AccountSettingId == accrualSetting.BaseSettingId).ToList();
                foreach (var account in overdueAccounts)
                {
                    AccountSetting accountSetting = _accountSettingService.GetAsync(accrualSetting.BaseSettingId).Result;
                    if (accountSetting == null)
                        throw new PawnshopApplicationException($"Настройка счетов {accrualSetting.BaseSettingId} не найдена");

                    IEnumerable<PenaltyRates> balances = CalculateForPenaltyAccrual((Contract)contract, accrualSetting, account, accrualDate);

                    Queue<PenaltyRates> balanceQueue = new Queue<PenaltyRates>(balances);
                    if (balances.Any())
                    {
                        var previousCalcPenalty = balanceQueue.Dequeue();
                        DateTime previousDate = previousCalcPenalty.Date;
                        
                        decimal accrualSum = 0;
                        while (balanceQueue.Count > 0)
                        {
                            var calcPenalty = balanceQueue.Dequeue();
                            if (calcPenalty.Date < previousDate)
                                throw new PawnshopApplicationException($"{nameof(calcPenalty.Date)}({calcPenalty.Date}) должен быть больше чем {nameof(previousDate)}({previousDate})");

                            int differenceInDays = (calcPenalty.Date - previousDate).Days;
                            decimal accrualPerDay = (calcPenalty.OverdueSum.Value / 100) * calcPenalty.Rate;
                            accrualSum += accrualPerDay * differenceInDays;
                            previousDate = calcPenalty.Date;
                        }

                        if (accrualSum > 0)
                        {
                            string penaltyBOSettingCode = Constants.BO_SETTING_PENALTYACCRUAL_PREFIX + accountSetting.Code;
                            var penaltyBOSettings = GetBOSettings(penaltyBOSettingCode.Replace(Constants.BO_SETTING_OFFBALANCE_POSTFIX, string.Empty));
                            var penaltyCashOrdersCost = GetCashOrdersCost(penaltyBOSettings, contract.Id, contract.IsContractRestructured);
                            var correctionSum = _penaltyAccrualRespoitory.GetCorrectionAmount(contract.Id, accrualDate, accountSetting.Id);
                            var correctionSumOkt2021 = GetCorrectionAmountOkt2021(contract.Id, accountSetting.Code, accrualDate);

                            accrualSum = Math.Round(accrualSum, 2);
                            decimal diffBtwAccrualSumAndCashOrdersCost = accrualSum - penaltyCashOrdersCost + correctionSum + correctionSumOkt2021;
                            diffBtwAccrualSumAndCashOrdersCost = Math.Round(diffBtwAccrualSumAndCashOrdersCost, 2);
                            decimal penaltyLimitBalance = _contractService.GetPenaltyLimitBalance(contract.Id, accrualDate);
                            if (diffBtwAccrualSumAndCashOrdersCost > 0)
                            {
                                amounts.Clear();
                                decimal amountToAccrual = Math.Abs(diffBtwAccrualSumAndCashOrdersCost);
                                if (amounts.ContainsKey(accrualSetting.AmountType))
                                    throw new PawnshopApplicationException($"{nameof(amounts)} уже содержит ключ {accrualSetting.AmountType}");

                                amounts[accrualSetting.AmountType] = amountToAccrual;
                                
                                if (contract.UsePenaltyLimit)
                                {
                                    amounts[AmountType.PenaltyLimit] = Math.Min(amountToAccrual, penaltyLimitBalance);
                                    amounts[AmountType.PenaltyWriteOffByLimit] = amountToAccrual - penaltyLimitBalance > 0 ? amountToAccrual - penaltyLimitBalance : 0;
                                }
                                
                                try
                                {
                                    RegisterBusinessOperation(contract, accrualDate, ContractActionType.PenaltyAccrual, "PENALTY",
                                    accountSetting.Code, branch, amountToAccrual, amounts, authorId);
                                    _eventLog.Log(EventCode.ContractPenaltyAccrual, EventStatus.Success, EntityType.Contract, contract.Id, $"Начисление штрафа, сумма = {amountToAccrual}", userId: authorId);
                                }
                                catch (Exception e)
                                {
                                    _eventLog.Log(EventCode.ContractPenaltyAccrual, EventStatus.Failed, EntityType.Contract, contract.Id, $"Начисление штрафа, сумма = {amountToAccrual}", e.StackTrace, userId: authorId);
                                    throw;
                                }
                            }
                        }
                    }
                }
            }
        }

        private decimal GetCashOrdersCost(List<BusinessOperationSetting> BOSettings, int contractId, bool isContractRestructured)
        {
            decimal cashOrdersCost = 0;
            foreach (BusinessOperationSetting businessOperationSetting in BOSettings)
            {
                if (businessOperationSetting == null)
                    throw new PawnshopApplicationException($"Коллекция {nameof(BOSettings)} не должна содержать null элементы");

                List<CashOrder> cashOrders = _cashOrderRepository.GetListByBusinessOperationSettingsIdAndContractId(businessOperationSetting.Id, contractId);
                if (isContractRestructured)
                {
                    var deferment = _clientDefermentService.GetActiveDeferment(contractId);
                    if (deferment.CreateDate.HasValue && deferment.EndDate.HasValue)
                        cashOrders = cashOrders.Where(order => order.OrderDate >= (deferment.CreateDate.Value > deferment.EndDate.Value ? deferment.CreateDate.Value : deferment.EndDate.Value)).ToList();
                }
                decimal localCashOrdersCost = 0;
                foreach (CashOrder cashOrder in cashOrders)
                {
                    if (cashOrder == null)
                        throw new PawnshopApplicationException(
                            $"Ожидалось что {nameof(cashOrders)} не будет содержать null элемент");

                    decimal cashOrderCost = cashOrder.StornoId.HasValue ? -cashOrder.OrderCost : cashOrder.OrderCost;
                    localCashOrdersCost += cashOrderCost;
                    if (cashOrdersCost < 0)
                        throw new PawnshopApplicationException(
                            $"Сумма сторнированных ордеров(договор {contractId}) по начислению штрафов превышает сумму начисленной пени");
                }

                cashOrdersCost += localCashOrdersCost;
            }

            return cashOrdersCost;
        }

        private List<BusinessOperationSetting> GetBOSettings(string BOSettingCode)
        {
            var BOSettingsListQuery = new ListQueryModel<BusinessOperationSettingFilter>
            {
                Page = null,
                Model = new BusinessOperationSettingFilter
                {
                    Code = BOSettingCode,
                    IsActive = true
                }
            };

            ListModel<BusinessOperationSetting> BOSettingsListModel = _businessOperationSettingService.List(BOSettingsListQuery);
            if (BOSettingsListModel == null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(BOSettingsListModel)} не будет null");

            if (BOSettingsListModel.List == null)
                throw new PawnshopApplicationException(
                    $"Ожидалось что {nameof(BOSettingsListModel)}.{nameof(BOSettingsListModel.List)} не будет null");

            List<BusinessOperationSetting> BOSettings = BOSettingsListModel.List;
            if (BOSettings.Count == 0)
                throw new PawnshopApplicationException($"Не найдено ни одной настройки бизнес операции по коду {BOSettingCode}");

            return BOSettings;
        }

        private decimal GetCorrectionAmountOkt2021(int ContractId, string accountSettingCode, DateTime accrualDate)
        {
            string penaltyCorrectionBOSettingCode = Constants.BO_SETTING_CORR_PENALTYACCRUAL_PREFIX + accountSettingCode;
            var penaltyBOSettingsListQuery = new ListQueryModel<BusinessOperationSettingFilter>
            {
                Page = null,
                Model = new BusinessOperationSettingFilter
                {
                    Code = penaltyCorrectionBOSettingCode,
                    IsActive = true
                }
            };

            ListModel<BusinessOperationSetting> penaltyBOSettingsListModel = _businessOperationSettingService.List(penaltyBOSettingsListQuery);
            if (penaltyBOSettingsListModel == null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(penaltyBOSettingsListModel)} не будет null");

            List<BusinessOperationSetting> penaltyBOSettings = penaltyBOSettingsListModel.List;
            if (penaltyBOSettings.Count == 0)
                throw new PawnshopApplicationException($"Не найдено ни одной настройки бизнес операции по коду {penaltyCorrectionBOSettingCode}");

            return _penaltyAccrualRespoitory.GetCorrectionAmountOkt2021(ContractId, penaltyBOSettings.First().Id, accrualDate);
        }

        private void RegisterBusinessOperation(IContract contract, DateTime date, ContractActionType actionType, string operationCodePrefix, string operationCode, Group branch, decimal amountToAccrual, IDictionary<AmountType, decimal> amounts, int authorId)
        {

            ContractAction action = new ContractAction
            {
                ActionType = actionType,
                AuthorId = authorId,
                TotalCost = amountToAccrual,
                ContractId = contract.Id,
                CreateDate = DateTime.Now,
                Date = date,
                Reason = $"{(actionType == ContractActionType.InstantDiscount ? "Скидка при начислении штрафа" : "Начисление штрафа")} для {contract.ContractNumber}"
            };

            using (var transaction = _contractActionService.BeginContractActionTransaction())
            {
                var code = $"{operationCodePrefix}.{operationCode}";
                if (contract.IsOffBalance)
                    code = string.Concat(code, ".OFFBALANCE");
                _contractActionService.Save(action);
                _businessOperationService.Register(contract, date, code, branch,
                    authorId, amounts, action: action);
                _contractActionOperationService.Register(contract, action, authorId, branchId: branch.Id, callActionRowBusinessOperation: false);
                transaction.Commit();
            }
        }

        bool IsFirstDateAfterHoliday(DateTime paymentDate, DateTime accrualDate)
        {
            return _holydayService.List(new ListQueryModel<HolidayFilter>
            {
                Model = new HolidayFilter
                {
                    PayDate = accrualDate.Date
                }
            }).Count > 0;
        }

        bool IsHoliday(DateTime paymentDate, DateTime accrualDate)
        {
            List<Holiday> holidays = _holydayService.List(new ListQueryModel<HolidayFilter>
            {
                Page = null,
                Model = new HolidayFilter
                {
                    BeginDate = paymentDate.Date,
                    EndDate = accrualDate.Date
                }
            }).List;

            holidays.AddRange(_holydayService.List(new ListQueryModel<HolidayFilter>
            {
                Page = null,
                Model = new HolidayFilter
                {
                    PayDate = paymentDate.Date
                }
            }).List);

            return !holidays.Any() && holidays.Any(x => x.Date.Date == paymentDate.Date && accrualDate.Date <= x.PayDate.Date);
        }
        (bool, Blackout) IsBlackoutDay(DateTime paymentDate)
        {
            List<Blackout> _blackouts = _blackoutService.List(new ListQueryModel<BlackoutFilter>
            { Page = null, Model = new BlackoutFilter { Date = paymentDate, IsPersonal = false } }).List;

            return (_blackouts.Any(), _blackouts.FirstOrDefault());
        }

        public IEnumerable<PenaltyRates> CalculateForPenaltyAccrual(Contract contract, AccrualBase accrualSetting, Account account, DateTime accrualDate)
        {
            IEnumerable<(DateTime, decimal)> balances = null;
            if (contract.IsContractRestructured)
                balances = _accountService.CalculateForPenaltyAccrualForRestructured(account.Id, accrualDate);
            else
                balances = _accountService.CalculateForPenaltyAccrual(account.Id, accrualDate);

            List<ContractRate> contractRates = contract.ContractRates.Where(x => x.RateSettingId == accrualSetting.RateSettingId).ToList();
            if (!contractRates.Any())
                throw new PawnshopApplicationException($"По Договору {contract.Id} не заполнен ContractRates");

            var overdues = balances.Select(x => new PenaltyRates(x.Item1, x.Item2, 0, true)).ToList();
            var rates = contractRates.Select(x => new PenaltyRates(x.Date, null, x.Rate, false)).OrderByDescending(x => x.Date).ToList();

            DateTime firstOverduesDate = overdues.OrderBy(x => x.Date).FirstOrDefault().Date;
            var overdueWithRateList = ConcatListsWithoutDublicate(overdues, rates);

            return FillRates(FillOverdueAmounts(overdueWithRateList, firstOverduesDate), rates);
        }

        private List<PenaltyRates> ConcatListsWithoutDublicate(List<PenaltyRates> mainList, List<PenaltyRates> conactList)
        {
            foreach (var concatElement in conactList)
            {
                if (!mainList.Contains(concatElement, new PenaltyRateComparer()))
                    mainList.Add(concatElement);
            }
            return mainList;
        }

        private List<PenaltyRates> FillOverdueAmounts(List<PenaltyRates> calcPenaltyList, DateTime firstOverduesDate)
        {
            calcPenaltyList = calcPenaltyList.OrderByDescending(x => x.Date).ToList();

            decimal prevAmount = 0;
            foreach (var calcPenalty in calcPenaltyList)
            {
                if (calcPenalty.OverdueSum is null)
                    calcPenalty.OverdueSum = prevAmount;
                else
                    prevAmount = calcPenalty.OverdueSum.Value;

                if (calcPenalty.Date <= firstOverduesDate.Date)
                    calcPenalty.OverdueSum = 0;
            }
            return calcPenaltyList;
        }

        private List<PenaltyRates> FillRates(List<PenaltyRates> calcPenaltyList, List<PenaltyRates> contractRates)
        {
            decimal prevRate = 0;

            calcPenaltyList.ForEach(x => {
                if (contractRates.Where(r => r.Date < x.Date).Any())
                {
                    x.Rate = contractRates.Where(r => r.Date < x.Date).First().Rate;
                    prevRate = x.Rate;
                } else
                {
                    x.Rate = x.Rate != 0 ? x.Rate : prevRate;
                }
            });
            return calcPenaltyList.OrderBy(x => x.Date).ToList();
        }
    }
}
