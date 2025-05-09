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
    public class AddressATETypeRepository : RepositoryBase, IRepository<AddressATEType>
    {
        public AddressATETypeRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(AddressATEType entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO AddressATETypes(Id, Code, ShortNameRus, ShortNameKaz, NameRus, NameKaz, IsActual)
VALUES(@Id, @Code, @ShortNameRus, @ShortNameKaz, @NameRus, @NameKaz, @IsActual)
SELECT SCOPE_IDENTITY()", entity,UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void InsertOrUpdate(List<AddressATEType> entities)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
IF EXISTS (SELECT * FROM AddressATETypes WHERE Id = @Id)
BEGIN
UPDATE AddressATETypes SET Code = @Code, ShortNameRus = ShortNameRus, ShortNameKaz = ShortNameKaz, NameRus = @NameRus, NameKaz = @NameKaz, IsActual = @IsActual
WHERE Id=@id
END
ELSE
BEGIN
INSERT INTO AddressATETypes(Id, Code, ShortNameRus, ShortNameKaz, NameRus, NameKaz, IsActual)
VALUES(@Id, @Code, @ShortNameRus, @ShortNameKaz, @NameRus, @NameKaz, @IsActual)
END", entities, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(AddressATEType entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE AddressATETypes SET Code = @Code, ShortNameRus = ShortNameRus, ShortNameKaz = ShortNameKaz, NameRus = @NameRus, NameKaz = @NameKaz, IsActual = @IsActual
WHERE Id=@id", entity,UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public AddressATEType Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<AddressATEType>(@"
SELECT * 
FROM AddressATETypes
WHERE Id=@id", new { id }, UnitOfWork.Transaction);
        }

        public AddressATEType Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<AddressATEType> List(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.Query<AddressATEType>(@"SELECT * FROM AddressATETypes", UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"SELECT COUNT(*) FROM AddressATETypes", UnitOfWork.Transaction);
        }
    }
}
