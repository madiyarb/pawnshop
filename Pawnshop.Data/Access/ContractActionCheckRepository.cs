using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Access
{
    public class ContractActionCheckRepository : RepositoryBase, IRepository<ContractActionCheck>
    {
        public ContractActionCheckRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ContractActionCheck entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO ContractActionChecks ( Name, Text, PayTypeId, ActionType, AuthorId, CreateDate )
VALUES ( @Name, @Text, @PayTypeId, @ActionType, @AuthorId, @CreateDate )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(ContractActionCheck entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE ContractActionChecks
SET Name = @Name, Text = @Text, PayTypeId = @PayTypeId, ActionType = @ActionType
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE ContractActionChecks SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ContractActionCheck Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<ContractActionCheck>(@"
SELECT *
FROM ContractActionChecks
WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public ContractActionCheck Find(object query)
        {
            throw new System.NotImplementedException();
        }
        public List<ContractActionCheck> Find(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var actionType = query?.Val<ContractActionType?>("ActionType");
            var payTypeId = query?.Val<int?>("PayTypeId");

            var pre = "DeleteDate IS NULL AND((ActionType IS NULL) OR(ActionType = @actionType OR ActionType IS NULL)) AND((ActionType IS NULL) OR(PayTypeId = @payTypeId OR PayTypeId IS NULL))";

            var condition = listQuery.Like(pre, "Name");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Name",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<ContractActionCheck>($@"
SELECT *
FROM ContractActionChecks
{condition} {order} {page}", new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter,
                actionType,
                payTypeId
            }, UnitOfWork.Transaction).ToList();
        }

        public List<ContractActionCheck> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "DeleteDate IS NULL";

            var condition = listQuery.Like(pre, "Name");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Name",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<ContractActionCheck>($@"
SELECT *
FROM ContractActionChecks
{condition} {order} {page}", new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like("DeleteDate IS NULL", "Name");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM ContractActionChecks
{condition}", new
            {
                listQuery.Filter
            }, UnitOfWork.Transaction);
        }
    }
}