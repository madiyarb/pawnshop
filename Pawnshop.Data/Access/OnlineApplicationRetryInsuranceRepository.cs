using Dapper;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.OnlineApplications;
using System.Collections.Generic;
using System.Linq;
using System;
using Pawnshop.Core.Exceptions;

namespace Pawnshop.Data.Access
{
    public class OnlineApplicationRetryInsuranceRepository : RepositoryBase, IRepository<OnlineApplicationRetryInsurance>
    {
        public OnlineApplicationRetryInsuranceRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
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

        public OnlineApplicationRetryInsurance Find(object query)
        {
            var contractId = query.Val<int?>("ContractId");

            if (!contractId.HasValue)
                throw new PawnshopApplicationException("Идентификатор контракта обязателен!");

            return UnitOfWork.Session.QueryFirstOrDefault<OnlineApplicationRetryInsurance>(@"SELECT *
  FROM OnlineApplicationRetryInsurances
 WHERE ContractId = @contractId",
                new { contractId }, UnitOfWork.Transaction);
        }

        public OnlineApplicationRetryInsurance Get(int id)
        {
            throw new NotImplementedException();
        }

        public void Insert(OnlineApplicationRetryInsurance entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.CreateDate = DateTime.Now;
                UnitOfWork.Session.Execute(@"INSERT INTO OnlineApplicationRetryInsurances ( CreateDate, ContractId, IsSuccessful, Attempts )
VALUES ( @CreateDate, @ContractId, @IsSuccessful, @Attempts )",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<OnlineApplicationRetryInsurance> List(ListQuery listQuery, object query = null)
        {
            var attempts = query?.Val<int?>("Attempts");

            if (!attempts.HasValue)
                attempts = 2;

            return UnitOfWork.Session.Query<OnlineApplicationRetryInsurance>(@"SELECT *
  FROM OnlineApplicationRetryInsurances
 WHERE IsSuccessful = 0
   AND Attempts < @attempts",
                new { attempts }, UnitOfWork.Transaction).ToList();
        }

        public void Update(OnlineApplicationRetryInsurance entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE OnlineApplicationRetryInsurances
   SET IsSuccessful = @IsSuccessful,
       Attempts = @Attempts
 WHERE Id = @Id",
                entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }
    }
}
