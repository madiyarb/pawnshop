using Newtonsoft.Json;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using System.Collections.Generic;
using System.Linq;
using System;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.InteresAccrual;
using Pawnshop.Services.Domains;
using Pawnshop.Data.Models.LegalCollection;
using Pawnshop.Services.ClientDeferments.Interfaces;

namespace Pawnshop.Services.AccountingCore
{
	public class InterestAccrualService : IInterestAccrual
	{
		private readonly IBusinessOperationService _businessOperationService;
		private readonly IDictionaryWithSearchService<BusinessOperationSetting, BusinessOperationSettingFilter> _businessOperationSettingService;
		private readonly ICashOrderService _cashOrderService;
		private readonly IEventLog _eventLog;
		private readonly IDictionaryWithSearchService<Group, BranchFilter> _branchService;
		private readonly int _authorId;
		private readonly IContractActionService _contractActionService;
		private readonly IContractActionOperationService _contractActionOperationService;
		private readonly IContractService _contractService;
		private readonly IContractPaymentScheduleService _contractPaymentScheduleService;
		private readonly IAccountService _accountService;
        private readonly IDomainService _domainService;
        private readonly ContractAdditionalInfoRepository _сontractAdditionalInfoRepository;
        private readonly LoanProductTypeRepository _loanProductTypeRepository;
		private readonly ClientDefermentRepository _clientDefermentRepository;
		private readonly IClientDefermentService _clientDefermentService;

        public InterestAccrualService(IBusinessOperationService businessOperationService,
			ICashOrderService cashOrderService,
			IDictionaryWithSearchService<BusinessOperationSetting, BusinessOperationSettingFilter> businessOperationSettingService,
			IEventLog eventLog,
			IDictionaryWithSearchService<Group, BranchFilter> branchService,
			ISessionContext sessionContext,
			IContractActionService contractActionService,
			IContractService contractService,
			IContractActionOperationService contractActionOperationService,
			IContractPaymentScheduleService contractPaymentScheduleService,
			IAccountService accountService,
			IDomainService domainService,
			ContractAdditionalInfoRepository сontractAdditionalInfoRepository,
            LoanProductTypeRepository loanProductTypeRepository,
			ClientDefermentRepository clientDefermentRepository,
			IClientDefermentService clientDefermentService)
		{
			_businessOperationService = businessOperationService;
			_businessOperationSettingService = businessOperationSettingService;
			_cashOrderService = cashOrderService;
			_eventLog = eventLog;
			_branchService = branchService;
			_authorId = sessionContext.IsInitialized ? sessionContext.UserId : Constants.ADMINISTRATOR_IDENTITY;
			_contractActionService = contractActionService;
			_contractActionOperationService = contractActionOperationService;
			_contractService = contractService;
			_contractPaymentScheduleService = contractPaymentScheduleService;
			_accountService = accountService;
			_domainService = domainService;
			_сontractAdditionalInfoRepository = сontractAdditionalInfoRepository;
            _loanProductTypeRepository = loanProductTypeRepository;
            _clientDefermentRepository = clientDefermentRepository;
			_clientDefermentService = clientDefermentService;
        }

		/// <summary>
		/// Расчёт суммы к начислению
		/// </summary>
		/// <param name="scheduledPercentCost">Сумма к начислению по графику</param>
		/// <param name="accrualProfit">Сумма начисленная ранее</param>
		/// <returns></returns>
		public decimal OnControlDate(decimal scheduledPercentCost, decimal accrualProfit)
		{
			return scheduledPercentCost - accrualProfit;
		}

		/// <summary>
		/// Расчёт суммы к начислению
		/// </summary>
		/// <param name="scheduledPercentCost">Сумма к начислению по графику</param>
		/// <param name="accrualProfit">Сумма начисленная ранее</param>
		/// <param name="calculatedPercentCost">Сумма к начислению подневно</param>
		/// <returns></returns>
		public decimal OnAnyDate(decimal scheduledPercentCost, decimal accrualProfit,
			decimal calculatedPercentCost)
		{
			return (calculatedPercentCost + scheduledPercentCost) - accrualProfit;
		}

		/// <summary>
		/// Начисление процентов в контрольную дату
		/// </summary>
		/// <param name="contract">Договор</param>
		/// <param name="accrualDate">Дата начисления</param>
		public void OnControlDate(IContract contract, int authorId, DateTime? accrualDate = null)
		{
			//вычисляем ScheduledPercentCost(запланированное начисление)
			var scheduledPercentCost = contract.GetSchedule().Where(x => x.Date.Date <= accrualDate.GetValueOrDefault(DateTime.Now).Date).Sum(x => x.PercentCost);

			//вычисляем AccProfit(начисленные проценты)
			decimal accProfit = CalculateAccrualProfit(contract.Id, contract.BranchId);

			var acc = OnControlDate(scheduledPercentCost, accProfit);

			if (acc > 0)
			{
				try
				{
                    var amountDict = new Dictionary<AmountType, decimal> { { AmountType.Loan, acc } };
                    RegisterBusinessOperation(contract, accrualDate.GetValueOrDefault(DateTime.Now), amountDict, authorId);
					_eventLog.Log(EventCode.ContractInterestAccrual, EventStatus.Success, EntityType.Contract, contract.Id, userId: authorId);
				}
				catch (Exception e)
				{
					_eventLog.Log(EventCode.ContractInterestAccrual, EventStatus.Failed, EntityType.Contract,
						contract.Id, responseData: JsonConvert.SerializeObject(e), userId: authorId);
					throw new PawnshopApplicationException($"Возникла ошибка при начислении: {e.Message}");
				}
			}
			else
				_eventLog.Log(EventCode.ContractInterestAccrual, EventStatus.Failed, EntityType.Contract, contract.Id, userId: authorId);
		}

		public ContractAction OnAnyDateAccrual(Contract contract, int authorId, DateTime? accrualDate = null,
            bool isFloatingDiscrete = false, IEnumerable<ContractRate> contractRates = null, decimal totalSum = 0)
		{
			var defermentInformation = _clientDefermentService.GetDefermentInformation(contract.Id, accrualDate);


			ContractAction interestAccrualAction = null;
			if(defermentInformation != null && defermentInformation.Status == Data.Models.Restructuring.RestructuringStatusEnum.Frozen)
			{
				return null;
			}
			else if(defermentInformation != null && defermentInformation.IsInDefermentPeriod && defermentInformation.Status == Data.Models.Restructuring.RestructuringStatusEnum.Restructured)
			{
                var interestAccrualModel = OnAnyDateDeferment(contract, authorId, accrualDate);
                if (interestAccrualModel != null)
				{
					try
					{
						interestAccrualAction = RegisterBusinessOperation(contract, interestAccrualModel.OperationDate, interestAccrualModel.AccrualDict, authorId);
						_eventLog.Log(EventCode.ContractInterestAccrual, EventStatus.Success, EntityType.Contract, contract.Id, interestAccrualModel.RequestLogJson, userId: authorId);
					}
					catch (Exception e)
					{
						_eventLog.Log(EventCode.ContractInterestAccrual, EventStatus.Failed, EntityType.Contract,
							contract.Id, requestData: interestAccrualModel.RequestLogJson, responseData: JsonConvert.SerializeObject(e), userId: authorId);
						throw new PawnshopApplicationException($"Возникла ошибка при начислении: {e.Message}");
					}
				}
			}
			else if(defermentInformation != null && !defermentInformation.IsInDefermentPeriod && defermentInformation.Status == Data.Models.Restructuring.RestructuringStatusEnum.Restructured)
			{
				var interestAccrualList = new List<InterestAccrualModel>();
				var mainInteresAccrual = OnAnyDate(contract, authorId, accrualDate, isFloatingDiscrete, contractRates, totalSum);
				interestAccrualList.Add(mainInteresAccrual);
                interestAccrualList.Add(OnAnyDateRestructured(contract, authorId, AmountType.AmortizedLoan, accrualDate));
                interestAccrualList.Add(OnAnyDateRestructured(contract, authorId, AmountType.AmortizedOverdueLoan, accrualDate));
				interestAccrualList.Add(OnAnyDateRestructured(contract, authorId, AmountType.AmortizedDebtPenalty, accrualDate));
				interestAccrualList.Add(OnAnyDateRestructured(contract, authorId, AmountType.AmortizedLoanPenalty, accrualDate));
				var mainDict = new Dictionary<AmountType, decimal>();
				var json = JsonConvert.SerializeObject(interestAccrualList);
                decimal penaltyLimitBalance = _contractService.GetPenaltyLimitBalance(contract.Id, accrualDate);
                foreach (var item in interestAccrualList.Where(x=> x != null))
                {
					mainDict.Add(item.AccrualDict.FirstOrDefault().Key, item.AccrualDict.FirstOrDefault().Value);
                    if (contract.UsePenaltyLimit && penaltyLimitBalance > 0 )
					{
						if (item.AccrualDict.FirstOrDefault().Key == AmountType.AmortizedDebtPenalty)
							mainDict.Add(AmountType.AmortizedDebtPenaltyLimit, Math.Min(item.AccrualDict.FirstOrDefault().Value, penaltyLimitBalance));
                        if (item.AccrualDict.FirstOrDefault().Key == AmountType.AmortizedLoanPenalty)
                            mainDict.Add(AmountType.AmortizedLoanPenaltyLimit, Math.Min(item.AccrualDict.FirstOrDefault().Value, penaltyLimitBalance));

                    }
                }

                if (interestAccrualList.Any(x=>x != null))
                {
                    try
                    {
                        interestAccrualAction = RegisterBusinessOperation(contract, interestAccrualList.FirstOrDefault(x=>x != null).OperationDate, mainDict, authorId);
                        _eventLog.Log(EventCode.ContractInterestAccrual, EventStatus.Success, EntityType.Contract, contract.Id, json, userId: authorId);
                    }
                    catch (Exception e)
                    {
                        _eventLog.Log(EventCode.ContractInterestAccrual, EventStatus.Failed, EntityType.Contract,
                            contract.Id, requestData: json, responseData: JsonConvert.SerializeObject(e), userId: authorId);
                        throw new PawnshopApplicationException($"Возникла ошибка при начислении: {e.Message}");
                    }
                }
            }
			else
			{
                var mainInterestAccrual = OnAnyDate(contract, authorId, accrualDate, isFloatingDiscrete, contractRates, totalSum);
                if(mainInterestAccrual != null)
				{
                    try
                    {
                        interestAccrualAction = RegisterBusinessOperation(contract, mainInterestAccrual.OperationDate, mainInterestAccrual.AccrualDict, authorId);
                        _eventLog.Log(EventCode.ContractInterestAccrual, EventStatus.Success, EntityType.Contract, contract.Id, mainInterestAccrual.RequestLogJson, userId: authorId);
                    }
                    catch (Exception e)
                    {
                        _eventLog.Log(EventCode.ContractInterestAccrual, EventStatus.Failed, EntityType.Contract,
                            contract.Id, requestData: mainInterestAccrual.RequestLogJson, responseData: JsonConvert.SerializeObject(e), userId: authorId);
                        throw new PawnshopApplicationException($"Возникла ошибка при начислении: {e.Message}");
                    }
                }
            }

			return interestAccrualAction;
        }

        private InterestAccrualModel OnAnyDate(Contract contract, int authorId, DateTime? accrualDate = null,
            bool isFloatingDiscrete = false, IEnumerable<ContractRate> contractRates = null, decimal totalSum = 0)
        {
            DateTime lastScheduleDate = contract.GetSchedule().Max(x => x.Date);
            DateTime closeDate = lastScheduleDate.Date > contract.MaturityDate.Date
                ? lastScheduleDate
                : contract.MaturityDate;

            DateTime operationDate = DateTime.Now;

            if (accrualDate.HasValue)
            {
                if (accrualDate.Value.Date < DateTime.Now.Date)
                {
                    operationDate = accrualDate.Value.Date.AddDays(1).AddSeconds(-1);
                }
                else if (accrualDate.Value.Date > DateTime.Now.Date)
                {
                    operationDate = accrualDate.Value.Date;
                }
            }

            //если вызывается начисление после даты окончания договора, то начисляем только по дату закрытия договора
            if (accrualDate > contract.MaturityDate)
            {
                accrualDate = closeDate;
            }

            string accrualDateString = accrualDate.GetValueOrDefault(DateTime.Now).ToString("dd.MM.yyyy");
            decimal acc = 0;
            bool calculationSuccess = false;
            ContractAction interestAccrualAction = null;
            try
            {
                var schedules = contract.GetSchedule();
                //вычисляем ScheduledPercentCost
                decimal scheduledPercentCost = schedules.Where(x => (x.Prolongated.HasValue && x.ActualDate.Value <= accrualDate.GetValueOrDefault(DateTime.Now).Date) || (!x.Prolongated.HasValue && x.Date.Date <= accrualDate.GetValueOrDefault(DateTime.Now).Date))?.Sum(x => x?.PercentCost ?? 0) ?? 0;

                //вычисляем AccProfit(уже было начислено)
                decimal accrualProfit = CalculateAccrualProfit(contract.Id, contract.BranchId);

                var nextPayment = schedules.Where(x => !x.ActualDate.HasValue && !x.ActionId.HasValue && x.Date.Date > accrualDate.GetValueOrDefault(DateTime.Now).Date && !x.Canceled.HasValue)
                    .OrderBy(x => x.Date).FirstOrDefault();
                var previousPayment = schedules.Where(x => (x.Prolongated.HasValue && x.ActualDate.Value <= accrualDate.GetValueOrDefault(DateTime.Now).Date) || (!x.Prolongated.HasValue && x.Date.Date <= accrualDate.GetValueOrDefault(DateTime.Now).Date) && !x.Canceled.HasValue)
                    .OrderByDescending(x => x.Date).FirstOrDefault();

                DateTime previousDate = contract.ContractDate;

                var contractIsOnline = _contractService.IsOnline(contract.Id);

                var isBuyCarProduct = false;
                if (contract.ProductTypeId.HasValue)
                {
                    LoanProductType productType = _loanProductTypeRepository.Get(contract.ProductTypeId.Value);
                    isBuyCarProduct = productType.Code == Constants.PRODUCT_BUYCAR;
                }

                if ((contract.CollateralType == CollateralType.Realty || contractIsOnline || isBuyCarProduct) && contract.SignDate.HasValue)
                {
                    previousDate = contract.SignDate.Value;
                }
                if (previousPayment != null)
                {
                    if (previousPayment.Prolongated.HasValue && !previousPayment.ActualDate.HasValue)
                        throw new PawnshopApplicationException($"Фактическая дата должна быть заполнена при заполненной плановой дате платежа после прологации({nameof(previousPayment.Prolongated)})");

                    previousDate = previousPayment.Prolongated.HasValue ?
                        previousPayment.ActualDate.Value : previousPayment.Date;
                }
                else
                {
                    if (contract.PercentPaymentType == PercentPaymentType.EndPeriod && !contract.Locked)
                        previousDate = previousDate.AddDays(-1);
                }

                //количество дней к начислению
                var days = (
                    accrualDate.GetValueOrDefault(DateTime.Now.Date > closeDate.Date ? closeDate : DateTime.Now).Date
                    -
                    previousDate.Date
                ).Days;

                decimal percentPerDay = 0;
                decimal calculatedPercentCost = 0;
                if (isFloatingDiscrete)
                {
                    var dateStart = contract.ContractDate;
                    var diff = ((nextPayment.Date.Year - dateStart.Year) * 12) + nextPayment.Date.Month - dateStart.Month - 1;
                    decimal prevRate = diff <= 0 ? contractRates.ElementAt(0).Rate : contractRates.ElementAt(diff).Rate;
                    calculatedPercentCost = totalSum * days * prevRate / 100;
                }
                else
                {
                    //сумма процентов в день
                    percentPerDay = (nextPayment?.PercentCost / nextPayment?.Period) ?? contract.LoanPercentCost;
                    //расчитываем сумму всех процентов за выбранный период
                    calculatedPercentCost = days * percentPerDay;
                }

                //рассчитываем сумму к текущему начислению
                acc = OnAnyDate(scheduledPercentCost, accrualProfit, calculatedPercentCost);
                acc = Math.Round(acc, 2);
                var requestLog = new { acc, calculatedPercentCost, accrualProfit, accrualDate = accrualDateString, scheduledPercentCost };
                string requestLogJson = JsonConvert.SerializeObject(requestLog);
                calculationSuccess = true;
                if (acc > 0)
				{
					return new InterestAccrualModel()
					{
						AccrualDict = new Dictionary<AmountType, decimal> { { AmountType.Loan, acc } },
						OperationDate = operationDate,
						RequestLogJson = requestLogJson,
					};
                }
                                
			}
            catch (Exception ex)
            {
                if (!calculationSuccess)
                    _eventLog.Log(EventCode.ContractInterestAccrual, EventStatus.Failed, EntityType.Contract, contract.Id, responseData: JsonConvert.SerializeObject(ex), userId: authorId);

                throw;
            }

            return null;
        }

        /// <summary>
        /// Начисление отсроченных процентов в дату входящая период отсрочки
        /// </summary>
        /// <param name="contract">Договор</param>
        /// <param name="authorId"></param>
        /// <param name="accrualDate">Дата начисления, если не заполнена - сегодня</param>
        private InterestAccrualModel OnAnyDateDeferment(Contract contract, int authorId, DateTime? accrualDate = null)
		{
			if (contract == null || contract.ClientDeferment == null)
				return null;

			if (!contract.ClientDeferment.StartDate.HasValue || !contract.ClientDeferment.EndDate.HasValue || !contract.ClientDeferment.IsRestructured)
				return null;

			DateTime operationDate = DateTime.Now;

			if (accrualDate.HasValue)
			{
				if (accrualDate.Value.Date < DateTime.Now.Date)
				{
					operationDate = accrualDate.Value.Date.AddDays(1).AddSeconds(-1);
				}
				else if (accrualDate.Value.Date > DateTime.Now.Date)
				{
					operationDate = accrualDate.Value.Date;
				}
			}
			else
			{
                accrualDate = DateTime.Now;
            }

			//если вызывается начисление после даты окончания отсрочки, то начисляем только на последний день отсрочки
			if (accrualDate > contract.ClientDeferment.EndDate.Value.Date)
			{
				accrualDate = contract.ClientDeferment.EndDate.Value;
			}

			string accrualDateString = accrualDate.GetValueOrDefault(DateTime.Now).ToString("dd.MM.yyyy");
			decimal acc = 0;
			bool calculationSuccess = false;
			ContractAction interestAccrualAction = null;
			try
			{
				//вычисляем сколько уже начислено
                var accrualProfit = CalculateAccrualProfitDeferment(contract.Id, contract.BranchId);

                var previousPayment = contract.RestructedPaymentSchedule.Where(x => x.Date < accrualDate.GetValueOrDefault(DateTime.Now).Date).OrderByDescending(x => x.Date).FirstOrDefault();

                var nextPayment = contract.RestructedPaymentSchedule.Where(x => x.Date >= accrualDate.GetValueOrDefault(DateTime.Now).Date).OrderBy(x => x.Date).FirstOrDefault();

                var deferredSchPercConst = nextPayment.AmortizedBalanceOfDefferedPercent.GetValueOrDefault(0);

				//вычесляем сколько должно было начислиться в этот текущий месяц по графику
                var monthlyPercentCost = nextPayment.AmortizedBalanceOfDefferedPercent.GetValueOrDefault(0) - previousPayment.AmortizedBalanceOfDefferedPercent.GetValueOrDefault(0);

                //количество дней к начислению
				var days = (
					accrualDate.GetValueOrDefault(DateTime.Now).Date
					-
                    previousPayment.Date.Date
				).Days;

				if(nextPayment != null && nextPayment.Period < days)
				{
					days = nextPayment.Period;
				}

				decimal percentPerDay = 0;
				decimal calculatedPercentCost = 0;
                
				percentPerDay = (monthlyPercentCost / nextPayment?.Period) ?? 0;
                //расчитываем сумму всех процентов за выбранный период
                calculatedPercentCost = days * percentPerDay;

				//рассчитываем сумму к текущему начислению
				acc = previousPayment.AmortizedBalanceOfDefferedPercent.GetValueOrDefault(0) + calculatedPercentCost - accrualProfit;
                acc = Math.Round(acc, 2);
				var requestLog = new { acc, calculatedPercentCost, accrualProfit, accrualDate = accrualDateString, deferredSchPercConst };
				string requestLogJson = JsonConvert.SerializeObject(requestLog);
				calculationSuccess = true;

				if (acc > 0)
				{
					return new InterestAccrualModel()
					{
						AccrualDict = new Dictionary<AmountType, decimal> { { AmountType.DefermentLoan, acc } },
						OperationDate = operationDate,
						RequestLogJson = requestLogJson
					};
				}
			}
			catch (Exception ex)
			{
				if (!calculationSuccess)
					_eventLog.Log(EventCode.ContractInterestAccrual, EventStatus.Failed, EntityType.Contract, contract.Id, responseData: JsonConvert.SerializeObject(ex), userId: authorId);

				throw;
			}

			return null;
		}

        /// <summary>
        /// Начисление процентов аммортизированных платежей после периода отсрочки
        /// </summary>
        /// <param name="contract">Договор</param>
        /// <param name="authorId"></param>
        /// <param name="accrualDate">Дата начисления, если не заполнена - сегодня</param>
        private InterestAccrualModel OnAnyDateRestructured(Contract contract, int authorId, AmountType type, DateTime? accrualDate = null)
        {
            DateTime lastScheduleDate = contract.GetSchedule().Max(x => x.Date);
            DateTime closeDate = lastScheduleDate.Date > contract.MaturityDate.Date
                ? lastScheduleDate
                : contract.MaturityDate;

            DateTime operationDate = DateTime.Now;

            if (accrualDate.HasValue)
            {
                if (accrualDate.Value.Date < DateTime.Now.Date)
                {
                    operationDate = accrualDate.Value.Date.AddDays(1).AddSeconds(-1);
                }
                else if (accrualDate.Value.Date > DateTime.Now.Date)
                {
                    operationDate = accrualDate.Value.Date;
                }
            }

            string accrualDateString = accrualDate.GetValueOrDefault(DateTime.Now).ToString("dd.MM.yyyy");
            decimal acc = 0;
            bool calculationSuccess = false;
            ContractAction interestAccrualAction = null;
            try
            {
                var schedules = contract.GetSchedule();
				decimal schedulePaymentCost = 0m;
                //вычисляем сумму платежей за прошлые периоды
                var pastSchedulePeriods = schedules.Where(y => y.Date <= accrualDate.GetValueOrDefault(DateTime.Now).Date);
                if (type == AmountType.AmortizedLoan)
                    schedulePaymentCost = contract.RestructedPaymentSchedule.Where(x => pastSchedulePeriods.Select(z => z.Id).Contains(x.Id)).Sum(x => x?.PaymentBalanceOfDefferedPercent ?? 0);
                else if (type == AmountType.AmortizedOverdueLoan)
                    schedulePaymentCost = contract.RestructedPaymentSchedule.Where(x => pastSchedulePeriods.Select(z => z.Id).Contains(x.Id)).Sum(x => x?.PaymentBalanceOfOverduePercent ?? 0);
                else if (type == AmountType.AmortizedDebtPenalty)
                    schedulePaymentCost = contract.RestructedPaymentSchedule.Where(x => pastSchedulePeriods.Select(z => z.Id).Contains(x.Id)).Sum(x => x?.PaymentPenaltyOfOverdueDebt ?? 0);
                else if (type == AmountType.AmortizedLoanPenalty)
                    schedulePaymentCost = contract.RestructedPaymentSchedule.Where(x => pastSchedulePeriods.Select(z => z.Id).Contains(x.Id)).Sum(x => x?.PaymentPenaltyOfOverduePercent ?? 0);

                //вычисляем AccProfit(уже было начислено)
                decimal accrualProfit = CalculateAccrualRestructured(contract.Id, contract.BranchId, type);

                var nextPayment = schedules.Where(x => x.Date > accrualDate.GetValueOrDefault(DateTime.Now).Date)
                    .OrderBy(x => x.Date).FirstOrDefault();

                var restructuredNextPayment = contract.RestructedPaymentSchedule.Where(x => x.Id == nextPayment.Id).FirstOrDefault();

                var previousPayment = schedules.Where(x => x.Date <= accrualDate.GetValueOrDefault(DateTime.Now).Date)
                    .OrderByDescending(x => x.Date).FirstOrDefault();

                DateTime previousDate = previousPayment.Date;
				//количество дней к начислению
                var days = (
                    accrualDate.GetValueOrDefault(DateTime.Now.Date > closeDate.Date ? closeDate : DateTime.Now).Date
                    -
                    previousDate.Date
                ).Days;

                decimal percentPerDay = 0;
                decimal calculatedPercentCost = 0;
                //сумма процентов в день
                if (type == AmountType.AmortizedLoan)
                    percentPerDay = (restructuredNextPayment.PaymentBalanceOfDefferedPercent / nextPayment?.Period) ?? 0;
                else if (type == AmountType.AmortizedOverdueLoan)
                    percentPerDay = (restructuredNextPayment.PaymentBalanceOfOverduePercent / nextPayment?.Period) ?? 0;
                else if (type == AmountType.AmortizedDebtPenalty)
                    percentPerDay = (restructuredNextPayment.PaymentPenaltyOfOverdueDebt / nextPayment?.Period) ?? 0;
                else if (type == AmountType.AmortizedLoanPenalty)
                    percentPerDay = (restructuredNextPayment.PaymentPenaltyOfOverduePercent / nextPayment?.Period) ?? 0;

                //расчитываем сумму всех процентов за выбранный период
                calculatedPercentCost = days * percentPerDay;

                //рассчитываем сумму к текущему начислению
                acc = OnAnyDate(schedulePaymentCost, accrualProfit, calculatedPercentCost);
                acc = Math.Round(acc, 2);
                var requestLog = new { acc, calculatedPercentCost, accrualProfit, accrualDate = accrualDateString, schedulePaymentCost, type };
                string requestLogJson = JsonConvert.SerializeObject(requestLog);
                calculationSuccess = true;
                if (acc > 0)
				{
                    return new InterestAccrualModel()
                    {
                        AccrualDict = new Dictionary<AmountType, decimal> { { type, acc } },
                        OperationDate = operationDate,
                        RequestLogJson = requestLogJson
                    };
                }
            }
            catch (Exception ex)
            {
                if (!calculationSuccess)
                    _eventLog.Log(EventCode.ContractInterestAccrual, EventStatus.Failed, EntityType.Contract, contract.Id, responseData: JsonConvert.SerializeObject(ex), userId: authorId);

                throw;
            }

            return null;
        }

        public void OnAnyDateOnOverdueDebt(IContract contract, int authorId, DateTime accrualDate)
		{
			if (contract == null)
				throw new ArgumentNullException(nameof(contract));

			if (contract.PercentPaymentType != PercentPaymentType.EndPeriod)
				throw new ArgumentException("Данный функционал предназначен только для дискретов", nameof(contract));

			if (contract.Status != ContractStatus.Signed && contract.Status != ContractStatus.SoldOut)
				throw new ArgumentException($"Поле {nameof(contract.Status)} должно быть один из ({ContractStatus.Signed}, {ContractStatus.SoldOut})", nameof(contract));

			string accrualDateString = accrualDate.ToString("dd.MM.yyyy");

			try
			{
				if (accrualDate.Date <= contract.MaturityDate)
					throw new PawnshopApplicationException("Дата начисления должна быть больше чем дата погашения");

				var accountsForContract = _accountService.GetAccountsForContractByAccrualTypeInterestAccrualOnOverdueDebt(contract.Id);
				string requestLogJson = JsonConvert.SerializeObject(new { accrualDate = accrualDateString });
				foreach (var account in accountsForContract)
				{
					decimal percentCostOnOverdueDebtForAccrual = 0;

					IEnumerable<(DateTime, decimal)> balances = _accountService.CalculateForInterestAccrualOnOverdueDebt(account.Id, accrualDate);

					if (balances == null || !balances.Any())
					{
						_eventLog.Log(EventCode.ContractInterestAccrualOnOverdueDebt, EventStatus.Failed,
							EntityType.Contract, contract.Id, requestLogJson, "В базе не найдены суммы для начисления",
							userId: authorId);
						return;
					}

					Queue<(DateTime, decimal)> balanceQueue = new Queue<(DateTime, decimal)>(balances.OrderBy(t => t.Item1));
					var calculatedPercentCostOnOverdueDebtCost = CalculatePercentCostOnOverdueDebt(balanceQueue, contract.LoanPercent);

					if (calculatedPercentCostOnOverdueDebtCost <= 0)
					{
						return;
					}

					var businessOperationSettings = new List<string>()
							{
								Constants.BO_SETTING_INTEREST_ACCRUAL_OVERDUEDEBT
							};

					var actualPercentCostOnOverDueDebt = _cashOrderService.GetSumOfCashOrderCostByBusinessOperationSettingCodesAndContractId(businessOperationSettings, contract.Id, accrualDate);

					if (actualPercentCostOnOverDueDebt < 0)
						throw new PawnshopApplicationException(
							$"Сумма сторнированных ордеров(договор {contract.Id}) по начислению процентов превышает сумму начисленных процентов на просроченный ОД");

					percentCostOnOverdueDebtForAccrual = Math.Round(calculatedPercentCostOnOverdueDebtCost - actualPercentCostOnOverDueDebt, 2);
					requestLogJson = JsonConvert.SerializeObject(new { percentCostOnOverdueDebtForAccrual, accrualDate = accrualDateString });

					if (percentCostOnOverdueDebtForAccrual <= 0)
					{
						_eventLog.Log(EventCode.ContractInterestAccrualOnOverdueDebt, EventStatus.Success,
							EntityType.Contract, contract.Id, requestLogJson,
							$"Начисление не было произведено, так как сумма начисления - {percentCostOnOverdueDebtForAccrual}",
							userId: authorId);
						return;
					}

					try
					{
						RegisterBusinessOperationForAccrualOnOverdueDebt(contract, accrualDate,
							percentCostOnOverdueDebtForAccrual, authorId);
						_eventLog.Log(EventCode.ContractInterestAccrualOnOverdueDebt, EventStatus.Success,
							EntityType.Contract, contract.Id, requestLogJson, userId: authorId);
					}
					catch (Exception e)
					{
						_eventLog.Log(EventCode.ContractInterestAccrualOnOverdueDebt, EventStatus.Failed,
							EntityType.Contract,
							contract.Id, requestData: requestLogJson, responseData: JsonConvert.SerializeObject(e),
							userId: authorId);
						throw new PawnshopApplicationException($"Возникла ошибка при начислении: {e.Message}");
					}
				}
			}
			catch (Exception ex)
			{
				_eventLog.Log(EventCode.ContractInterestAccrualOnOverdueDebt, EventStatus.Failed, EntityType.Contract, contract.Id, responseData: JsonConvert.SerializeObject(ex), userId: authorId);
				throw;
			}
		}

		public void ManualInterestAccrualOnOverdueDebt(IContract contract, int authorId, DateTime accrualDate)
		{
			var done = false;
			if (contract.NextPaymentDate.HasValue)
			{
				var nextPaymentDate = contract.NextPaymentDate.Value.MaxOf(Constants.INTEREST_ACCRUAL_ON_OVERDUE_DEBT_DATE);

				var lastAccrualDate =
					_contractActionService.FindLastDateInterestAccrualOnOverdue(nextPaymentDate,
						accrualDate, contract.Id);

				lastAccrualDate = lastAccrualDate.MaxOf(nextPaymentDate);

				if (lastAccrualDate != default && lastAccrualDate.Date != accrualDate.Date)
				{
					while (lastAccrualDate < accrualDate)
					{
						lastAccrualDate = lastAccrualDate.AddDays(1);
						OnAnyDateOnOverdueDebt(contract, authorId, lastAccrualDate);
					}

					done = true;
				}
			}

			if (!done)
				OnAnyDateOnOverdueDebt(contract, authorId, accrualDate);
		}

        private decimal CalculatePercentCostOnOverdueDebt(Queue<(DateTime, decimal)> balanceQueue, decimal loanPercent)
        {
            decimal percentCost = 0;

            while (balanceQueue.Count > 1)
            {
                var firstOverdue = balanceQueue.Dequeue();
                var secondOverdue = balanceQueue.Peek();

                percentCost += (secondOverdue.Item1.Date - firstOverdue.Item1.Date).Days * secondOverdue.Item2 * loanPercent / 100;
            }

            return percentCost;
        }

        /// <summary>
        /// Расчёт уже начисленной прибыли
        /// </summary>
        /// <param name="contractId">Договор</param>
        /// <returns></returns>
        private decimal CalculateAccrualProfit(int contractId, int branchId)
		{
			string _operationSettingCode = "INTEREST_ACCRUAL";
			string interestAccrualOverduedebtSettingCode = Constants.BO_SETTING_INTEREST_ACCRUAL_OVERDUEDEBT;
			string holidayProfitSettingCode = Constants.BO_SETTING_INTEREST_ACCRUAL_ON_HOLIDAYS;
			var _operationSettingCodeMigration = "INTEREST_ACCRUAL_MIGRATION";
			List<BusinessOperationSetting> operationSettings = new List<BusinessOperationSetting>();
			operationSettings.AddRange(_businessOperationSettingService.List(new ListQueryModel<BusinessOperationSettingFilter>
			{
				Model = new BusinessOperationSettingFilter
				{
					Code = _operationSettingCode,
					IsActive = true
				}
			}).List);

			//действующие настройки для миграций
			operationSettings.AddRange(_businessOperationSettingService.List(new ListQueryModel<BusinessOperationSettingFilter>
			{
				Model = new BusinessOperationSettingFilter
				{
					Code = _operationSettingCodeMigration,
					IsActive = true
				}
			}).List);

			operationSettings.AddRange(_businessOperationSettingService.List(new ListQueryModel<BusinessOperationSettingFilter>
			{
				Model = new BusinessOperationSettingFilter
				{
					Code = holidayProfitSettingCode,
					IsActive = true
				}
			}).List);

			//действующие настройки для миграций
			operationSettings.AddRange(_businessOperationSettingService.List(new ListQueryModel<BusinessOperationSettingFilter>
			{
				Model = new BusinessOperationSettingFilter
				{
					Code = interestAccrualOverduedebtSettingCode,
					IsActive = true
				}
			}).List);

			if (!operationSettings.Any())
				throw new PawnshopApplicationException($"Настройки бизнес-операции \"{_operationSettingCode}\" или \"{_operationSettingCodeMigration}\" не найдены");

			//вычисляем AccProfit
			decimal accProfit = 0;
			foreach (var setting in operationSettings)
			{
				var orders = _cashOrderService.List(new ListQueryModel<CashOrderFilter>
				{
					Page = null,
					Model = new CashOrderFilter
					{
						BusinessOperationSettingId = setting.Id,//TODO:настройка бизнес-операции
						ContractId = contractId,
						OwnerId = branchId
					}
				})
					.List;

				decimal accProfitToAdd = orders?.Sum(x => x.StornoId.HasValue ? -x.OrderCost : x.OrderCost) ?? 0;
				accProfit += accProfitToAdd;
			}

			return accProfit;
		}

        /// <summary>
        /// Расчёт уже начисленной прибыли в период отсрочки
		/// При условий что договор реструктуризирован
        /// </summary>
        /// <param name="contractId">Договор</param>
        /// <returns></returns>
        private decimal CalculateAccrualProfitDeferment(int contractId, int branchId)
        {
            string _operationSettingCode = "DEFERMENT_PROFIT_INTEREST_ACCRUAL";
            List<BusinessOperationSetting> operationSettings = new List<BusinessOperationSetting>();
            operationSettings.AddRange(_businessOperationSettingService.List(new ListQueryModel<BusinessOperationSettingFilter>
            {
                Model = new BusinessOperationSettingFilter
                {
                    Code = _operationSettingCode,
                    IsActive = true
                }
            }).List);

            if (!operationSettings.Any())
                throw new PawnshopApplicationException($"Настройки бизнес-операции \"{_operationSettingCode}\" не найден");

            decimal accProfit = 0;
            foreach (var setting in operationSettings)
            {
                var orders = _cashOrderService.List(new ListQueryModel<CashOrderFilter>
                {
                    Page = null,
                    Model = new CashOrderFilter
                    {
                        BusinessOperationSettingId = setting.Id,
                        ContractId = contractId,
                        OwnerId = branchId
                    }
                }).List;

                decimal accProfitToAdd = orders?.Sum(x => x.StornoId.HasValue ? -x.OrderCost : x.OrderCost) ?? 0;
                accProfit += accProfitToAdd;
            }

            return accProfit;
        }

        /// <summary>
        /// Расчёт уже начисленной прибыли после периода отсрочки
        /// При условий что договор реструктуризирован
        /// </summary>
        /// <param name="contractId">Договор</param>
        /// <returns></returns>
        private decimal CalculateAccrualRestructured(int contractId, int branchId, AmountType type)
        {
			string _operationSettingCode = "";

			if (type == AmountType.AmortizedLoan)
				_operationSettingCode = "AMORTIZED_PROFIT_INTEREST_ACCRUAL";
			else if (type == AmountType.AmortizedOverdueLoan)
				_operationSettingCode = "AMORTIZED_OVERDUE_PROFIT_INTEREST_ACCRUAL";
			else if (type == AmountType.AmortizedDebtPenalty)
				_operationSettingCode = "AMORTIZED_DEBT_PENALTY_ACCRUAL";
			else if (type == AmountType.AmortizedLoanPenalty)
				_operationSettingCode = "AMORTIZED_LOAN_PENALTY_ACCRUAL";

			if (_operationSettingCode == "")
				return -1;

            List<BusinessOperationSetting> operationSettings = new List<BusinessOperationSetting>();
            operationSettings.AddRange(_businessOperationSettingService.List(new ListQueryModel<BusinessOperationSettingFilter>
            {
                Model = new BusinessOperationSettingFilter
                {
                    Code = _operationSettingCode,
                    IsActive = true
                }
            }).List);

            if (!operationSettings.Any())
                throw new PawnshopApplicationException($"Настройки бизнес-операции \"{_operationSettingCode}\" не найден");

            decimal accProfit = 0;
            foreach (var setting in operationSettings)
            {
                var orders = _cashOrderService.List(new ListQueryModel<CashOrderFilter>
                {
                    Page = null,
                    Model = new CashOrderFilter
                    {
                        BusinessOperationSettingId = setting.Id,
                        ContractId = contractId,
                        OwnerId = branchId
                    }
                }).List;

                decimal accProfitToAdd = orders?.Sum(x => x.StornoId.HasValue ? -x.OrderCost : x.OrderCost) ?? 0;
                accProfit += accProfitToAdd;
            }

            return accProfit;
        }

        /// <summary>
        /// Регистрация бизнес-операции
        /// </summary>
        /// <param name="contract">Договор</param>
        /// <param name="accrualDate">Дата начисления</param>
        /// <param name="acc">Сумма к начислению</param>
        private ContractAction RegisterBusinessOperation(IContract contract, DateTime accrualDate, Dictionary<AmountType, decimal> amountDict, int authorId)
		{
			ContractAction action = new ContractAction
			{
				ActionType = ContractActionType.InterestAccrual,
				AuthorId = _authorId,
				TotalCost = amountDict.Select(x=>x.Value).Sum(),
				ContractId = contract.Id,
				CreateDate = DateTime.Now,
				Date = accrualDate,
				Reason = $"Начисление процентов для {contract.ContractNumber}"
			};

			var branch = _branchService.GetAsync(contract.BranchId).Result;

			using (var transaction = _contractActionService.BeginContractActionTransaction())
			{
				_contractActionService.Save(action);
				_businessOperationService.Register(contract, accrualDate, contract.IsOffBalance ? Constants.BO_INTEREST_ACCRUAL_OFFBALANCE : Constants.BO_INTEREST_ACCRUAL, branch, _authorId, amountDict, action: action);
				_contractActionOperationService.Register(contract, action, authorId, branchId: branch.Id, callActionRowBusinessOperation: false);
				transaction.Commit();
			}

			return action;
		}

		/// <summary>
		/// Регистрация бизнес-операции для начисления на выходных
		/// </summary>
		/// <param name="contract">Договор</param>
		/// <param name="accrualDate">Дата начисления</param>
		/// <param name="acc">Сумма к начислению</param>
		private void RegisterBusinessOperationForAccrualOnOverdueDebt(IContract contract, DateTime accrualDate, decimal acc, int authorId)
		{
			ContractAction action = new ContractAction
			{
				ActionType = ContractActionType.InterestAccrualOnOverdueDebt,
				AuthorId = _authorId,
				TotalCost = acc,
				ContractId = contract.Id,
				CreateDate = DateTime.Now,
				Date = accrualDate,
				Reason = $"Начисление процентов для {contract.ContractNumber}"
			};

			var branch = _branchService.GetAsync(contract.BranchId).Result;

			using (var transaction = _contractActionService.BeginContractActionTransaction())
			{
				var amountDict = new Dictionary<AmountType, decimal> { { AmountType.Loan, acc } };
				_contractActionService.Save(action);

				var code = Constants.BO_INTEREST_ACCRUAL_OVERDUEDEBT;

				if (contract.IsOffBalance)
					code = string.Concat(code, Constants.BO_OFFBALANCE_POSTFIX);

				_businessOperationService.Register(contract, accrualDate, code, branch, _authorId, amountDict, action: action);
				_contractActionOperationService.Register(contract, action, authorId, branchId: branch.Id, callActionRowBusinessOperation: false);
				Contract contractFromDB = _contractService.Get(contract.Id);
				if (contractFromDB == null)
					throw new PawnshopApplicationException($"Договор {contract.Id} не найден");

				if (contractFromDB.MaturityDate.Date != contract.MaturityDate.Date)
					throw new PawnshopApplicationException($"Дата погашения договора {contract.Id} не соответствует полученным данным");

				ContractPaymentSchedule currentPayment = contractFromDB.PaymentSchedule.Where(
					x => !x.ActualDate.HasValue
					&& !x.Canceled.HasValue
					&& !x.ActionId.HasValue
					&& x.Date.Date == contract.MaturityDate.Date)
					.OrderBy(x => x.Date).FirstOrDefault();

				if (currentPayment == null)
					throw new PawnshopApplicationException("График текущего платежа не найден");

				currentPayment.PercentCost += acc;
				_contractPaymentScheduleService.Save(contractFromDB.PaymentSchedule, contractFromDB.Id, authorId);
				transaction.Commit();
			}
		}
	}
}