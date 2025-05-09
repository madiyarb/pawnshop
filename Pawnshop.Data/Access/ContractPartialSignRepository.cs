using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pawnshop.Data.Access
{
    public class ContractPartialSignRepository : RepositoryBase, IRepository<ContractPartialSign>
    {
        public ContractPartialSignRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("DELETE FROM ContractPartialSign WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ContractPartialSign Find(object query)
        {
            throw new NotImplementedException();
        }

        public ContractPartialSign Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<ContractPartialSign>(@"
                SELECT *
                FROM ContractPartialSign
                WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public ContractPartialSign GetByContractId(int contractId)
        {
            var contractPartialSigns = UnitOfWork.Session.Query<ContractPartialSign>(@"
                SELECT *
                FROM ContractPartialSign
                WHERE Id = @contractId AND DeleteDate IS NULL
                ORDER BY Id desc",
                new { contractId }, UnitOfWork.Transaction).ToList();

            if (contractPartialSigns.Count > 1)
                throw new PawnshopApplicationException($"Для Договора с Id {contractId} активных записей в ContractPartialSign более одного");

            return contractPartialSigns.FirstOrDefault();
        }

        public void Insert(ContractPartialSign entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO ContractPartialSign ( Id, TotalAmount, AuthorId, CreateDate )
                    VALUES ( @Id, @TotalAmount, @AuthorId, @CreateDate )",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<ContractPartialSign> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Update(ContractPartialSign entity)
        {
            throw new NotImplementedException();
        }
    }
}
