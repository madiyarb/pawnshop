using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using BusinessOperationSetting = Pawnshop.Data.Models.AccountingCore.BusinessOperationSetting;

namespace Pawnshop.Data.Access.AccountingCore
{
    public class BusinessOperationSettingRepository : RepositoryBase, IRepository<BusinessOperationSetting>
    {
        public BusinessOperationSettingRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(BusinessOperationSetting entity)
        {
            using var transaction = BeginTransaction();

            entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO BusinessOperationSettings ( BusinessOperationId, OrderBy, Code, Name, DebitSettingId, CreditSettingId, AmountType, Reason, IsActive, CreateDate, AuthorId, DeleteDate, NameAlt, PayTypeId, OrderType, DefaultArticleTypeId )
VALUES ( @BusinessOperationId, @OrderBy, @Code, @Name, @DebitSettingId, @CreditSettingId, @AmountType, @Reason, @IsActive, @CreateDate, @AuthorId, @DeleteDate, @NameAlt, @PayTypeId, @OrderType, @DefaultArticleTypeId  )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public void Update(BusinessOperationSetting entity)
        {
            using var transaction = BeginTransaction();

            UnitOfWork.Session.Execute(@"
UPDATE BusinessOperationSettings
SET BusinessOperationId = @BusinessOperationId,
OrderBy = @OrderBy,
Code = @Code,
Name = @Name,
DebitSettingId = @DebitSettingId,
CreditSettingId = @CreditSettingId,
AmountType = @AmountType,
Reason = @Reason,
IsActive = @IsActive,
DeleteDate = @DeleteDate,
NameAlt = @NameAlt,
PayTypeId = @PayTypeId,
DefaultArticleTypeId = @DefaultArticleTypeId,
OrderType = @OrderType
WHERE Id = @Id", entity, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public void Delete(int id)
        {
            using var transaction = BeginTransaction();

            UnitOfWork.Session.Execute(@"UPDATE BusinessOperationSettings SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new {id}, UnitOfWork.Transaction);

            transaction.Commit();
        }

        public BusinessOperationSetting Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<BusinessOperationSetting>(@"
SELECT *
FROM BusinessOperationSettings
WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public async Task<BusinessOperationSetting> GetAsync(int id)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<BusinessOperationSetting>(@"
SELECT *
FROM BusinessOperationSettings
WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }
        
        public BusinessOperationSetting Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<BusinessOperationSetting> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var businessOperationId = query?.Val<int?>("BusinessOperationId");
            var code = query?.Val<string>("Code");
            var isActive = query?.Val<bool?>("IsActive");
            var amountType = query?.Val<AmountType?>("AmountType");
            var payTypeId = query?.Val<int?>("PayTypeId");
            var codes = query?.Val<List<string>>("Codes");

            var pre = "bos.DeleteDate IS NULL";
            pre += businessOperationId.HasValue ? " AND bos.BusinessOperationId = @businessOperationId" : string.Empty;
            pre += string.IsNullOrWhiteSpace(code) ? string.Empty : " AND bos.Code = @code";
            pre += isActive.HasValue ? " AND bos.IsActive = @isActive" : string.Empty;
            pre += amountType.HasValue ? " AND bos.AmountType = @amountType" : string.Empty;
            pre += payTypeId.HasValue ? " AND (bos.PayTypeId IS NULL OR bos.PayTypeId = @payTypeId)" : string.Empty;
            pre += codes != null && codes.Count > 0 ? " AND bos.Code IN @codes" : string.Empty;

            var condition = listQuery.Like(pre, "Code", "Name", "NameAlt");

            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "OrderBy",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<BusinessOperationSetting>($@"
SELECT *
FROM BusinessOperationSettings bos WITH (NOLOCK)
{condition} {order} {page}", new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter,
                businessOperationId,
                code,
                isActive,
                amountType,
                payTypeId, 
                codes
            }, UnitOfWork.Transaction).AsList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var businessOperationId = query?.Val<int?>("BusinessOperationId");
            var code = query?.Val<string>("Code");
            var isActive = query?.Val<bool?>("IsActive");
            var amountType = query?.Val<AmountType?>("AmountType");
            var payTypeId = query?.Val<int?>("PayTypeId");
            var codes = query?.Val<List<string>>("Codes");

            var pre = "bos.DeleteDate IS NULL";
            pre += businessOperationId.HasValue ? " AND bos.BusinessOperationId = @businessOperationId" : string.Empty;
            pre += string.IsNullOrWhiteSpace(code) ? string.Empty : " AND bos.Code = @code";
            pre += isActive.HasValue ? " AND bos.IsActive = @isActive" : string.Empty;
            pre += amountType.HasValue ? " AND bos.AmountType = @amountType" : string.Empty;
            pre += payTypeId.HasValue ? " AND (bos.PayTypeId IS NULL OR bos.PayTypeId = @payTypeId)" : string.Empty;
            pre += codes != null && codes.Count > 0 ? " AND bos.Code IN @codes" : string.Empty;

            var condition = listQuery.Like(pre, "Code", "Name", "NameAlt");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM BusinessOperationSettings bos
{condition}", new
            {
                listQuery.Filter,
                businessOperationId,
                code,
                isActive,
                amountType,
                payTypeId,
                codes
            }, UnitOfWork.Transaction);
        }

        public int RelationCount(int id)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
SELECT SUM(BusinessOperationSettingCounts.BusinessOperationSettingCount)
FROM (
    SELECT COUNT(*) as BusinessOperationSettingCount FROM CashOrders WHERE BusinessOperationSettingId = @id
    UNION ALL
    SELECT COUNT(*) as BusinessOperationSettingCount FROM AccountRecords WHERE BusinessOperationSettingId = @id
) BusinessOperationSettingCounts", new { id });
        }
    }
}