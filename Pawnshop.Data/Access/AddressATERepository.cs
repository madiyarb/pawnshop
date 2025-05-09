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
    public class AddressATERepository : RepositoryBase, IRepository<AddressATE>
    {
        public AddressATERepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(AddressATE entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                    INSERT INTO AddressATEs(ParentId, ATETypeId, FullPathRus, FullPathKaz, NameRus, NameKaz, KATOCode, IsActual, ModifyDate, RCACode)
                    VALUES(@ParentId, @ATETypeId, @FullPathRus, @FullPathKaz, @NameRus, @NameKaz, @KATOCode, @IsActual, @ModifyDate, @RCACode)
                    SELECT SCOPE_IDENTITY()", entity,UnitOfWork.Transaction);
                                    transaction.Commit();
            }
        }

        public void InsertOrUpdate(List<AddressATE> entities)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    IF EXISTS (SELECT * FROM AddressATEs WHERE Id = @Id)
                    BEGIN
                    UPDATE AddressATEs SET Id = @Id, ParentId = @ParentId, ATETypeId = @ATETypeId, FullPathRus = @FullPathRus, FullPathKaz = @FullPathKaz, NameRus = @NameRus, NameKaz = @NameKaz, KATOCode = @KATOCode, IsActual = @IsActual, ModifyDate = @ModifyDate, RCACode = @RCACode
                    WHERE Id=@Id
                    END
                    ELSE
                    BEGIN
                    INSERT INTO AddressATEs(Id, ParentId, ATETypeId, FullPathRus, FullPathKaz, NameRus, NameKaz, KATOCode, IsActual, ModifyDate, RCACode)
                    VALUES(@Id, @ParentId, @ATETypeId, @FullPathRus, @FullPathKaz, @NameRus, @NameKaz, @KATOCode, @IsActual, @ModifyDate, @RCACode)
                    END", entities, UnitOfWork.Transaction);
                                    transaction.Commit();
            }
        }

        public void Update(AddressATE entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE AddressATEs SET ParentId = @ParentId, ATETypeId = @ATETypeId, FullPathRus = @FullPathRus, FullPathKaz = @FullPathKaz, NameRus = @NameRus, NameKaz = @NameKaz, KATOCode = @KATOCode, IsActual = @IsActual, ModifyDate = @ModifyDate, RCACode = @RCACode
                    WHERE Id=@id", entity,UnitOfWork.Transaction);
                                    transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public AddressATE Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<AddressATE>(@"
                    SELECT * 
                    FROM AddressATEs
                    WHERE Id=@id", new { id }, UnitOfWork.Transaction);
        }

        public AddressATE Get(string katoCode)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<AddressATE>(@"
                    SELECT * 
                    FROM AddressATEs
                    WHERE katoCode=@katoCode", new { katoCode }, UnitOfWork.Transaction);
        }

        public AddressATE GetByParentId(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<AddressATE>(@"
                    SELECT * 
                    FROM AddressATEs
                    WHERE ParentId=@id", new { id }, UnitOfWork.Transaction);
        }

        public AddressATE GetByNameInChilds(string name, string parentKatoCode, bool isKazName = true)
        {
            if(isKazName)
            {
                return UnitOfWork.Session.QuerySingleOrDefault<AddressATE>(@"
                    SELECT ateChild.* 
                    FROM AddressATEs ateParent
                    left join AddressATEs ateChild on ateChild.ParentId = ateParent.Id
                    where ateChild.Id is not null
                    and ateParent.KATOCode = @parentKatoCode
                    and ateChild.NameKaz = @name", new { name, parentKatoCode }, UnitOfWork.Transaction);
            }
            else
            {
                return UnitOfWork.Session.QuerySingleOrDefault<AddressATE>(@"
                    SELECT ateChild.* 
                    FROM AddressATEs ateParent
                    left join AddressATEs ateChild on ateChild.ParentId = ateParent.Id
                    where ateChild.Id is not null
                    and ateParent.KATOCode = @parentKatoCode
                    and ateChild.NameRus = @name", new { name, parentKatoCode }, UnitOfWork.Transaction);
            }
        }

        public AddressATE Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<AddressATE> List(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.Query<AddressATE>(@"SELECT * FROM AddressATEs Where  isActual = 1", UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"SELECT COUNT(*) FROM AddressATEs Where  isActual = 1", UnitOfWork.Transaction);
        }

        public DateTime GetLastModifiedDate()
        {
            return UnitOfWork.Session.ExecuteScalar<DateTime>(@"SELECT ISNULL(MAX(ModifyDate),'1970-01-01') FROM AddressATEs Where  isActual = 1", UnitOfWork.Transaction);
        }

        public string GetKatoCodeOfRegion(int ateId)
        {
            return UnitOfWork.Session.ExecuteScalar<string>(@"
                WITH allRows AS (
                    SELECT Id, KATOCode, ParentId
                    FROM [AddressATEs]
                    WHERE Id = @ateId
                    UNION ALL
                    SELECT a1.ID,a1.KATOCode, a1.ParentId
                    FROM [AddressATEs] a1
                    JOIN allRows a2 on a2.ParentId = a1.Id
                )   

                SELECT KATOCode FROM allRows
                WHERE ParentId = 1", new { ateId }, UnitOfWork.Transaction);
        }
    }
}
