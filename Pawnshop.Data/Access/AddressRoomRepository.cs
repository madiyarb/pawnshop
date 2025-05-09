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
    public class AddressRoomRepository : RepositoryBase, IRepository<AddressRoom>
    {
        public AddressRoomRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(AddressRoom entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO AddressRooms(Id, BuildingId, RoomTypeId, FullPathRus, FullPathKaz, Number, IsActual, ModifyDate, RCACode)
VALUES(@Id, @BuildingId, @RoomTypeId, @FullPathRus, @FullPathKaz, @Number, @IsActual, @ModifyDate, @RCACode)
SELECT SCOPE_IDENTITY()", entity,UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        public void InsertOrUpdate(List<AddressRoom> entities)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
IF EXISTS (SELECT * FROM AddressRooms WHERE Id = @Id)
BEGIN
UPDATE AddressRooms SET BuildingId = @BuildingId, RoomTypeId = @RoomTypeId, FullPathRus = @FullPathRus, FullPathKaz = @FullPathKaz, Number = @Number, IsActual = @IsActual, ModifyDate = @ModifyDate, RCACode = @RCACode
WHERE Id=@Id
END
ELSE
BEGIN
INSERT INTO AddressRooms(Id, BuildingId, RoomTypeId, FullPathRus, FullPathKaz, Number, IsActual, ModifyDate, RCACode)
VALUES(@Id, @BuildingId, @RoomTypeId, @FullPathRus, @FullPathKaz, @Number, @IsActual, @ModifyDate, @RCACode)
END", entities, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(AddressRoom entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE AddressRooms SET BuildingId = @BuildingId, RoomTypeId = @RoomTypeId, FullPathRus = @FullPathRus, FullPathKaz = @FullPathKaz, Number = @Number, IsActual = @IsActual, ModifyDate = @ModifyDate, RCACode = @RCACode
WHERE Id=@id", entity,UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public AddressRoom Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<AddressRoom>(@"
SELECT * 
FROM AddressRooms
WHERE Id=@id", new { id }, UnitOfWork.Transaction);
        }

        public AddressRoom Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<AddressRoom> List(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.Query<AddressRoom>(@"SELECT * FROM AddressRooms", UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"SELECT COUNT(*) FROM AddressRooms", UnitOfWork.Transaction);
        }
    }
}
