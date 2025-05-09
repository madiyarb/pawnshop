using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts.Kdn;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Access
{
    public class ContractKdnRequestRepository : RepositoryBase, IRepository<ContractKdnRequest>
    {
        public ContractKdnRequestRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public ContractKdnRequest Find(object query)
        {
            throw new NotImplementedException();
        }

        public ContractKdnRequest Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<ContractKdnRequest>(@"
                SELECT *
                FROM ContractKdnRequests
                WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public void Insert(ContractKdnRequest entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO ContractKdnRequests ( KdnCalculationId, ClientId, IncomeRequest, FCBRequestId, KDNScore, Debt, IncomeResponse, CreditInfoId, CorrelationId, RequestDate )
                    VALUES ( @KdnCalculationId, @ClientId, @IncomeRequest, @FCBRequestId, @KDNScore, @Debt, @IncomeResponse, @CreditInfoId, @CorrelationId, @RequestDate )
                    SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<ContractKdnRequest> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Update(ContractKdnRequest entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ContractKdnRequests
                    SET KdnCalculationId = @KdnCalculationId, ClientId = @ClientId, IncomeRequest = @IncomeRequest, 
                        FCBRequestId = @FCBRequestId, KDNScore = @KDNScore, Debt = @Debt,
                        IncomeResponse = @IncomeResponse, CreditInfoId = @CreditInfoId
                    WHERE Id = @Id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
    }
}
