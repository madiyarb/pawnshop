using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries.Address;

namespace Pawnshop.Data.Access
{
    public class AddressTypeRepository : RepositoryBase, IRepository<AddressType>
    {
        public AddressTypeRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(AddressType entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO AddressTypes(Code, Name, CBId, IsIndividual)
VALUES(@Code, @Name, @CBId, @IsIndividual)
SELECT SCOPE_IDENTITY()", entity,UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(AddressType entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE AddressTypes SET Code = @Code, Name=@Name, CBId=@CBId, IsIndividual=@IsIndividual
WHERE Id=@id", entity,UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE AddressTypes SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public AddressType Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<AddressType>(@"
SELECT * 
FROM AddressTypes
WHERE Id=@id", new { id }, UnitOfWork.Transaction);
        }

        public AddressType Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<AddressType> List(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.Query<AddressType>(@"SELECT * FROM AddressTypes WHERE DeleteDate IS NULL", new { }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"SELECT COUNT(*) FROM AddressTypes WHERE DeleteDate IS NULL", new { }, UnitOfWork.Transaction);
        }
    }
}
