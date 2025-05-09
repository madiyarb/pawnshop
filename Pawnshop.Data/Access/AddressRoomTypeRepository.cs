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
    public class AddressRoomTypeRepository : RepositoryBase, IRepository<AddressRoomType>
    {
        public AddressRoomTypeRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(AddressRoomType entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO AddressRoomTypes(Id, Code, ShortNameRus, ShortNameKaz, NameRus, NameKaz, IsActual)
VALUES(@Id, @Code, @ShortNameRus, @ShortNameKaz, @NameRus, @NameKaz, @IsActual)
SELECT SCOPE_IDENTITY()", entity,UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        public void InsertOrUpdate(List<AddressRoomType> entities)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
IF EXISTS (SELECT * FROM AddressRoomTypes WHERE Id = @Id)
BEGIN
UPDATE AddressRoomTypes SET Code = @Code, ShortNameRus = ShortNameRus, ShortNameKaz = ShortNameKaz, NameRus = @NameRus, NameKaz = @NameKaz, IsActual = @IsActual
WHERE Id=@Id
END
ELSE
BEGIN
INSERT INTO AddressRoomTypes(Id, Code, ShortNameRus, ShortNameKaz, NameRus, NameKaz, IsActual)
VALUES(@Id, @Code, @ShortNameRus, @ShortNameKaz, @NameRus, @NameKaz, @IsActual)
END", entities, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(AddressRoomType entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE AddressRoomTypes SET Code = @Code, ShortNameRus = ShortNameRus, ShortNameKaz = ShortNameKaz, NameRus = @NameRus, NameKaz = @NameKaz, IsActual = @IsActual
WHERE Id=@id", entity,UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public AddressRoomType Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<AddressRoomType>(@"
SELECT * 
FROM AddressRoomTypes
WHERE Id=@id", new { id }, UnitOfWork.Transaction);
        }

        public AddressRoomType Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<AddressRoomType> List(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.Query<AddressRoomType>(@"SELECT * FROM AddressRoomTypes", UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"SELECT COUNT(*) FROM AddressRoomTypes", UnitOfWork.Transaction);
        }
    }
}
