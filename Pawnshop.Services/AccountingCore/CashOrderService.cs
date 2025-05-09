using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Access.AccountingCore;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Contracts.Expenses;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Integrations.UKassa;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;
using User = Pawnshop.Data.Models.Membership.User;

namespace Pawnshop.Services.AccountingCore
{
    public class CashOrderService : ICashOrderService
    {
        private readonly CashOrderRepository _cashOrderRepository;
        private readonly CashOrderNumberCounterRepository _orderNumberCounterRepository;
        private readonly AccountRecordService _accountRecordService;
        private readonly IDictionaryWithSearchService<Currency, CurrencyFilter> _currencyService;
        private readonly UserRepository _userRepository;
        private readonly GroupRepository _groupRepository;
        private readonly IUKassaService _ukassaService;
        private readonly IContractActionService _contractActionService;
        private readonly BusinessOperationRepository _businessOperationRepository;
        private readonly BusinessOperationSettingRepository _businessOperationSettingRepository;
        private readonly ContractExpenseRowOrderRepository _contractExpenseRowOrderRepository;
        private readonly ISessionContext _sessionContext;
        private readonly LanguagesRepository _languagesRepository;
        
        private readonly List<string> _auctionBusinessOperationSettingCodes = new List<string>
        {
            Constants.AUCTION_BOS_EXPENSE_PAYMENT_CASH
        };

        public CashOrderService(CashOrderRepository cashOrderRepository,
            CashOrderNumberCounterRepository orderNumberCounterRepository,
            AccountRecordService accountRecordService,
            IDictionaryWithSearchService<Currency, CurrencyFilter> currencyService,
            UserRepository userRepository, GroupRepository groupRepository, IUKassaService ukassaService,
            IContractActionService contractActionService,
            BusinessOperationRepository businessOperationRepository,
            BusinessOperationSettingRepository businessOperationSettingRepository,
            ContractExpenseRowOrderRepository contractExpenseRowOrderRepository,
            ISessionContext sessionContext, LanguagesRepository languagesRepository)
        {
            _cashOrderRepository = cashOrderRepository;
            _orderNumberCounterRepository = orderNumberCounterRepository;
            _accountRecordService = accountRecordService;
            _currencyService = currencyService;
            _userRepository = userRepository;
            _groupRepository = groupRepository;
            _ukassaService = ukassaService;
            _contractActionService = contractActionService;
            _businessOperationRepository = businessOperationRepository;
			_businessOperationSettingRepository = businessOperationSettingRepository;
            _contractExpenseRowOrderRepository = contractExpenseRowOrderRepository;
            _sessionContext = sessionContext;
            _languagesRepository = languagesRepository;
        }

        public IDbTransaction BeginCashOrderTransaction()
        {
            return _cashOrderRepository.BeginTransaction();
        }

        public IDictionary<int, (int, DateTime)> Delete(int id, int authorId, int branchId)
        {
            User user = _userRepository.Get(authorId);
            if (user == null)
                throw new PawnshopApplicationException($"Пользователь {authorId} не найден");

            Group branch = _groupRepository.Get(branchId);
            if (branch == null)
                throw new PawnshopApplicationException($"Филиал {branchId} не найден");

            var accountDictionary = new Dictionary<int, (int, DateTime)>();
            CashOrder cashOrder = _cashOrderRepository.GetAsync(id).Result;
            if (cashOrder == null)
                throw new PawnshopApplicationException($"Кассовый ордер {id} не найден");

            if (cashOrder.ApproveStatus != OrderStatus.Approved)
            {
                _cashOrderRepository.Delete(cashOrder.Id);
                return accountDictionary;
            }

            if (cashOrder.OrderType == OrderType.CashIn || cashOrder.OrderType == OrderType.CashOut || cashOrder.OrderType == OrderType.Payment)
            {
                Cancel(cashOrder, user.Id, branch);
                return accountDictionary;
            }

            var accountRecordQuery = new ListQueryModel<AccountRecordFilter>
            {
                Page = null,
                Model = new AccountRecordFilter
                {
                    OrderId = cashOrder.Id,
                }
            };
            ListModel<AccountRecord> accountRecordsListModel = _accountRecordService.List(accountRecordQuery);
            if (accountRecordsListModel == null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(accountRecordsListModel)} не будет null");

            List<AccountRecord> accountRecords = accountRecordsListModel.List;
            if (accountRecords == null)
                throw new PawnshopApplicationException($"Ожидалось что {nameof(accountRecords)} не будет null");

            using (IDbTransaction transaction = _cashOrderRepository.BeginTransaction())
            {
                foreach (AccountRecord accountRecord in accountRecords)
                {
                    _accountRecordService.Delete(accountRecord.Id);
                    if (accountDictionary.ContainsKey(accountRecord.AccountId))
                    {
                        (int _, DateTime date) = accountDictionary[accountRecord.AccountId];
                        if (date < accountRecord.Date)
                            continue;
                    }

                    accountDictionary[accountRecord.AccountId] = (accountRecord.Id, accountRecord.Date);
                }

                //if (cashOrder.CreditAccountId.HasValue)
                //    _accountRecordService.RecalculateBalanceOnAccount(cashOrder.CreditAccountId.Value);

                //if (cashOrder.DebitAccountId.HasValue)
                //    _accountRecordService.RecalculateBalanceOnAccount(cashOrder.DebitAccountId.Value);

                _cashOrderRepository.Delete(id);
                transaction.Commit();
                return accountDictionary;
            }
        }

        public void Delete(int id)
        {
            CashOrder cashOrder = _cashOrderRepository.GetAsync(id).Result;
            if (cashOrder == null)
                throw new PawnshopApplicationException($"Кассовый ордер {id} не найден");

            _cashOrderRepository.Delete(id);
        }

        public Task<CashOrder> GetAsync(int id)
        {
            return _cashOrderRepository.GetAsync(id);
        }

        public async Task<CashOrder> GetByStornoIdAsync(int stornoId)
        {
            return await _cashOrderRepository.GetOrderByStornoIdAsync(stornoId);
        }

        public ListModel<CashOrder> List(ListQueryModel<CashOrderFilter> listQuery)
        {
            return new ListModel<CashOrder>()
            {
                List = _cashOrderRepository.List(listQuery, listQuery.Model),
                Count = _cashOrderRepository.Count(listQuery, listQuery.Model),
            };
        }

        public ListModel<CashOrder> List(ListQuery listQuery)
        {
            return new ListModel<CashOrder>()
            {
                List = _cashOrderRepository.List(listQuery),
                Count = _cashOrderRepository.Count(listQuery),
            };
        }

        public CashOrder Save(CashOrder model)
        {
            if (model.OrderDate.Date == model.RegDate.Date) model.OrderDate = model.RegDate;
            if (model.OrderDate.TimeOfDay == TimeSpan.Zero) model.OrderDate = model.OrderDate.AddDays(1).Subtract(new TimeSpan(0, 0, 0, 1));

            if (model.Id > 0) _cashOrderRepository.Update(model);
            else _cashOrderRepository.Insert(model);
            return model;
        }

        public void UndoDelete(int id)
        {
            _cashOrderRepository.UndoDelete(id);
        }

        public CashOrder Register(OrderType orderType, Contract contract, ContractAction action, ContractActionRow row,
            int authorId, Group branch, Currency currency)
        {
            var code = CodeForOrderByOrderType(branch, orderType);

            var order = new CashOrder
            {
                OrderType = orderType,
                ClientId = row.LoanSubjectId.HasValue
                    ? contract.Subjects.FirstOrDefault(x => x.Id == row.LoanSubjectId.Value).ClientId
                    : contract.ClientId,
                CreditAccountId = row.CreditAccountId,
                DebitAccountId = row.DebitAccountId,
                OrderCost = Math.Round(row.Cost, 2),
                OrderDate = action.Date,
                Note = string.IsNullOrWhiteSpace(action.Note)
                    ? GetDefaultNote(row.PaymentType, action.ActionType)
                    : action.Note,
                Reason = action.Reason,
                RegDate = DateTime.Now,
                UserId = action.EmployeeId > 0 ? action.EmployeeId : null,
                ProveType = action.EmployeeId > 0 ? ProveType.NotProven : 0,
                OwnerId = branch.Id,
                BranchId = branch.Id,
                AuthorId = authorId,
                CurrencyId = currency.Id
            };
            using (var transaction = _cashOrderRepository.BeginTransaction())
            {

                order.OrderNumber = _orderNumberCounterRepository.Next(
                    order.OrderType, order.OrderDate.Year,
                    branch.Id, code);

                if (row.LoanSubjectId.HasValue &&
                    contract.Subjects?.FirstOrDefault(x => x.Id == row.LoanSubjectId.Value) != null)
                {
                    var subject = contract.Subjects?.FirstOrDefault(x => x.Id == row.LoanSubjectId.Value);
                    if (subject != null)
                    {
                        if (subject.Subject.Code == "MERCHANT" && contract.ProductTypeId.HasValue &&
                            contract.ProductType.Code == "BUYCAR")
                            order.Reason =
                                $"Оплата по поручению согласно п 1.17 Договора займа №{contract.ContractNumber} от {contract.ContractDate:dd.MM.yyyy}";
                    }
                }

                if (action.Status.HasValue && action.Status == ContractActionStatus.Await)
                {
                    order.ApproveStatus = OrderStatus.WaitingForApprove;
                }

                Save(order);

                transaction.Commit();
            }
            return order;
        }

        public CashOrder Migrate(CashOrder order)
        {
            var records = _accountRecordService.Build(order, true);

            using (var transaction = _cashOrderRepository.BeginTransaction())
            {
                Save(order);
                if (order.ApproveStatus == OrderStatus.Approved)
                {
                    foreach (var record in records)
                    {
                        _accountRecordService.Register(record, true);
                    }
                }

                transaction.Commit();
            }



            return order;
        }

        private static string CodeForOrderByOrderType(Group branch, OrderType orderType)
        {
            string code = orderType switch
            {
                OrderType.CashIn => branch.Configuration.CashOrderSettings.CashInNumberCode,
                OrderType.CashOut => branch.Configuration.CashOrderSettings.CashOutNumberCode,
                OrderType.Memorial => $"{branch?.Configuration?.ContractSettings?.NumberCode ?? "ERROR"}-MEM",
                OrderType.OffBalanceIn => $"{branch?.Configuration?.ContractSettings?.NumberCode ?? "ERROR"}-ПВО",
                OrderType.OffBalanceOut => $"{branch?.Configuration?.ContractSettings?.NumberCode ?? "ERROR"}-РВО",
                OrderType.Payment => $"{branch?.Configuration?.ContractSettings?.NumberCode ?? "ERROR"}-ПЛТ",
                _ => throw new ArgumentOutOfRangeException(nameof(orderType), orderType, "Тип ордера не задан или не найден")
            };

            return code;
        }

        private static string GetDefaultNote(AmountType paymentType, ContractActionType actionType)
        {
            return paymentType switch
            {
                AmountType.Debt => actionType == ContractActionType.PartialBuyout ||
                                   actionType == ContractActionType.PartialPayment
                    ? "Частичный основной долг"
                    : "Основной долг",
                AmountType.Loan => "Проценты",
                AmountType.Penalty => "Штраф за просрочку",
                AmountType.Duty => "Госпошлина",
                _ => string.Empty
            };
        }
        /// <summary>
        /// Создание/геренация ордера
        /// </summary>
        /// <param name="debit">Счет дебета</param>
        /// <param name="credit">Счет кредита</param>
        /// <param name="date">Дата</param>
        /// <param name="reason">Причина</param>
        /// <param name="authorId">Автор</param>
        /// <param name="branch">Филиал</param>
        /// <param name="orderType">Вид ордера</param>
        /// <param name="clientId">Контрагент-клиент ордера</param>
        /// <param name="userId">Контрагент-пользователь ордера</param>
        /// <param name="businessOperationId">Идентфикатор бизнес-операции</param>
        /// <param name="businessOperationSettingId">Идентфикатор настройки бизнес-операции</param>
        /// <param name="status">Статус ордера</param>
        /// <param name="contractActionId">Действие на договоре</param>
        /// <returns>Возвращает кассовые ордера и если статус позволяет - новые записи для выписки по счетам</returns>
        public (CashOrder, List<AccountRecord>) Build(Account debit, Account credit, decimal amount, DateTime date, string reason, string reasonKaz,
            int authorId, Group branch, OrderType orderType, int? clientId = null, int? userId = null, int? contractId = null, int? businessOperationId = null,
            int? businessOperationSettingId = null, OrderStatus? status = null, Currency currency = null, int? contractActionId = null,
            int? payOperationId = null, BusinessOperationSetting businessOperationSetting = null, ProcessingType? processingType = null, long? processingId = null,
            string note = null)
        {
            if (amount < 0)
                throw new ArgumentException("Сумма ордера не может быть отрицательной", nameof(amount));

            if (processingId.HasValue && !processingType.HasValue)
                throw new ArgumentException($"Тип процессинга должен быть заполнен вместе с заполненным {nameof(processingId)}", nameof(processingType));

            if (!processingId.HasValue && processingType.HasValue)
                throw new ArgumentException($"Референс процессинга должен быть заполнен вместе с заполненным {nameof(processingType)}", nameof(processingId));

            amount = Math.Round(amount, 2);
            if (amount == 0)
                throw new PawnshopApplicationException("Сумма ордера не может быть равной 0");

            var order = new CashOrder();

            if (currency == null)
            {
                currency = _currencyService.List(new ListQuery { Page = null }).List.FirstOrDefault(x => x.IsDefault);
                if (currency == null) throw new PawnshopApplicationException("Валюта по умолчанию не найдена");

                order.OrderCostNC = amount;
            }
            else
            {
                order.OrderCostNC = amount * currency.CurrentNationalBankExchangeRate * currency.CurrentNationalBankExchangeQuantity;
            }
            var bo = _businessOperationRepository.GetAsync(businessOperationId.Value).Result;
            var businessOperationSett = _businessOperationSettingRepository.GetAsync(businessOperationSettingId.Value).Result;
            if (bo != null && businessOperationSett != null && businessOperationSett.DefaultArticleTypeId.HasValue && bo.HasExpenseArticleType) 
            {
	            var branchId = 0;
	            if (businessOperationSett.Code == Constants.BO_SETTING_IMPRESTS && bo.Code == Constants.BUSINESS_OPERATION_EXPENSE_CREATION) 
	            {
		            var mainBranch = _groupRepository.Find(new { Name = Constants.BKS });
		            branchId = mainBranch.Id;
	            }
	            else 
	            {
		            branchId = branch.Id;
	            }
	            order.OrderExpense = new OrderExpense()
		            {
			            Id = order.Id,
			            ArticleTypeId = (int)businessOperationSett.DefaultArticleTypeId,
			            BranchId = branchId
				};
            }
          
			if (status == null)
            {
                if (businessOperationId != null)
                {
	                if (bo != null)
                        status = bo.OrdersCreateStatus.HasValue ? (OrderStatus)bo.OrdersCreateStatus.Value : OrderStatus.WaitingForApprove;
                }
                else
                    status = OrderStatus.Approved;
            }

            var records = new List<AccountRecord>();

            order.OrderDate = date;
            order.CurrencyId = currency.Id;
            order.ApproveStatus = status.Value;
            order.AuthorId = authorId;
            order.RegDate = DateTime.Now;
            order.OrderCost = amount;
            order.Reason = reason;
            order.ReasonKaz = reasonKaz;
            order.BranchId = branch.Id;
            order.OwnerId = branch.Id;
            order.OrderType = orderType;
            order.DebitAccountId = debit?.Id;
            order.DebitAccount = debit;
            order.CreditAccountId = credit?.Id;
            order.CreditAccount = credit;
            order.ContractId = contractId;
            order.BusinessOperationId = businessOperationId;
            order.BusinessOperationSettingId = businessOperationSettingId;
            order.ContractActionId = contractActionId;
            order.ClientId = clientId;
            order.UserId = userId;
            order.OperationId = payOperationId;
            order.BusinessOperationSetting = businessOperationSetting;
            order.ProcessingType = processingType;
            order.ProcessingId = processingId;
            order.Note = CreateNote(order, note);

            if (status == OrderStatus.Approved)
            {
                order.ApproveDate = DateTime.Now;
                order.ApprovedId = authorId;
                records.AddRange(_accountRecordService.Build(order));
            }

            return (order, records);
        }

        public (CashOrder, List<AccountRecord>) Register((CashOrder, List<AccountRecord>) order, Group branch, bool isMigration = false)
        {
            if (order.Item2 == null) throw new PawnshopApplicationException("При регистрации должны присутствовать записи(AccountRecord)");

            var code = CodeForOrderByOrderType(branch, order.Item1.OrderType);

            using (var transaction = _cashOrderRepository.BeginTransaction())
            {
                order.Item1.OrderNumber = _orderNumberCounterRepository.Next(order.Item1.OrderType, order.Item1.OrderDate.Year, branch.Id, code);
                Save(order.Item1);
                if (order.Item1.ApproveStatus == OrderStatus.Approved)
                {
                    foreach (var record in order.Item2)
                    {
                        record.OrderId = order.Item1.Id;
                        _accountRecordService.Register(record, isMigration);
                    }

                    if (order.Item1.BusinessOperationSettingId.HasValue)
                    {
                        _ukassaService.CreateCheckRequest(order.Item1);
                    }
                }
                transaction.Commit();
                return order;
            }

        }

        public CashOrder Register(CashOrder order, Group branch)
        {
            if (order == null) throw new PawnshopApplicationException("При регистрации должны присутствовать записи(AccountRecord)");

            var code = CodeForOrderByOrderType(branch, order.OrderType);

            using (var transaction = _cashOrderRepository.BeginTransaction())
            {
                if (order.Id == 0)
                    order.OrderNumber = _orderNumberCounterRepository.Next(order.OrderType, order.OrderDate.Year, branch.Id, code);

                Save(order);
                if (order.ApproveStatus == OrderStatus.Approved)
                {
                    var records = _accountRecordService.Build(order);
                    foreach (var record in records)
                    {
                        record.OrderId = order.Id;
                        _accountRecordService.Register(record);
                    }

                    if (order.BusinessOperationSettingId.HasValue)
                    {
                        _ukassaService.CreateCheckRequest(order);
                    }
                }

                transaction.Commit();
                return order;
            }

        }

        public async Task CancelWithoutRecalculateAsync(CashOrder order, int authorId, Group branch)
        {
            if (order == null)
                throw new PawnshopApplicationException("Ордер для сторнирования не может быть пустым");

            if (order.ApproveStatus != OrderStatus.Approved)
                throw new PawnshopApplicationException($"Ордер {order.Id} не подтвержден, его невозможно сторнировать");

            if (order.DeleteDate.HasValue)
                throw new PawnshopApplicationException($"Ордер {order.Id} удален, его невозможно сторнировать");

            CashOrder stornoOrder;

            stornoOrder = await _cashOrderRepository.GetOrderByStornoIdAsync(order.Id);

            if (stornoOrder != null)
                return;

            var bo = await _businessOperationRepository.GetAsync(order.BusinessOperationId.Value);
            var printLanguage = await _cashOrderRepository.GetCashOrderPrintLanguageByOrderIdAsync(order.Id);
            DateTime newOrderDate = DateTime.Now;

            var newOrder = new CashOrder()
            {
                CreditAccountId = order.DebitAccountId,
                CreditAccount = order.DebitAccount,
                DebitAccountId = order.CreditAccountId,
                DebitAccount = order.CreditAccount,
                OrderCost = order.OrderCost,
                OrderCostNC = order.OrderCostNC,
                ClientId = order.ClientId,
                OrderDate = order.OrderDate.CompareTo(newOrderDate) > 0 ? order.OrderDate : newOrderDate,
                CurrencyId = order.CurrencyId,
                ApproveStatus = OrderStatus.Approved, 
                ApprovedId = authorId,
                AuthorId = authorId,
                RegDate = newOrderDate,
                Reason = $"Обратная проводка для {order.Reason}",
                ReasonKaz = $"{order.ReasonKaz} үшін кері өткізу",
                BranchId = order.BranchId,
                OwnerId = order.OwnerId,
                UserId = order.UserId,
                ContractActionId = order.ContractActionId,
                ExpenseTypeId = order.ExpenseTypeId,
                OperationId = order.OperationId,
                OrderType = order.OrderType switch
                {
                    OrderType.CashIn => OrderType.CashOut,
                    OrderType.CashOut => OrderType.CashIn,
                    OrderType.Memorial => OrderType.Memorial,
                    OrderType.OffBalanceIn => OrderType.OffBalanceOut,
                    OrderType.OffBalanceOut => OrderType.OffBalanceIn,
                    OrderType.Payment => OrderType.Payment,
                    _ => throw new NotImplementedException()
                },
                ContractId = order.ContractId,
                BusinessOperationId = order.BusinessOperationId,
                BusinessOperationSettingId = order.BusinessOperationSettingId,
                StornoId = order.Id
            };

            using (IDbTransaction transaction = BeginCashOrderTransaction())
            {
                newOrder.OrderNumber = _orderNumberCounterRepository.Next(order.OrderType, order.OrderDate.Year, order.BranchId,
                CodeForOrderByOrderType(branch, newOrder.OrderType));
                Save(newOrder);
                if (newOrder.ApproveStatus == OrderStatus.Approved)
                {
                    List<AccountRecord> records = new List<AccountRecord>();

                    records.AddRange(_accountRecordService.Build(newOrder));

                    records.ForEach(record =>
                    {
                        record.OrderId = newOrder.Id;
                        _accountRecordService.Register(record);
                    });
                    if (order.BusinessOperationSettingId.HasValue)
                    {
                        _ukassaService.CreateCheckRequest(newOrder);
                    }
                    if (printLanguage != null)
                    {
                        await _cashOrderRepository.SetCashOrderPrintLanguage(newOrder.Id, printLanguage.Id, _sessionContext.UserId);
                    }
                }
                transaction.Commit();

                return;
            }
        }

        public async Task<IEnumerable<CashOrder>> GetMultipleByIds(IEnumerable<int> cashOrderIds)
        {
            return await _cashOrderRepository.GetMultipleByIds(cashOrderIds);
        }

        public CashOrder Cancel(CashOrder order, int authorId, Group branch)
        {
            if (order == null)
                throw new PawnshopApplicationException("Ордер для сторнирования не может быть пустым");

            if (order.ApproveStatus != OrderStatus.Approved)
                throw new PawnshopApplicationException($"Ордер {order.Id} не подтвержден, его невозможно сторнировать");

            if (order.DeleteDate.HasValue)
                throw new PawnshopApplicationException($"Ордер {order.Id} удален, его невозможно сторнировать");

            CashOrder stornoOrder;

            stornoOrder = _cashOrderRepository.GetOrderByStornoId(order.Id);

            if (stornoOrder != null)
                return null; //throw new PawnshopApplicationException($"Ордер {order.Id} уже сторнирован");

            var bo = _businessOperationRepository.Get(order.BusinessOperationId.Value);
            var printLanguage = _cashOrderRepository.GetCashOrderPrintLanguageByOrderId(order.Id);
            DateTime newOrderDate = DateTime.Now;
            var newOrder = new CashOrder()
            {
                CreditAccountId = order.DebitAccountId,
                CreditAccount = order.DebitAccount,
                DebitAccountId = order.CreditAccountId,
                DebitAccount = order.CreditAccount,
                OrderCost = order.OrderCost,
                OrderCostNC = order.OrderCostNC,
                ClientId = order.ClientId,
                OrderDate = order.OrderDate.CompareTo(newOrderDate) > 0 ? order.OrderDate : newOrderDate,
                CurrencyId = order.CurrencyId,
                ApproveStatus = OrderStatus.Approved, //bo.OrdersCreateStatus.HasValue ? (OrderStatus)bo.OrdersCreateStatus.Value : 0,
                ApprovedId = authorId,
                AuthorId = authorId,
                RegDate = newOrderDate,
                Reason = $"Обратная проводка для {order.Reason}",
                ReasonKaz = $"{order.ReasonKaz} үшін кері өткізу",
                BranchId = order.BranchId,
                OwnerId = order.OwnerId,
                UserId = order.UserId,
                ContractActionId = order.ContractActionId,
                ExpenseTypeId = order.ExpenseTypeId,
                OperationId = order.OperationId,
                OrderType = order.OrderType switch
                {
                    OrderType.CashIn => OrderType.CashOut,
                    OrderType.CashOut => OrderType.CashIn,
                    OrderType.Memorial => OrderType.Memorial,
                    OrderType.OffBalanceIn => OrderType.OffBalanceOut,
                    OrderType.OffBalanceOut => OrderType.OffBalanceIn,
                    OrderType.Payment => OrderType.Payment,
                    _ => throw new NotImplementedException()
                },
                ContractId = order.ContractId,
                BusinessOperationId = order.BusinessOperationId,
                BusinessOperationSettingId = order.BusinessOperationSettingId,
                StornoId = order.Id
            };


            using (IDbTransaction transaction = BeginCashOrderTransaction())
            {
                newOrder.OrderNumber = _orderNumberCounterRepository.Next(order.OrderType, order.OrderDate.Year, order.BranchId,
                CodeForOrderByOrderType(branch, newOrder.OrderType));
                Save(newOrder);
                if (newOrder.ApproveStatus == OrderStatus.Approved)
                {
                    List<AccountRecord> records = new List<AccountRecord>();

                    records.AddRange(_accountRecordService.Build(newOrder));

                    records.ForEach(record =>
                    {
                        record.OrderId = newOrder.Id;
                        _accountRecordService.Register(record);
                    });
                    if (order.BusinessOperationSettingId.HasValue)
                    {
                        _ukassaService.CreateCheckRequest(newOrder);
                    }
                    if(printLanguage != null)
                    {
                        _cashOrderRepository.SetCashOrderPrintLanguage(newOrder.Id, printLanguage.Id, _sessionContext.UserId);
                    }
                }
                transaction.Commit();

                return newOrder;
            }
        }

        public int RelationCount(int id) => _cashOrderRepository.RelationCount(id);

        public CashOrder Find(CashOrderFilter filter) => _cashOrderRepository.Find(filter);

        public List<CashOrder> GetCashOrdersForApprove(List<CashOrder> cashOrders)
        {
            var cashOrder = cashOrders.FirstOrDefault();
            var contractActionId = cashOrder != null ? cashOrder.ContractActionId : 0;
            return _cashOrderRepository.GetOrdersByContractActionId(contractActionId ?? 0);
        }

        public decimal GetSumOfCashOrderCostByBusinessOperationSettingCodesAndContractId(List<string> codes, int contractId, DateTime date) =>
            _cashOrderRepository.GetSumOfCashOrderCostByBusinessOperationSettingCodesAndContractId(codes, contractId, date);


        public async Task ChangeStatusForRelatedOrders(CashOrder cashOrder, OrderStatus status, int userId, Group branch, bool forSupport)
        {
            var list = await _contractActionService.GetRelatedContractActionsByOrder(cashOrder);
            await ChangeStatusForOrders(list, status, userId, branch, forSupport);
        }

        public async Task ChangeStatusForRelatedOrders(int contractActionId, OrderStatus status, int userId, Group branch, bool forSupport)
        {
            var list = await _contractActionService.GetRelatedContractActionsByActionId(contractActionId);
            await ChangeStatusForOrders(list, status, userId, branch, forSupport);
        }

        public async Task ChangeStatusForOrders(List<int> relatedContractActions, OrderStatus status, int userId, Group branch, bool forSupport)
        {
            using (IDbTransaction transaction = BeginCashOrderTransaction())
            {
                foreach (var contractActionId in relatedContractActions.OrderBy(x => x))
                {
                    var relatedOrders = _cashOrderRepository.GetAllOrdersByContractActionId(contractActionId);
                    foreach (var order in relatedOrders.OrderBy(x => x.Id))
                    {
                        if (order.OrderDate.Date != DateTime.Today.Date && !forSupport)
                            throw new PawnshopApplicationException("Данный ордер не может быть подтвержден. Обратитесь к администратору.");

                        if (order.OperationId.HasValue)
                            throw new PawnshopApplicationException("Нельзя изменять статус кассового ордера, привязанного к платежной операции!");

                        if (order.DeleteDate.HasValue)
                            throw new PawnshopApplicationException($"Зависимый кассовый ордер {order.Id} является удаленным, нельзя его подтвердить");

                        if (order.ApproveStatus == OrderStatus.Prohibited)
                            throw new PawnshopApplicationException($"Зависимый Кассовый ордер {order.Id} является отклоненным, нельзя его подтвердить");
                        
                        var prevStatus = order.ApproveStatus;
                        order.ApprovedId = userId;
                        order.ApproveStatus = status;
                        order.ApproveDate = DateTime.Now;
                        Save(order);
                        if (prevStatus == OrderStatus.WaitingForApprove)
                        {
                            Register(order, branch);
                        }
                    }
                }
                transaction.Commit();
            }
        }

        public async Task<(bool, List<int>)> CheckOrdersForConfirmation(int contractActionId)
        {
            var relatedContractActions = await _contractActionService.GetRelatedContractActionsByActionId(contractActionId);
            var exists = false;
            foreach (var id in relatedContractActions.OrderBy(x => x))
            {
                var relatedOrders = _cashOrderRepository.GetAllOrdersByContractActionId(id);
                foreach (var order in relatedOrders)
                {
                    if (order.ApproveStatus == OrderStatus.WaitingForConfirmation)
                        exists = true;
                }
            }
            return (exists, relatedContractActions);
        }

        public decimal GetAccountSettingDebitTurnsByActionIds(List<int> actionIds, string accountSettingCode)
        {
            return _cashOrderRepository.GetAccountSettingDebitTurnsByActionIds(actionIds, accountSettingCode);
        }
        public decimal GetAccountSettingCreditTurnsByActionIds(List<int> actionIds, string accountSettingCode)
        {
            return _cashOrderRepository.GetAccountSettingCreditTurnsByActionIds(actionIds, accountSettingCode);
        }

        public async Task<(ContractExpenseRowOrder, List<CashOrder>)> GetRelatedContractExpenseOrders(int orderId)
        {
            var list = new List<CashOrder>();
            var relatedContractExpenseRowOrders = await GetRelatedContractExpenseRowOrderList(orderId);
            foreach (var contractExpenseRowOrder in relatedContractExpenseRowOrders.Item2)
            {
                if (contractExpenseRowOrder.OrderId == orderId)
                    continue;
                if (!list.Contains(contractExpenseRowOrder.Order))
                    list.Add(contractExpenseRowOrder.Order);
            }
            return (relatedContractExpenseRowOrders.Item1, list);
        }

        public async Task<(ContractExpenseRowOrder, List<ContractExpenseRowOrder>)> GetRelatedContractExpenseRowOrderList(int orderId)
        {
            var relatedContractExpenseRowOrders = new List<ContractExpenseRowOrder>();
            var relatedContractExpenseRowOrder = _contractExpenseRowOrderRepository.GetByOrderId(orderId);
            if (relatedContractExpenseRowOrder != null)
            {
                relatedContractExpenseRowOrders = _contractExpenseRowOrderRepository.GetByContractExpenseRowId(relatedContractExpenseRowOrder.ContractExpenseRowId);
                if (relatedContractExpenseRowOrders == null)
                    throw new PawnshopApplicationException($"Ожидалось что {nameof(_contractExpenseRowOrderRepository)}.{nameof(_contractExpenseRowOrderRepository.GetByContractExpenseRowId)} не вернет null");
            }
            return (relatedContractExpenseRowOrder, relatedContractExpenseRowOrders);
        }

        public async Task<bool> CashOrdersExists(int contractActionId)
        {
            var ordersList = _cashOrderRepository.GetAllCashOrdersByContractActionId(contractActionId);
            return ordersList.Count > 0;
        }

        public async Task<List<int>> GetAllRelatedOrdersByContractActionId(int contractActionId)
        {
            var orderIds = new List<int>();
            var list = await _contractActionService.GetRelatedContractActionsByActionId(contractActionId);
            foreach (var actionId in list)
                orderIds.AddRange(_cashOrderRepository.GetAllOrdersByContractActionId(actionId).Select(x => x.Id).ToList());

            return orderIds.Distinct().ToList();
        }

        public async Task ChangeLanguageForRelatedCashOrder(CashOrder cashOrder, int languageId)
        {
            var list = await _contractActionService.GetRelatedContractActionsByOrder(cashOrder);
            await ChangeLanguageForOrders(list, languageId); 
        }

        public async Task ChangeLanguageForOrders(List<int> relatedContractActions, int languageId)
        {
            using (IDbTransaction transaction = BeginCashOrderTransaction())
            {
                foreach (var contractActionId in relatedContractActions.OrderBy(x => x))
                {
                    var relatedOrderIds = await _cashOrderRepository.GetOrderIdsForCashOrderPrintForms(contractActionId);
                    foreach (var orderId in relatedOrderIds)
                    {
                        await _cashOrderRepository.SetCashOrderPrintLanguage(orderId, languageId, _sessionContext.UserId);
                    }
                }
                transaction.Commit();
            }
        }

        public async Task DeleteCashOrderPrintLanguageForOrder(CashOrder cashOrder)
        {
            var list = await _contractActionService.GetRelatedContractActionsByOrder(cashOrder);
            await DeleteCashOrderPrintLanguageForContractActions(list);
        }

        public async Task DeleteCashOrderPrintLanguageForContractActions(List<int> relatedContractActions)
        {
            using (IDbTransaction transaction = BeginCashOrderTransaction())
            {
                foreach (var contractActionId in relatedContractActions.OrderBy(x => x))
                {
                    var relatedOrderIds = await _cashOrderRepository.GetOrderIdsForCashOrderPrintForms(contractActionId);
                    foreach (var orderId in relatedOrderIds)
                    {
                        await _cashOrderRepository.DeleteCashOrderPrintLanguageByOrderId(orderId);
                    }
                }
                transaction.Commit();
            }
        }

        /// <summary>
        /// Insert PrintLanguage for specific CashOrder -- DOES NOT place languageId for related cashOrders - only for cashOrders with check and printing
        /// </summary>
        /// <param name="cashOrder"></param>
        /// <param name="languageId"></param>
        /// <returns></returns>
        public async Task SetLanguageIfNecessary(CashOrder cashOrder, int? languageId)
        {
            if (languageId != null)
            {
                var language = _languagesRepository.Get(languageId.Value);
                if (language == null)
                    throw new PawnshopApplicationException($"Язык с идентификатором {languageId.Value} не найден");

                if (cashOrder.OrderType == OrderType.CashIn || cashOrder.OrderType == OrderType.CashOut || cashOrder.OrderType == OrderType.Payment)
                {
                    await _cashOrderRepository.SetCashOrderPrintLanguage(cashOrder.Id, languageId.Value, _sessionContext.UserId);
                }
            }
        }

        public async Task<decimal> GetContractTotalOperationAmount(int contractId, List<string> boOperationSettings, DateTime startDate, DateTime endDate)
        {
            return await _cashOrderRepository.GetContractTotalOperationAmount(contractId, boOperationSettings, startDate, endDate);
        }

        public async Task<DateTime> GetContractLastOperationDate(int contractId, List<string> boOperationSettings, DateTime tillDate)
        {
            return await _cashOrderRepository.GetContractLastOperationDate(contractId, boOperationSettings, tillDate);
        }
        
        
        private string CreateNote(CashOrder order, string? note)
        {
            if (!_auctionBusinessOperationSettingCodes.Contains(order.BusinessOperationSetting.Code))
            {
                return note;
            }

            return order.BusinessOperationSetting.Code switch
            {
                // расход
                Constants.AUCTION_BOS_EXPENSE_PAYMENT_CASH => Constants.AUCTION_EXPENSE_PAYMENT_CASH_NOTE,
                _ => note
            };
        }

        /// <summary>
        /// Получение идентификатора контр-агента
        /// </summary>
        /// <param name="order"></param>
        /// <param name="clientId">Клиент контр-агент</param>
        // todo пока оставляю
        private int? SetUserId(CashOrder order, int? clientId)
        {
            if (!_auctionBusinessOperationSettingCodes.Contains(order.BusinessOperationSetting.Code))
            {
                return clientId;
            }

            // в качестве контр-агента высупает не клиент а сотрудник 
            return order.BusinessOperationSetting.Code switch
            {
                Constants.AUCTION_BOS_CASH_IN_SELLING => order.UserId,
                _ => clientId
            };
        }
    }
}