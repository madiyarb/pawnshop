using System.Collections.Generic;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;
using Dapper;
using System.Linq;
using System.Diagnostics.Contracts;

namespace Pawnshop.Data.Access
{
    public class ContractCheckValueRepository : RepositoryBase, IRepository<ContractCheckValue>
    {
        public ContractCheckValueRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ContractCheckValue entity)
        {
            throw new System.NotImplementedException();
        }

        public void Update(ContractCheckValue entity)
        {
            throw new System.NotImplementedException();
        }

        public void Delete(int id)
        {
            throw new System.NotImplementedException();
        }

        public ContractCheckValue Get(int id)
        {
            throw new System.NotImplementedException();
        }

        public ContractCheckValue Find(object query)
        {
            throw new System.NotImplementedException();
        }
        public List<ContractCheckValue> Find(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }

        public List<ContractCheckValue> List(ListQuery listQuery, object query = null)
        {
            var contractId = query?.Val<int?>("ContractId");
            return UnitOfWork.Session.Query<ContractCheckValue, ContractCheck, User, ContractCheckValue>(@"
                    SELECT val.*, ch.*, u.* FROM ContractCheckValues val WITH(NOLOCK)
                    LEFT JOIN ContractChecks ch ON ch.Id = val.CheckId
			        LEFT JOIN Users u ON val.AuthorId = u.Id
                    WHERE val.ContractId = @id",
                (val, ch, u) =>
                {
                    val.Check = ch;
                    val.Author = u;
                    return val;
                },
                new { id = contractId }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }
    }
}

