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
    public class InsuranceOnlineRequestRepository : RepositoryBase, IRepository<InsuranceOnlineRequest>
    {
        public InsuranceOnlineRequestRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(InsuranceOnlineRequest entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                    INSERT INTO InsuranceOnlineRequests(ContractId, RequestData, CreateDate)
                        VALUES(@ContractId, @RequestData, @CreateDate)
                            SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(InsuranceOnlineRequest entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE InsuranceOnlineRequests SET ContractId = @ContractId, RequestData = @RequestData
                            WHERE Id = @id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public InsuranceOnlineRequest Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<InsuranceOnlineRequest>(@"
                SELECT * 
                    FROM InsuranceOnlineRequests
                        WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public InsuranceOnlineRequest Find(object query)
        {
            var contractId = query?.Val<int?>("ContractId");

            if (!contractId.HasValue)
                return null;

            var condition = "WHERE DeleteDate IS NULL AND ContractId = @contractId";

            return UnitOfWork.Session.QueryFirstOrDefault<InsuranceOnlineRequest>($"SELECT * FROM InsuranceOnlineRequests {condition}",
                new { contractId }, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE InsuranceOnlineRequests SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<InsuranceOnlineRequest> List(ListQuery listQuery, object query = null)
        {
            var contractId = query?.Val<int?>("ContractId");

            var condition = "WHERE DeleteDate IS NULL";

            condition += contractId.HasValue ? " AND ContractId = @contractId" : string.Empty;

            return UnitOfWork.Session.Query<InsuranceOnlineRequest>($@"
                SELECT * FROM InsuranceOnlineRequests {condition}", new { contractId },
                UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            var contractId = query?.Val<int?>("ContractId");

            var condition = "WHERE DeleteDate IS NULL";

            condition += contractId.HasValue ? " AND ContractId = @contractId" : string.Empty;

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                    SELECT COUNT(*) FROM InsuranceOnlineRequests {condition}", new { contractId },
                    UnitOfWork.Transaction);
        }
    }
}