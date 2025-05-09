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
    public class AddressGeonimRepository : RepositoryBase, IRepository<AddressGeonim>
    {
        public AddressGeonimRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(AddressGeonim entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO AddressGeonims(Id, ParentId, ATETypeId, ATEId, GeonimTypeId, FullPathRus, FullPathKaz, NameRus, NameKaz, KATOCode, IsActual, ModifyDate, RCACode)
VALUES(@Id, @ParentId, @ATETypeId, @ATEId, @GeonimTypeId, @FullPathRus, @FullPathKaz, @NameRus, @NameKaz, @KATOCode, @IsActual, @ModifyDate, @RCACode)
SELECT SCOPE_IDENTITY()", entity,UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        public void InsertOrUpdate(List<AddressGeonim> entities)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
IF EXISTS (SELECT * FROM AddressGeonims WHERE Id = @Id)
BEGIN
UPDATE AddressGeonims SET ParentId = @ParentId, ATETypeId = @ATETypeId, ATEId = @ATEId, GeonimTypeId = @GeonimTypeId, FullPathRus = @FullPathRus, FullPathKaz = @FullPathKaz, NameRus = @NameRus, NameKaz = @NameKaz, KATOCode = @KATOCode, IsActual = @IsActual, ModifyDate = @ModifyDate, RCACode = @RCACode
WHERE Id=@Id
END
ELSE
BEGIN
INSERT INTO AddressGeonims(Id, ParentId, ATETypeId, ATEId, GeonimTypeId, FullPathRus, FullPathKaz, NameRus, NameKaz, KATOCode, IsActual, ModifyDate, RCACode)
VALUES(@Id, @ParentId, @ATETypeId, @ATEId, @GeonimTypeId, @FullPathRus, @FullPathKaz, @NameRus, @NameKaz, @KATOCode, @IsActual, @ModifyDate, @RCACode)
END", entities, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(AddressGeonim entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE AddressGeonims SET ParentId = @ParentId, ATETypeId = @ATETypeId, ATEId = @ATEId, GeonimTypeId = @GeonimTypeId, FullPathRus = @FullPathRus, FullPathKaz = @FullPathKaz, NameRus = @NameRus, NameKaz = @NameKaz, KATOCode = @KATOCode, IsActual = @IsActual, ModifyDate = @ModifyDate, RCACode = @RCACode
WHERE Id=@id", entity,UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public AddressGeonim Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<AddressGeonim>(@"
SELECT * 
FROM AddressGeonims
WHERE Id=@id", new { id }, UnitOfWork.Transaction);
        }

        public AddressGeonim Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<AddressGeonim> List(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.Query<AddressGeonim>(@"SELECT * FROM AddressGeonims", UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"SELECT COUNT(*) FROM AddressGeonims", UnitOfWork.Transaction);
        }

        public DateTime GetLastModifiedDate()
        {
            return UnitOfWork.Session.ExecuteScalar<DateTime>(@"SELECT ISNULL(MAX(ModifyDate),'1970-01-01') FROM AddressGeonims", UnitOfWork.Transaction);
        }
    }
}
