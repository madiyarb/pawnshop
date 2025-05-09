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
    public class UKassaSectionsRepository : RepositoryBase, IRepository<UKassaSection>
    {
        public UKassaSectionsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            UnitOfWork.Session.Execute(@"
                UPDATE UKassaSections
                    SET DeleteDate = dbo.GETASTANADATE()
                         WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public UKassaSection Find(object query)
        {
            throw new NotImplementedException();
        }

        public UKassaSection Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<UKassaSection>(@"
                SELECT Id, Name, NameAlt, CreateDate, DeleteDate, AuthorId
                    FROM UKassaSections
                        WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public void Insert(UKassaSection entity)
        {
            entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
            IF NOT EXISTS (SELECT Id FROM UKassaSections WHERE Name = @Name and NameAlt = @NameAlt)
            BEGIN
                INSERT INTO UKassaSections ( Name, NameAlt, CreateDate, DeleteDate, AuthorId )
                VALUES ( @Name, @NameAlt, dbo.GETASTANADATE(), null , @AuthorId);
            SELECT SCOPE_IDENTITY();
            END
            ELSE SELECT 0", entity, UnitOfWork.Transaction);
        }

        public List<UKassaSection> List(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.Query<UKassaSection>(@"
                SELECT Id, Name, NameAlt, CreateDate, DeleteDate
                    FROM UKassaSections", UnitOfWork.Transaction).ToList();
        }

        public void Update(UKassaSection entity)
        {
            UnitOfWork.Session.Execute(@"
                UPDATE UKassaSections
                    SET Name = @Name, NameAlt = @NameAlt
                        WHERE Id = @Id", entity, UnitOfWork.Transaction);
        }
    }
}
