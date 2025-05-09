using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.ClientDeferments.Interfaces;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Dictionaries;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.AccountingCore
{
    public class TakeAwayToDelayService : IService, ITakeAwayToDelay
    {
        private readonly IAccountService _accountService;
        private readonly IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> _accountSettingService;
        private readonly IBusinessOperationService _businessOperationService;
        private readonly IDictionaryWithSearchService<Group, BranchFilter> _branchService;
        private readonly int _authorId;
        private readonly IContractActionService _contractActionService;
        private readonly IContractActionOperationService _contractActionOperationService;
        private readonly IEventLog _eventLog;
        private readonly IHolidayService _holidayService;
        private readonly IContractPaymentScheduleService _contractPaymentScheduleService;
        private readonly IContractService _contractService;
        private readonly ICashOrderService _cashOrderService;
        private readonly IClientDefermentService _clientDefermentService;

        public TakeAwayToDelayService(IAccountService accountService,
            IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> accountSettingService,
            IBusinessOperationService businessOperationService,
            IDictionaryWithSearchService<Group, BranchFilter> branchService,
            ISessionContext sessionContext, IEventLog eventLog,
            IContractActionService contractActionService,
            IContractActionOperationService contractActionOperationService,
            IHolidayService holidayService,
            IContractPaymentScheduleService contractPaymentScheduleService,
            IContractService contractService, 
            ICashOrderService cashOrderService,
            IClientDefermentService clientDefermentService)
        {
            _accountService = accountService;
            _accountSettingService = accountSettingService;
            _businessOperationService = businessOperationService;
            _branchService = branchService;
            _authorId = sessionContext.IsInitialized ? sessionContext.UserId : Constants.ADMINISTRATOR_IDENTITY;
            _contractActionService = contractActionService;
            _contractActionOperationService = contractActionOperationService;
            _holidayService = holidayService;
            _contractPaymentScheduleService = contractPaymentScheduleService;
            _contractService = contractService;
            _eventLog = eventLog;
            _cashOrderService = cashOrderService;
            _clientDefermentService = clientDefermentService;
        }

        public void TakeAwayToDelay(IContract contract, DateTime? takeAwayDate, DateTime? valueDate, int authorId, bool isMigration = false)
        {
            ValidateDates(takeAwayDate, valueDate);

            var defermentInformation = _clientDefermentService.GetDefermentInformation(contract.Id, valueDate);
            if (defermentInformation != null && 
                ((defermentInformation.IsInDefermentPeriod && defermentInformation.Status == Data.Models.Restructuring.RestructuringStatusEnum.Restructured) || 
                defermentInformation.Status == Data.Models.Restructuring.RestructuringStatusEnum.Frozen))
            {
                return;
            }
            
            var scheduleItem = ValidateSchedule(contract, takeAwayDate.Value);

            if (ValidateHoliday(contract, valueDate.Value, authorId))
            {
                return;
            }

            List<Account> accounts = _accountService.List(new ListQueryModel<AccountFilter>
            { Model = new AccountFilter { ContractId = contract.Id, IsOpen = true } }).List;
            Group branch = _branchService.GetAsync(contract.BranchId).Result;
            var requestData1 = new { takeAwayDate, authorId, isMigration };
            string requestData = JsonConvert.SerializeObject(requestData1);
            var profitOffBalanceSettings = new List<AccountSetting>();
            try
            {
                IDictionary<AmountType, decimal> amounts = new Dictionary<AmountType, decimal>();

                //настройки начисленных процентов
                var profitSettings = _accountSettingService.List(new ListQueryModel<AccountSettingFilter>
                {
                    Model = new AccountSettingFilter
                    {
                        Code = "PROFIT"
                    }
                }).List;

                if (contract.IsOffBalance)
                {
                    profitOffBalanceSettings.AddRange(_accountSettingService.List(new ListQueryModel<AccountSettingFilter>
                    {
                        Model = new AccountSettingFilter
                        {
                            Code = "PROFIT_OFFBALANCE"
                        }
                    }).List);
                }

                //настройки основного долга
                var accountSettings = _accountSettingService.List(new ListQueryModel<AccountSettingFilter>
                {
                    Model = new AccountSettingFilter
                    {
                        Code = "ACCOUNT"
                    }
                }).List;

                decimal profitToOverdue = 0;//Начисленные проценты
                decimal accountToOverdue = 0;//Основной долг
                decimal profitOffBalanceToOverdue = 0;//Основной долг

                decimal paidProfit = 0;
                decimal paidAccount = 0;
                if (contract.PercentPaymentType != PercentPaymentType.EndPeriod)
                {
                    paidProfit = GetPaidAmountOnDayoff(contract.Id, scheduleItem, new List<string> { Constants.BO_SETTING_PAYMENT_PROFIT});
                    paidAccount = GetPaidAmountOnDayoff(contract.Id, scheduleItem, new List<string> { Constants.BO_SETTING_PAYMENT_ACCOUNT });
                }

                foreach (var setting in profitSettings)
                {
                    foreach (var account in accounts.Where(x => x.AccountSettingId == setting.Id))
                    {
                        profitToOverdue += Math.Abs(_accountService.GetAccountBalance(account.Id, scheduleItem.Date));
                    }
                }

                profitToOverdue -= paidProfit;

                foreach (var setting in accountSettings)
                {
                    foreach (var account in accounts.Where(x => x.AccountSettingId == setting.Id))
                    {
                        accountToOverdue += Math.Abs(_accountService.GetAccountBalance(account.Id, scheduleItem.Date)) - scheduleItem.DebtLeft;
                        var requestData2 = new { takeAwayDate, authorId, branchId = branch.Id, amounts, scheduleItem, accountToOverdue, profitToOverdue };
                        requestData = JsonConvert.SerializeObject(requestData2);
                    }
                }

                accountToOverdue -= paidAccount;

                foreach (var setting in profitOffBalanceSettings)
                {
                    foreach (var account in accounts.Where(x => x.AccountSettingId == setting.Id))
                    {
                        profitOffBalanceToOverdue += Math.Abs(_accountService.GetAccountBalance(account.Id, scheduleItem.Date));
                    }
                }

                profitToOverdue = Math.Round(profitToOverdue, 2);
                accountToOverdue = Math.Round(accountToOverdue, 2);
                profitOffBalanceToOverdue = Math.Round(profitOffBalanceToOverdue, 2);

                if (accountToOverdue < 0)
                    throw new PawnshopApplicationException($"{nameof(accountToOverdue)} не должен быть меньше нуля, на просрочку вынесена сумма больше чем по графику");

                if (profitToOverdue > 0)
                    amounts.Add(AmountType.OverdueLoan, profitToOverdue);

                if (accountToOverdue > 0) 
                    amounts.Add(AmountType.OverdueDebt, accountToOverdue);

                if(profitOffBalanceToOverdue != 0)
                    amounts.Add(AmountType.OverdueLoanOffBalance, Math.Abs(profitOffBalanceToOverdue));

                var requestData3 = new { takeAwayDate, authorId, branchId = branch.Id, amounts, scheduleItem, accountToOverdue, profitToOverdue };
                requestData = JsonConvert.SerializeObject(requestData3);
                if (amounts.Any())
                {
                    RegisterBusinessOperation(contract, scheduleItem, branch, valueDate.Value, amounts, false, authorId);
                    _eventLog.Log(EventCode.TakeAwayToDelay, EventStatus.Success, EntityType.Contract, contract.Id, requestData: requestData, userId: authorId);
                    string amountsJson = JsonConvert.SerializeObject(amounts);
                }

            }
            catch (Exception ex)
            {
                _eventLog.Log(EventCode.TakeAwayToDelay, EventStatus.Failed, EntityType.Contract, contract.Id, requestData: requestData, responseData: JsonConvert.SerializeObject(ex), userId: authorId);
                throw new PawnshopApplicationException($"При выносе на просрочке договора {contract.Id} произошла ошибка, для подробной ошибки зайдите в журнал событий");
            }
        }
        
        /// <summary>
        /// Возвращает оплаченную сумму срочных процентов в период с КД (не включительно) до момента выноса
        /// Считает сумму только для договоров с переносом КД в выходной день
        /// </summary>
        /// <param name="contractId">Id договора</param>
        /// <param name="scheduleItem">Пункт графика, по которому выполняется вынос на просрочку</param>
        /// <returns></returns>
        private decimal GetPaidAmountOnDayoff(int contractId, IPaymentScheduleItem scheduleItem, List<string> operationCodes)
        {
            decimal result = 0;

            if (!scheduleItem.NextWorkingDate.HasValue || operationCodes.Count() == 0)
            {
                return result;
            }

            result = _cashOrderService.GetContractTotalOperationAmount(contractId,
                                                                       operationCodes,
                                                                       scheduleItem.Date.AddDays(1),
                                                                       scheduleItem.NextWorkingDate.Value.AddDays(1)).Result;
            return result;
        }

        private bool ValidateHoliday(IContract contract, DateTime valueDate, int authorId)
        {
            DateTime? nextWorkingDate;
            var isHoliday = _holidayService.IsHoliday(valueDate, out nextWorkingDate);

            if (isHoliday && valueDate >= Constants.TAKEAWAY_DATE_MOVE_HOLIDAY)
            {
                if (!nextWorkingDate.HasValue)
                {
                    nextWorkingDate = valueDate;
                }
                MoveDateToNextDay(contract.Id, valueDate, nextWorkingDate.Value, authorId);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Перенос на следующий день в случае когда текущая дата выпадает на выходной день
        /// </summary>
        private void MoveDateToNextDay(int contractId, DateTime date, DateTime nextWorkingDate, int authorId)
        {
            var scheduleItems = _contractPaymentScheduleService.GetListByContractId(contractId).ToList();
            if (scheduleItems.Where(x => x.Date == date && x.ActualDate == null && x.Canceled == null).ToList().Count == 0)
            {
                return;
            }
            Contract contract = _contractService.GetOnlyContract(contractId);
            

            foreach (var scheduleItem in scheduleItems.Where(x => x.Date == date && x.ActualDate == null && x.Canceled == null))
            {

                if (scheduleItem.NextWorkingDate.HasValue && contract.PercentPaymentType != PercentPaymentType.EndPeriod)
                {
                    return;
                }
                
                if (contract.PercentPaymentType == PercentPaymentType.EndPeriod)
                {
                    scheduleItem.PercentCost += contract.LoanPercentCost;
                    scheduleItem.Date = date.AddDays(1);
                    contract.NextPaymentDate = date.AddDays(1);
                    contract.MaturityDate = date.AddDays(1);
                }
                scheduleItem.NextWorkingDate = scheduleItem.NextWorkingDate.HasValue ? scheduleItem.NextWorkingDate : nextWorkingDate;
            }
            
            ContractAction action = new ContractAction
            {
                ActionType = ContractActionType.MoveScheduleToNextDate,
                AuthorId = _authorId,
                TotalCost = 0,
                ContractId = contract.Id,
                CreateDate = DateTime.Now,
                Date = date,
                Reason = $"Перенос КД на следующий день в связи с выходным днем {contract.ContractNumber}"
            };
            
            contract.Positions = _contractService.GetPositionsByContractId(contractId);

            var requestData = new { date, authorId };
            string requestDataSerialized = JsonConvert.SerializeObject(requestData);
            
            using (var transaction = _contractActionService.BeginContractActionTransaction())
            {
                try
                {
                    _contractPaymentScheduleService.Save(scheduleItems, contractId, authorId);
                    _contractService.Save(contract);
                    _contractActionService.Save(action);
                    _eventLog.Log(EventCode.MoveControlDateToNextDay, EventStatus.Success, EntityType.Contract ,contract.Id, requestData: requestDataSerialized, userId: authorId);
                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    _eventLog.Log(EventCode.MoveControlDateToNextDay, EventStatus.Failed, EntityType.Contract, contract.Id, requestData: requestDataSerialized, responseData: JsonConvert.SerializeObject(ex), userId: authorId);
                    throw new PawnshopApplicationException($"При изменении контрольной даты договора {contract.Id} произошла ошибка, для подробной ошибки зайдите в журнал событий");
                }
            }
        }

        private void ValidateDates (DateTime? takeAwayDate, DateTime? valueDate)
        {
            if (!takeAwayDate.HasValue) 
            {
                takeAwayDate = DateTime.Now.AddDays(-1);
            }

            if (!valueDate.HasValue)
            {
                valueDate = takeAwayDate.Value;
            }
        }
        
        private IPaymentScheduleItem ValidateSchedule(IContract contract, DateTime takeAwayDate)
        {
            var scheduleItem = contract.GetSchedule()
                .FirstOrDefault(x => x.Date.Date == takeAwayDate.Date);
            if (scheduleItem == default)
            {
                throw new PawnshopApplicationException(
                    $"Не найдена оплата по графику платежей на {takeAwayDate.Date:dd.MM.yyyy} ");
            }
            return scheduleItem;
        }
        
        private void RegisterBusinessOperation(IContract contract, IPaymentScheduleItem scheduleItem, Group branch, DateTime valueDate, IDictionary<AmountType, decimal> amounts, bool isMigration, int authorId)
        {
            DateTime date = valueDate.Date.AddDays(1).AddSeconds(-1);

            ContractAction action = new ContractAction
            {
                ActionType = ContractActionType.MoveToOverdue,
                AuthorId = _authorId,
                TotalCost = amounts.Values.Sum(),
                ContractId = contract.Id,
                CreateDate = DateTime.Now,
                Date = date,
                Reason = $"Вынос на просрочку для {contract.ContractNumber}"
            };

            using (var transaction = _contractActionService.BeginContractActionTransaction())
            {
                _contractActionService.Save(action);
                _businessOperationService.Register(contract, date, contract.IsOffBalance ? "MOVE_TO_OVERDUE.OFFBALANCE" : "MOVE_TO_OVERDUE", branch, _authorId, amounts, action: action, isMigration: isMigration);
                _contractActionOperationService.Register(contract, action, authorId, branchId: branch.Id, callActionRowBusinessOperation: false);
                transaction.Commit();
            }
        }
    }
}
