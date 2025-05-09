using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts.Kdn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pawnshop.Data.Access
{
    public class ContractKdnEstimatedIncomeRepository : RepositoryBase, IRepository<ContractKdnEstimatedIncome>
    {
        public ContractKdnEstimatedIncomeRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ContractKdnEstimatedIncome entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO ContractKdnEstimatedIncomes ( ContractId, ContractPositionId, EstimatedIncome, AuthorId, CreateDate )
                    VALUES ( @ContractId, @ContractPositionId, @EstimatedIncome, @AuthorId, @CreateDate )
                    SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(ContractKdnEstimatedIncome entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ContractKdnEstimatedIncomes
                    SET ContractId = @ContractId, ContractPositionId = @ContractPositionId, EstimatedIncome = @EstimatedIncome, AuthorId = @AuthorId
                    WHERE Id = @Id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("DELETE FROM ContractKdnEstimatedIncomes WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ContractKdnEstimatedIncome Find(object query)
        {
            throw new NotImplementedException();
        }

        public ContractKdnEstimatedIncome Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<ContractKdnEstimatedIncome>(@"
                SELECT *
                FROM ContractKdnEstimatedIncomes
                WHERE Id = @id", new { id });
        }

        public List<ContractKdnEstimatedIncome> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty);
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "ContractId",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<ContractKdnEstimatedIncome>($@"
                SELECT *
                FROM ContractKdnEstimatedIncomes
                {condition} {order} {page}",
                new
                {
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter
                }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty);

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                SELECT COUNT(*)
                FROM ContractKdnEstimatedIncomes
                {condition}",
                new
                {
                   listQuery.Filter
                }, UnitOfWork.Transaction);
        }
    }
}
