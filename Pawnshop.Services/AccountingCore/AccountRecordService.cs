using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access.AccountingCore;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.AccountingCore
{
    public class AccountRecordService : IAccountRecordService
    {
        private readonly AccountRecordRepository _repository;
        private readonly AccountPlanRepository _accountPlanRepository;
        private readonly IAccountService _accountService;
        public AccountRecordService(AccountRecordRepository repository, AccountPlanRepository accountPlanRepository, IAccountService accountService)
        {
            _repository = repository;
            _accountPlanRepository = accountPlanRepository;
            _accountService = accountService;
        }

        public AccountRecord Save(AccountRecord model)
        {
            var m = new Data.Models.AccountingCore.AccountRecord(model);
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

        public async Task<AccountRecord> GetAsync(int id)
        {
            return await _repository.GetAsync(id);
        }

        public ListModel<AccountRecord> List(ListQuery listQuery)
        {
            return new ListModel<AccountRecord>
            {
                List = _repository.List(listQuery).AsEnumerable<AccountRecord>().ToList(),
                Count = _repository.Count(listQuery)
            };
        }

        public ListModel<AccountRecord> List(ListQueryModel<AccountRecordFilter> listQuery)
        {
            return new ListModel<AccountRecord>
            {
                List = _repository.List(listQuery, listQuery.Model).AsEnumerable<AccountRecord>().ToList(),
                Count = _repository.Count(listQuery, listQuery.Model)
            };
        }

        public List<AccountRecord> Build(CashOrder order, bool isMigration = false)
        {
            var records = new List<AccountRecord>();

            if (!order.BusinessOperationSettingId.HasValue) throw new ArgumentNullException(nameof(order.BusinessOperationSettingId), "Не задана настройка бизнес-операции!");

            DateTime orderDate = order.OrderDate;
            DateTime orderDateFlooredToSeconds = new DateTime(orderDate.Year, orderDate.Month, orderDate.Day, orderDate.Hour, orderDate.Minute, orderDate.Second);
            if (order.CreditAccountId.HasValue)
            {
                if (_repository.Find(new AccountRecordFilter { AccountId = order.CreditAccountId, OrderId = order.Id }) == null)
                {
                    var newRecord = new AccountRecord(order.CreditAccount, order.OrderCost, order.OrderCostNC, order.AuthorId, order.BusinessOperationSettingId.Value,
                        order.Id, false, orderDateFlooredToSeconds, order.DebitAccount, reason: order.Reason);
                    records.Add(newRecord);
                }
            }

            if (order.DebitAccountId.HasValue)
            {
                if (_repository.Find(new AccountRecordFilter { AccountId = order.DebitAccountId, OrderId = order.Id }) == null)
                {
                    var newRecord = new AccountRecord(order.DebitAccount, order.OrderCost, order.OrderCostNC, order.AuthorId, order.BusinessOperationSettingId.Value,
                        order.Id, true, orderDateFlooredToSeconds, order.CreditAccount, reason: order.Reason);
                    records.Add(newRecord);
                }
            }

            return records;
        }

        public AccountRecord Register(AccountRecord record, bool isMigration = false)
        {
            using (var transaction = _repository.BeginTransaction())
            {
                if (isMigration) Save(record);
                else RecalculateBalanceWithNewRecord(_accountService.GetAsync(record.AccountId).Result, record, isMigration);

                transaction.Commit();
                return record;
            }
        }

        public void RecalculateBalanceWithNewRecord(IAccount account, AccountRecord newRecord, bool isMigration = false)
        {

            var activeRecords = List(new ListQueryModel<AccountRecordFilter>
            {
                Page = null,
                Model = new AccountRecordFilter { AccountId = account.Id, BeginDate = newRecord.Date }
            }).List;

            Save(newRecord);

            var accountPlan = _accountPlanRepository.Get(account.AccountPlanId);

            if (accountPlan == null) throw new PawnshopApplicationException($"План для счета {account.AccountNumber}(Id={account.Id}) {account.Name} не найден!");

            activeRecords = activeRecords.Where(x => x.Date > newRecord.Date).ToList();
            activeRecords.Add(newRecord);
            List<AccountRecord> neededAccountRecords = activeRecords.OrderBy(x => x.Date).ThenBy(x => x.Id).ToList();
            AccountRecord previous = null;
            if (neededAccountRecords.Count > 0)
            {
                AccountRecord firstAccountRecord = neededAccountRecords.First();
                previous = _repository.GetLastRecordByAccountIdAndEndDate(firstAccountRecord.AccountId, firstAccountRecord.Id, firstAccountRecord.Date);
            }

            for (int i = 0; i < neededAccountRecords.Count; i++)
            {
                AccountRecord record = neededAccountRecords[i];
                if (previous == null)
                {
                    record.IncomingBalance = 0;
                    record.IncomingBalanceNC = 0;
                }
                else
                {
                    record.IncomingBalance = previous.OutgoingBalance;
                    record.IncomingBalanceNC = previous.OutgoingBalanceNC;
                }

                if (record.IsDebit)
                {
                    record.OutgoingBalance = record.IncomingBalance - record.Amount;
                    record.OutgoingBalanceNC = record.IncomingBalanceNC - record.AmountNC;

                }
                else
                {
                    record.OutgoingBalance = record.IncomingBalance + record.Amount;
                    record.OutgoingBalanceNC = record.IncomingBalanceNC + record.AmountNC;
                }

                if (i == neededAccountRecords.Count - 1)
                    record.CheckBalance(accountPlan, account.RedBalanceAllowed, isMigration);

                Save(record);
                previous = record;
            }

            account.Balance = previous?.OutgoingBalance ?? 0;
            account.BalanceNC = previous?.OutgoingBalanceNC ?? 0;
            account.LastMoveDate = previous.Date;
            _accountService.Save(account as Account);
        }

        public void RecalculateBalanceOnAccount(int accountId, bool isMigration = false, DateTime? beginDate = null)
        {
            IAccount account = _accountService.GetAsync(accountId).Result;

            var activeRecords = List(new ListQueryModel<AccountRecordFilter>
            {
                Page = null,
                Model = new AccountRecordFilter
                {
                    AccountId = account.Id,
                    BeginDate = beginDate
                }
            });

            var accountPlan = _accountPlanRepository.Get(account.AccountPlanId);

            if (accountPlan == null) throw new PawnshopApplicationException($"План для счета {account.AccountNumber}(Id={account.Id}) {account.Name} не найден!");

            List<AccountRecord> neededAccountRecords = activeRecords.List.OrderBy(x => x.Date).ThenBy(x => x.Id).ToList();
            AccountRecord previous = null;
            if (neededAccountRecords.Count > 0)
            {
                AccountRecord firstAccountRecord = neededAccountRecords.First();
                previous = _repository.GetLastRecordByAccountIdAndEndDate(firstAccountRecord.AccountId, firstAccountRecord.Id, firstAccountRecord.Date);
            }

            for (int i = 0; i < neededAccountRecords.Count; i++)
            {
                AccountRecord record = neededAccountRecords[i];
                if (previous == null)
                {
                    record.IncomingBalance = 0;
                    record.IncomingBalanceNC = 0;
                }
                else
                {
                    record.IncomingBalance = previous.OutgoingBalance;
                    record.IncomingBalanceNC = previous.OutgoingBalanceNC;
                }

                if (record.IsDebit)
                {
                    record.OutgoingBalance = record.IncomingBalance - record.Amount;
                    record.OutgoingBalanceNC = record.IncomingBalanceNC - record.AmountNC;
                }
                else
                {
                    record.OutgoingBalance = record.IncomingBalance + record.Amount;
                    record.OutgoingBalanceNC = record.IncomingBalanceNC + record.AmountNC;
                }

                if (i == neededAccountRecords.Count - 1)
                    record.CheckBalance(accountPlan, account.RedBalanceAllowed, isMigration);
                
                Save(record);
                previous = record;
            }

            account.Balance = previous?.OutgoingBalance ?? 0;
            account.BalanceNC = previous?.OutgoingBalanceNC ?? 0;
            account.LastMoveDate = previous?.Date ?? (activeRecords.List.Any() ? activeRecords.List.Max(x => x.Date) : DateTime.Now);
            _accountService.Save(account as Account);
        }
    }
}
