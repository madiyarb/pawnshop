using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Crm;
using Pawnshop.Services.PaymentSchedules;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Pawnshop.Core;
using Pawnshop.Services.Restructuring;

namespace Pawnshop.Services.Contracts.PartialPayment
{
    public class ContractActionPartialPaymentService : IContractActionPartialPaymentService
    {
        private readonly IContractActionOperationService _contractActionOperationService;
        private readonly IContractService _contractService;
        private readonly ICrmPaymentService _crmPaymentService;
        private readonly IContractActionService _contractActionService;
        private readonly IContractPaymentScheduleService _contractPaymentScheduleService;
        private readonly ContractRepository _contractRepository;
        private readonly GroupRepository _groupRepository;
        private readonly LoanPercentRepository _loanPercentRepository;
        private readonly IPaymentScheduleService _paymentScheduleService;
        private readonly IRestructuringService _restructuringService;

        private string _badDateError = "Дата не может быть меньше даты последнего действия по договору";

        public ContractActionPartialPaymentService(
            IContractActionOperationService contractActionOperationService,
            IContractService contractService,
            ICrmPaymentService crmPaymentService,
            IContractActionService contractActionService,
            ContractRepository contractRepository,
            GroupRepository groupRepository,
            LoanPercentRepository loanPercentRepository,
            IContractPaymentScheduleService contractPaymentScheduleService,
            IPaymentScheduleService paymentScheduleService,
            IRestructuringService restructuringService)
        {
            _contractRepository = contractRepository;
            _contractActionOperationService = contractActionOperationService;
            _contractService = contractService;
            _crmPaymentService = crmPaymentService;
            _contractActionService = contractActionService;
            _groupRepository = groupRepository;
            _loanPercentRepository = loanPercentRepository;
            _contractPaymentScheduleService = contractPaymentScheduleService;
            _paymentScheduleService = paymentScheduleService;
            _restructuringService = restructuringService;
        }

        public async Task<ContractAction> Exec(ContractAction action, int authorId, int branchId, bool unsecuredContractSignNotallowed, bool forceExpensePrepaymentReturn)
         {

            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (action.ContractId <= 0)
                throw new ArgumentException();

            Group branch = _groupRepository.Get(branchId);
            if (branch == null)
                throw new PawnshopApplicationException($"Филиал {branchId} не найден");

            var parentContract = _contractService.Get(action.ContractId);
            if (parentContract.Status != ContractStatus.Signed)
                throw new InvalidOperationException();

            if (parentContract.Actions.Any() && action.Date.Date < parentContract.Actions.Max(x => x.Date).Date)
                throw new PawnshopApplicationException(_badDateError);

            decimal contractDue = 0;
            contractDue += _contractService.GetAccountBalance(action.ContractId, action.Date);
            contractDue += _contractService.GetOverdueAccountBalance(action.ContractId, action.Date);
            contractDue += _contractService.GetProfitBalance(action.ContractId, action.Date);
            contractDue += _contractService.GetOverdueProfitBalance(action.ContractId, action.Date);
            contractDue += _contractService.GetPenyAccountBalance(action.ContractId, action.Date);
            contractDue += _contractService.GetPenyProfitBalance(action.ContractId, action.Date);

            var defermentProfit = _contractService.GetContractAccountBalance(action.ContractId, Constants.DEFERMENT_PROFIT_ACCOUNT, action.Date);
            var amortizedProfit = _contractService.GetContractAccountBalance(action.ContractId, Constants.AMORTIZED_PROFIT_ACCOUNT, action.Date);
            var amortizedPenyAccount = _contractService.GetContractAccountBalance(action.ContractId, Constants.AMORTIZED_PENY_ACCOUNT_ACCOUNT, action.Date);
            var amortizedPenyProfit = _contractService.GetContractAccountBalance(action.ContractId, Constants.AMORTIZED_PENY_PROFIT_ACCOUNT, action.Date);
            contractDue += defermentProfit;
            contractDue += amortizedProfit;
            contractDue += amortizedPenyAccount;
            contractDue += amortizedPenyProfit;

            decimal defermentProfitCost = 0m;
            decimal amortizedProfitCost = 0m;
            decimal amortizedPenyAccountCost = 0m;
            decimal amortizedPenyProfitCost = 0m;


            if (action.Cost >= contractDue)
                throw new PawnshopApplicationException("Воспользуйтесь функционалом выкупа, сумма больше основного долга!");

            if (parentContract.ContractTransfers?.FirstOrDefault(t => t.BackTransferDate is null) != null)
                throw new PawnshopApplicationException("Данный функционал не предусмотрен для переданных договоров");

            if (parentContract.SettingId.HasValue)
            {
                var product = _loanPercentRepository.Get(parentContract.SettingId.Value);
                var errors = new List<string>();
                if (product.PartialPaymentRequiredSum.HasValue && action.Cost < product.PartialPaymentRequiredSum)
                {
                    errors.Add($"Ограничение суммы для ЧДП, минимум {product.PartialPaymentRequiredSum}");
                }

                if (product.PartialPaymentRequiredPercent.HasValue && action.Cost < (parentContract.LeftLoanCost * (product.PartialPaymentRequiredPercent / 100)))
                {
                    errors.Add($"Ограничение суммы для ЧДП, минимум {(parentContract.LeftLoanCost * (product.PartialPaymentRequiredPercent / 100))} ({product.PartialPaymentRequiredPercent}%)");
                }

                if (errors.Count > 0)
                {
                    throw new PawnshopApplicationException(errors.ToArray());
                }
            }

            using (var transaction = _contractRepository.BeginTransaction())
            {
                _crmPaymentService.Enqueue(parentContract);
                _contractActionOperationService.Register(parentContract, action, authorId, branchId: branchId, forceExpensePrepaymentReturn: forceExpensePrepaymentReturn);

                contractDue = 0;
                contractDue += _contractService.GetAccountBalance(action.ContractId, action.Date);
                contractDue += _contractService.GetOverdueAccountBalance(action.ContractId, action.Date);

                var debtRow = action.Rows.FirstOrDefault(x => x.PaymentType == AmountType.Debt);
                parentContract.LoanCost = contractDue - (debtRow != null ? debtRow.Cost : 0);

                //увеличиваем лимит кредитной линии на сумму ОД оплаты
                if (parentContract.ContractClass == ContractClass.Tranche)
                {
                    var creditLineContract = _contractRepository.Get(parentContract.CreditLineId.Value);
                    decimal cost = action.Rows.Where(x => x.PaymentType == AmountType.Debt || x.PaymentType == AmountType.OverdueDebt).Sum(x => x.Cost);
                    if (cost > 0)
                    {
                        ContractActionRow creditLineActionRow = new ContractActionRow
                        {
                            PaymentType = AmountType.CreditLineLimit,
                            Cost = cost
                        };
                        var creditLineActionRows = new List<ContractActionRow>();
                        creditLineActionRows.Add(creditLineActionRow);
                        ContractAction creditLineAction = new ContractAction()
                        {
                            ActionType = ContractActionType.Payment,
                            Date = action.Date,
                            Reason = $"Увеличение лимита КЛ при частичном погашении договора займа {parentContract.ContractNumber} от {parentContract.ContractDate.ToString("dd.MM.yyyy")}",
                            TotalCost = cost,
                            Cost = cost,
                            ContractId = creditLineContract.Id,
                            ParentActionId = action.Id,
                            ParentAction = action,
                            PayTypeId = action.PayTypeId ?? null,
                            RequisiteId = action.RequisiteId ?? null,
                            RequisiteCost = action.PayTypeId.HasValue ? (int)action.Cost : 0,
                            Checks = action.Checks,
                            Files = action.Files
                        };
                        creditLineAction.Rows = creditLineActionRows.ToArray();
                        _contractActionOperationService.Register(creditLineContract, creditLineAction, authorId, branchId, forceExpensePrepaymentReturn: forceExpensePrepaymentReturn);
                        action.ChildActionId = creditLineAction.Id;
                        _contractActionService.Save(action);
                    }
                }

                var historyId = await _contractPaymentScheduleService.InsertContractPaymentScheduleHistory(parentContract.Id, action.Id, 10);
                foreach (var item in parentContract.PaymentSchedule)
                {
                    if (item.ActionId.HasValue)
                    {
                        var act = await _contractActionService.GetAsync(item.ActionId.Value);
                        if (act != null)
                        {
                            if (act.ActionType != ContractActionType.PartialPayment)
                            { item.ActionType = (int)act.ActionType; }
                            else
                            {
                                if (act.ActionType == ContractActionType.PartialPayment &&
                                    Math.Round(act.Cost) == Math.Round(item.DebtCost))
                                { item.ActionType = (int)act.ActionType; }
                            }
                        }
                    }
                    await _contractPaymentScheduleService.InsertContractPaymentScheduleHistoryItems(historyId, item);
                }

                List<ContractPaymentSchedule> paidScheduleItems;
                var unpaidScheduleItemList = new List<ContractPaymentSchedule>();

                if (parentContract.PercentPaymentType == PercentPaymentType.EndPeriod)
                {
                    paidScheduleItems = parentContract.PaymentSchedule.ToList();

                    var unpaidScheduleItem = parentContract.PaymentSchedule.Where(x => x.ActionId == null && x.Prolongated == null && x.ActualDate == null).FirstOrDefault();
                    if (unpaidScheduleItem.Date > action.Date)
                    {
                        paidScheduleItems.Remove(unpaidScheduleItem);
                    }

                    parentContract.SignDate = action.Date;//.AddDays(1);
                    parentContract.LoanPeriod = 30;
                    parentContract.FirstPaymentDate = action.Date.AddDays(30);
                }
                else
                {
                    parentContract.SignDate = action.Date;
                    paidScheduleItems = parentContract.PaymentSchedule.Where(x => x.ActionId != null && x.ActionType != 40).ToList();

                    parentContract.LoanPeriod = parentContract.LoanPeriod - paidScheduleItems.Count() * 30;

                    //если есть просрочка
                    unpaidScheduleItemList = parentContract.PaymentSchedule.Where(x => (x.ActionId == null && x.ActualDate == null && x.Date < action.Date) || (x.ActionId != null && x.ActionType == 40)).ToList();
                    foreach (var unpaidScheduleItem in unpaidScheduleItemList)
                    {
                        if (unpaidScheduleItem.Date < action.Date && unpaidScheduleItem.ActionType != 40)
                        {
                            parentContract.PaymentSchedule.Remove(unpaidScheduleItem);
                            parentContract.LoanPeriod -= 30;
                        }
                        else if (unpaidScheduleItem.ActionType == 40)
                        {
                            parentContract.PaymentSchedule.Remove(unpaidScheduleItem);
                        }
                    }
                    if (parentContract.IsContractRestructured)
                    {
                        var actionDefermentLoan = action.Rows.FirstOrDefault(x => x.PaymentType == AmountType.DefermentLoan);
                        if (actionDefermentLoan != null)
                        {
                            defermentProfitCost = actionDefermentLoan.Cost;
                            defermentProfit -= actionDefermentLoan.Cost;
                        }

                        var actionAmortizedLoan = action.Rows.FirstOrDefault(x => x.PaymentType == AmountType.AmortizedLoan);
                        if (actionAmortizedLoan != null)
                        {
                            amortizedProfitCost = actionAmortizedLoan.Cost;
                            amortizedProfit -= actionAmortizedLoan.Cost;
                        }

                        var actionAmortizedDebtPenalty = action.Rows.FirstOrDefault(x => x.PaymentType == AmountType.AmortizedDebtPenalty);
                        if (actionAmortizedDebtPenalty != null)
                        {
                            amortizedPenyAccountCost = actionAmortizedDebtPenalty.Cost;
                            amortizedPenyAccount -= actionAmortizedDebtPenalty.Cost;
                        }

                        var actionAmortizedLoanPenalty = action.Rows.FirstOrDefault(x => x.PaymentType == AmountType.AmortizedLoanPenalty);
                        if (actionAmortizedLoanPenalty != null)
                        {
                            amortizedPenyProfitCost = actionAmortizedLoanPenalty.Cost;
                            amortizedPenyProfit -= actionAmortizedLoanPenalty.Cost;
                        }
                    }
                    contractDue -= _contractService.GetOverdueAccountBalance(action.ContractId, action.Date);
                    parentContract.LoanCost = contractDue - (debtRow != null ? debtRow.Cost : 0);
                    parentContract.FirstPaymentDate = parentContract.PaymentSchedule.OrderBy(x => x.Date).FirstOrDefault(x => x.ActionId == null).Date;
                    if (parentContract.FirstPaymentDate.Value.Day == parentContract.SignDate.Value.Day)
                    {
                        parentContract.SignDate = parentContract.FirstPaymentDate.Value.AddDays(-30);
                    }
                }

                bool migratedFromOnline = false;
                if (parentContract.ProductType != null && parentContract.ProductType.Code != null && parentContract.ProductType.Code == "TSO_MIGRATION")
                {
                    migratedFromOnline = true;
                }

                _paymentScheduleService.BuildForPartialPayment(parentContract, parentContract.SignDate.Value, parentContract.FirstPaymentDate.Value, parentContract.LoanCost,
                    defermentProfit, amortizedProfit, amortizedPenyAccount, amortizedPenyProfit);

                foreach (var paidScheduleItem in paidScheduleItems)
                {
                    if (parentContract.PaymentSchedule.All(x => x.Id != paidScheduleItem.Id))
                    {
                        parentContract.PaymentSchedule.Add(paidScheduleItem);
                    }
                }

                var percentCost = action.Rows.Where(x => x.PaymentType == AmountType.Loan).Sum(x => x.Cost);

                if (parentContract.PercentPaymentType != PercentPaymentType.EndPeriod)
                {
                    foreach (var unpaidScheduleItem in unpaidScheduleItemList)
                    {
                        parentContract.PaymentSchedule.Add(unpaidScheduleItem);
                    }
                }

                var kdToday = parentContract.PaymentSchedule.Where(x => x.ActionId == null && x.Canceled == null && x.DeleteDate == null && x.Date == action.Date).FirstOrDefault();
                decimal discountSum = 0;
                var discount = action.Discount.Discounts;
                if (discount != null)
                {
                    foreach (var discountItem in discount)
                    {
                        foreach (var row in discountItem.Rows)
                        {
                            if (row.PaymentType == AmountType.Loan)
                                discountSum += row.SubtractedCost;
                        }
                    }
                }

                var period = 0;

                if (paidScheduleItems.Any())
                    period = Math.Abs((paidScheduleItems.Max(x => x.Date) - action.Date).Days);
                else
                    period = Math.Abs((parentContract.ContractDate.Date - action.Date).Days);

                if (parentContract.IsContractRestructured)
                {
                    parentContract.PaymentSchedule.Add(new RestructuredContractPaymentSchedule(
                        date: action.Date,
                        contractId: action.ContractId,
                        actualDate: action.Date,
                        debtLeft: contractDue - (debtRow != null ? debtRow.Cost : 0),
                        debtCost: (debtRow != null ? debtRow.Cost : 0),
                        percentCost: kdToday != null ? 0 : percentCost + discountSum,
                        createDate: DateTime.Now,
                        period: period,
                        paymentBalanceOfDefferedPercent: defermentProfitCost,
                        paymentBalanceOfOverduePercent: amortizedProfitCost,
                        paymentPenaltyOfOverdueDebt: amortizedPenyAccountCost,
                        paymentPenaltyOfOverduePercent: amortizedPenyProfitCost,
                        amortizedBalanceOfDefferedPercent: defermentProfit,
                        amortizedBalanceOfOverduePercent: amortizedProfit,
                        amortizedPenaltyOfOverdueDebt: amortizedPenyAccount,
                        amortizedPenaltyOfOverduePercent: amortizedPenyProfit));
                }
                else
                {
                    parentContract.PaymentSchedule.Add(new ContractPaymentSchedule()
                    {
                        Date = action.Date,
                        ContractId = action.ContractId,
                        ActualDate = action.Date,
                        DebtLeft = contractDue - (debtRow != null ? debtRow.Cost : 0),
                        DebtCost = (debtRow != null ? debtRow.Cost : 0),
                        PercentCost = kdToday != null ? 0 : percentCost + discountSum,
                        CreateDate = DateTime.Now,
                        Period = period
                    });
                }
                _contractPaymentScheduleService.Save(parentContract.PaymentSchedule, parentContract.Id, authorId, false);

                transaction.Commit();
            }

            return action;
        }

        public async Task<List<ContractPartialPayment>> GetContractPartialPayments(int ContractId)
        {
            var list = new List<ContractPartialPayment>();
            var versions = await _contractPaymentScheduleService.GetScheduleVersionsWithoutChangedControlDate(ContractId);
            if (versions != null)
            {
                if (!versions.Any())
                    return list;

                var lastActionId = versions[0].ActionId;
                var lastAction = await _contractActionService.GetAsync(lastActionId);
                int number = 0;
                var contract = _contractRepository.GetOnlyContract(ContractId);
                if (contract != null)
                {
                    if (contract.PartialPaymentParentId == null)
                        versions.Remove(versions[0]);

                    foreach (var version in versions)
                    {
                        var action = await _contractActionService.GetAsync(version.ActionId);
                        if (action != null)
                        {
                            var listScheduleRows = new List<ContractPaymentSchedule>();
                            var cpp = new ContractPartialPayment();
                            if (number == 0 && contract.PartialPaymentParentId != null)
                            {
                                decimal partialPaymentCost = 0;
                                var baseContract = contract.ParentId.HasValue ? _contractRepository.GetOnlyContract(contract.ParentId.Value) : contract;
                                var pppContract = _contractRepository.GetOnlyContract(contract.PartialPaymentParentId.Value);
                                if (pppContract != null)
                                {
                                    var pppActions = await _contractActionService.GetContractActionsByContractId(pppContract.Id);
                                    var ppAction = pppActions.Where(x => x.ActionType == ContractActionType.PartialPayment).FirstOrDefault();
                                    if (ppAction != null)
                                    {
                                        partialPaymentCost = ppAction.Cost;
                                    }
                                }
                                listScheduleRows = _contractPaymentScheduleService.GetListByContractId(pppContract.Id);
                                if (listScheduleRows != null && listScheduleRows.Count > 0)
                                {
                                    //var debtLeftPayment = listScheduleRows.Where(x => x.ActionType == 40).FirstOrDefault();
                                    list.Add(new ContractPartialPayment()
                                    {
                                        AdditionNumber = number = version.Number,
                                        ParentContractNumber = baseContract.ContractNumber,
                                        ParentContractDate = baseContract.ContractDate,
                                        PartialPaymentCost = partialPaymentCost,
                                        DebtLeft = contract.LoanCost,
                                        PaymentSchedules = listScheduleRows,
                                        ActionDate = action.Date,
                                        ActionDateTime = action.CreateDate,
                                        MaturityDate = contract.MaturityDate,
                                        LoanPeriod = contract.LoanPeriod,
                                        ActionType = (int)action.ActionType
                                    });
                                }
                            }
                            else
                            {
                                listScheduleRows = await _contractPaymentScheduleService.GetScheduleRowsAfterPartialPayment(version.ActionId, ContractId);
                                if (listScheduleRows != null && listScheduleRows.Count > 0)
                                {
                                    var ppRow = await _contractPaymentScheduleService.GetLastPartialPaymentScheduleHistoryRow(ContractId, version.ActionId);
                                    list.Add(new ContractPartialPayment()
                                    {
                                        AdditionNumber = number = version.Number,
                                        ParentContractNumber = contract.ContractNumber,
                                        ParentContractDate = contract.ContractDate,
                                        PartialPaymentCost = ppRow != null ? ppRow.DebtCost : 0,
                                        DebtLeft = ppRow != null ? ppRow.DebtLeft : 0,
                                        PaymentSchedules = listScheduleRows,
                                        ActionDate = action.Date,
                                        ActionDateTime = action.CreateDate,
                                        ActionType = (int)action.ActionType
                                    });
                                }
                            }
                            lastActionId = version.ActionId;
                            lastAction = await _contractActionService.GetAsync(lastActionId);
                        }
                    }

                    var schedulePartialPaymentLastOne = await _contractPaymentScheduleService.GetLastPartialPaymentScheduleRow(ContractId, lastActionId);
                    var schedulesPartialPaymentLast = await _contractPaymentScheduleService.GetScheduleRowsAfterLastPartialPayment(ContractId, lastActionId);
                    var schedulesList = new List<ContractPaymentSchedule>();
                    //schedulesList.Add(schedulePartialPaymentLastOne);
                    schedulesList.AddRange(schedulesPartialPaymentLast);
                    list.Add(new ContractPartialPayment()
                    {
                        AdditionNumber = number + 1,
                        ParentContractNumber = contract.ContractNumber,
                        ParentContractDate = contract.ContractDate,
                        PartialPaymentCost = schedulePartialPaymentLastOne != null ? schedulePartialPaymentLastOne.DebtCost : 0,
                        DebtLeft = schedulePartialPaymentLastOne != null ? schedulePartialPaymentLastOne.DebtLeft : 0,
                        PaymentSchedules = schedulesList,
                        ActionDate = schedulePartialPaymentLastOne != null ? schedulePartialPaymentLastOne.Date : new DateTime(),
                        ActionDateTime = schedulePartialPaymentLastOne != null ? schedulePartialPaymentLastOne.CreateDate : new DateTime(),
                        ActionType = (int)lastAction.ActionType
                    });
                }
            }
            return list;
        }

        public async Task<ContractPartialPayment> GetContractParentPayments(int ContractId)
        {
            var contract = _contractRepository.GetOnlyContract(ContractId);
            if (contract != null)
            {
                var listScheduleRows = new List<ContractPaymentSchedule>();
                listScheduleRows = await _contractPaymentScheduleService.GetScheduleRowsBeforePartialPayment(ContractId);
                if (listScheduleRows != null && listScheduleRows.Count > 0)
                {
                    var debtLeftPayment = listScheduleRows.Where(x => x.ActionType == 40).FirstOrDefault();
                    return new ContractPartialPayment()
                    {
                        AdditionNumber = 1,
                        ParentContractNumber = contract.ContractNumber,
                        ParentContractDate = contract.ContractDate,
                        PartialPaymentCost = debtLeftPayment != null ? debtLeftPayment.DebtCost : 0,
                        DebtLeft = debtLeftPayment != null ? debtLeftPayment.DebtLeft : 0,
                        PaymentSchedules = listScheduleRows,
                        ActionDate = new DateTime()
                    };
                }
            }
            return null;
        }
    }
}
