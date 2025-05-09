using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.UKassa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pawnshop.Data.Access
{
    public class UKassaKassasRepository : RepositoryBase, IRepository<UKassaKassa>
    {

        public UKassaKassasRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            UnitOfWork.Session.Execute(@"
                UPDATE UKassaKassas
                    SET DeleteDate = dbo.GETASTANADATE()
                         WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public UKassaKassa Find(object query)
        {
            throw new NotImplementedException();
        }

        public UKassaKassa Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<UKassaKassa>(@"
                SELECT Id, SectionId, Name, NameAlt, CreateDate, DeleteDate, AuthorId
                    FROM UKassaKassas
                        WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public void Insert(UKassaKassa entity)
        {
            entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
            IF NOT EXISTS (SELECT Id FROM UKassaKassas WHERE SectionId = @SectionId and Name = @Name and NameAlt = @NameAlt)
            BEGIN
                INSERT INTO UKassaKassas ( Name, SectionId, NameAlt, CreateDate, DeleteDate, AuthorId )
                VALUES ( @SectionId, @Name, @NameAlt, dbo.GETASTANADATE(), null , @AuthorId);
            SELECT SCOPE_IDENTITY();
            END
            ELSE SELECT 0", entity, UnitOfWork.Transaction);
        }

        public List<UKassaKassa> List(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.Query<UKassaKassa>(@"
                SELECT Id, SectionId, Name, NameAlt, CreateDate, DeleteDate
                    FROM UKassaKassas", UnitOfWork.Transaction).ToList();
        }

        public void Update(UKassaKassa entity)
        {
            UnitOfWork.Session.Execute(@"
                UPDATE UKassaKassas
                    SET SectionId = @SectionId, Name = @Name, NameAlt = @NameAlt
                        WHERE Id = @Id", entity, UnitOfWork.Transaction);
        }
    }
}
