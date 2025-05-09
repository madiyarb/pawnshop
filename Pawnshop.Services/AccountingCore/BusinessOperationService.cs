using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Access.AccountingCore;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.PayOperations;
using Pawnshop.Services.Exceptions;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Pawnshop.Core.Extensions;
using Type = Pawnshop.AccountingCore.Models.Type;

namespace Pawnshop.Services.AccountingCore
{
    /// <summary>
    /// Бизнес-операции
    /// </summary>
    public class BusinessOperationService : IBusinessOperationService
    {
        private readonly BusinessOperationRepository _repository;
        private readonly GroupRepository _groupRepository;
        private readonly ContractLoanSubjectRepository _contractLoanSubjectRepository;
        private readonly LoanSubjectRepository _loanSubjectRepository;
        private readonly TypeRepository _typeRepository;
        private readonly IDictionaryWithSearchService<BusinessOperationSetting, BusinessOperationSettingFilter> _businessOperationSettingService;
        private readonly IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> _accountSettingService;
        private readonly ICashOrderService _cashOrderService;
        private readonly IAccountService _accountService;
        private readonly IAccountPlanSettingService _accountPlanSettingService;
        private readonly IDictionaryService<Type> _typeService;

        public BusinessOperationService(BusinessOperationRepository repository,
            IDictionaryWithSearchService<BusinessOperationSetting, BusinessOperationSettingFilter> businessOperationSettingService,
            IDictionaryWithSearchService<AccountSetting, AccountSettingFilter> accountSettingService,
            ICashOrderService cashOrderService,
            IAccountService accountService,
            IAccountPlanSettingService accountPlanSettingService,
            IDictionaryService<Type> typeService,
            GroupRepository groupRepository,
            ContractLoanSubjectRepository contractLoanSubjectRepository,
            LoanSubjectRepository loanSubjectRepository,
            TypeRepository typeRepository)
        {
            _repository = repository;
            _accountSettingService = accountSettingService;
            _businessOperationSettingService = businessOperationSettingService;
            _cashOrderService = cashOrderService;
            _accountService = accountService;
            _accountPlanSettingService = accountPlanSettingService;
            _typeService = typeService;
            _groupRepository = groupRepository;
            _contractLoanSubjectRepository = contractLoanSubjectRepository;
            _loanSubjectRepository = loanSubjectRepository;
            _typeRepository = typeRepository;
        }

        public BusinessOperation Save(BusinessOperation model)
        {
            var m = new Data.Models.AccountingCore.BusinessOperation(model);
            if (model.Id > 0)
            {
                _repository.Update(m);
            }
            else
            {
                _repository.Insert(m);
            }

            model.Id = m.Id;
            return m;
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

        public async Task<BusinessOperation> GetAsync(int id)
        {
            return await _repository.GetAsync(id);
        }

        public ListModel<BusinessOperation> List(ListQuery listQuery)
        {
            return new ListModel<BusinessOperation>
            {
                List = _repository.List(listQuery).AsEnumerable<BusinessOperation>().ToList(),
                Count = _repository.Count(listQuery)
            };
        }

        public ListModel<BusinessOperation> List(ListQueryModel<BusinessOperationFilter> listQuery)
        {
            return new ListModel<BusinessOperation>
            {
                List = _repository.List(listQuery, listQuery.Model).AsEnumerable<BusinessOperation>().ToList(),
                Count = _repository.Count(listQuery, listQuery.Model)
            };
        }

        public List<(CashOrder, List<AccountRecord>)> Register(
            DateTime date,
            string code,
            int branchId,
            int authorId,
            IDictionary<AmountType, decimal> amounts,
            int payTypeId, 
            OrderStatus? orderStatus = null,
            int? orderUserId = null,
            string note = null,
            int? remittanceBranchId = null,
            int? clientId = null)
        {
            
            Group branch = _groupRepository.Get(branchId);
            if (branch == null)
                throw new PawnshopApplicationException($"Филиал {branchId} не найден");

            Pawnshop.AccountingCore.Models.Type contractAllType = _typeRepository.GetByCode(Constants.TYPE_HIERARCHY_CONTRACTS_ALL);
            if (contractAllType == null)
                throw new PawnshopApplicationException($"Тип иерархии с кодом '{Constants.TYPE_HIERARCHY_CONTRACTS_ALL}' не найден");

            Pawnshop.AccountingCore.Models.Type termsAllType = _typeRepository.GetByCode(Constants.TYPE_HIERARCHY_TERMS_ALL);
            if (termsAllType == null)
                throw new PawnshopApplicationException($"Тип иерархии с кодом '{Constants.TYPE_HIERARCHY_TERMS_ALL}' не найден");

            var operation = FindBusinessOperation(contractAllType.Id, code, branch.Id, branch.OrganizationId);
            date = ProcessBusinessOperationDate(date);
            date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);

            if (orderStatus == null && operation.OrdersCreateStatus.HasValue)
                orderStatus = (OrderStatus)operation.OrdersCreateStatus.Value;

            List<(CashOrder, List<AccountRecord>)> ordersWithRecords = new List<(CashOrder, List<AccountRecord>)>();
            List<ContractActionRow> rows = new List<ContractActionRow>();
            foreach (var (key, value) in amounts.Where(x => x.Value > 0))
            {
                var settings = FindSettings(key, operation, payTypeId);
                Account debit = null;
                Account credit = null;
                foreach (var setting in settings.OrderBy(x => x.OrderBy))
                {
                    if (setting.CreditSettingId.HasValue)
                    {
                        credit = FindConsolidatedAccountForOperation(_accountSettingService.GetAsync(setting.CreditSettingId.Value).Result, branch, contractAllType.Id, termsAllType.Id);
                    }

                    if (setting.DebitSettingId.HasValue)
                    {
                        debit = FindConsolidatedAccountForOperation(_accountSettingService.GetAsync(setting.DebitSettingId.Value).Result, branch, contractAllType.Id, termsAllType.Id);
                    }

                    var reason = setting.Reason;
                    var reasonKaz = setting.ReasonKaz;

                    if (setting.Code == Constants.BO_REMITTANCE_IN || setting.Code == Constants.BO_REMITTANCE_OUT)
                    {
                        Group remittanceBranch = _groupRepository.Get(remittanceBranchId.Value);
                        if (remittanceBranch == null)
                            throw new PawnshopApplicationException($"Филиал передачи {remittanceBranchId} не найден");

                        reason = $"{setting.Reason} {remittanceBranch.DisplayName}";
                        reasonKaz = $"{setting.ReasonKaz} {remittanceBranch.DisplayName}";
                    }

                    (CashOrder, List<AccountRecord>) orderWithRecords =
                    _cashOrderService.Build(
                        debit: debit,
                        credit: credit,
                        amount: value,
                        date: date,
                        reason: reason,
                        reasonKaz: reasonKaz,
                        authorId: authorId,
                        branch: branch,
                        orderType: setting.OrderType,
                        businessOperationId: setting.BusinessOperationId,
                        businessOperationSettingId: setting.Id,
                        businessOperationSetting: setting,
                        status: orderStatus,
                        userId: orderUserId,
                        note: note,
                        clientId: clientId
                    );
                    ordersWithRecords.Add(orderWithRecords);
                    debit = credit = null;
                }
            }

            using (var transaction = _cashOrderService.BeginCashOrderTransaction())
            {
                ordersWithRecords.OrderBy(x => x.Item1.BusinessOperationSetting.OrderBy).ToList().ForEach(orderWithRecords =>
                {
                    _cashOrderService.Register(orderWithRecords, branch);
                });

                transaction.Commit();
                return ordersWithRecords;
            }
        }

        /// <summary>
        /// Регистрация бизнес оперций без связки c Contract/ContractAction
        /// </summary>
        /// <param name="branchFromId">Идентификатор филиала отправителя</param>
        /// <param name="typeCode">Код типа иерархии</param>
        /// <param name="branchToId">Идентификатор филиала получателя</param>
        /// <param name="orderUserId">Идентификатор контр-агента</param>
        /// <param name="clientId">Идентификатор контрагент-клиента ордера</param>
        /// <param name="orderStatus">Статус ордера при создании</param>
        public async Task<List<(CashOrder, List<AccountRecord>)>> ExecuteRegistrationAsync(
            DateTime date,
            string businessOperationCode,
            int branchFromId,
            int authorId,
            IDictionary<AmountType, decimal> amounts,
            string typeCode,
            int? branchToId = null,
            OrderStatus? orderStatus = null,
            int? orderUserId = null,
            string note = null,
            int? clientId = null)
        {
            Group branchFrom = await _groupRepository.GetAsync(branchFromId);
            if (branchFrom == null)
            {
                throw new PawnshopApplicationException($"Филиал с Id: {branchFromId} не найден");
            }
            
            Type type = await _typeRepository.GetByCodeAsync(typeCode);
            if (type == null)
            {
                throw new PawnshopApplicationException($"Тип иерархии с кодом '{Constants.TYPE_HIERARCHY_TERMS_ALL}' не найден");
            }

            var operation = await FindBusinessOperationAsync(type, businessOperationCode);

            date = ProcessBusinessOperationDate(date);
            date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);

            if (orderStatus == null && operation.OrdersCreateStatus.HasValue)
            {
                orderStatus = (OrderStatus)operation.OrdersCreateStatus.Value;
            }

            var ordersWithRecords = new List<(CashOrder, List<AccountRecord>)>();
            foreach (var (amountType, amount) in amounts.Where(x => x.Value > 0))
            {
                var settings = FindSettings(amountType, operation, null);
                Account debit = null;
                Account credit = null;
                foreach (var setting in settings.OrderBy(x => x.OrderBy))
                {
                    if (setting.CreditSettingId.HasValue)
                    {
                        var creditAccountSetting = await _accountSettingService.GetAsync(setting.CreditSettingId.Value);
                        if (creditAccountSetting is null)
                        {
                            throw new PawnshopApplicationException("Настройки для кредитного счёта не найдены!");
                        }
                        var foundCredit = await _accountService.GetByAccountSettingIdAsync(creditAccountSetting.Id);
                        credit = foundCredit ?? throw new PawnshopApplicationException(
                            $"Кредитовый счёт с CreditSettingId: {setting.CreditSettingId.Value} не найден!");
                    }

                    if (setting.DebitSettingId.HasValue)
                    {
                        var debitAccountSetting = await _accountSettingService.GetAsync(setting.DebitSettingId.Value);
                        if (debitAccountSetting is null)
                        {
                            throw new PawnshopApplicationException("Настройки для дебитого счёта не найдены!");
                        }
                        var foundDebit = await _accountService.GetByAccountSettingIdAsync(debitAccountSetting.Id);
                        debit = foundDebit ?? throw new PawnshopApplicationException(
                            $"Дебитовый счёт с AccountSettingId: {debitAccountSetting.Id} не найден!");
                    }

                    var reason = setting.Reason;
                    var reasonKaz = setting.ReasonKaz;

                    if (setting.Code == Constants.BO_REMITTANCE_IN || setting.Code == Constants.BO_REMITTANCE_OUT)
                    {
                        reason = $"{setting.Reason} {branchFrom.DisplayName}";
                        reasonKaz = $"{setting.ReasonKaz} {branchFrom.DisplayName}";
                    }

                    (CashOrder, List<AccountRecord>) orderWithRecords =
                        _cashOrderService.Build(
                            debit: debit,
                            credit: credit,
                            amount: amount,
                            date: date,
                            reason: reason,
                            reasonKaz: reasonKaz,
                            authorId: authorId,
                            branch: branchFrom,
                            orderType: setting.OrderType,
                            businessOperationId: setting.BusinessOperationId,
                            businessOperationSettingId: setting.Id,
                            businessOperationSetting: setting,
                            status: orderStatus,
                            userId: orderUserId,
                            note: note,
                            clientId: clientId
                        );

                    orderWithRecords.Item1.CreditAccount = credit;
                    orderWithRecords.Item1.CreditAccountId = credit.Id;
                    
                    orderWithRecords.Item1.DebitAccount = debit;
                    orderWithRecords.Item1.DebitAccountId = debit.Id;
                    ordersWithRecords.Add(orderWithRecords);
                    debit = credit = null;
                }
            }

            using var transaction = _cashOrderService.BeginCashOrderTransaction();
            
            var orderedOrdersWithRecords = ordersWithRecords
                .OrderBy(x => x.Item1.BusinessOperationSetting.OrderBy);

            foreach (var orderWithRecords in orderedOrdersWithRecords)
            {
                _cashOrderService.Register(orderWithRecords, branchFrom);
            }

            transaction.Commit();

            return ordersWithRecords;
        }

        /// <summary>
        /// Регистрация бизнес оперций
        /// </summary>
        /// <param name="typeCode">Код типа иерархии</param>
        /// <param name="remittanceBranchId">Филиал получатель</param>
        /// <param name="orderStatus">Статус ордера при создании</param>
        /// <param name="orderUserId">идентификатор контр агента</param>
        /// <param name="note">Заметка</param>
        public async Task<List<(CashOrder, List<AccountRecord>)>> ExecuteRegistrationAsync(
            DateTime date,
            string businessOperationCode,
            int authorId,
            IDictionary<AmountType, decimal> amounts,
            string typeCode,
            int remittanceBranchId,
            int? contractActionId,
            Contract? contract,
            OrderStatus? orderStatus = null,
            int? orderUserId = null,
            string note = null,
            int? clientId = null)
        {
            Group remittanceBranch = await _groupRepository.GetAsync(remittanceBranchId);
            if (remittanceBranch == null)
            {
                throw new PawnshopApplicationException($"Филиал передачи {remittanceBranchId} не найден");
            }

            Type type = await _typeRepository.GetByCodeAsync(typeCode);
            if (type == null)
            {
                throw new PawnshopApplicationException($"Тип иерархии с кодом '{typeCode}' не найден");
            }

            var businessOperation = await FindBusinessOperationAsync(type, businessOperationCode);

            date = ProcessBusinessOperationDate(date);
            date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);

            if (orderStatus == null && businessOperation.OrdersCreateStatus.HasValue)
            {
                orderStatus = (OrderStatus)businessOperation.OrdersCreateStatus.Value;
            }
            
            var ordersWithRecords = new List<(CashOrder, List<AccountRecord>)>();
            foreach (var (amountType, amount) in amounts.Where(x => x.Value > 0))
            {
                var settings = FindSettings(amountType, businessOperation, null);
                Account debit;
                Account credit;
                foreach (var setting in settings.OrderBy(x => x.OrderBy))
                {
                    if (!setting.CreditSettingId.HasValue)
                    {
                        throw new PawnshopApplicationException("Настройки для кредитного счёта не найдены!");
                    }
                    
                    var foundCreditAccount = await GetAccount(setting.CreditSettingId.Value, contract);
                    if (foundCreditAccount is null)
                    {
                        throw new PawnshopApplicationException(
                            $"Кредитовый счёт с AccountSettingsId: {setting.CreditSettingId.Value} не найден!");
                    }

                    credit = foundCreditAccount;

                    if (!setting.DebitSettingId.HasValue)
                    {
                        throw new PawnshopApplicationException("Настройки для дебитового счёта не найдены!");
                    }
                    
                    var foundDebitAccount = await GetAccount(setting.DebitSettingId.Value, contract);
                    if (foundDebitAccount is null)
                    {
                        throw new PawnshopApplicationException(
                            $"Дебитовый счёт с AccountSettingsId: {setting.CreditSettingId.Value} не найден!");
                    }

                    debit = foundDebitAccount;

                    var reason = setting.Reason;
                    var reasonKaz = setting.ReasonKaz;

                    if (setting.Code == Constants.BO_REMITTANCE_IN || setting.Code == Constants.BO_REMITTANCE_OUT)
                    {
                        reason = $"{setting.Reason} {remittanceBranch.DisplayName}";
                        reasonKaz = $"{setting.ReasonKaz} {remittanceBranch.DisplayName}";
                    }

                    (CashOrder, List<AccountRecord>) orderWithRecords =
                        _cashOrderService.Build(
                            debit: debit,
                            credit: credit,
                            amount: amount,
                            date: date,
                            reason: reason,
                            reasonKaz: reasonKaz,
                            authorId: authorId,
                            branch: remittanceBranch,
                            orderType: setting.OrderType,
                            contractId: contract.Id,
                            contractActionId: contractActionId,
                            businessOperationId: setting.BusinessOperationId,
                            businessOperationSettingId: setting.Id,
                            businessOperationSetting: setting,
                            status: orderStatus,
                            userId: orderUserId,
                            note: note,
                            clientId: clientId
                        );

                    orderWithRecords.Item1.CreditAccount = credit;
                    orderWithRecords.Item1.CreditAccountId = credit.Id;
                    
                    orderWithRecords.Item1.DebitAccount = debit;
                    orderWithRecords.Item1.DebitAccountId = debit.Id;
                    ordersWithRecords.Add(orderWithRecords);
                    debit = credit = null;
                }
            }
            
            using (var transaction = _cashOrderService.BeginCashOrderTransaction())
            {
                var orderedOrdersWithRecords = ordersWithRecords
                    .OrderBy(x => x.Item1.BusinessOperationSetting.OrderBy);

                foreach (var orderWithRecords in orderedOrdersWithRecords)
                {
                    _cashOrderService.Register(orderWithRecords, remittanceBranch);
                }

                transaction.Commit();
            }
            
            return ordersWithRecords;
        }
        
        /// <summary>
        /// Регистрация бизнес оперций
        /// </summary>
        /// <param name="code">Код бизнес операции</param>
        /// <param name="branch">Филиал получаетель</param>
        /// <param name="amounts">объекты KeyValuePair|AmountType, decimal| для создания оредров </param>
        /// <param name="orderUserId">идентификатор контр-агента </param>
        public List<(CashOrder, List<AccountRecord>)> Register(
            IContract contract,
            DateTime date,
            string code,
            Group branch,
            int authorId,
            IDictionary<AmountType, decimal> amounts,
            int? payTypeId = null,
            bool isMigration = false,
            ContractAction action = null,
            OrderStatus? orderStatus = null,
            int? typeId = null,
            int? orderUserId = null,
            PayOperation payOperation = null,
            IContract creditLine = null)
        {
            if (payOperation != null && payOperation.PayType == null)
                throw new ArgumentException($"Поле {payOperation.PayType} не должно быть null", nameof(payOperation));

            var operation = FindBusinessOperation(typeId ?? contract.ContractTypeId, code, branch.Id, branch.OrganizationId);

            if (orderStatus == null && operation.OrdersCreateStatus.HasValue)
                orderStatus = (OrderStatus)operation.OrdersCreateStatus.Value;

            if (orderStatus == OrderStatus.WaitingForApprove && action != null)
                action.Status = ContractActionStatus.Await;

            date = ProcessBusinessOperationDate(date);
            date = new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second);

            List<(CashOrder, List<AccountRecord>)> ordersWithRecords = new List<(CashOrder, List<AccountRecord>)>();
            List<ContractActionRow> rows = new List<ContractActionRow>();
            foreach (var (key, value) in amounts.Where(x => x.Value > 0))
            {
                var settings = FindSettings(key, operation, payTypeId);

                var accounts = _accountService.List(new ListQueryModel<AccountFilter>
                {
                    Page = null,
                    Model = new AccountFilter
                    {
                        ContractId = contract.Id,
                        IsOpen = true,
                        IsOutmoded = false
                    }
                }).List;

                List<Account> accountsExpense = null;

                if (creditLine != null && key == AmountType.Expense)
                {
                    accountsExpense = _accountService.List(new ListQueryModel<AccountFilter>
                    {
                        Page = null,
                        Model = new AccountFilter
                        {
                            ContractId = creditLine.Id,
                            IsOpen = true,
                            IsOutmoded = false
                        }
                    }).List;
                }

                Account debit = null;
                Account credit = null;

                foreach (var setting in settings.OrderBy(x => x.OrderBy))
                {
                    if (setting.CreditSettingId.HasValue)
                    {
                        if (creditLine != null && setting.Code == Constants.BUSINESS_OPERATION_EXPENSE_PAYMENT)
                            credit = FindAccountForOperation(accountsExpense, _accountSettingService.GetAsync(setting.CreditSettingId.Value).Result, creditLine, branch);
                        else
                            credit = FindAccountForOperation(accounts, _accountSettingService.GetAsync(setting.CreditSettingId.Value).Result, contract, branch);
                    }

                    if (setting.DebitSettingId.HasValue)
                    {
                        debit = FindAccountForOperation(accounts, _accountSettingService.GetAsync(setting.DebitSettingId.Value).Result, contract, branch);
                    }

                    int? payOperationIdForCurrentIteration = null;
                    if (payOperation != null)
                    {
                        if (debit?.Id == payOperation.PayType.AccountId
                            || credit?.Id == payOperation.PayType.AccountId)
                            payOperationIdForCurrentIteration = payOperation.Id;
                    }

                    int clientId = contract.ClientId;
                    if (setting.Code == Constants.BO_SETTING_MERCHANT_CASH_OUT
                        || setting.Code == Constants.BO_SETTING_MERCHANT_CURRENT_ACCOUNT_OUT)
                    {
                        LoanSubject merchantLoanSubject = _loanSubjectRepository.GetByCode(Constants.LOAN_SUBJECT_MERCHANT);
                        if (merchantLoanSubject == null)
                            throw new PawnshopApplicationException($"Тип субъекта договора {Constants.LOAN_SUBJECT_MERCHANT} не найден");

                        List<ContractLoanSubject> contractLoanSubjects =
                            _contractLoanSubjectRepository.GetListByContractIdAndLoanSubjectId(contract.Id, merchantLoanSubject.Id);

                        if (contractLoanSubjects.Count == 0)
                            throw new PawnshopApplicationException($"По данному виду договора должен быть хотя бы один субъект с типом '{Constants.LOAN_SUBJECT_MERCHANT}'");

                        if (contractLoanSubjects.Count > 1)
                            throw new PawnshopApplicationException($"Количество субъектов типа {Constants.LOAN_SUBJECT_MERCHANT} не должно быть больше одного");

                        ContractLoanSubject merchant = contractLoanSubjects.Single();
                        clientId = merchant.ClientId;
                    }

                    (CashOrder, List<AccountRecord>) orderWithRecords =
                        _cashOrderService.Build(
                            debit: debit,
                            credit: credit,
                            payOperationId: payOperationIdForCurrentIteration,
                            amount: value,
                            date: date,
                            reason:
                            $"{setting.Reason} по договору {contract.ContractNumber} от {contract.ContractDate:dd.MM.yyyy}",
                            reasonKaz:
                            $"{contract.ContractDate:dd.MM.yyyy} {contract.ContractNumber} шарт бойынша {setting.ReasonKaz}",
                            authorId: authorId,
                            branch: branch,
                            orderType: setting.OrderType,
                            clientId: clientId,
                            contractId: contract.Id,
                            businessOperationId: setting.BusinessOperationId,
                            businessOperationSettingId: setting.Id,
                            businessOperationSetting: setting,
                            status: orderStatus,
                            userId: orderUserId,
                            contractActionId: action?.Id,
                            processingType: action?.ProcessingType,
                            processingId: action?.ProcessingId
                        );
                    ordersWithRecords.Add(orderWithRecords);

                    if (action != null)
                    {
                        rows.Add(item: new ContractActionRow
                        {
                            ActionId = action.Id,
                            BusinessOperationSettingId = orderWithRecords.Item1.BusinessOperationSettingId,
                            Cost = orderWithRecords.Item1.OrderCost,
                            CreditAccountId = orderWithRecords.Item1.CreditAccountId,
                            CreditAccount = orderWithRecords.Item1.CreditAccount,
                            DebitAccountId = orderWithRecords.Item1.DebitAccountId,
                            DebitAccount = orderWithRecords.Item1.DebitAccount,
                            PaymentType = key
                        });
                    }

                    debit = credit = null;
                }
            }

            using (var transaction = _cashOrderService.BeginCashOrderTransaction())
            {
                ordersWithRecords.OrderBy(x => x.Item1.BusinessOperationSetting.OrderBy).ToList().ForEach(orderWithRecords =>
                {
                    _cashOrderService.Register(orderWithRecords, branch, isMigration);
                    if (action != null)
                    {
                        foreach (var row in rows.Where(x => x.CreditAccountId == orderWithRecords.Item1.CreditAccountId && x.DebitAccountId == orderWithRecords.Item1.DebitAccountId))
                        {
                            row.OrderId = orderWithRecords.Item1.Id;
                        }
                    }
                });

                if (action != null)
                {
                    action.Rows = rows.ToArray();
                }

                transaction.Commit();
                return ordersWithRecords;
            }
        }

        public BusinessOperation FindBusinessOperation(int contractTypeId, string code, int branchId, int organizationId)
        {
            BusinessOperation operation = null;

            var found = false;
            var curentType = _typeService.GetAsync(contractTypeId).Result;
            while (!found)
            {
                var operations = List(new ListQueryModel<BusinessOperationFilter>
                {
                    Page = null,
                    Model = new BusinessOperationFilter
                    {
                        TypeId = curentType.Id,
                        Code = code,
                        BranchId = branchId,
                        OrganizationId = organizationId
                    }
                });
                if (operations.List.Any() && operations.List.Count == 1)
                {
                    operation = operations.List.FirstOrDefault();
                    break;
                }
                if (operations.List.Any() && operations.List.Count >= 1) throw new BusinessOperationNotFoundException($"Выбранная операция ({code}) найдена {operations.List.Count} раз, проверьте настройки!");
                if (!operations.List.Any() && !curentType.ParentId.HasValue) throw new BusinessOperationNotFoundException($"Выбранная операция ({code}) не найдена!");
                if (!operations.List.Any() && curentType.ParentId.HasValue) curentType = curentType.Parent;
            }

            return operation;
        }

        public async Task<BusinessOperation> FindBusinessOperationAsync(Type type, string code, int? branchId = null, int? organizationId = null)
        {
            BusinessOperation operation = null;

            var operations = await GetBusinessOperationsAsync(
                businessOperationCode: code,
                typeId: type.Id,
                branchId: branchId,
                organizationId);
            
            if (operations.IsNullOrEmpty())
            {
                throw new BusinessOperationNotFoundException("Операция не найдена!");
            }
                
            if (operations.Count > 1)
            {
                throw new BusinessOperationNotFoundException(
                    $"Выбранная операция ({code}) найдена {operations.Count} раз, проверьте настройки!");
            }

            if (operations.Count == 1)
            {
                operation = operations.First();
            }

            return operation;
        }
        
        private async Task<List<Data.Models.AccountingCore.BusinessOperation>> GetBusinessOperationsAsync(
            string businessOperationCode,
            int? typeId = null,
            int? branchId = null,
            int? organizationId = null)
        {
            var filterQuery = new BusinessOperationQueryFilter
            {
                Code = businessOperationCode,
                TypeId = typeId,
                OrganizationId = organizationId,
                BranchId = branchId,
                AccountId = null,
                IsManual = null
            };

            var operations = await _repository.ListAsync(filterQuery);
            return operations;
        }
        
        public Account FindConsolidatedAccountForOperation(AccountSetting setting, Group branch, int typeId, int periodTypeId)
        {
            if (setting == null)
                throw new ArgumentNullException(nameof(setting));

            if (branch == null)
                throw new ArgumentNullException(nameof(branch));

            var accountPlanSetting = _accountPlanSettingService.Find(branch.OrganizationId, setting.Id, branch.Id,
                typeId, periodTypeId);
            if (accountPlanSetting == null || !accountPlanSetting.AccountId.HasValue)
                throw new PawnshopApplicationException($"Не найдена настройка для консолидированного счета: {setting.Name}({setting.Code})");

            return _accountService.GetAsync(accountPlanSetting.AccountId.Value).Result;
        }

        public Account FindAccountForOperation(List<Account> accounts, AccountSetting setting, IContract contract, Group branch)
        {
            if (accounts == null)
                throw new ArgumentNullException(nameof(accounts));

            if (setting == null)
                throw new ArgumentNullException(nameof(setting));

            if (contract == null)
                throw new ArgumentNullException(nameof(contract));

            if (branch == null)
                throw new ArgumentNullException(nameof(branch));

            if (setting.IsConsolidated)
            {
                Group contractBranch = _groupRepository.Get(contract.BranchId);
                if (contractBranch == null)
                    throw new PawnshopApplicationException($"Филиал {contract.BranchId} не найден");

                Group neededBranch = setting.SearchBranchBySessionContext ? branch : contractBranch;
                var accountPlanSetting = _accountPlanSettingService.Find(neededBranch.OrganizationId, setting.Id, neededBranch.Id,
                    contract.ContractTypeId, contract.PeriodTypeId);
                if (accountPlanSetting == null || !accountPlanSetting.AccountId.HasValue)
                    throw new PawnshopApplicationException($"Не найдена настройка для консолидированного счета: {setting.Name}({setting.Code})");

                return _accountService.GetAsync(accountPlanSetting.AccountId.Value).Result;
            }

            var account = accounts.FirstOrDefault(x => x.AccountSettingId == setting.Id);
            if (account == null)
            {
                account = _accountService.OpenForContract(contract, setting);
                accounts.Add(account);
            }
            return account;
        }

        public string GetOperationCode(ContractActionType actionType, CollateralType collateralType, bool isFromEmployee = false, bool isReceivable = false, bool isInitialFee = false)
        {
            return actionType switch
            {
                ContractActionType.Sign => "SIGN",
                ContractActionType.Prolong => "PROLONG",
                ContractActionType.Buyout => "PAYMENT",
                ContractActionType.PartialBuyout => "PAYMENT",
                ContractActionType.PartialPayment => "PARTIAL_PAYMENT",
                ContractActionType.Selling => "SELLING",
                //ContractActionType.Transfer => "",
                ContractActionType.MonthlyPayment => "PAYMENT",
                ContractActionType.Addition => "PAYMENT",
                ContractActionType.Prepayment => PrepaymentCode(collateralType, isFromEmployee, isReceivable, isInitialFee),
                ContractActionType.Refinance => "PAYMENT",
                ContractActionType.Payment => "PAYMENT",
                ContractActionType.InterestAccrualOnOverdueDebt => Constants.BO_INTEREST_ACCRUAL_OVERDUEDEBT,
                ContractActionType.PrepaymentReturn => PrepaymentReturnCode(isInitialFee),
                ContractActionType.PrepaymentToTransit => Constants.BO_PREPAYMENT_MOVE_TO_TRANSIT,
                ContractActionType.PrepaymentFromTransit => Constants.BO_PREPAYMENT_MOVE_FROM_TRANSIT,
                ContractActionType.PenaltyLimitAccrual => Constants.BO_PENALTY_LIMIT_ACCRUAL,
                ContractActionType.PenaltyLimitWriteOff => Constants.BO_PENALTY_LIMIT_WRITEOFF,
                ContractActionType.CreditLineClose => Constants.BO_CREDITLINE_CLOSE,
                ContractActionType.RestructuringCred => Constants.BO_RESTRUCTURING_CRED,
                ContractActionType.RestructuringTranches => Constants.BO_RESTRUCTURING_TRANCHES,
                ContractActionType.RestructuringTransferToTransitCred => Constants.BO_RESTRUCTURING_TRANSFER_TO_TRANSIT_CRED,
                ContractActionType.RestructuringTransferToTransitTranches => Constants.BO_RESTRUCTURING_TRANSFER_TO_TRANSIT_TRANCHES,
                ContractActionType.RestructuringTransferToAccountCred => Constants.BO_RESTRUCTURING_TRANSFER_TO_ACCOUNT_CRED,
                ContractActionType.RestructuringTransferToAccountTranches => Constants.BO_RESTRUCTURING_TRANSFER_TO_ACCOUNT_TRANCHES,
                ContractActionType.BuyoutRestructuringTranches => Constants.BO_BUYOUT_RESTRUCTURING_TRANCHES,
                ContractActionType.BuyoutRestructuringCred => Constants.BO_BUYOUT_RESTRUCTURING_CRED,
                _ => throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null)
            };
        }

        private string PrepaymentCode(CollateralType collateralType, bool isFromEmployee, bool isReceivable, bool isInitialFee)
        {
            //if (collateralType == CollateralType.Unsecured) return "PREPAYMENT_UNSECURED";
            if (isFromEmployee) return "PREPAYMENT_FROM_EMPLOYEE";
            if (isReceivable) return "PREPAYMENT_FROM_ONLINE_WITH_DELAY";
            if (isInitialFee) return "INITIALFEE";
            return "PREPAYMENT";
        }

        private string PrepaymentReturnCode(bool isInitialFee)
        {
            if (isInitialFee) return "INITIALFEE_RETURN";
            return "PREPAYMENT_RETURN";
        }

        private List<BusinessOperationSetting> FindSettings(AmountType amountType, BusinessOperation operation, int? payTypeId)
        {
            var settings = _businessOperationSettingService.List(new ListQueryModel<BusinessOperationSettingFilter>
            {
                Page = null,
                Model = new BusinessOperationSettingFilter
                {
                    BusinessOperationId = operation.Id,
                    AmountType = amountType,
                    IsActive = true,
                    PayTypeId = payTypeId
                }
            });

            if (!settings.List.Any()) throw new PawnshopApplicationException($"Настройки для операции {operation.Name}({operation.Code}) не найдены для {amountType}, {nameof(payTypeId)}={payTypeId}!");

            return settings.List;
        }

        private DateTime ProcessBusinessOperationDate(DateTime date)
        {
            DateTime now = DateTime.Now;
            if (date.Date == now.Date)
                date = now;
            else if (date.Date > now.Date)
                date = date.Date;
            else
                date = date.Date.AddDays(1).AddSeconds(-1);

            return date;
        }

        private async Task<Account> GetAccount(int accountSettingId, Contract contract)
        {
            var accountSetting = await _accountSettingService.GetAsync(accountSettingId);
            if (accountSetting is null)
            {
                throw new InvalidOperationException("Настройки для счёта не найдены!");
            }

            if (accountSetting.IsConsolidated)
            {
                return await _accountService.GetByAccountSettingIdAsync(accountSetting.Id);
            }

            return await _accountService.GetByAccountSettingId(contract.Id, accountSetting.Id);
        }
    }
}
