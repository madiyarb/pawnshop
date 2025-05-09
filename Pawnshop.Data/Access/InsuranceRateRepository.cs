using System;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Data.Models.Insurances;

namespace Pawnshop.Data.Access
{
    public class InsuranceRateRepository : RepositoryBase, IRepository<InsuranceRate>
    {
        public InsuranceRateRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(InsuranceRate entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                    INSERT INTO InsuranceRates(InsuranceCompanyId, AmountFrom, AmountTo, Rate, CreateDate, AuthorId)
                        VALUES(@InsuranceCompanyId, @AmountFrom, @AmountTo, @Rate, @CreateDate, @AuthorId)
                            SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(InsuranceRate entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE InsuranceRates SET InsuranceCompanyId = @InsuranceCompanyId, AmountFrom = @AmountFrom, AmountTo = @AmountTo, Rate = @Rate
                            WHERE Id = @id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public InsuranceRate Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<InsuranceRate>(@"
                SELECT * 
                    FROM InsuranceRates
                        WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public InsuranceRate Find(object query)
        {
            var insuranceCompanyId = query?.Val<int?>("InsuranceCompanyId");
            var loanCost = query?.Val<decimal?>("LoanCost");

            var condition = "WHERE DeleteDate IS NULL";

            condition += insuranceCompanyId.HasValue ? " AND InsuranceCompanyId = @insuranceCompanyId" : string.Empty;
            condition += loanCost.HasValue ? " AND @loanCost >= AmountFrom AND @loanCost < AmountTo" : string.Empty;

            return UnitOfWork.Session.Query<InsuranceRate>($@"
                SELECT * FROM InsuranceRates {condition}", new { insuranceCompanyId, loanCost },
                UnitOfWork.Transaction).FirstOrDefault();
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE InsuranceRates SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<InsuranceRate> List(ListQuery listQuery, object query = null)
        {
            var insuranceCompanyId = query?.Val<int?>("InsuranceCompanyId");

            var condition = "WHERE DeleteDate IS NULL";

            condition += insuranceCompanyId.HasValue ? " AND InsuranceCompanyId = @insuranceCompanyId" : string.Empty;

            return UnitOfWork.Session.Query<InsuranceRate>($@"
                SELECT * FROM InsuranceRates {condition}", new { insuranceCompanyId },
                UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            var insuranceCompanyId = query?.Val<int?>("InsuranceCompanyId");

            var condition = "WHERE DeleteDate IS NULL";

            condition += insuranceCompanyId.HasValue ? " AND InsuranceCompanyId = @insuranceCompanyId" : string.Empty;

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                    SELECT COUNT(*) FROM InsuranceRates {condition}", new { insuranceCompanyId },
                    UnitOfWork.Transaction);
        }
    }
}