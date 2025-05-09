using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Access
{
    public class OrganizationRepository : RepositoryBase, IRepository<Organization>
    {
        public OrganizationRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(Organization entity)
        {
            throw new System.NotImplementedException();
        }

        public void Update(Organization entity)
        {
            UnitOfWork.Session.Execute(@"
UPDATE Organizations SET Name = @Name, Locked = @Locked, Uid = @Uid, Configuration = @Configuration
 WHERE Id = @Id", entity, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            throw new System.NotImplementedException();
        }

        public Organization Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<Organization>(@"SELECT * FROM Organizations WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public Organization Find(object query)
        {
            throw new System.NotImplementedException();
        }

        public List<Organization> List(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.Query<Organization>(@"SELECT * FROM Organizations").ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"SELECT COUNT(*) FROM Organizations");
        }

        public List<Organization> List(object query = null, Page page = null)
        {
            return UnitOfWork.Session.Query<Organization>(@"SELECT * FROM Organizations", null, UnitOfWork.Transaction).ToList();
        }

        public async Task<Organization> GetByName(string name)
        {
            return await UnitOfWork.Session.QueryFirstOrDefaultAsync<Organization>(
                @"Select * FROM Organizations where Name = @name", new {name}, UnitOfWork.Transaction);
        }
    }
}