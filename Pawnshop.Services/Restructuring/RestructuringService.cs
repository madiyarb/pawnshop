using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Restructuring;
using Pawnshop.Data.Models.TasLabRecruit;
using Pawnshop.Services.ClientDeferments.Interfaces;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.LoanPercent;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.PaymentSchedules;
using Pawnshop.Services.TasLabRecruit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Services.Domains;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Services.Calculation;
using Newtonsoft.Json;
using Pawnshop.Services.Models.Calculation;
using System.Globalization;
using Pawnshop.Services.Clients;
using Pawnshop.Core.Extensions;
using Pawnshop.Services.Collection;
using Pawnshop.Data.Models.Collection;

namespace Pawnshop.Services.Restructuring
{
    public class RestructuringService : IRestructuringService
    {
        private readonly ISessionContext _sessionContext;

        private readonly IDictionaryWithSearchService<Group, BranchFilter> _branchService;
        private readonly IPaymentScheduleService _paymentScheduleService;
        private readonly IContractService _contractService;
        private readonly ILoanPercentService _loanPercentService;
        private readonly ITasLabRecruitService _recruitService;
        private readonly IClientDefermentService _clientDefermentService;
        private readonly IDomainService _domainService;
        private readonly ContractRepository _contractRepository;
        private readonly ContractPaymentScheduleRepository _contractPaymentScheduleRepository;
        private readonly DomainValueRepository _domainValueRepository;
        private readonly ClientRepository _clientRepository;
        private readonly RestructuredContractPaymentScheduleRepository _restructuredScheduleRepository;
        private readonly IContractActionRowBuilder _contractActionRowBuilder;
        private readonly IContractPaymentScheduleService _contractPaymentScheduleService;
        private readonly IContractActionOperationService _contractActionOperationService;
        private readonly IContractActionService _contractActionService;
        private readonly IAccountService _accountService;
        private readonly ContractAdditionalInfoRepository _contractAdditionalInfoRepository;
        private readonly IClientBlackListService _clientBlackListService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly CollectionStatusRepository _collectionStatusRepository;
        private readonly ICollectionService _collectionService;

        public RestructuringService(
            ISessionContext sessionContext,
            IDictionaryWithSearchService<Group, BranchFilter> branchService,
            ClientRepository clientRepository,
            ContractRepository contractRepository,
            IPaymentScheduleService paymentScheduleService,
            ContractPaymentScheduleRepository contractPaymentScheduleRepository,
            DomainValueRepository domainValueRepository,
            IContractService contractService,
            ILoanPercentService loanPercentService,
            ITasLabRecruitService recruitService,
            IClientDefermentService clientDefermentService,
            IDomainService domainService,
            RestructuredContractPaymentScheduleRepository restructuredScheduleRepository,
            IContractActionRowBuilder contractActionRowBuilder,
            IContractPaymentScheduleService contractPaymentScheduleService,
            IContractActionOperationService contractActionOperationService,
            IContractActionService contractActionService,
            IAccountService accountService,
            ContractAdditionalInfoRepository contractAdditionalInfoRepository,
            IClientBlackListService clientBlackListService,
            IUnitOfWork unitOfWork,
            CollectionStatusRepository collectionStatusRepository,
            ICollectionService collectionService)
        {
            _sessionContext = sessionContext;
            _branchService = branchService;
            _clientRepository = clientRepository;
            _contractRepository = contractRepository;
            _paymentScheduleService = paymentScheduleService;
            _domainValueRepository = domainValueRepository;
            _contractService = contractService;
            _loanPercentService = loanPercentService;
            _recruitService = recruitService;
            _clientDefermentService = clientDefermentService;
            _domainService = domainService;
            _contractPaymentScheduleRepository = contractPaymentScheduleRepository;
            _restructuredScheduleRepository = restructuredScheduleRepository;
            _contractActionRowBuilder = contractActionRowBuilder;
            _contractPaymentScheduleService = contractPaymentScheduleService;
            _contractActionOperationService = contractActionOperationService;
            _contractActionService = contractActionService;
            _accountService = accountService;
            _contractAdditionalInfoRepository = contractAdditionalInfoRepository;
            _clientBlackListService = clientBlackListService;
            _unitOfWork = unitOfWork;
            _collectionStatusRepository = collectionStatusRepository;
            _collectionService = collectionService;
        }

        public async Task<RestructuredContractPaymentScheduleVm> BuildRestructuredSchedule(RestructuringModel restructuringModel)
        {
            ValidateRestructuringModel(restructuringModel);

            var contract = await _contractService.GetOnlyContractAsync(restructuringModel.ContractId.Value);
            var defermentInformation = _clientDefermentService.GetDefermentInformation(restructuringModel.ContractId.Value);

            if (contract.ContractClass == ContractClass.CreditLine)
                throw new PawnshopApplicationException("Реструктуризация не доступна для кредитных линий!");

            if (defermentInformation != null)
                throw new PawnshopApplicationException("Реструктуризация по контракту была проведена!");

            var contractAdditionalInfo = await _contractAdditionalInfoRepository.GetAsync(contract.Id);
            if (contractAdditionalInfo != null && contractAdditionalInfo.ChangedControlDate != null)
                throw new PawnshopApplicationException($"Контрольная Дата была уже изменена для договора с id = {contract.Id}");

            if (!contract.SettingId.HasValue)
                throw new PawnshopApplicationException("Реструктуризация для старого дискрета запрещена!");

            var setting = _loanPercentService.Get(contract.SettingId.Value);
            if (setting.IsFloatingDiscrete)
                throw new PawnshopApplicationException("Реструктуризация для плавающей ставки запрещена!");

            var actions = await _contractActionService.GetByContractIdAndDates(restructuringModel.ContractId.Value, restructuringModel.StartDefermentDate.AddDays(1).Date, restructuringModel.EndDefermentDate.Date);
            if (actions.Any(x => x.ActionType != ContractActionType.Prepayment))
                throw new PawnshopApplicationException($"В договоре #{contract.ContractNumber} с даты {restructuringModel.StartDefermentDate.Date.ToString("dd.MM.yyyy")} по дате {restructuringModel.EndDefermentDate.Date.ToString("dd.MM.yyyy")} все действия кроме аванса должны быть отменены или сторнированы");

            RestructuringDataModel restructuringData = new RestructuringDataModel();
            restructuringData.ContractPaymentSchedule = (await _contractPaymentScheduleRepository.GetContractPaymentSchedules(restructuringModel.ContractId.Value)).ToList();
            restructuringData.Setting = setting;
            var periodAfterStartingDeferment = restructuringData.ContractPaymentSchedule.OrderBy(x => x.Date).FirstOrDefault(x => x.Date > restructuringModel.StartDefermentDate);
            var periodBeforeStartingDeferment = restructuringData.ContractPaymentSchedule.OrderByDescending(x => x.Date).FirstOrDefault(x => x.Date < restructuringModel.StartDefermentDate);
            restructuringData.PercentToAccural = Math.Round((periodAfterStartingDeferment.PercentCost / periodAfterStartingDeferment.Period) * (restructuringModel.StartDefermentDate - periodBeforeStartingDeferment.Date).Days, 2);

            // график до отсрочки
            restructuringData.S1 = restructuringData.ContractPaymentSchedule.Where(x => x.Date.Date < restructuringModel.StartDefermentDate.Date).ToList();
            restructuringData.futurePaymentScheduleItem = restructuringData.ContractPaymentSchedule.OrderBy(x => x.Date).FirstOrDefault(x => x.Date.Date >= restructuringModel.StartDefermentDate.Date);

            // график до отсрочки (оплаченные платежи)
            restructuringData.S1Paid = restructuringData.S1.Where(s => s.Status != ScheduleStatus.Overdue && s.Status != ScheduleStatus.Canceled).Select(s => s).ToList();
            restructuringData.DebtLeft = GetDebt(restructuringModel.ContractId.Value, restructuringModel.StartDefermentDate) + GetOverdueDebt(restructuringModel.ContractId.Value, restructuringModel.StartDefermentDate);

            // график до отсрочки (просроченные платежи)
            restructuringData.S1Expired = BuildBeforeDefermentSchedulePeriod(restructuringModel.ContractId.Value, restructuringData.S1.Where(s => s.Status == ScheduleStatus.Overdue).Select(s => s).ToList());

            bool isOnControlDate = restructuringModel.StartDefermentDate.Day == restructuringData.futurePaymentScheduleItem.Date.Day;
            // график отсрочки
            restructuringData.S2 = BuildDefermentSchedulePeriod(restructuringModel, restructuringData.futurePaymentScheduleItem, isOnControlDate, contract.LoanPercent);
            restructuringData.S2.ForEach(x => x.DebtLeft = restructuringData.DebtLeft);
            restructuringData.S2.OrderBy(x => x.Date).FirstOrDefault().PercentCost = restructuringData.PercentToAccural;

            DateTime maturityDateS3;
            DateTime beginDateS3;
            int day;
            maturityDateS3 = restructuringData.S2.OrderByDescending(x => x.Date).FirstOrDefault().Date.AddMonths(restructuringModel.RestructuredMonthCount.Value);
            beginDateS3 = restructuringData.S2.OrderByDescending(x => x.Date).FirstOrDefault().Date.AddMonths(1);
            day = restructuringData.ContractPaymentSchedule.OrderByDescending(x => x.Date).FirstOrDefault().Date.Day;
            if (day > restructuringModel.EndDefermentDate.Day)
            {
                maturityDateS3 = new DateTime(maturityDateS3.Year, maturityDateS3.Month, day).AddMonths(-1);
                beginDateS3 = new DateTime(beginDateS3.Year, beginDateS3.Month, day).AddMonths(-1);
            }
            else
            {
                maturityDateS3 = new DateTime(maturityDateS3.Year, maturityDateS3.Month, day);
                beginDateS3 = new DateTime(beginDateS3.Year, beginDateS3.Month, day);
            }

            var scheduleItem = restructuringData.S2.OrderBy(s => s.Date).LastOrDefault();
            restructuringData.S3 = _paymentScheduleService.BuildAfterDefermentSchedulePeriod(
            restructuringModel.RestructuredMonthCount.Value,
            _paymentScheduleService.Build(
                restructuringData.Setting.ScheduleType.Value,
                restructuringData.DebtLeft,
                restructuringData.Setting.LoanPercent,
                restructuringModel.EndDefermentDate.AddDays( - (isOnControlDate ? 1 : 0)),
                maturityDateS3,
                firstPaymentDate: beginDateS3,
                isMigrated: contract.ProductType?.Code == "TSO_MIGRATION",
                isRestructuring: true,
                upcomingPaymentsCount: restructuringModel.RestructuredMonthCount),
            scheduleItem.AmortizedBalanceOfDefferedPercent.HasValue ? scheduleItem.AmortizedBalanceOfDefferedPercent.Value : 0,
            scheduleItem.AmortizedBalanceOfOverduePercent.HasValue ? scheduleItem.AmortizedBalanceOfOverduePercent.Value : 0,
            scheduleItem.AmortizedPenaltyOfOverdueDebt.HasValue ? scheduleItem.AmortizedPenaltyOfOverdueDebt.Value : 0,
            scheduleItem.AmortizedPenaltyOfOverduePercent.HasValue ? scheduleItem.AmortizedPenaltyOfOverduePercent.Value : 0);

            var restructuredBaseSchedule = new List<ContractPaymentSchedule>();
            restructuredBaseSchedule.AddRange(restructuringData.S1Paid);
            restructuredBaseSchedule.AddRange(restructuringData.S1Expired);
            restructuredBaseSchedule.AddRange(restructuringData.S2);
            restructuredBaseSchedule.AddRange(restructuringData.S3);

            var restructuredUpdatedSchedule = MapToRestructuredScheduleModel(restructuredBaseSchedule, restructuringModel.ContractId.Value);
            
            var fp = restructuredUpdatedSchedule.Find(x => x.Date == restructuringModel.EndDefermentDate.Date);
            CalculateAPRModel model4APR = new CalculateAPRModel();
            model4APR.LoanCost = (double)fp.DebtLeft + 
                (double)fp.AmortizedBalanceOfDefferedPercent +
                (double)fp.AmortizedBalanceOfOverduePercent +
                (double)fp.AmortizedPenaltyOfOverdueDebt +
                (double)fp.AmortizedPenaltyOfOverduePercent;

            model4APR.ScheduleData = new List<AprScheduleModel>();
            foreach (var item in restructuredUpdatedSchedule.Where(x => x.Date > restructuringModel.StartDefermentDate && !x.DeleteDate.HasValue))
            {
                double sum = (double)(item.DebtCost + 
                    item.PercentCost + 
                    item.PaymentBalanceOfDefferedPercent + 
                    item.PaymentBalanceOfOverduePercent +
                    item.PaymentPenaltyOfOverdueDebt +
                    item.PaymentPenaltyOfOverduePercent);

                model4APR.ScheduleData.Add(new AprScheduleModel() { SchedulePaymentAmount = sum, ScheduleDays = (item.Date - restructuringModel.StartDefermentDate).Days });
            }

            var apr = _contractService.CalculateAPRAfterRestructuring(model4APR);

            RestructuredContractPaymentScheduleVm model = new RestructuredContractPaymentScheduleVm()
            {
                RestructuredContractPaymentScheduleList = restructuredUpdatedSchedule,
                Apr = apr
            };

            return model;
        }

        private decimal GetDebt(int contractId, DateTime startDefermentDate)
        {
            var account = _contractService.GetContractAccountBalance(
                    contractId,
                    Constants.ACCOUNT_SETTING_ACCOUNT,
                    startDefermentDate);
            return account;
        }

        private decimal GetOverdueDebt(int contractId, DateTime startDefermentDate)
        {
            var overdueAccount = _contractService.GetContractAccountBalance(
                    contractId,
                    Constants.ACCOUNT_SETTING_OVERDUE_ACCOUNT,
                    startDefermentDate);
            return overdueAccount;
        }

        private void ValidateRestructuringModel(RestructuringModel restructuringModel)
        {
            if (!restructuringModel.ContractId.HasValue)
                throw new PawnshopApplicationException($"Идентификатор договора отсутствует {restructuringModel.ContractId.Value}");

            //if (!restructuringModel.DefermentMonthCount.HasValue)
            //    throw new PawnshopApplicationException($"Укажте количество месяцев для реструктуризации");

            if (restructuringModel.StartDefermentDate == DateTime.MinValue)
                throw new PawnshopApplicationException($"Укажите дату начала реструктуризации");

            if (restructuringModel.EndDefermentDate == DateTime.MinValue)
                throw new PawnshopApplicationException($"Укажите дату конца реструктуризации");

            if (!restructuringModel.RestructuredMonthCount.HasValue)
                throw new PawnshopApplicationException($"Укажите количество месяцев после реструктуризиции");

            if (!restructuringModel.DefermentTypeId.HasValue)
                throw new PawnshopApplicationException($"Укажите тип отсрочки");

            // на данный момент это не нужно
            // на второй итерации сделаем, пока пусть будет вот так
            //var domainValue = _domainService.getDomainCodeById(restructuringModel.DefermentTypeId);
            //if (domainValue.Code == Constants.DEFERMENT_TYPE_EMERGENCY_MODE && restructuringModel.DocumentFile == null)
            //    throw new PawnshopApplicationException($"Загрузите файл подтвердающего документа об отсрочке");
        }

        private List<RestructuredContractPaymentSchedule> MapToRestructuredScheduleModel(IEnumerable<ContractPaymentSchedule> restructuredScheduleModel, int contractId)
        {
            List<RestructuredContractPaymentSchedule> updatedScheduleList = new List<RestructuredContractPaymentSchedule>();
            foreach (var s in restructuredScheduleModel)
            {
                if (s is RestructuredContractPaymentSchedule restructuredItem)
                {
                    updatedScheduleList.Add(new RestructuredContractPaymentSchedule(null, contractId, s.Date, s.ActualDate, s.DebtLeft, s.DebtCost, s.PercentCost, s.PenaltyCost,
                        DateTime.Now, s.DeleteDate, s.ActionId, s.Canceled, s.Prolongated, s.Status, s.Period, s.Revision, s.ActionType, s.NextWorkingDate,
                        restructuredItem.PaymentBalanceOfDefferedPercent, restructuredItem.AmortizedBalanceOfDefferedPercent, restructuredItem.PaymentBalanceOfOverduePercent, restructuredItem.AmortizedBalanceOfOverduePercent,
                        restructuredItem.PaymentPenaltyOfOverduePercent, restructuredItem.AmortizedPenaltyOfOverduePercent, restructuredItem.PaymentPenaltyOfOverdueDebt, restructuredItem.AmortizedPenaltyOfOverdueDebt));
                }
                else
                {
                    updatedScheduleList.Add(new RestructuredContractPaymentSchedule(null, contractId, s.Date, s.ActualDate, s.DebtLeft, s.DebtCost, s.PercentCost, s.PenaltyCost,
                        DateTime.Now, s.DeleteDate, s.ActionId, s.Canceled, s.Prolongated, s.Status, s.Period, s.Revision, s.ActionType, s.NextWorkingDate, 0, 0, 0, 0, 0, 0, 0, 0));
                }
            }

            return updatedScheduleList;
        }

        private List<RestructuredContractPaymentSchedule> BuildBeforeDefermentSchedulePeriod(
            int contractId, 
            List<ContractPaymentSchedule> schedule)
        {
            var restructuredSchedule = new List<RestructuredContractPaymentSchedule>();

            for (int i = 0; i < schedule.Count; i++)
            {
                var s = schedule[i];

                var profit = _contractService.GetContractAccountBalance(
                    contractId,
                    Constants.ACCOUNT_SETTING_PROFIT,
                    s.Date);

                var penyProfit = _contractService.GetContractAccountBalance(
                    contractId,
                    Constants.ACCOUNT_SETTING_PENY_PROFIT,
                    s.Date);

                var penyAccount = _contractService.GetContractAccountBalance(
                    contractId,
                    Constants.ACCOUNT_SETTING_PENY_ACCOUNT,
                    s.Date);

                var overdueProfit = _contractService.GetContractAccountBalance(
                    contractId,
                    Constants.ACCOUNT_SETTING_OVERDUE_PROFIT,
                    s.Date);

                restructuredSchedule.Add(new RestructuredContractPaymentSchedule(s.Id, 0, s.Date, s.ActualDate,  s.DebtLeft, s.DebtCost, s.PercentCost, s.PenaltyCost, DateTime.Now, null, s.ActionId, 
                        s.Canceled, s.Prolongated,  s.Status, s.Period, s.Revision, s.ActionType, s.NextWorkingDate, 
                        0, 0, 0, overdueProfit + profit, 
                        0, penyProfit, 0, penyAccount));
            }

            return restructuredSchedule;
        }

        private List<RestructuredContractPaymentSchedule> BuildDefermentSchedulePeriod(
            RestructuringModel restructuringModel, 
            ContractPaymentSchedule defStartScheduleItem,
            bool isOnControlDate,
            decimal nominalRate = 0)
        {
            var restructuredSchedule = new List<RestructuredContractPaymentSchedule>();
            var controlDate = defStartScheduleItem.Date;

            var profit = _contractService.GetContractAccountBalance(
                restructuringModel.ContractId.Value,
                Constants.ACCOUNT_SETTING_PROFIT,
                DateTime.Today.Date);

            var penyProfit = _contractService.GetContractAccountBalance(
                restructuringModel.ContractId.Value, 
                Constants.ACCOUNT_SETTING_PENY_PROFIT,
                DateTime.Today.Date);

            var penyAccount = _contractService.GetContractAccountBalance(
                restructuringModel.ContractId.Value, 
                Constants.ACCOUNT_SETTING_PENY_ACCOUNT,
                DateTime.Today.Date);

            var overdueProfit = _contractService.GetContractAccountBalance(
                restructuringModel.ContractId.Value, 
                Constants.ACCOUNT_SETTING_OVERDUE_PROFIT,
                DateTime.Today.Date);

            var debt = GetDebt(restructuringModel.ContractId.Value, restructuringModel.StartDefermentDate);
            var overdueDebt = GetOverdueDebt(restructuringModel.ContractId.Value, restructuringModel.StartDefermentDate);

            var domainValue = _domainService.getDomainCodeById(restructuringModel.DefermentTypeId);
            var currentNominalRate = nominalRate * 360 / 100;
            var daysAfterPaymentBeforeDeferment = Math.Abs(defStartScheduleItem.Date.AddMonths(-1).Subtract(restructuringModel.StartDefermentDate.Date).Days);
            var daysAfterDefermentBeforePayment = Math.Abs(restructuringModel.StartDefermentDate.Date.Subtract(defStartScheduleItem.Date).Days);

            bool isMilitary = domainValue.Code == Constants.DEFERMENT_TYPE_MILITARY_PERSONNEL;
            var amortizedBalanceOfOverduePercent = Math.Round((debt * currentNominalRate / 360 * daysAfterPaymentBeforeDeferment) + overdueProfit, 2);

            if (!isOnControlDate)
            {
                AddRestructuredScheduleItem(restructuredSchedule, restructuringModel.StartDefermentDate, ScheduleStatus.RestructuredPayment,
                    daysAfterPaymentBeforeDeferment, defStartScheduleItem.Revision, 0, amortizedBalanceOfOverduePercent, penyProfit, penyAccount);
            }

            decimal? amortizedBalanceOfDefferedPercent = CalcAmortizedBalanceOfDefferedPercent(debt + overdueDebt, currentNominalRate / 365, daysAfterDefermentBeforePayment, null, isMilitary, null);
            for (int i = 0; i < restructuringModel.EndDefermentDate.Date.MonthDifference(restructuringModel.StartDefermentDate.Date); i++)
            {
                var yearlyAccuralDays = 360;
                var accuralDays = 30;
                amortizedBalanceOfDefferedPercent = i == 0 ? amortizedBalanceOfDefferedPercent : CalcAmortizedBalanceOfDefferedPercent(debt + overdueDebt, currentNominalRate / yearlyAccuralDays, accuralDays, restructuredSchedule, isMilitary, !isOnControlDate ? i : i - 1);
                AddRestructuredScheduleItem(restructuredSchedule, controlDate.AddMonths(i), ScheduleStatus.RestructuredPayment,
                    accuralDays, defStartScheduleItem.Revision, amortizedBalanceOfDefferedPercent, amortizedBalanceOfOverduePercent, penyProfit, penyAccount);
            }

            var gapBeforeDeferment = Math.Abs(restructuredSchedule[restructuredSchedule.Count - 1].Date.Subtract(restructuringModel.EndDefermentDate).Days);

            if (!isOnControlDate)
            {
                amortizedBalanceOfDefferedPercent = CalcAmortizedBalanceOfDefferedPercent(debt + overdueDebt, currentNominalRate / 365, gapBeforeDeferment, restructuredSchedule, isMilitary, restructuredSchedule.Count - 1);
                AddRestructuredScheduleItem(restructuredSchedule, restructuringModel.EndDefermentDate.Date, ScheduleStatus.RestructuredPayment,
                    gapBeforeDeferment, defStartScheduleItem.Revision, amortizedBalanceOfDefferedPercent, amortizedBalanceOfOverduePercent, penyProfit, penyAccount);
            }

            return restructuredSchedule;
        }

        private void AddRestructuredScheduleItem(List<RestructuredContractPaymentSchedule> schedule, DateTime date, ScheduleStatus status, int period, int revision, decimal? amortizedPercent, decimal? amortizedOverduePercent, decimal? penyProfit, decimal? penyAccount)
        {
            schedule.Add(new RestructuredContractPaymentSchedule(0, 0, date, null, 0, 0, 0, 0, DateTime.Now, null, null, null, null, status, period,
                        revision, null, null,
                        0, amortizedPercent, 0, amortizedOverduePercent,
                        0, penyProfit, 0, penyAccount));
        }

        private decimal? CalcAmortizedBalanceOfDefferedPercent(decimal debt, decimal currentNominalRate, int daysBeforePayment, List<RestructuredContractPaymentSchedule> schedule, bool isMilitary, int? index)
        {
            if (isMilitary)
                return 0;
            var debtRate = (debt * currentNominalRate * daysBeforePayment);
            var amortizedPercent = schedule != null && schedule.Count > 0 ? schedule[index.HasValue ? index.Value : 0].AmortizedBalanceOfDefferedPercent : 0;

            return debtRate + amortizedPercent;
        }

        private List<RestructuredContractPaymentSchedule> BuildAfterDefermentSchedulePeriod(
            RestructuringModel restructuringModel, 
            List<ContractPaymentSchedule> schedulePartAfterDeferment, 
            RestructuredContractPaymentSchedule lastSchedulePart3)
        {
            var restructuredSchedule = new List<RestructuredContractPaymentSchedule>();

            var amortizedBalanceOfDefferedPercent = lastSchedulePart3.AmortizedBalanceOfDefferedPercent.HasValue ? lastSchedulePart3.AmortizedBalanceOfDefferedPercent.Value : 0;
            var amortizedBalanceOfOverduePercent = lastSchedulePart3.AmortizedBalanceOfOverduePercent.HasValue ? lastSchedulePart3.AmortizedBalanceOfOverduePercent.Value : 0;
            var amortizedPenaltyOfOverdueDebt = lastSchedulePart3.AmortizedPenaltyOfOverdueDebt.HasValue ? lastSchedulePart3.AmortizedPenaltyOfOverdueDebt.Value : 0;
            var amortizedPenaltyOfOverduePercent = lastSchedulePart3.AmortizedPenaltyOfOverduePercent.HasValue ? lastSchedulePart3.AmortizedPenaltyOfOverduePercent.Value : 0;

            var paymentBalanceOfDefferedPercent = amortizedBalanceOfDefferedPercent / restructuringModel.RestructuredMonthCount;
            var paymentBalanceOfOverduePercent = amortizedBalanceOfOverduePercent / restructuringModel.RestructuredMonthCount;
            var paymentPenaltyOfOverdueDebt = amortizedPenaltyOfOverdueDebt / restructuringModel.RestructuredMonthCount;
            var paymentPenaltyOfOverduePercent = amortizedPenaltyOfOverduePercent / restructuringModel.RestructuredMonthCount;

            decimal? fractionalDefferedPercent = 0.0m;
            decimal? fractionalOverduePercent = 0.0m;
            decimal? fractionalPenaltyOfOverdueDebt = 0.0m;
            decimal? fractionalPenaltyOfOverduePercent = 0.0m;

            schedulePartAfterDeferment.ForEach(s =>
            {
                var currentPaymentDefferedPercent = paymentBalanceOfDefferedPercent + fractionalDefferedPercent;
                var currentPaymentOverduePercent = paymentBalanceOfOverduePercent + fractionalOverduePercent;
                var currentPaymentPenaltyOfOverdueDebt = paymentPenaltyOfOverdueDebt + fractionalPenaltyOfOverdueDebt;
                var currentPaymentPenaltyOfOverduePercent = paymentPenaltyOfOverduePercent + fractionalPenaltyOfOverduePercent;

                var roundedPaymentDefferedPercent = Math.Floor(currentPaymentDefferedPercent.Value * 100) / 100;
                var roundedPaymentOverduePercent = Math.Floor(currentPaymentOverduePercent.Value * 100) / 100;
                var roundedPaymentPenaltyOfOverdueDebt = Math.Floor(currentPaymentPenaltyOfOverdueDebt.Value * 100) / 100;
                var roundedPaymentPenaltyOfOverduePercent = Math.Floor(currentPaymentPenaltyOfOverduePercent.Value * 100) / 100;

                fractionalDefferedPercent = currentPaymentDefferedPercent - roundedPaymentDefferedPercent;
                fractionalOverduePercent = currentPaymentOverduePercent - roundedPaymentOverduePercent;
                fractionalPenaltyOfOverdueDebt = currentPaymentPenaltyOfOverdueDebt - roundedPaymentPenaltyOfOverdueDebt;
                fractionalPenaltyOfOverduePercent = currentPaymentPenaltyOfOverduePercent - roundedPaymentPenaltyOfOverduePercent;

                amortizedBalanceOfDefferedPercent -= roundedPaymentDefferedPercent;
                amortizedBalanceOfOverduePercent -= roundedPaymentOverduePercent;
                amortizedPenaltyOfOverdueDebt -= roundedPaymentPenaltyOfOverdueDebt;
                amortizedPenaltyOfOverduePercent -= roundedPaymentPenaltyOfOverduePercent;

                if(amortizedBalanceOfDefferedPercent > 0 && amortizedBalanceOfDefferedPercent < 0.1m)
                {
                    roundedPaymentDefferedPercent += amortizedBalanceOfDefferedPercent;
                    amortizedBalanceOfDefferedPercent = 0;
                }
                if (amortizedBalanceOfOverduePercent > 0 && amortizedBalanceOfOverduePercent < 0.1m)
                {
                    roundedPaymentOverduePercent += amortizedBalanceOfOverduePercent;
                    amortizedBalanceOfOverduePercent = 0;
                }
                if (amortizedPenaltyOfOverdueDebt > 0 && amortizedPenaltyOfOverdueDebt < 0.1m)
                {
                    roundedPaymentPenaltyOfOverdueDebt += amortizedPenaltyOfOverdueDebt;
                    amortizedPenaltyOfOverdueDebt = 0;
                }
                if (amortizedPenaltyOfOverduePercent > 0 && amortizedPenaltyOfOverduePercent < 0.1m)
                {
                    roundedPaymentPenaltyOfOverduePercent += amortizedPenaltyOfOverduePercent;
                    amortizedPenaltyOfOverduePercent = 0;
                }
                    
                restructuredSchedule.Add(new RestructuredContractPaymentSchedule(0, 0, s.Date, s.ActualDate, s.DebtLeft, s.DebtCost, s.PercentCost, s.PenaltyCost, s.CreateDate, null, null, s.Canceled, s.Prolongated,
                    s.Status, s.Period, s.Revision, s.ActionType, s.NextWorkingDate,  
                    roundedPaymentDefferedPercent, amortizedBalanceOfDefferedPercent, roundedPaymentOverduePercent, amortizedBalanceOfOverduePercent, 
                    roundedPaymentPenaltyOfOverdueDebt, amortizedPenaltyOfOverdueDebt, roundedPaymentPenaltyOfOverduePercent, amortizedPenaltyOfOverduePercent));
            });

            return restructuredSchedule;
        }

        public async Task SaveRestructuredPaymentSchedule(RestructuringSaveModel restructuringSaveModel, int branchId)
        {
            var contract = await _contractService.GetAsync(restructuringSaveModel.ContractId);
            if (contract == null)
                throw new PawnshopApplicationException($"Не найден контракт с идентификатором {restructuringSaveModel.ContractId}");

            var apr = Convert.ToDecimal(restructuringSaveModel.Apr, CultureInfo.InvariantCulture);
            var systemApr = contract.ContractDate.Date >= Constants.APR_CHANGED_DATE ? Constants.MAX_APR_V2 : Constants.MAX_APR_OLD;
            if (apr > systemApr)
                throw new PawnshopApplicationException(
                    $"Cтавка ГЭСВ ({apr}) превышает допустимое значение, попробуйте выбрать другие сроки договора с id = {restructuringSaveModel.ContractId}!");

            var domainValue = _domainService.getDomainCodeById(restructuringSaveModel.DefermentTypeId);
            bool isMilitary = domainValue.Code == Constants.DEFERMENT_TYPE_MILITARY_PERSONNEL;
            
            Contract creditLine = null;
            if(contract.ContractClass == ContractClass.Tranche)
                creditLine = _contractService.FillPositions4Contract(contract.CreditLineId.Value);

            var restructuredSchedule = JsonConvert.DeserializeObject<List<RestructuredContractPaymentSchedule>>(restructuringSaveModel.RestructuredSchedule);

            var newMaturityDate = restructuredSchedule.OrderBy(s => s.Date).LastOrDefault().Date;
            var newNextPaymentDate = restructuredSchedule.Where(s => s.Status == ScheduleStatus.FuturePayment).FirstOrDefault().Date;

            if(contract.ContractClass == ContractClass.CreditLine)
                throw new PawnshopApplicationException($"Нельзя проводить реструктуризацию для кредитной линий");

            if (branchId != contract.BranchId)
                throw new PawnshopApplicationException("Нельзя создовать отсрочку по контракту в другом филиале!");

            var action1 = await CreateContractAction(
                restructuringSaveModel.ContractId,
                restructuringSaveModel.DefermentTypeId,
                restructuringSaveModel.StartDefermentDate,
                restructuringSaveModel.EndDefermentDate,
                branchId,
                creditLine);

            var branch = await _branchService.GetAsync(contract.BranchId);

            var clientDeferment = _clientDefermentService.GetActiveDeferment(contract.Id);
            if (clientDeferment == null)
                throw new PawnshopApplicationException($"Не найдена запись по ClientId={contract.ClientId} и ContractId={contract.Id} для проведения реструктуризаций");

            TransferMaturityAndNextPaymentDates(contract, newMaturityDate, newNextPaymentDate, restructuringSaveModel.DefermentTypeId, creditLine);
            
            restructuredSchedule.ForEach(x =>
            {
                if (x.Status == ScheduleStatus.RestructuredPayment)
                    x.ActionId = action1.Id;
            });

            if (contract.PeriodTypeId == Constants.SHORT_TERM_PERIOD_TYPE_ID)
            {
                using (var transaction = _unitOfWork.BeginTransaction())
                {
                    var model = new RestructuringActionModel()
                    {
                        RestructurionActionType = contract.ContractClass == ContractClass.Credit ? ContractActionType.RestructuringTransferToTransitCred : ContractActionType.RestructuringTransferToTransitTranches,
                        Contract = contract,
                        Branch = branch,
                        ClientDeferment = clientDeferment,
                        StartDefermentDate = restructuringSaveModel.StartDefermentDate,
                        EndDefermentDate = restructuringSaveModel.EndDefermentDate,
                        DefermentTypeId = restructuringSaveModel.DefermentTypeId,
                        AuthorId = _sessionContext.UserId
                    };
                    var closedAccount = _accountService.GetByAccountSettingCode(contract.Id, Constants.ACCOUNT_SETTING_ACCOUNT);
                    var action2 = await CreateActionTransferToTransit(model);
                    action2.Data.ClosedAccountId = closedAccount.Id;
                    await _accountService.CloseAccount(closedAccount.Id);
                    await _contractService.UpdatePeriodType(Constants.LONG_TERM_PERIOD_TYPE_ID, model.Contract.Id);
                    model.Contract.PeriodTypeId = Constants.LONG_TERM_PERIOD_TYPE_ID;
                    var action3 = await CreateActionTransferToAccount(model);
                    var openedAccount = _accountService.GetByAccountSettingCode(contract.Id, Constants.ACCOUNT_SETTING_ACCOUNT);
                    action3.Data.OpenedAccountId = openedAccount.Id;

                    action1.ChildAction = action2;
                    action1.ChildActionId = action2.Id;
                    _contractActionService.Save(action1);
                    action2.ParentAction = action1;
                    action2.ParentActionId = action1.Id;
                    action2.ChildAction = action3;
                    action2.ChildActionId = action3.Id;
                    _contractActionService.Save(action2);
                    action3.ParentAction = action2;
                    action3.ParentActionId = action2.Id;
                    _contractActionService.Save(action3);

                    transaction.Commit();
                }
            }


            contract.APR = Convert.ToDecimal(restructuringSaveModel.Apr, CultureInfo.InvariantCulture);
            var oldSchedule = (await _contractPaymentScheduleRepository.GetContractPaymentSchedules(restructuringSaveModel.ContractId)).ToList();
            oldSchedule.ForEach(x => x.DeleteDate = DateTime.Now);
            _contractService.Save(contract);

            foreach (var item in restructuredSchedule)
            {
                await _contractPaymentScheduleRepository.InsertAsync(item);

                if ((restructuringSaveModel.StartDefermentDate <= item.Date || item.Status == ScheduleStatus.Overdue && restructuringSaveModel.StartDefermentDate >= item.Date) && !item.DeleteDate.HasValue)
                    await _restructuredScheduleRepository.InsertAsync(item);
            }

            foreach (var item in oldSchedule)
            {
                await _contractPaymentScheduleRepository.UpdateAsync(item);
            }

            if (!isMilitary)
                await _clientBlackListService.InsertIntoBlackListAsync(contract.ClientId);

            var collectionStatus = await _collectionStatusRepository.GetByContractIdAsync(contract.Id);
            if(collectionStatus != null && collectionStatus.IsActive)
            {
                var close = new CollectionClose()
                { 
                    ContractId = contract.Id,
                    ActionId = action1.Id,
                    DelayDays = 0
                };
                _collectionService.CloseContractCollection(close);
            }
        }

        private async Task<ContractAction> CreateContractAction(
            int contractId,
            int defermentTypeId,
            DateTime startDefermentDate,
            DateTime endDefermentDate,
            int branchId,
            Contract creditLine)
        {
            int authorId = _sessionContext.UserId;
            var contract = _contractRepository.Get(contractId);
            var branch = await _branchService.GetAsync(contract.BranchId);
            if (branchId != contract.BranchId)
                throw new PawnshopApplicationException("Нельзя создовать отсрочку по контракту в другом филиале!");

            if (contract.ContractClass != ContractClass.CreditLine)
            {
                var defermentTypeName = _domainValueRepository.Get(defermentTypeId).Name;
                var clientDeferment = _clientDefermentService.GetActiveDeferment(contract.Id);
                var actionType = contract.ContractClass == ContractClass.Credit ? ContractActionType.RestructuringCred : ContractActionType.RestructuringTranches;
                var action = new ContractAction()
                {
                    ActionType = actionType,
                    Date = startDefermentDate.Date,
                    Reason = $"Отсрочка по {contract.ContractNumber} от {contract.ContractDate.ToString("dd.MM.yyyy")} для {defermentTypeName} на {endDefermentDate.MonthDifference(startDefermentDate)} месяцев.",
                    TotalCost = 0,
                    Cost = 0,
                    ContractId = contract.Id,
                    AuthorId = authorId,
                    ClientDefermentId = clientDeferment?.Id,
                    CreateDate = DateTime.Now,
                    Rows = ActionRowBuild(contract, authorId, startDefermentDate, actionType).ToArray()
                };

                if (action.Data == null)
                    action.Data = new ContractActionData();

                action.Data.Branch = branch;
                action.Data.Apr = contract.APR.Value;

                action.Data.MaturityDate = contract.MaturityDate;
                action.Data.NextPaymentDate = contract.NextPaymentDate;
                if (contract.ContractClass == ContractClass.Tranche)
                    action.Data.CreditLineMaturityDate = creditLine.MaturityDate;

                _contractActionOperationService.Register(contract, action, authorId, branchId);

                var historyId = await _contractPaymentScheduleService.InsertContractPaymentScheduleHistory(contract.Id, action.Id, (int)ContractActionStatus.Canceled);
                foreach (var item in contract.PaymentSchedule)
                {
                    if (item.ActionId.HasValue)
                    {
                        var act = await _contractActionService.GetAsync(item.ActionId.Value);
                        if (act != null)
                            if (act.ActionType != ContractActionType.PartialPayment)
                                item.ActionType = (int)act.ActionType;
                            else 
                                if (act.ActionType == ContractActionType.PartialPayment && Math.Round(act.Cost) == Math.Round(item.DebtCost))
                                    item.ActionType = (int)act.ActionType;
                    }
                    await _contractPaymentScheduleService.InsertContractPaymentScheduleHistoryItems(historyId, item);
                }

                await CreateDeferment(contract, defermentTypeId, startDefermentDate, endDefermentDate);

                return action;
            }

            return null;
        }

        private async Task<ContractAction> CreateActionTransferToAccount(RestructuringActionModel model)
        {
            var actionType = model.Contract.ContractClass == ContractClass.Credit ? ContractActionType.RestructuringTransferToAccountCred : ContractActionType.RestructuringTransferToAccountTranches;
            var action = new ContractAction()
            {
                ActionType = actionType,
                Date = DateTime.Now,
                Reason = $"Перенос суммы с транзитного на счет ОД",
                TotalCost = 0,
                Cost = 0,
                ContractId = model.Contract.Id,
                AuthorId = model.AuthorId,
                ClientDefermentId = model.ClientDeferment?.Id,
                CreateDate = DateTime.Now,
                Rows = ActionRowBuild(model.Contract, model.AuthorId, model.StartDefermentDate, actionType).ToArray()
            };

            if (action.Data == null)
                action.Data = new ContractActionData();

            action.Data.Branch = model.Branch;
            action.Data.Apr = model.Contract.APR.Value;

            _contractActionOperationService.Register(model.Contract, action, model.AuthorId, model.Branch.Id);

            return action;
        }

        private async Task<ContractAction> CreateActionTransferToTransit(RestructuringActionModel model)
        {
            var actionType = model.Contract.ContractClass == ContractClass.Credit ? ContractActionType.RestructuringTransferToTransitCred : ContractActionType.RestructuringTransferToTransitTranches;
            var action = new ContractAction()
            {
                ActionType = actionType,
                Date = DateTime.Now,
                Reason = $"Перенос суммы с транзитного на счет ОД",
                TotalCost = 0,
                Cost = 0,
                ContractId = model.Contract.Id,
                AuthorId = model.AuthorId,
                ClientDefermentId = model.ClientDeferment?.Id,
                CreateDate = DateTime.Now,
                Rows = ActionRowBuild(model.Contract, model.AuthorId, model.StartDefermentDate, actionType).ToArray()
            };

            if (action.Data == null)
                action.Data = new ContractActionData();

            action.Data.Branch = model.Branch;
            action.Data.Apr = model.Contract.APR.Value;

            _contractActionOperationService.Register(model.Contract, action, model.AuthorId, model.Branch.Id);

            return action;
        }

        private void TransferMaturityAndNextPaymentDates(Contract contract, DateTime lastPaymentDate, DateTime nextPaymentDate, int defermentTypeId, Contract creditLine = null)
        {
            var domainValue = _domainService.getDomainCodeById(defermentTypeId);
            bool isMilitary = domainValue.Code == Constants.DEFERMENT_TYPE_MILITARY_PERSONNEL;

            if (contract.ContractClass == ContractClass.Tranche)
            {
                if (creditLine.MaturityDate < lastPaymentDate)
                {
                    if (!isMilitary)
                    {
                        creditLine.MaturityDate = lastPaymentDate;
                        _contractService.Save(creditLine);
                    }
                }
                contract.NextPaymentDate = nextPaymentDate;
                contract.MaturityDate = lastPaymentDate;
                _contractService.Save(contract);
            }
            else if (contract.ContractClass == ContractClass.Credit)
            {
                contract.NextPaymentDate = nextPaymentDate;
                contract.MaturityDate = lastPaymentDate;
                _contractService.Save(contract);
            }
        }

        private List<ContractActionRow> ActionRowBuild(Contract contract, int authorId, DateTime startDefermentDate, ContractActionType actionType)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            var actionRows = _contractActionRowBuilder.Build(contract, new ContractDutyCheckModel
            {
                ActionType = actionType,
                ContractId = contract.Id,
                EmployeeId = authorId,
                Date = startDefermentDate
            });

            return actionRows;
        }

        private async Task CreateDeferment(Contract contract, int defermentTypeId, DateTime startDefermentDate, DateTime endDefermentDate)
        {
            var defermentTypeCode = _domainService.getDomainCodeById(defermentTypeId).Code;

            switch (defermentTypeCode)
            {
                case Constants.DEFERMENT_TYPE_MILITARY_PERSONNEL:
                    var clientIIN = _clientRepository.GetOnlyClient(contract.ClientId).IdentityNumber;
                    var recruitIINResponse = await _recruitService.GetRecruitByIIN(clientIIN);
                    var recruit = new Recruit
                    {
                        IIN = recruitIINResponse.IIN,
                        Status = recruitIINResponse.Status,
                        Date = recruitIINResponse.Date,
                    };
                    await _clientDefermentService.CreateDefermentForMilitary(contract, recruit);
                    break;
                default:
                    await _clientDefermentService.CreateDeferment(contract, startDefermentDate, endDefermentDate, defermentTypeCode);
                    break;
            }
        }
    }
}
