using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Discounts;
using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Data.Models.LegalCollection;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.AbsOnline;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.CreditLines;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.AccountingCore
{
    public class InscriptionService : IInscriptionService
    {
        private readonly IBusinessOperationService _businessOperationService;
        private readonly IBusinessOperationSettingService _businessOperationSettingService;
        private readonly int _authorId;
        private readonly IDictionaryWithSearchService<Group, BranchFilter> _branchService;
        private readonly IAccountService _accountService;
        private readonly IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> _accountSettingService;
        private readonly IContractService _contractService;
        private readonly IAccountPlanSettingService _accountPlanSettingService;
        private readonly ICashOrderService _cashOrderService;
        private readonly InscriptionRepository _repository;
        private readonly ContractDiscountRepository _contractDiscountRepository;
        private readonly IAbsOnlineService _absOnlineService;
        private readonly InscriptionRepository _inscriptionRepository;
        private readonly IServiceProvider _serviceProvider;

        public InscriptionService(
            ISessionContext sessionContext,
            IBusinessOperationService businessOperationService,
            IDictionaryWithSearchService<Group, BranchFilter> branchService,
            IAccountService accountService,
            IAccountPlanSettingService accountPlanSettingService,
            IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> accountSettingService,
            IBusinessOperationSettingService businessOperationSettingService,
            IContractService contractService, 
            ICashOrderService cashOrderService,
            InscriptionRepository repository,
            ContractDiscountRepository contractDiscountRepository,
            IAbsOnlineService absOnlineService,
            InscriptionRepository inscriptionRepository,
            IServiceProvider serviceProvider
            )
        {
            _businessOperationService = businessOperationService;
            _branchService = branchService;
            _accountService = accountService;
            _accountSettingService = accountSettingService;
            _accountPlanSettingService = accountPlanSettingService;
            _businessOperationSettingService = businessOperationSettingService;
            _authorId = sessionContext.IsInitialized ? sessionContext.UserId : Constants.ADMINISTRATOR_IDENTITY;
            _contractService = contractService;
            _cashOrderService = cashOrderService;
            _repository = repository;
            _contractDiscountRepository = contractDiscountRepository;
            _absOnlineService = absOnlineService;
            _inscriptionRepository = inscriptionRepository;
            _serviceProvider = serviceProvider;
        }

        public IEnumerable<CashOrder> RestoreOnBalance(IContract contract, DateTime date,
            List<InscriptionRow> rows)
        {
            if (contract is null)
                throw new ArgumentNullException(nameof(contract), "Договор не найден");

            IEnumerable<CashOrder> orders = new List<CashOrder>();

            if (rows != null && rows.Any())
            {
                var rowsToRestore = rows.Select(t => (t.PaymentType, t.Cost)).Distinct().ToDictionary(t => t.PaymentType, t => t.Cost);
                var registeredOrders = RegisterBusinessOperation(contract, date, Constants.BO_RESTORE_ON_BALANCE, rowsToRestore);

                orders = registeredOrders.Select(t => t.Item1);
            }

            return orders;
        }

        public void RestoreOnBalance(IContract contract, DateTime date) => RegisterBusinessOperation(contract, date, Constants.BO_RESTORE_ON_BALANCE, GetAmounts(contract.Id, date, true));
        public void RestoreOnBalanceOnBuyout(IContract contract, DateTime date) => RegisterBusinessOperation(contract, date, Constants.BO_INSCRIPTION_OFFBALANCE_WRITEOFF, GetAmounts(contract.Id, date, true));

        private List<(CashOrder, List<AccountRecord>)> RegisterBusinessOperation(IContract contract, DateTime date, string code, IDictionary<AmountType, decimal> amounts)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract), "Договор не найден");

            if (amounts == null && !amounts.Any())
                throw new ArgumentNullException(nameof(contract), "Суммы для списания не найдены");

            if(string.IsNullOrWhiteSpace(code))
                throw new ArgumentNullException(nameof(code), "Код бизнес опепрации не найден");

            var branch = _branchService.GetAsync(contract.BranchId).Result;

            return _businessOperationService.Register(contract, date, code, branch, _authorId, amounts);
        }

        private IDictionary<AmountType, decimal> GetAmounts(int contractId, DateTime date, bool isOffBalance = false)
        {
            var codes = new List<string>
            {
                Constants.ACCOUNT_SETTING_PROFIT,
                Constants.ACCOUNT_SETTING_OVERDUE_PROFIT,
                Constants.ACCOUNT_SETTING_PENY_ACCOUNT,
                Constants.ACCOUNT_SETTING_PENY_PROFIT,
                Constants.ACCOUNT_SETTING_ACCOUNT,
                Constants.ACCOUNT_SETTING_OVERDUE_ACCOUNT
            };

            if (isOffBalance)
                for (int i = 0; i < codes.Count; i++)
                    codes[i] += Constants.BO_SETTING_OFFBALANCE_POSTFIX;

            return GetAmountsBySettingCodes(contractId, date, codes.ToArray());
        }

        private IDictionary<AmountType, decimal> GetAmountsBySettingCodes(int contractId,
            DateTime date, params string[] codes)
        {
            IDictionary<AmountType, decimal> amounts = new ConcurrentDictionary<AmountType, decimal>();
            List<Account> accounts = _accountService.List(new ListQueryModel<AccountFilter>
            {
                Page = null,
                Model = new AccountFilter
                {
                    ContractId = contractId,
                    SettingCodes = codes
                }
            }).List;

            foreach (var account in accounts)
            {
                if (account.AccountSetting.DefaultAmountType != null)
                    amounts.Add(account.AccountSetting.DefaultAmountType.Value,
                        Math.Abs(Math.Abs(_accountService.GetAccountBalance(account.Id, date))));
            }

            return amounts;
        }

        public void WriteOffOnBuyout(Contract contract, ContractAction action)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract), "Договор не найден");
            if (action == null)
                throw new ArgumentNullException(nameof(contract), "Действие не найдено");

            var prePaymentBalance = GetAmountsBySettingCodes(contract.Id, action.CreateDate, Constants.ACCOUNT_SETTING_DEPO).FirstOrDefault().Value;
            if (prePaymentBalance == 0)
                throw new PawnshopApplicationException("Аванс не может быть равен нулю");

            if (contract.Inscription != null)
            {
                if (prePaymentBalance < contract.Inscription.TotalCost)
                {
                    var cost = contract.Inscription.TotalCost - prePaymentBalance;
                    IDictionary<AmountType, decimal> amountsToWriteOff = new Dictionary<AmountType, decimal>();
                    var rows = action.Rows.Where(t=> t.DebitAccountId.HasValue).OrderByDescending(t => t.PaymentType);

                    foreach (var row in rows)
                    {
                        if (cost > 0)
                        {
                            var costToWriteOff = cost - row.Cost;
                            amountsToWriteOff.Add(row.PaymentType, costToWriteOff >= 0 ? row.Cost : Math.Abs(costToWriteOff));
                            cost = costToWriteOff;
                        }
                    }

                    amountsToWriteOff = GetAmountsToWriteOff(contract.Id, action.Date, amountsToWriteOff);
                    RegisterBusinessOperation(contract, action.CreateDate, Constants.BO_INSCRIPTION_WRITEOFF, amountsToWriteOff);
                }
            }
        }

        private IDictionary<AmountType, decimal> GetAmountsToWriteOff(int contractId, DateTime date, IDictionary<AmountType, decimal> rows)
        {
            var amounts = GetAmounts(contractId, date);

            IDictionary<AmountType, decimal> amountsToWriteOff = new Dictionary<AmountType, decimal>();
            foreach (var (amountType, cost) in rows)
            {
                var sumToWriteOff = amounts[amountType] - cost;
                amountsToWriteOff.Add(amountType, sumToWriteOff);

            }

            return amountsToWriteOff;
        }

        public List<InscriptionRow> GetInscriptionRows(Inscription inscription, DateTime date)
        {
            if (inscription is null)
                throw new ArgumentNullException(nameof(inscription), "Исполнительная надпись не найдена");

            IDictionary<AmountType, decimal> amountsToWriteOff = new Dictionary<AmountType, decimal>();
            var contract = _contractService.Get(inscription.ContractId);
            string businessOperationCode = string.Empty;

            var accounts = _accountService.List(new ListQueryModel<AccountFilter>
            {
                Page = null,
                Model = new AccountFilter
                {
                    ContractId = contract.Id
                }
            }).List;

            switch (inscription.Status)
            {
                case InscriptionStatus.Approved:
                    businessOperationCode = Constants.BO_INSCRIPTION_WRITEOFF;
                    var rowsToWriteOff = GetAmountsFromRows(inscription.Rows);
                    amountsToWriteOff = GetAmountsToWriteOff(contract.Id, date, rowsToWriteOff);
                    break;

                case InscriptionStatus.Denied:
                    businessOperationCode = Constants.BO_RESTORE_ON_BALANCE;
                    amountsToWriteOff = GetAmounts(inscription.ContractId, date, true);
                    break;
            }

            var branch = _branchService.GetAsync(contract.BranchId).Result;
            var businessOperation = _businessOperationService.List(new ListQueryModel<BusinessOperationFilter>
            {
                Page = null,
                Model = new BusinessOperationFilter
                {
                    Code = businessOperationCode
                }
            }).List.FirstOrDefault();

            var settings = _businessOperationSettingService.List(new ListQueryModel<BusinessOperationSettingFilter>
            {
                Page = null,
                Model = new BusinessOperationSettingFilter
                {
                    BusinessOperationId = businessOperation.Id,
                    IsActive = true
                }
            }).List;

            var inscriptionRows = new List<InscriptionRow>();

            foreach (var setting in settings)
            {
                var row = new InscriptionRow();

                if (setting.CreditSettingId.HasValue)
                {
                    var creditSetting = _accountSettingService.GetAsync(setting.CreditSettingId.Value).Result;

                    row.CreditAccountId = accounts.FirstOrDefault(x => x.AccountSettingId == creditSetting.Id)?.Id;
                    if (row.CreditAccountId.HasValue) row.CreditAccount = _accountService.GetAsync(row.CreditAccountId.Value).Result;
                }

                if (setting.DebitSettingId.HasValue)
                {
                    var debitSetting = _accountSettingService.GetAsync(setting.DebitSettingId.Value).Result;

                    if (debitSetting.IsConsolidated)
                    {
                        row.DebitAccountId = _accountPlanSettingService.Find(branch.OrganizationId, debitSetting.Id,
                            contract.BranchId, contract.ContractTypeId, contract.PeriodTypeId).AccountId;
                    }
                    else
                    {
                        row.DebitAccountId = accounts.FirstOrDefault(x => x.AccountSettingId == debitSetting.Id)?.Id;
                    }

                    if (row.DebitAccountId.HasValue) row.DebitAccount = _accountService.GetAsync(row.DebitAccountId.Value).Result;
                }

                row.PaymentType = setting.AmountType;
                var hasTypeAmountsToWriteOff = amountsToWriteOff.Keys.Any(t => t == setting.AmountType);

                if (hasTypeAmountsToWriteOff)
                    row.Cost = amountsToWriteOff[setting.AmountType];

                if (row.Cost > 0)
                    inscriptionRows.Add(row);
            }

            return inscriptionRows;
        }

        public async Task<List<InscriptionRow>> GetInscriptionRowsAsync(Inscription inscription, Contract contract, DateTime date)
        {
            if (inscription is null)
                throw new ArgumentNullException(nameof(inscription), "Исполнительная надпись не найдена");
            
            contract ??= await _contractService.GetOnlyContractAsync(inscription.ContractId);
            if (contract is null)
                throw new ArgumentNullException(nameof(contract), "Контракт не найден");
            
            IDictionary<AmountType, decimal> amountsToWriteOff = new Dictionary<AmountType, decimal>();
            var businessOperationCode = string.Empty;
            
            switch (inscription.Status)
            {
                case InscriptionStatus.Approved:
                    businessOperationCode = Constants.BO_INSCRIPTION_WRITEOFF;
                    var rowsToWriteOff = GetAmountsFromRows(inscription.Rows);
                    amountsToWriteOff = GetAmountsToWriteOff(contract.Id, date, rowsToWriteOff);
                    break;

                case InscriptionStatus.Denied:
                    businessOperationCode = Constants.BO_RESTORE_ON_BALANCE;
                    amountsToWriteOff = GetAmounts(inscription.ContractId, date, true);
                    break;
            }

            var branch = await _branchService.GetAsync(contract.BranchId);
            var businessOperation = _businessOperationService.List(new ListQueryModel<BusinessOperationFilter>
            {
                Page = null,
                Model = new BusinessOperationFilter
                {
                    Code = businessOperationCode
                }
            }).List.FirstOrDefault();
            
            var accounts = _accountService.List(new ListQueryModel<AccountFilter>
            {
                Page = null,
                Model = new AccountFilter
                {
                    ContractId = contract.Id
                }
            }).List;

            var settings = _businessOperationSettingService.List(new ListQueryModel<BusinessOperationSettingFilter>
            {
                Page = null,
                Model = new BusinessOperationSettingFilter
                {
                    BusinessOperationId = businessOperation.Id,
                    IsActive = true
                }
            }).List;

            var inscriptionRows = new List<InscriptionRow>();

            foreach (var setting in settings)
            {
                var row = new InscriptionRow();

                if (setting.CreditSettingId.HasValue)
                {
                    var creditSetting = await _accountSettingService.GetAsync(setting.CreditSettingId.Value);

                    row.CreditAccountId = accounts.FirstOrDefault(x => x.AccountSettingId == creditSetting.Id)?.Id;
                    if (row.CreditAccountId.HasValue)
                    {
                        row.CreditAccount = await _accountService.GetAsync(row.CreditAccountId.Value);
                    }
                }

                if (setting.DebitSettingId.HasValue)
                {
                    var debitSetting = await _accountSettingService.GetAsync(setting.DebitSettingId.Value);

                    if (debitSetting.IsConsolidated)
                    {
                        row.DebitAccountId = _accountPlanSettingService
                            .Find(branch.OrganizationId,
                                debitSetting.Id,
                                contract.BranchId,
                                contract.ContractTypeId,
                                contract.PeriodTypeId)
                            .AccountId;
                    }
                    else
                    {
                        row.DebitAccountId = accounts.FirstOrDefault(x => x.AccountSettingId == debitSetting.Id)?.Id;
                    }

                    if (row.DebitAccountId.HasValue)
                    {
                        row.DebitAccount = await _accountService.GetAsync(row.DebitAccountId.Value);
                    }
                }

                row.PaymentType = setting.AmountType;
                var hasTypeAmountsToWriteOff = amountsToWriteOff.Keys.Any(t => t == setting.AmountType);

                if (hasTypeAmountsToWriteOff)
                    row.Cost = amountsToWriteOff[setting.AmountType];

                if (row.Cost > 0)
                    inscriptionRows.Add(row);
            }

            return inscriptionRows;
        }

        private IDictionary<AmountType, decimal> GetAmountsFromRows(List<InscriptionRow> rows) => rows.ToDictionary(t => t.PaymentType, t => t.Cost);

        public InscriptionAction RestoreOrders(Inscription inscription, Contract contract, InscriptionActionType actiontype)
        {
            if (inscription is null)
                throw new ArgumentNullException(nameof(inscription), "Исполнительная надпись не найдена"); 
            
            if (contract is null)
                throw new ArgumentNullException(nameof(contract), "Договор не найден");

            var action = inscription.Actions.FirstOrDefault(t => t.ActionType == actiontype);
            var branch = _branchService.GetAsync(contract.BranchId).Result;

            if (action != null && action.Rows.Any())
            {
                foreach (var row in action.Rows)
                {
                    if (row.OrderId.HasValue)
                        row.Cost += _cashOrderService.Cancel(_cashOrderService.GetAsync(row.OrderId.Value).Result,
                            _authorId,
                            contract.Branch).OrderCost;
                }
            }

            return action;
        }

        public IDictionary<int, (int, DateTime)> DeleteOrders(Inscription inscription, Contract contract, InscriptionActionType actiontype, int authorId, int branchId)
        {
            if (inscription is null)
                throw new ArgumentNullException(nameof(inscription), "Исполнительная надпись не найдена");

            if (contract is null)
                throw new ArgumentNullException(nameof(contract), "Договор не найден");

            var recalculateBalanceAccountDict = new Dictionary<int, (int, DateTime)>();
            var action = inscription.Actions.FirstOrDefault(t => t.ActionType == actiontype);
            var branch = _branchService.GetAsync(contract.BranchId).Result;
            if (action != null && action.Rows.Any())
            {
                foreach (var row in action.Rows)
                {
                    if (row.OrderId.HasValue)
                    {
                        IDictionary<int, (int, DateTime)> actionsWithDatesDict = _cashOrderService.Delete(row.OrderId.Value, authorId, branchId);
                        if (actionsWithDatesDict == null)
                            throw new PawnshopApplicationException(
                                $"Ожидалось что {nameof(_cashOrderService)}.{nameof(_cashOrderService.Delete)} не вернет null");

                        foreach ((int accountId, (int accountRecordId, DateTime accountDate)) in actionsWithDatesDict)
                        {
                            if (recalculateBalanceAccountDict.ContainsKey(accountId))
                            {
                                (int _, DateTime date) = recalculateBalanceAccountDict[accountId];
                                if (date < accountDate)
                                    continue;
                            }

                            recalculateBalanceAccountDict[accountId] = (accountRecordId, accountDate);
                        }
                    }
                }
            }

            return recalculateBalanceAccountDict;
        }

        public IEnumerable<CashOrder> WriteOffBalance(int contractId, List<InscriptionRow> rows, DateTime date)
        {
            IEnumerable<CashOrder> orders = new List<CashOrder>();

            if (rows != null && rows.Any())
            {
                var contract = _contractService.Get(contractId);

                var registeredOrders = RegisterBusinessOperation(contract, date, Constants.BO_INSCRIPTION_WRITEOFF, GetAmountsFromRows(rows));

                orders = registeredOrders.Select(t => t.Item1);
            }

            return orders;
        }

        public async Task<Inscription> GetAsync(int? inscriptionId)
        {
            if (!inscriptionId.HasValue)
            {
                return null;
            }
            
            return await _repository.GetAsync((int)inscriptionId);
        }

        public async Task<Inscription> GetOnlyInscriptionAsync(int inscriptionId)
        {
            return await _repository.GetOnlyInscriptionAsync(inscriptionId);
        }

        public Inscription Save(Inscription model)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model), "Исполнительная надпись не найдена");

            if (model.Id > 0) _repository.Update(model);
            else _repository.Insert(model);

            return model;
        }

        public void Delete(int id) => _repository.Delete(id);
        public Inscription Get(int id) => _repository.Get(id);

        public void DeleteAction(int id) => _repository.DeleteAction(id);
        public void InsertAction(InscriptionAction action) => _repository.InsertAction(action);
        public void SaveActionRows(InscriptionAction action) => _repository.SaveActionRows(action);
        public IDbTransaction BeginInscriptionTransaction() => _repository.BeginTransaction();

        public ListModel<Inscription> List(ListQuery listQuery)
        {
            return new ListModel<Inscription>()
            {
                List = _repository.List(listQuery),
                Count = _repository.Count(listQuery),
            };
        }

        public async Task<Inscription> StopAccruals(UpdateLegalCaseCommand request)
        {
            var contract = _contractService.Get(request.ContractId);
            if (contract == null)
            {
                throw new PawnshopApplicationException($"Договор {nameof(contract)} не найден");
            }

            if (contract.InscriptionId > 0)
            {
                throw new PawnshopApplicationException("Исполнительная надпись уже была сохранена ранее");
            }

            if (request.StateFeeAmount.HasValue && request.StateFeeAmount > 0)
            {
                request.Rows.Add(new InscriptionRow
                {
                    Cost = (decimal)request.StateFeeAmount,
                    PaymentType = AmountType.Duty
                });
            }

            var inscription = new Inscription
            {
                Status = InscriptionStatus.Approved,
                AuthorId = request.UserId,
                CreateDate = DateTime.Now,
                Date = DateTime.Now.Date,
                TotalCost = (decimal)request.TotalCost,
                Rows = request.Rows,
                ContractId = contract.Id,

                Actions = new List<InscriptionAction>
                {
                    new InscriptionAction
                    {
                        ActionType = InscriptionActionType.Сreation,
                        AuthorId = request.UserId,
                        CreateDate = DateTime.Now,
                        Date = DateTime.Now.Date,
                        Rows = request.Rows,
                    }
                }
            };

            CheckForDiscountsExistance(contract);
            contract.IsOffBalance = true;

            var orders = WriteOffBalance(inscription.ContractId, null, DateTime.Now);
            var action = new InscriptionAction
            {
                ActionType = InscriptionActionType.Confirm,
                AuthorId = request.UserId,
                CreateDate = DateTime.Now,
                Date = inscription.Date,
                InscriptionId = inscription.Id
            };

            UpdateActionRows(orders, inscription.Rows);
            action.Rows = inscription.Rows;

            inscription.Actions.Add(action);
            await _inscriptionRepository.SaveContractWithInscriptionAsync(contract, inscription);

            await _absOnlineService.SendNotificationCreditLineChangedAsync(contract.Id, contract);

            return inscription;
        }

        public async Task ResumeAccruals(UpdateLegalCaseCommand request)
        {
            var contract = _contractService.Get(request.ContractId);
            if (contract == null)
            {
                throw new PawnshopApplicationException($"Договор {request.ContractId} не найден");
            }

            // пока оставляю
            // if (contract.InscriptionId is null || !contract.IsOffBalance)
            // {
            //     throw new PawnshopApplicationException("Проверьте контракт. Некорректные данные исп. надписи");
            // }

            var inscription = await _inscriptionRepository.GetAsync((int)contract.InscriptionId);

            if (inscription == null)
            {
                throw new PawnshopApplicationException($"Исп. надпись c Id: {request.ContractId} не найдена");
            }

            if (inscription.Status >= InscriptionStatus.Denied)
            {
                throw new PawnshopApplicationException("Исполнительная надпись уже была отозвана или исполнена");
            }

            InscriptionAction action = await CreateWithdrawAction(request.UserId, inscription);

            contract.IsOffBalance = false;
            contract.InscriptionId = null;

            inscription.Status = InscriptionStatus.Denied;
            var rowsToWriteOff = await GetInscriptionRowsAsync(inscription, contract, DateTime.Now);
            if (rowsToWriteOff.Any())
            {
                var orders = RestoreOnBalance(contract, DateTime.Now, rowsToWriteOff);
                var rows = rowsToWriteOff.Select(t =>
                    new InscriptionRow
                    {
                        Cost = t.Cost,
                        PaymentType = t.PaymentType
                    }).Distinct().ToList();
                UpdateActionRows(orders, rows);

                action.Rows = rows;
            }

            RestoreOrders(inscription, contract, InscriptionActionType.Confirm);

            await _inscriptionRepository.SaveInscriptionWithActions(contract, inscription, action);

            await _absOnlineService.SendNotificationCreditLineChangedAsync(contract.Id);

            if (contract.ContractClass == ContractClass.Tranche && contract.CreditLineId != null)
            {
                var creditLineService = _serviceProvider.GetRequiredService<ICreditLineService>();
                var balance = await creditLineService.GetCurrentlyDebtForCreditLine(contract.CreditLineId.Value);

                var prepaymentBalanceOnTranche = balance.ContractsBalances
                    .SingleOrDefault(cb => cb.ContractId == contract.Id)
                    .PrepaymentBalance;

                if (prepaymentBalanceOnTranche > 0)
                {
                    using var transaction = BeginInscriptionTransaction();

                    creditLineService.MovePrepaymentFromTrancheToCreditLine(
                        contract.CreditLineId.Value,
                        contract.Id,
                        prepaymentBalanceOnTranche,
                        request.UserId,
                        contract.BranchId,
                        autoApprove: true);

                    transaction.Commit();
                }
            }
        }

        public async Task AddInscriptionRow(int inscriptionId, Inscription inscription, InscriptionRow row)
        {
            inscription ??= await GetAsync(inscriptionId);
            if (inscription is null)
            {
                throw new PawnshopApplicationException($"Исп. надпись с Id: {inscriptionId} не найдена");
            }

            await _inscriptionRepository.AddInscriptionRow(inscription, row);
        }

        public async Task ExecuteInscription(UpdateLegalCaseCommand request)
        {
            var contract = _contractService.Get(request.ContractId);
            if (contract.InscriptionId == null || contract.InscriptionId == 0)
            {
                throw new PawnshopApplicationException($"У контракта {contract.Id} нет исполнительной надписи");
            }

            Inscription inscription = await GetAsync(contract.InscriptionId);
            if (inscription is null)
            {
                throw new PawnshopApplicationException(
                    $"Исполнительная надпись с Id: {contract.InscriptionId} не найдена ");
            }

            var action = new InscriptionAction
            {
                ActionType = InscriptionActionType.Execution,
                AuthorId = request.UserId,
                CreateDate = DateTime.Now,
                Date = DateTime.Now,
                InscriptionId = inscription.Id
            };

            inscription.Status = InscriptionStatus.Executed;
            inscription.Actions.Add(action);

            await _inscriptionRepository.SaveInscriptionWithActions(contract, inscription, action);
            await _absOnlineService.SendNotificationCreditLineChangedAsync(inscription.ContractId);
        }

        public async Task<bool> ApproveInscriptionAsync(int inscriptionId, Inscription inscription = null)
        {
            inscription ??= await _inscriptionRepository.GetOnlyInscriptionAsync(inscriptionId);
            if (inscription is null)
            {
                return false;
            }
            
            var action = new InscriptionAction
            {
                ActionType = InscriptionActionType.Confirm,
                AuthorId = inscription.AuthorId,
                CreateDate = DateTime.Now,
                Date = inscription.Date,
                InscriptionId = inscription.Id,
                Rows = inscription.Rows
            };

            inscription.Status = InscriptionStatus.Approved;
            inscription.Actions.Add(action);

            using var transaction = _inscriptionRepository.BeginTransaction();

            try
            {
                await _inscriptionRepository.SaveInscriptionAsync(inscription);
            
                transaction.Commit();
            
                await _absOnlineService.SendNotificationCreditLineChangedAsync(inscription.ContractId);
                return true;
            }
            catch (Exception e)
            {
                transaction.Rollback();
                throw new PawnshopApplicationException(e.Message);
            }
        }

        
        private void UpdateActionRows(IEnumerable<CashOrder> orders, List<InscriptionRow> rows)
        {
            if (!orders.Any() || orders == null)
            {
                return;
            }
            foreach (var order in orders)
            {
                var amountType = _businessOperationSettingService.Get(order.BusinessOperationSettingId.Value)
                    .AmountType;
                var row = rows.FirstOrDefault(t => t.PaymentType == amountType && t.OrderId is null);
                if (row != null)
                {
                    row.OrderId = order.Id;
                }
                else
                {
                    rows.Add(new InscriptionRow
                    {
                        Cost = order.OrderCost,
                        PaymentType = amountType
                    });
                }
            }
        }
        
        private async Task<InscriptionAction> CreateWithdrawAction(int userId, Inscription inscription)
        {
            var action = new InscriptionAction
            {
                ActionType = InscriptionActionType.Withdraw,
                AuthorId = userId,
                CreateDate = DateTime.Now,
                Date = DateTime.Now,
                InscriptionId = inscription.Id
            };

            return action;
        }
        
        private void CheckForDiscountsExistance(Contract contract)
        {
            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            List<ContractDiscount> contractDiscounts = _contractDiscountRepository.GetByContractId(contract.Id);
            if (contractDiscounts == null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(contractDiscounts)} не будет null");

            List<ContractDiscount> activeContractDiscounts =
                contractDiscounts.Where(d => d.Status == ContractDiscountStatus.Accepted).ToList();

            if (activeContractDiscounts.Count > 0)
            {
                var activeNonTypicalContractDiscounts = activeContractDiscounts.Where(d => !d.IsTypical).ToList();
                if (activeNonTypicalContractDiscounts.Count > 0)
                {
                    throw new PawnshopApplicationException(
                        $"По данному договору({contract.ContractNumber}) существуют скидки по сумме, перед созданием исполнительной надписи отмените все скидки");
                }
            }
        }
    }
}