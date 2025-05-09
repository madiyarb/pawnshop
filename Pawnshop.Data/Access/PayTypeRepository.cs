using Dapper;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Dictionaries;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Pawnshop.Data.Models.Base;

namespace Pawnshop.Data.Access
{
    public class PayTypeRepository : RepositoryBase, IRepository<PayType>
    {
        public PayTypeRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(PayType entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO PayTypes ( Name, OperationCode, RequiredRequisiteTypeId, AccountId, IsDefault, AuthorId, CreateDate, AccountantUploadRequired, UseSystemType )
VALUES ( @Name, @OperationCode, @RequiredRequisiteTypeId, @AccountId, @IsDefault, @AuthorId, @CreateDate, @AccountantUploadRequired, @UseSystemType )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                InsertOrUpdateActionTypes(entity);

                transaction.Commit();
            }
        }

        private void InsertOrUpdateActionTypes(PayType entity)
        {
            UnitOfWork.Session.Execute("DELETE FROM PayTypeContractActions WHERE PayTypeId = @Id", entity, UnitOfWork.Transaction);

            if (entity.Rules != null)
                entity.Rules.ForEach(actionType =>
                {
                    UnitOfWork.Session.Execute("INSERT INTO PayTypeContractActions (PayTypeId, ActionType, CollateralType) VALUES (@payTypeId, @ActionType, @CollateralType)", new { payTypeId = entity.Id, actionType }, UnitOfWork.Transaction);
                });
        }

        public void Update(PayType entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE PayTypes
   SET Name = @Name,
       RequiredRequisiteTypeId = @RequiredRequisiteTypeId,
       AccountId = @AccountId,
       IsDefault = @IsDefault,
       OperationCode = @OperationCode,
       AccountantUploadRequired = @AccountantUploadRequired,
       UseSystemType = @UseSystemType
 WHERE Id = @Id", entity, UnitOfWork.Transaction);

                InsertOrUpdateActionTypes(entity);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE PayTypes SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public PayType Get(int id)
        {
            var result = UnitOfWork.Session.Query<PayType>(@"
                SELECT *
                FROM PayTypes
                WHERE Id = @id", new { id }, UnitOfWork.Transaction).FirstOrDefault();

            result.Rules = UnitOfWork.Session.Query<PayTypeContractAction>(@"
                SELECT *
                FROM PayTypeContractActions
                WHERE PayTypeId = @id", new { id }, UnitOfWork.Transaction).ToList();

            return result;
        }

        public async Task<PayType> GetByOperationCode(string payTypeCode)
        {
            var parameters = new { OperationCode = payTypeCode};
            var sqlQuery = @"
                SELECT * FROM PayTypes
                WHERE DeleteDate IS NULL
                  AND OperationCode = @OperationCode";

            return await UnitOfWork.Session
                .QueryFirstOrDefaultAsync<PayType>(sqlQuery, parameters, UnitOfWork.Transaction);
        }

        public PayType Find(object query)
        {
            var code = query?.Val<string>("Code");
            var isDefault = query?.Val<bool?>("IsDefault");

            var pre = "DeleteDate IS NULL";
            pre += string.IsNullOrEmpty(code) ? string.Empty : " AND OperationCode = @code";
            pre += !isDefault.HasValue ? string.Empty : " AND IsDefault = @isDefault";

            var condition = new ListQuery().Like(pre, "Name");

            return UnitOfWork.Session.Query<PayType>(@$"
                SELECT *
                FROM PayTypes
{condition}", new { code, isDefault }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<PayType> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var actionType = query?.Val<ContractActionType?>("ActionType");
            var collateralType = query?.Val<CollateralType?>("CollateralType");
            var useSystemType = query?.Val<UseSystemType?>("UseSystemType");

            var pre = "DeleteDate IS NULL";
            pre += actionType.HasValue ? " AND act.ActionType = @actionType" : string.Empty;
            pre += collateralType.HasValue ? " AND (act.CollateralType = @collateralType OR act.CollateralType IS NULL)" : string.Empty;

            if (useSystemType.HasValue)
            {
                pre += useSystemType switch
                {
                    UseSystemType.OFFLINE => $" AND pt.UseSystemType != {(int)UseSystemType.ONLINE}",
                    UseSystemType.ONLINE => $" AND pt.UseSystemType != {(int)UseSystemType.OFFLINE}",
                    _ => string.Empty,
                };
            }

            var condition = listQuery.Like(pre, "Name");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Name",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<PayType>($@"
                SELECT DISTINCT pt.*
                FROM PayTypes pt
                LEFT JOIN PayTypeContractActions act ON act.PayTypeId = pt.Id
                {condition} {order} {page}", new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter,
                actionType,
                collateralType
            }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var actionType = query?.Val<ContractActionType?>("ActionType");

            var pre = "DeleteDate IS NULL";
            pre += actionType.HasValue ? " AND act.ActionType = @actionType" : string.Empty;

            var condition = listQuery.Like(pre, "Name");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                SELECT COUNT(DISTINCT pt.Id)
                FROM PayTypes pt
                LEFT JOIN PayTypeContractActions act ON act.PayTypeId = pt.Id
                {condition}", new
            {
                listQuery.Filter,
                actionType
            }, UnitOfWork.Transaction);
        }

        public int RelationCount(int id)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
SELECT SUM(q)
FROM (
SELECT COUNT(*) as q
FROM ContractActions
WHERE PayTypeId = @id
UNION
SELECT COUNT(*) as q
FROM PayOperations
WHERE PayTypeId = @id) as t", new { id }, UnitOfWork.Transaction);
        }

        public async Task<PayType> GetByRequisiteType(int requisiteTypeId)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<PayType>(@"SELECT *
  FROM PayTypes
 WHERE RequiredRequisiteTypeId = @requisiteTypeId
   AND DeleteDate IS NULL",
                new { requisiteTypeId }, UnitOfWork.Transaction);
        }
    }
}