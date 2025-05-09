using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Postponements;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Postponements;

namespace Pawnshop.Data.Access
{
    public class ContractPostponementRepository : RepositoryBase, IRepository<ContractPostponement>
    {
        public ContractPostponementRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ContractPostponement entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO ContractPostponements
( ContractId, PostponementId, AuthorId, CreateDate, Date )
VALUES
( @ContractId, @PostponementId, @AuthorId, ISNULL(@CreateDate,dbo.GETASTANADATE()), @Date )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(ContractPostponement entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE ContractPostponements
SET PostponementId=@PostponementId, Date=@Date
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            UnitOfWork.Session.Execute("UPDATE ContractPostponements SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public ContractPostponement Get(int id)
        {
            return UnitOfWork.Session.Query<ContractPostponement, Postponement, ContractPostponement>(@"
SELECT 
cp.*, p.*
FROM ContractPostponements cp
LEFT JOIN Postponements p ON cp.PostponementId=p.Id
WHERE cp.Id = @id", (cp, p) => {
                if (cp != null)
                {
                    cp.Postponement = p;
                }
                return cp;
            }, new { id }).FirstOrDefault();
        }

        public List<ContractPostponement> GetByContractId(int contractId)
        {
            return UnitOfWork.Session.Query<ContractPostponement, Postponement, ContractPostponement>(@"
SELECT 
cp.*, p.*
FROM ContractPostponements cp
LEFT JOIN Postponements p ON cp.PostponementId=p.Id
WHERE cp.ContractId = @contractId AND cp.DeleteDate IS NULL", (cp, p) => {
                if (cp != null)
                {
                    cp.Postponement = p;
                }
                return cp;
            }, new { contractId }, UnitOfWork.Transaction).ToList();
        }

        public ContractPostponement Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<ContractPostponement> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }
    }
}