using System;
using System.Collections.Generic;
using Dapper;
using Microsoft.Data.SqlClient;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Data.Access
{
    public class ContractNumberCounterRepository : RepositoryBase, IRepository<ContractNumberCounter>
    {
        public ContractNumberCounterRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ContractNumberCounter entity)
        {
            throw new System.NotImplementedException();
        }

        public void Update(ContractNumberCounter entity)
        {
            try
            {
                using (var transaction = BeginTransaction())
                {
                    UnitOfWork.Session.Execute(@"
                        IF NOT EXISTS (SELECT Id FROM ContractNumberCounters WHERE Year = @Year AND BranchId = @BranchId)
                        BEGIN
                            INSERT INTO ContractNumberCounters ( Year, BranchId, Counter )
                            VALUES ( @Year, @BranchId, @Counter )
                        END
                        ELSE
                        BEGIN
                            UPDATE ContractNumberCounters
                            SET Counter = @Counter
                            WHERE Year = @Year AND BranchId = @BranchId
                        END", entity, UnitOfWork.Transaction);

                    transaction.Commit();
                }
            }
            catch (SqlException ex)
            {
                throw new PawnshopApplicationException("Время ожидания операции истекло. Попробуйте повторить операцию");
            }
        }

        public void Delete(int id)
        {
            throw new System.NotImplementedException();
        }

        public ContractNumberCounter Get(int id)
        {
            throw new System.NotImplementedException();
        }

        public ContractNumberCounter Find(object query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var year = query.Val<int?>("Year");
            var branchId = query.Val<int>("BranchId");
            var condition = "WHERE Year = @year AND BranchId = @branchId";

            return UnitOfWork.Session.QuerySingleOrDefault<ContractNumberCounter>($@"
SELECT *
FROM ContractNumberCounters
{condition}", new
            {
                year,
                branchId
            }, UnitOfWork.Transaction);
        }

        public string Next(int year, int branch, string code)
        {
            var counter = Find(new
            {
                Year = year,
                BranchId = branch
            }) ?? new ContractNumberCounter
            {
                Year = year,
                BranchId = branch,
                Counter = 0,
            };
            counter.Counter++;
            Update(counter);

            return $"{counter.Year.ToString().Substring(2, 2)}{code}{counter.Counter:D4}";
        }

        public List<ContractNumberCounter> List(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }
    }
}