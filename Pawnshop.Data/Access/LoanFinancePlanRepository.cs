using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts.LoanFinancePlans;

namespace Pawnshop.Data.Access
{
    public class LoanFinancePlanRepository : RepositoryBase, IRepository<LoanFinancePlan>
    {
        private readonly ISessionContext _sessionContext;
        public LoanFinancePlanRepository(IUnitOfWork unitOfWork, ISessionContext sessionContext) : base(unitOfWork)
        {
            _sessionContext = sessionContext;
        }

        public void Insert(LoanFinancePlan entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                    INSERT INTO LoanFinancePlans(ContractId, LoanPurposeId, Description, OwnFunds, DebtFunds,
                                                    CreateDate, AuthorId)
                        VALUES(@ContractId, @LoanPurposeId, @Description, @OwnFunds, @DebtFunds,
                                                    @CreateDate, @AuthorId)
                            SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(LoanFinancePlan entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE LoanFinancePlans SET ContractId = @ContractId, LoanPurposeId = @LoanPurposeId, Description = @Description,
                                        OwnFunds = @OwnFunds, DebtFunds = @DebtFunds, CreateDate = @CreateDate, AuthorId = @AuthorId 
                            WHERE Id = @id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public LoanFinancePlan Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<LoanFinancePlan>(@"
                SELECT * 
                FROM LoanFinancePlans
                WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public LoanFinancePlan Find(object query)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE LoanFinancePlans SET DeleteDate = dbo.GETASTANADATE(), AuthorId=@author WHERE Id = @id", new { id = id , author = _sessionContext.UserId}, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<LoanFinancePlan> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var contractId = query?.Val<int>("ContractId");

            return UnitOfWork.Session.Query<LoanFinancePlan>($@"
                SELECT * FROM LoanFinancePlans
                WHERE DeleteDate IS NULL AND ContractId = @contractId", new { contractId },
            UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var contractId = query?.Val<int>("ContractId");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                    SELECT COUNT(*) FROM LoanFinancePlans
                    WHERE DeleteDate IS NULL AND ContractId = @contractId", new { contractId },
            UnitOfWork.Transaction);
        }
    }
}