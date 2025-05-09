using Newtonsoft.Json;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Mintos;
using Pawnshop.Services.AbsOnline;
using Pawnshop.Services.AccountingCore;
using Pawnshop.Services.Calculation;
using Pawnshop.Services.Crm;
using Pawnshop.Services.PenaltyLimit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Services.Models.Calculation;

namespace Pawnshop.Services.Contracts
{
    public class ContractActionBuyoutService : IContractActionBuyoutService
    {
        private readonly IContractActionCheckService _contractActionCheckService;
        private readonly IInscriptionService _inscriptionService;
        private readonly ContractRepository _contractRepository;
        private readonly PayTypeRepository _payTypeRepository;
        private readonly GroupRepository _groupRepository;
        private readonly ContractActionRepository _contractActionRepository;
        private readonly MintosContractRepository _mintosContractRepository;
        private readonly MintosContractActionRepository _mintosContractActionRepository;
        private readonly UserRepository _userRepository;
        private readonly ICrmPaymentService _crmPaymentService;
        private readonly IContractService _contractService;
        private readonly IContractActionRowBuilder _contractActionRowBuilder;
        private readonly IContractActionOperationService _contractActionOperationService;
        private readonly IEventLog _eventLog;
        private readonly IContractPaymentScheduleService _contractPaymentScheduleService;
        private readonly IContractCloseService _contractCloseService;
        private readonly IAccountService _accountService;
        private readonly IContractActionService _contractActionService;
        private readonly IAbsOnlineService _absOnlineServce;
        private string _badDateError = "Дата не может быть меньше даты последнего действия по договору";

        public ContractActionBuyoutService(IContractActionCheckService contractActionCheckService,
            ContractRepository contractRepository, PayTypeRepository payTypeRepository, GroupRepository groupRepository,
            IContractActionRowBuilder contractActionRowBuilder,
            ICrmPaymentService crmPaymentService, IContractService contractService,
            IContractActionOperationService contractActionOperationService,
            ContractActionRepository contractActionRepository,
            MintosContractRepository mintosContractRepository, IEventLog eventLog,
            MintosContractActionRepository mintosContractActionRepository, UserRepository userRepository,
            IContractPaymentScheduleService contractPaymentScheduleService,
            IContractCloseService contractCloseService,
            IAccountService accountService,
            IContractActionService contractActionService,
            IAbsOnlineService absOnlineServce
            )
        {
            _contractRepository = contractRepository;
            _contractActionCheckService = contractActionCheckService;
            _payTypeRepository = payTypeRepository;
            _groupRepository = groupRepository;
            _contractActionRowBuilder = contractActionRowBuilder;
            _crmPaymentService = crmPaymentService;
            _contractService = contractService;
            _contractActionOperationService = contractActionOperationService;
            _contractActionRepository = contractActionRepository;
            _mintosContractRepository = mintosContractRepository;
            _eventLog = eventLog;
            _mintosContractActionRepository = mintosContractActionRepository;
            _userRepository = userRepository;
            _contractPaymentScheduleService = contractPaymentScheduleService;
            _contractCloseService = contractCloseService;
            _accountService = accountService;
            _contractActionService = contractActionService;
            _absOnlineServce = absOnlineServce;
        }
        public async Task Execute(ContractAction action, int authorId, int branchId, bool forceExpensePrepaymentReturn, bool autoApporve, ContractAction prepaymentAction = null)
        {
            action.CreateDate = DateTime.Now;
            action.AuthorId = authorId;
            _contractActionCheckService.ContractActionCheck(action);
            var contract = _contractRepository.Get(action.ContractId);
            Validate(action, contract, authorId, branchId);

            using (var transaction = _contractRepository.BeginTransaction())
            {
                decimal extraExpensesCost = action.ExtraExpensesCost ?? 0;
                decimal contractExpenseAccountBalance = _contractService.GetExtraExpensesCost(contract.CreditLineId ?? contract.Id, action.Date);
                if (extraExpensesCost != contractExpenseAccountBalance)
                    throw new PawnshopApplicationException("Суммы доп расходов не сходятся");

                OrderStatus? orderStatus = null;
                if (autoApporve)
                    orderStatus = OrderStatus.Approved;

                decimal contractDueBeforeBuyOut = 0;
                contractDueBeforeBuyOut += _contractService.GetAccountBalance(action.ContractId, action.Date);
                contractDueBeforeBuyOut += _contractService.GetOverdueAccountBalance(action.ContractId, action.Date);

                //Для кредитной линии
                if (contract.ContractClass == ContractClass.CreditLine && action.BuyoutCreditLine)
                {
                    ContractActionRow creditLineActionRow = new ContractActionRow
                    {
                        PaymentType = AmountType.CreditLineLimit,
                        Cost = _contractService.GetContractAccountBalance(contract.Id, Constants.ACCOUNT_SETTING_CREDIT_LINE_LIMIT)
                    };
                    var creditLineActionRows = new List<ContractActionRow>();
                    creditLineActionRows.Add(creditLineActionRow);
                    action.Rows = creditLineActionRows.ToArray();
                    action.Cost = creditLineActionRow.Cost;
                    action.TotalCost = creditLineActionRow.Cost;
                    action.ActionType = ContractActionType.CreditLineClose;
                    action.Reason = $"Списание лимита при закрытии кредитной линии по договору {contract.ContractNumber} от {contract.ContractDate.ToString("dd.MM.yyyy")}";
                }
                _contractActionOperationService.Register(contract, action, authorId, branchId, forceExpensePrepaymentReturn: forceExpensePrepaymentReturn, orderStatus: orderStatus);

                //для Траншей
                if (contract.ContractClass == ContractClass.Tranche)
                {
                    var creditLineContract = _contractRepository.Get(contract.CreditLineId.Value);
                    //выкупаем ли кредитную линию
                    if (action.BuyoutCreditLine)
                    {
                        var activeContractsCount = await _contractRepository.GetActiveTranchesCount(contract.Id);
                        if (activeContractsCount == 1)
                        {
                            ContractActionRow creditLineActionRow = new ContractActionRow
                            {
                                PaymentType = AmountType.CreditLineLimit,
                                Cost = _contractService.GetContractAccountBalance(creditLineContract.Id, Constants.ACCOUNT_SETTING_CREDIT_LINE_LIMIT)
                            };
                            var creditLineActionRows = new List<ContractActionRow>();
                            creditLineActionRows.Add(creditLineActionRow);
                            ContractAction creditLineAction = new ContractAction()
                            {
                                ActionType = ContractActionType.CreditLineClose,
                                Date = action.Date,
                                Reason = $"Выкуп договора займа {contract.ContractNumber} от {contract.ContractDate.ToString("dd.MM.yyyy")}",
                                TotalCost = creditLineActionRow.Cost,
                                Cost = creditLineActionRow.Cost,
                                ContractId = creditLineContract.Id,
                                ParentActionId = action.Id,
                                ParentAction = action,
                                PayTypeId = action.PayTypeId ?? null,
                                RequisiteId = action.RequisiteId ?? null,
                                RequisiteCost = action.PayTypeId.HasValue ? (int)action.Cost : 0,
                                Checks = action.Checks,
                                Files = action.Files,
                                BuyoutCreditLine = true
                            };
                            creditLineAction.Rows = creditLineActionRows.ToArray();
                            creditLineContract.BuyoutReasonId = contract.BuyoutReasonId;
                            _contractActionOperationService.Register(creditLineContract, creditLineAction, authorId, branchId, forceExpensePrepaymentReturn: forceExpensePrepaymentReturn, orderStatus: orderStatus);
                            action.ChildActionId = creditLineAction.Id;
                            _contractActionService.Save(action);
                            _contractService.Save(creditLineContract);
                        }
                    }
                    else
                    {
                        decimal contractDue = contractDueBeforeBuyOut;
                        ContractActionRow creditLineActionRow = new ContractActionRow
                        {
                            PaymentType = AmountType.CreditLineLimit,
                            Cost = contractDue
                        };
                        var creditLineActionRows = new List<ContractActionRow>();
                        creditLineActionRows.Add(creditLineActionRow);
                        ContractAction creditLineAction = new ContractAction()
                        {
                            ActionType = ContractActionType.Buyout,
                            Date = action.Date,
                            Reason = $"Выкуп договора займа {contract.ContractNumber} от {contract.ContractDate.ToString("dd.MM.yyyy")}",
                            TotalCost = contractDue,
                            Cost = contractDue,
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
                        _contractActionOperationService.Register(creditLineContract, creditLineAction, authorId, branchId, forceExpensePrepaymentReturn: forceExpensePrepaymentReturn, orderStatus: orderStatus);
                        action.ChildActionId = creditLineAction.Id;
                        _contractActionService.Save(action);
                    }
                }

                if (autoApporve)
                    await ExecuteOnApprove(action, authorId, branchId, prepaymentAction);
            }
        }
        
        /// <summary>
        /// Метод выкупа
        /// </summary>
        /// <param name="forceExpensePrepaymentReturn">Флаг, указывающий на необходимость принудительного возврата авансовых платежей.</param>
        /// <param name="autoApprove"></param>
        /// <param name="childAction"></param>
        /// <param name="auctionOrderRequestId"></param>
        /// <exception cref="PawnshopApplicationException">Выбрасывается, если суммы дополнительных расходов не совпадают.</exception>
        /// <exception cref="Exception">Выбрасывается в случае любой другой ошибки во время выполнения транзакции.</exception>
        /// <returns>Список созданных денежных ордеров.</returns>
        public async Task Execute(
            ContractAction contractAction,
            int authorId,
            int branchId,
            bool forceExpensePrepaymentReturn,
            bool autoApprove,
            ContractAction? childAction,
            Contract? contract)
        {
            contractAction.CreateDate = DateTime.Now;
            contractAction.AuthorId = authorId;
            _contractActionCheckService.ContractActionCheck(contractAction);

            contract ??= _contractRepository.Get(contractAction.ContractId);

            Validate(contractAction, contract, authorId, branchId);

            using var transaction = _contractRepository.BeginTransaction();
            try
            {
                OrderStatus? orderStatus = null;
                if (autoApprove)
                {
                    orderStatus = OrderStatus.Approved;
                }

                decimal contractDueBeforeBuyOut = 0;
                contractDueBeforeBuyOut += _contractService
                    .GetAccountBalance(contractAction.ContractId, contractAction.Date);

                contractDueBeforeBuyOut += _contractService
                    .GetOverdueAccountBalance(contractAction.ContractId, contractAction.Date);
                
                _contractActionOperationService.Register(
                    contract,
                    contractAction,
                    authorId,
                    branchId,
                    forceExpensePrepaymentReturn: forceExpensePrepaymentReturn,
                    orderStatus: orderStatus);

                //для Траншей
                if (contract.ContractClass == ContractClass.Tranche)
                {
                    var creditLineContract = _contractRepository.Get(contract.CreditLineId.Value);
                    //выкупаем ли кредитную линию
                    if (contractAction.BuyoutCreditLine)
                    {
                        var activeContractsCount = await _contractRepository.GetActiveTranchesCount(contract.Id);
                        if (activeContractsCount == 1)
                        {
                            var creditLineActionRow = new ContractActionRow
                            {
                                PaymentType = AmountType.CreditLineLimit,
                                Cost = _contractService.GetContractAccountBalance(
                                    contractId: creditLineContract.Id,
                                    accountSettingCode: Constants.ACCOUNT_SETTING_CREDIT_LINE_LIMIT)
                            };

                            var creditLineActionRows = new List<ContractActionRow> { creditLineActionRow };
                            var creditLineAction = new ContractAction
                            {
                                ActionType = ContractActionType.CreditLineClose,
                                Date = contractAction.Date,
                                Reason =
                                    $"Выкуп договора займа {contract.ContractNumber} от {contract.ContractDate:dd.MM.yyyy}",
                                TotalCost = creditLineActionRow.Cost,
                                Cost = creditLineActionRow.Cost,
                                ContractId = creditLineContract.Id,
                                ParentActionId = contractAction.Id,
                                ParentAction = contractAction,
                                PayTypeId = contractAction.PayTypeId,
                                RequisiteId = contractAction.RequisiteId,
                                RequisiteCost = contractAction.PayTypeId.HasValue ? (int)contractAction.Cost : 0,
                                Checks = contractAction.Checks,
                                Files = contractAction.Files,
                                BuyoutCreditLine = true,
                                Rows = creditLineActionRows.ToArray(),
                                BuyoutReasonId = contract.BuyoutReasonId
                            };

                            _contractActionOperationService.Register(
                                creditLineContract,
                                creditLineAction,
                                authorId,
                                branchId,
                                forceExpensePrepaymentReturn: forceExpensePrepaymentReturn,
                                orderStatus: orderStatus);

                            contractAction.ChildActionId = creditLineAction.Id;
                            _contractActionService.Save(contractAction);
                            _contractService.Save(creditLineContract);
                        }
                    }
                    else
                    {
                        decimal contractDue = contractDueBeforeBuyOut;
                        var creditLineActionRow = new ContractActionRow
                        {
                            PaymentType = AmountType.CreditLineLimit,
                            Cost = contractDue
                        };

                        var creditLineActionRows = new List<ContractActionRow> { creditLineActionRow };

                        var creditLineAction = new ContractAction
                        {
                            ActionType = ContractActionType.Buyout,
                            Date = contractAction.Date,
                            Reason =
                                $"Выкуп договора займа {contract.ContractNumber} от {contract.ContractDate:dd.MM.yyyy}",
                            TotalCost = contractDue,
                            Cost = contractDue,
                            ContractId = creditLineContract.Id,
                            ParentActionId = contractAction.Id,
                            ParentAction = contractAction,
                            PayTypeId = contractAction.PayTypeId,
                            RequisiteId = contractAction.RequisiteId,
                            RequisiteCost = contractAction.PayTypeId.HasValue ? (int)contractAction.Cost : 0,
                            Checks = contractAction.Checks,
                            Files = contractAction.Files,
                            Rows = creditLineActionRows.ToArray()
                        };

                        _contractActionOperationService.Register(
                            creditLineContract,
                            creditLineAction,
                            authorId,
                            branchId,
                            forceExpensePrepaymentReturn: forceExpensePrepaymentReturn,
                            orderStatus: orderStatus);

                        contractAction.ChildActionId = creditLineAction.Id;
                        _contractActionService.Save(contractAction);
                    }
                }

                if (autoApprove)
                {
                    await ExecuteOnApprove(contractAction, authorId, branchId, childAction, contract);
                }

                transaction.Commit();
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
        }


        public async Task<List<ContractAction>> ExecuteWithReturnContractAction(ContractAction action, int authorId, int branchId, bool forceExpensePrepaymentReturn,
            bool autoApporve, ContractAction prepaymentAction = null, bool forceBuyOutCreditLine = false, ContractExpense expense = null)
        {
            List<ContractAction> returnedContractActions = new List<ContractAction>();
            action.CreateDate = DateTime.Now;
            action.AuthorId = authorId;
            _contractActionCheckService.ContractActionCheck(action);
            var contract = _contractRepository.Get(action.ContractId);
            Validate(action, contract, authorId, branchId);

            using (var transaction = _contractRepository.BeginTransaction())
            {
                decimal extraExpensesCost = action.ExtraExpensesCost ?? 0;
                decimal contractExpenseAccountBalance = _contractService.GetExtraExpensesCost(contract.CreditLineId ?? contract.Id, action.Date);
                if (extraExpensesCost != contractExpenseAccountBalance)
                    throw new PawnshopApplicationException("Суммы доп расходов не сходятся");

                OrderStatus? orderStatus = null;
                if (autoApporve)
                    orderStatus = OrderStatus.Approved;

                decimal contractDueBeforeBuyOut = 0;
                contractDueBeforeBuyOut += _contractService.GetAccountBalance(action.ContractId, action.Date);
                contractDueBeforeBuyOut += _contractService.GetOverdueAccountBalance(action.ContractId, action.Date);

                returnedContractActions.Add(_contractActionOperationService.Register(contract, action, authorId, branchId, 
                    forceExpensePrepaymentReturn: forceExpensePrepaymentReturn, orderStatus: orderStatus));

                var parentActionId = action.Id;
                var parentAction = action;
                //для Траншей
                if (contract.ContractClass == ContractClass.Tranche)
                {
                    var creditLineContract = _contractRepository.Get(contract.CreditLineId.Value);
                    //выкупаем ли кредитную линию
                    if (action.BuyoutCreditLine)
                    {
                        var activeContractsCount = await _contractRepository.GetActiveTranchesCount(contract.Id);
                        if (activeContractsCount == 1 || forceBuyOutCreditLine)
                        {
                            ContractActionRow creditLineActionRow = new ContractActionRow
                            {
                                PaymentType = AmountType.CreditLineLimit,
                                Cost = _contractService.GetContractAccountBalance(creditLineContract.Id, Constants.ACCOUNT_SETTING_CREDIT_LINE_LIMIT)
                            };
                            var creditLineActionRows = new List<ContractActionRow>();
                            creditLineActionRows.Add(creditLineActionRow);
                            ContractAction creditLineAction = new ContractAction()
                            {
                                ActionType = ContractActionType.CreditLineClose,
                                Date = action.Date,
                                Reason = $"Выкуп договора займа {contract.ContractNumber} от {contract.ContractDate.ToString("dd.MM.yyyy")}",
                                TotalCost = creditLineActionRow.Cost,
                                Cost = creditLineActionRow.Cost,
                                ContractId = creditLineContract.Id,
                                //ParentActionId = action.Id,
                                //ParentAction = action,
                                PayTypeId = action.PayTypeId ?? null,
                                RequisiteId = action.RequisiteId ?? null,
                                RequisiteCost = action.PayTypeId.HasValue ? (int)action.Cost : 0,
                                Checks = action.Checks,
                                Files = action.Files,
                                BuyoutCreditLine = true,
                                Expense = expense
                            };
                            creditLineAction.Rows = creditLineActionRows.ToArray();
                            creditLineContract.BuyoutReasonId = contract.BuyoutReasonId;
                            returnedContractActions.Add(_contractActionOperationService.Register(creditLineContract,
                                creditLineAction,
                                authorId, branchId, forceExpensePrepaymentReturn: forceExpensePrepaymentReturn,
                                orderStatus: orderStatus));
                            //action.ChildActionId = creditLineAction.Id;
                            //_contractActionService.Save(action);
                            _contractService.Save(creditLineContract);
                            parentActionId = creditLineAction.Id;
                            parentAction = creditLineAction;
                        }
                    }
                    else
                    {
                        decimal contractDue = contractDueBeforeBuyOut;
                        ContractActionRow creditLineActionRow = new ContractActionRow
                        {
                            PaymentType = AmountType.CreditLineLimit,
                            Cost = contractDue
                        };
                        var creditLineActionRows = new List<ContractActionRow>();
                        creditLineActionRows.Add(creditLineActionRow);
                        ContractAction creditLineAction = new ContractAction()
                        {
                            ActionType = ContractActionType.Buyout,
                            Date = action.Date,
                            Reason = $"Выкуп договора займа {contract.ContractNumber} от {contract.ContractDate.ToString("dd.MM.yyyy")}",
                            TotalCost = contractDue,
                            Cost = contractDue,
                            ContractId = creditLineContract.Id,
                            PayTypeId = action.PayTypeId ?? null,
                            RequisiteId = action.RequisiteId ?? null,
                            RequisiteCost = action.PayTypeId.HasValue ? (int)action.Cost : 0,
                            Checks = action.Checks,
                            Files = action.Files
                        };
                        creditLineAction.Rows = creditLineActionRows.ToArray();
                        returnedContractActions.Add(_contractActionOperationService.Register(creditLineContract, creditLineAction, authorId, branchId, forceExpensePrepaymentReturn: forceExpensePrepaymentReturn, orderStatus: orderStatus));
                        parentActionId = creditLineAction.Id;
                        parentAction = creditLineAction;
                    }

                    if (contract.IsContractRestructured && contract.ContractClass == ContractClass.Tranche)
                    {
                        decimal contractDue = contractDueBeforeBuyOut;
                        var actionRows = _contractActionRowBuilder.Build(contract, new ContractDutyCheckModel
                        {
                            ActionType = ContractActionType.BuyoutRestructuringTranches,
                            ContractId = contract.Id,
                            EmployeeId = authorId,
                            Date = action.Date
                        });
                        ContractAction buyoutRestructuringCredAction = new ContractAction()
                        {
                            ActionType = ContractActionType.BuyoutRestructuringTranches,
                            Date = action.Date,
                            Reason = $"Выкуп договора займа {contract.ContractNumber} от {contract.ContractDate.ToString("dd.MM.yyyy")} (Реструктуризация)",
                            TotalCost = actionRows.Sum(row => row.Cost),
                            Cost = actionRows.Sum(row => row.Cost),
                            ContractId = contract.Id,
                            ParentActionId = parentActionId,
                            ParentAction = parentAction,
                            PayTypeId = action.PayTypeId ?? null,
                            RequisiteId = action.RequisiteId ?? null,
                            RequisiteCost = action.PayTypeId.HasValue ? (int)action.Cost : 0,
                            Checks = action.Checks,
                            Files = action.Files
                        };
                        buyoutRestructuringCredAction.Rows = actionRows.ToArray();
                        returnedContractActions.Add(_contractActionOperationService.Register(contract, buyoutRestructuringCredAction, authorId, branchId, forceExpensePrepaymentReturn: forceExpensePrepaymentReturn, orderStatus: orderStatus));
                        parentAction.ChildActionId = buyoutRestructuringCredAction.Id;
                        _contractActionService.Save(parentAction);
                    }

                    returnedContractActions.Add(_contractCloseService.CloseContractByCreditLine(contract, action.Date, authorId,
                        prepaymentAction ?? action, orderStatus: orderStatus));
                }

                if (autoApporve)
                    await ExecuteOnApprove(action, authorId, branchId, prepaymentAction);
                transaction.Commit();
                return returnedContractActions;
            }
        }

        public async Task ExecuteOnApprove(ContractAction action, int authorId, int branchId, ContractAction prepaymentAction = null)
        {
            var contract = _contractRepository.Get(action.ContractId);
            await ProcessContractAction(action, authorId, branchId, prepaymentAction, contract);
        }
        
        public async Task ExecuteOnApprove(
            ContractAction action,
            int authorId,
            int branchId,
            ContractAction childAction = null,
            Contract contract = null)
        {
            contract ??= _contractRepository.Get(action.ContractId);
            await ProcessContractAction(action, authorId, branchId, childAction, contract);
        }

        private async Task ProcessContractAction(
            ContractAction action,
            int authorId,
            int branchId,
            ContractAction prepaymentAction,
            Contract contract)
        {
            Validate(action, contract, authorId, branchId);

            action.CreateDate = DateTime.Now;
            action.AuthorId = authorId;

            using (var transaction = _contractRepository.BeginTransaction())
            {
                if (contract.ContractClass != ContractClass.CreditLine)
                {

                    if (contract.ContractClass != ContractClass.Tranche)
                    {
                        _contractCloseService.Exec(contract, action.Date, authorId, prepaymentAction ?? action, orderStatus: OrderStatus.Approved);
                    }
                    _crmPaymentService.Enqueue(contract);

                    foreach (var schedule in contract.PaymentSchedule.Where(x => x.ActionId == null))
                        schedule.Canceled = DateTime.Now;

                    foreach (var position in contract.Positions)
                        position.Status = ContractPositionStatus.BoughtOut;

                    contract.Status = ContractStatus.BoughtOut;
                    contract.BuyoutDate = action.Date;
                    contract.NextPaymentDate = null;
                    _contractRepository.Update(contract);
                    _contractPaymentScheduleService.Save(contract.PaymentSchedule, contract.Id, authorId);
                    _contractActionRepository.Update(action);
                }
                else
                {
                    if (action.BuyoutCreditLine)
                    {
                        var activeContractsCount = await _contractRepository.GetActiveTranchesCount(contract.Id);
                        if (activeContractsCount == 0)
                        {
                            _contractCloseService.Exec(contract, action.Date, authorId, prepaymentAction ?? action, orderStatus: Data.Models.CashOrders.OrderStatus.Approved);

                            _crmPaymentService.Enqueue(contract);

                            foreach (var schedule in contract.PaymentSchedule.Where(x => x.ActionId == null))
                                schedule.Canceled = DateTime.Now;

                            foreach (var position in contract.Positions)
                                position.Status = ContractPositionStatus.BoughtOut;

                            contract.Status = ContractStatus.BoughtOut;
                            contract.BuyoutDate = action.Date;
                            contract.NextPaymentDate = null;
                            foreach (var position in contract.Positions)
                                position.Status = ContractPositionStatus.BoughtOut;
                            _contractRepository.Update(contract);
                        }
                    }
                }

                transaction.Commit();
            }

            //// отправка уведомления в CRM о закрытии займа
            //await _absOnlineServce.SendNotificationCloseContractAsync(contract.Id, contract);

            ScheduleMintosPaymentUpload(_contractRepository.Get(action.ContractId), action);
        }

        private void ScheduleMintosPaymentUpload(Contract contract, ContractAction action)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            var mintosContracts = _mintosContractRepository.GetByContractId(contract.Id);
            MintosContract mintosContract = null;
            if (mintosContracts.Count > 1)
            {
                mintosContract = mintosContracts.Where(x => x.MintosStatus.Contains("active")).FirstOrDefault();
                if (mintosContracts.Where(x => x.MintosStatus.Contains("active")).Count() > 1)
                {
                    _eventLog.Log(
                        EventCode.MintosContractUpdate,
                        EventStatus.Failed,
                        EntityType.MintosContract,
                        mintosContract.Id,
                        JsonConvert.SerializeObject(mintosContracts),
                        $"Договор {contract.ContractNumber}({contract.Id}) выгружен в Mintos {mintosContracts.Where(x => x.MintosStatus.Contains("active")).Count()} раз(а)");
                }
            }
            else if (mintosContracts.Count == 1)
            {
                mintosContract = mintosContracts.FirstOrDefault();
                if (mintosContract != null &&
                    mintosContracts.Where(x => x.MintosStatus.Contains("active")).Count() == 0)
                {
                    _eventLog.Log(
                        EventCode.MintosContractUpdate,
                        EventStatus.Failed,
                        EntityType.MintosContract,
                        mintosContract.Id,
                        JsonConvert.SerializeObject(mintosContracts),
                        $"Договор {contract.ContractNumber}({contract.Id}) выгружен в Mintos, но не проверен/утвержден модерацией Mintos)");
                }
            }
            else
            {
                return;
            }

            if (mintosContracts == null || mintosContract == null) return;
            if (!mintosContract.MintosStatus.Contains("active")) return;

            try
            {
                MintosContractAction mintosAction = new MintosContractAction(action);

                using (var transaction = _mintosContractRepository.BeginTransaction())
                {
                    mintosAction.MintosContractId = mintosContract.Id;

                    _mintosContractActionRepository.Insert(mintosAction);

                    transaction.Commit();
                }
            }
            catch (Exception e)
            {
                _eventLog.Log(
                    EventCode.MintosContractUpdate,
                    EventStatus.Failed,
                    EntityType.MintosContract,
                    mintosContract.Id,
                    responseData: e.Message);
            }
        }

        private void Validate(ContractAction action, Contract contract, int authorId, int branchId)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            if (action.ActionType != ContractActionType.Buyout && action.ActionType != ContractActionType.CreditLineClose && action.ActionType != ContractActionType.BuyoutRestructuringCred)
                throw new ArgumentException($"Свойство {action.ActionType} должно быть одним из {ContractActionType.Buyout}, {ContractActionType.CreditLineClose}, {ContractActionType.BuyoutRestructuringCred}", nameof(action));

            if (!action.PayTypeId.HasValue)
                throw new ArgumentException($"Свойство {action.PayTypeId} не должен быть null", nameof(action));

            _contractActionCheckService.ContractActionCheck(action);
            if (contract == null)
                throw new PawnshopApplicationException($"Договор {action.ContractId} не найден");

            if (contract.Status != ContractStatus.Signed && !action.isFromSelling)
                throw new PawnshopApplicationException(
                    "Выкуп невозможен, так как данный договор не является действуюшим");

            if (contract.Actions.Any() && action.Date.Date < contract.Actions.Max(x => x.Date).Date)
                throw new PawnshopApplicationException(_badDateError);

            PayType payType = null;
            if (action.PayTypeId.HasValue)
            {
                payType = _payTypeRepository.Get(action.PayTypeId.Value);
                if (payType == null)
                    throw new PawnshopApplicationException($"Тип оплаты {action.PayTypeId.Value} не найден");
            }

            Group branch = _groupRepository.Get(branchId);
            if (branch == null)
                throw new PawnshopApplicationException($"Филиал {branchId} не найден");

            User author = _userRepository.Get(authorId);
            if (author == null)
                throw new PawnshopApplicationException($"Пользователь {authorId} не найден");

            if (contract.Actions.Any() && action.Date.Date < contract.Actions.Max(x => x.Date).Date)
                throw new PawnshopApplicationException(_badDateError);
        }
    }
}