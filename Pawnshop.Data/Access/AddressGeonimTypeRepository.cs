using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Dictionaries.Address;

namespace Pawnshop.Data.Access
{
    public class AddressGeonimTypeRepository : RepositoryBase, IRepository<AddressGeonimType>
    {
        public AddressGeonimTypeRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(AddressGeonimType entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO AddressGeonimTypes(Id, Code, ShortNameRus, ShortNameKaz, NameRus, NameKaz, IsActual)
VALUES(@Id, @Code, @ShortNameRus, @ShortNameKaz, @NameRus, @NameKaz, @IsActual)
SELECT SCOPE_IDENTITY()", entity,UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        public void InsertOrUpdate(List<AddressGeonimType> entities)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
IF EXISTS (SELECT * FROM AddressGeonimTypes WHERE Id = @Id)
BEGIN
UPDATE AddressGeonimTypes SET Code = @Code, ShortNameRus = ShortNameRus, ShortNameKaz = ShortNameKaz, NameRus = @NameRus, NameKaz = @NameKaz, IsActual = @IsActual
WHERE Id=@Id
END
ELSE
BEGIN
INSERT INTO AddressGeonimTypes(Id, Code, ShortNameRus, ShortNameKaz, NameRus, NameKaz, IsActual)
VALUES(@Id, @Code, @ShortNameRus, @ShortNameKaz, @NameRus, @NameKaz, @IsActual)
END", entities, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(AddressGeonimType entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE AddressGeonimTypes SET Code = @Code, ShortNameRus = ShortNameRus, ShortNameKaz = ShortNameKaz, NameRus = @NameRus, NameKaz = @NameKaz, IsActual = @IsActual
WHERE Id=@id", entity,UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public AddressGeonimType Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<AddressGeonimType>(@"
SELECT * 
FROM AddressGeonimTypes
WHERE Id=@id", new { id }, UnitOfWork.Transaction);
        }

        public AddressGeonimType Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<AddressGeonimType> List(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.Query<AddressGeonimType>(@"SELECT * FROM AddressGeonimTypes", UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"SELECT COUNT(*) FROM AddressGeonimTypes", UnitOfWork.Transaction);
        }
    }
}
