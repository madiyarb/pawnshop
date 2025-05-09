using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.AccountingCore;

namespace Pawnshop.Data.Access.AccountingCore
{
    public class AccountRecordRepository : RepositoryBase, IRepository<AccountRecord>
    {
        public AccountRecordRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(AccountRecord entity)
        {
            using var transaction = BeginTransaction();

            entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO AccountRecords ( AccountId, CorrAccountId, BusinessOperationSettingId, Date, Amount, IsDebit, IncomingBalance, IncomingBalanceNC, OutgoingBalance, OutgoingBalanceNC, Reason, CreateDate, AuthorId, AmountNC, OrderId)
VALUES ( @AccountId, @CorrAccountId, @BusinessOperationSettingId, @Date, @Amount, @IsDebit, @IncomingBalance, @IncomingBalanceNC, @OutgoingBalance, @OutgoingBalanceNC, @Reason, @CreateDate, @AuthorId, @AmountNC, @OrderId )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public void Update(AccountRecord entity)
        {
            using var transaction = BeginTransaction();

            UnitOfWork.Session.Execute(@"
UPDATE AccountRecords
SET AccountId = @AccountId, CorrAccountId = @CorrAccountId, BusinessOperationSettingId = @BusinessOperationSettingId, Date = @Date, Amount = @Amount,
IsDebit = @IsDebit, IncomingBalance = @IncomingBalance, IncomingBalanceNC = @IncomingBalanceNC, OutgoingBalance = @OutgoingBalance,
OutgoingBalanceNC = @OutgoingBalanceNC, Reason = @Reason, AmountNC = @AmountNC, OrderId = @OrderId
WHERE Id = @Id", entity, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public void Delete(int id)
        {
            using var transaction = BeginTransaction();

            UnitOfWork.Session.Execute(@"UPDATE AccountRecords SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new {id}, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public AccountRecord Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<AccountRecord>(@"
SELECT *
FROM AccountRecord
WHERE Id = @id", new { id });
        }

        public async Task<AccountRecord> GetAsync(int id)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<AccountRecord>(@"
SELECT *
FROM AccountRecord
WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public AccountRecord GetLastRecordByAccountIdAndEndDate(int accountId, int accountRecordId, DateTime endDate) 
        {
            return UnitOfWork.Session.Query<AccountRecord>(@"
                SELECT TOP 1 *
                FROM AccountRecords
                WHERE AccountId = @accountId AND Date <= @endDate
                AND Id < @accountRecordId
                AND DeleteDate IS NULL ORDER BY Date DESC, Id DESC", 
                new { accountId, endDate, accountRecordId }, UnitOfWork.Transaction)
                .FirstOrDefault();
        }

        public AccountRecord Find(object query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            var accountId = query?.Val<int?>("AccountId");
            var corrAccountId = query?.Val<int?>("CorrAccountId");
            var operationSettingId = query?.Val<int?>("OperationSettingId");
            var orderId = query?.Val<int?>("OrderId");

            var pre = "ar.DeleteDate IS NULL";
            pre += accountId.HasValue ? " AND ar.AccountId = @accountId" : string.Empty;
            pre += corrAccountId.HasValue ? " AND ar.CorrAccountId = @corrAccountId" : string.Empty;
            pre += operationSettingId.HasValue ? " AND ar.OperationSettingId = @operationSettingId" : string.Empty;
            pre += orderId.HasValue ? " AND ar.OrderId = @orderId" : string.Empty;

            return UnitOfWork.Session.QuerySingleOrDefault<AccountRecord>(@$"
SELECT TOP 1 *
FROM AccountRecords ar
WHERE {pre} ", new
            {
                accountId,
                corrAccountId,
                operationSettingId,
                orderId
            }, UnitOfWork.Transaction);
        }

        public List<AccountRecord> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var accountId = query?.Val<int?>("AccountId");
            var corrAccountId = query?.Val<int?>("CorrAccountId");
            var businessOperationSettingId = query?.Val<int?>("BusinessOperationSettingId");
            var businessOperationId = query?.Val<int?>("BusinessOperationId");
            var amountMax = query?.Val<int?>("AmountMax");
            var amountMin = query?.Val<int?>("AmountMin");
            var incomingBalanceMax = query?.Val<int?>("IncomingBalanceMax");
            var incomingBalanceMin = query?.Val<int?>("IncomingBalanceMin");
            var outgoingBalanceMax = query?.Val<int?>("OutgoingBalanceMax");
            var outgoingBalanceMin = query?.Val<int?>("OutgoingBalanceMin");
            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var onlyFirst = query?.Val<bool?>("OnlyFirst");
            int? orderId = query?.Val<int?>("OrderId");
            var pre = "ar.DeleteDate IS NULL";
            var from = "FROM AccountRecords ar";

            pre += accountId.HasValue ? " AND ar.AccountId = @accountId" : string.Empty;
            pre += corrAccountId.HasValue ? " AND ar.CorrAccountId = @corrAccountId" : string.Empty;
            pre += businessOperationSettingId.HasValue ? " AND ar.BusinessOperationSettingId = @businessOperationSettingId" : string.Empty;
            pre += amountMax.HasValue ? " AND ar.Amount <= @amountMax" : string.Empty;
            pre += amountMin.HasValue ? " AND ar.Amount >= @amountMin" : string.Empty;
            pre += incomingBalanceMax.HasValue ? " AND ar.IncomingBalance <= @incomingBalanceMax" : string.Empty;
            pre += incomingBalanceMin.HasValue ? " AND ar.IncomingBalance >= @incomingBalanceMin" : string.Empty;
            pre += outgoingBalanceMax.HasValue ? " AND ar.OutgoingBalance<= @outgoingBalanceMax" : string.Empty;
            pre += beginDate.HasValue ? " AND ar.Date >= @beginDate" : string.Empty;
            pre += endDate.HasValue ? " AND ar.Date <= @endDate" : string.Empty;
            pre += orderId.HasValue ? " AND ar.OrderId = @orderId" : string.Empty;

            if (businessOperationId.HasValue)
            {
                from += " JOIN BusinessOperationSettings bos ON ar.BusinessOperationSettingId = bos.Id ANd bos.BusinessOperationId = @businessOperationId";
            }
            var condition = listQuery.Like(pre);
            var order = listQuery.Order(string.Empty, listQuery?.Sort ?? new Sort
            {
                Name = "ar.Date DESC, ar.Id",
                Direction = SortDirection.Desc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<AccountRecord>($@"
SELECT {(onlyFirst.HasValue ? "TOP 1" : string.Empty)} *
{from}
{condition} {order} {page}", new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter,
                accountId,
                corrAccountId,
                businessOperationSettingId,
                amountMax,
                amountMin,
                incomingBalanceMax,
                incomingBalanceMin,
                outgoingBalanceMax,
                outgoingBalanceMin,
                businessOperationId,
                beginDate,
                endDate,
                orderId
            }, UnitOfWork.Transaction).AsList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var accountId = query?.Val<int?>("AccountId");
            var corrAccountId = query?.Val<int?>("CorrAccountId");
            var businessOperationSettingId = query?.Val<int?>("BusinessOperationSettingId");
            var businessOperationId = query?.Val<int?>("BusinessOperationId");
            var amountMax = query?.Val<int?>("AmountMax");
            var amountMin = query?.Val<int?>("AmountMin");
            var incomingBalanceMax = query?.Val<int?>("IncomingBalanceMax");
            var incomingBalanceMin = query?.Val<int?>("IncomingBalanceMin");
            var outgoingBalanceMax = query?.Val<int?>("OutgoingBalanceMax");
            var outgoingBalanceMin = query?.Val<int?>("OutgoingBalanceMin");
            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            int? orderId = query?.Val<int?>("OrderId");
            var pre = "ar.DeleteDate IS NULL";
            var from = "FROM AccountRecords ar";

            pre += accountId.HasValue ? " AND ar.AccountId = @accountId" : string.Empty;
            pre += corrAccountId.HasValue ? " AND ar.CorrAccountId = @corrAccountId" : string.Empty;
            pre += businessOperationSettingId.HasValue ? " AND ar.BusinessOperationSettingId = @businessOperationSettingId" : string.Empty;
            pre += amountMax.HasValue ? " AND ar.Amount <= @amountMax" : string.Empty;
            pre += amountMin.HasValue ? " AND ar.Amount >= @amountMin" : string.Empty;
            pre += incomingBalanceMax.HasValue ? " AND ar.IncomingBalance <= @incomingBalanceMax" : string.Empty;
            pre += incomingBalanceMin.HasValue ? " AND ar.IncomingBalance >= @incomingBalanceMin" : string.Empty;
            pre += outgoingBalanceMax.HasValue ? " AND ar.OutgoingBalance<= @outgoingBalanceMax" : string.Empty;
            pre += outgoingBalanceMin.HasValue ? " AND ar.OutgoingBalance >= @outgoingBalanceMin" : string.Empty;
            pre += beginDate.HasValue ? " AND ar.Date >= @beginDate" : string.Empty;
            pre += endDate.HasValue ? " AND ar.Date <= @endDate" : string.Empty;
            pre += orderId.HasValue ? " AND ar.OrderId = @orderId" : string.Empty;

            if (businessOperationId.HasValue)
            {
                from += " JOIN BusinessOperationSettings bos ON ar.BusinessOperationSettingId = bos.Id ANd bos.BusinessOperationId = @businessOperationId";
            }

            var condition = listQuery.Like(pre);

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(ar.Id)
{from}
{condition}", new
            {
                listQuery.Filter,
                accountId,
                corrAccountId,
                businessOperationSettingId,
                amountMax,
                amountMin,
                incomingBalanceMax,
                incomingBalanceMin,
                outgoingBalanceMax,
                outgoingBalanceMin,
                businessOperationId,
                beginDate,
                endDate,
                orderId
            }, UnitOfWork.Transaction);
        }

        public int RelationCount(int id)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
SELECT SUM(AccountRecordCounts.AccountRecordCount)
FROM (
    SELECT COUNT(*) as AccountRecordCount FROM CashOrders WHERE AccountRecordId = @id
) AccountRecordCounts", new { id }, UnitOfWork.Transaction);
        }
    }
}