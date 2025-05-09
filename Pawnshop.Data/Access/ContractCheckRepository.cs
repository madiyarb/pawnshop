using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Access
{
    public class ContractCheckRepository : RepositoryBase, IRepository<ContractCheck>
    {
        public ContractCheckRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ContractCheck entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO ContractChecks ( Name, Text, Code, CollateralType, PeriodRequired, DefaultPeriodAddedInYears, AuthorId, CreateDate )
VALUES ( @Name, @Text, @Code, @CollateralType, @PeriodRequired, @DefaultPeriodAddedInYears, @AuthorId, @CreateDate )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(ContractCheck entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE ContractChecks
SET Name = @Name, Text = @Text, Code = @Code, CollateralType = @CollateralType, PeriodRequired = @PeriodRequired, DefaultPeriodAddedInYears = @DefaultPeriodAddedInYears
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE ContractChecks SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ContractCheck Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<ContractCheck>(@"
SELECT *
FROM ContractChecks
WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public ContractCheck Find(object query)
        {
            throw new System.NotImplementedException();
        }
        public List<ContractCheck> Find(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            var collateralType= query?.Val<CollateralType?>("CollateralType");

            var pre = "DeleteDate IS NULL AND((CollateralType IS NULL) OR(CollateralType = @actionType OR CollateralType IS NULL))";

            var condition = listQuery.Like(pre, "Name");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "Name",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<ContractCheck>($@"
SELECT *
FROM ContractChecks
{condition} {order} {page}", new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter,
                collateralType
            }, UnitOfWork.Transaction).ToList();
        }

        public List<ContractCheck> List(ListQuery listQuery, object query = null)
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

            return UnitOfWork.Session.Query<ContractCheck>($@"
SELECT *
FROM ContractChecks
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
FROM ContractChecks
{condition}", new
            {
                listQuery.Filter
            }, UnitOfWork.Transaction);
        }
    }
}