using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Data.Models.Mintos;
using Pawnshop.Core.Impl;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Dapper;

namespace Pawnshop.Data.Access
{
    public class MintosBlackListRepository : RepositoryBase, IRepository<MintosBlackList>
    {
        public MintosBlackListRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
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

        public MintosBlackList Find(object query)
        {
            throw new NotImplementedException();
        }

        public MintosBlackList Get(int id)
        {
            throw new NotImplementedException();
        }

        public void Insert(MintosBlackList entity)
        {
                UnitOfWork.Session.Execute(@"
INSERT INTO MintosBlackList
( ContractId, LockUntilDate )
VALUES ( @ContractId, @LockUntilDate )", entity, UnitOfWork.Transaction);
        }

        public void Insert(int contractId)
        {
               UnitOfWork.Session.Execute(@"
INSERT INTO MintosBlackList
( ContractId, LockUntilDate )
VALUES ( @id, dbo.GETASTANADATE() + 2 )", new { id = contractId}, UnitOfWork.Transaction);
        }

        public List<MintosBlackList> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Update(MintosBlackList entity)
        {
            throw new NotImplementedException();
        }
    }
}
