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
    public class AddressBuildingRepository : RepositoryBase, IRepository<AddressBuilding>
    {
        public AddressBuildingRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(AddressBuilding entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO AddressBuildings(Id, ParentId, ATEId, GeonimId, BuildingTypeId, FullPathRus, FullPathKaz, Number, IsActual, ModifyDate, RCACode, ParentRCACode)
VALUES(@Id, @ParentId, @ATEId, @GeonimId, @BuildingTypeId, @FullPathRus, @FullPathKaz, @Number, @IsActual, @ModifyDate, @RCACode, @ParentRCACode)
SELECT SCOPE_IDENTITY()", entity,UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        public void InsertOrUpdate(List<AddressBuilding> entities)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
IF EXISTS (SELECT * FROM AddressBuildings WHERE Id = @Id)
BEGIN
UPDATE AddressBuildings SET ParentId = @ParentId, ATEId = @ATEId, GeonimId = @GeonimId, BuildingTypeId = @BuildingTypeId, FullPathRus = @FullPathRus, FullPathKaz = @FullPathKaz, Number = @Number, IsActual = @IsActual, ModifyDate = @ModifyDate, RCACode = @RCACode, ParentRCACode = @ParentRCACode
WHERE Id=@Id
END
ELSE
BEGIN
INSERT INTO AddressBuildings(Id, ParentId, ATEId, GeonimId, BuildingTypeId, FullPathRus, FullPathKaz, Number, IsActual, ModifyDate, RCACode, ParentRCACode)
VALUES(@Id, @ParentId, @ATEId, @GeonimId, @BuildingTypeId, @FullPathRus, @FullPathKaz, @Number, @IsActual, @ModifyDate, @RCACode, @ParentRCACode)
END", entities, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(AddressBuilding entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE AddressBuildings SET ParentId = @ParentId, ATEId = @ATEId, GeonimId = @GeonimId, BuildingTypeId = @BuildingTypeId, FullPathRus = @FullPathRus, FullPathKaz = @FullPathKaz, Number = @Number, IsActual = @IsActual, ModifyDate = @ModifyDate, RCACode = @RCACode, ParentRCACode = @ParentRCACode
WHERE Id=@id", entity,UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public AddressBuilding Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<AddressBuilding>(@"
SELECT * 
FROM AddressBuildings
WHERE Id=@id", new { id }, UnitOfWork.Transaction);
        }

        public AddressBuilding Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<AddressBuilding> List(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.Query<AddressBuilding>(@"SELECT * FROM AddressBuildings", UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"SELECT COUNT(*) FROM AddressBuildings", UnitOfWork.Transaction);
        }
    }
}
