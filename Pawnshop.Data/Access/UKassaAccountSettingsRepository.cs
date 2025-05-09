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
    public class UKassaAccountSettingsRepository : RepositoryBase, IRepository<UKassaAccountSettings>
    {
        public UKassaAccountSettingsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            UnitOfWork.Session.Execute(@"
                UPDATE UKassaAccountSettings
                    SET DeleteDate = dbo.GETASTANADATE()
                         WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public UKassaAccountSettings Find(object query)
        {
            throw new NotImplementedException();
        }

        public UKassaAccountSettings GetByAccountId(int accountId)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<UKassaAccountSettings>(@"
                SELECT Id, AccountId, KassaId, SectionId, CreateDate, DeleteDate, AuthorId
                    FROM UKassaAccountSettings
                        WHERE AccountId = @accountId and DeleteDate is null", new { accountId }, UnitOfWork.Transaction);
        }

        public UKassaAccountSettings Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<UKassaAccountSettings>(@"
                SELECT Id, AccountId, KassaId, SectionId, CreateDate, DeleteDate, AuthorId
                    FROM UKassaAccountSettings
                        WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public void Insert(UKassaAccountSettings entity)
        {
            entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
            IF NOT EXISTS (SELECT Id FROM UKassaAccountSettings WHERE AccountId = @AccountId)
            BEGIN
                INSERT INTO UKassaAccountSettings ( AccountId, KassaId, SectionId, CreateDate, DeleteDate, AuthorId )
                VALUES ( @AccountId, @KassaId, @SectionId, dbo.GETASTANADATE(), null , @AuthorId);
            SELECT SCOPE_IDENTITY();
            END
            ELSE SELECT 0", entity, UnitOfWork.Transaction);
        }

        public List<UKassaAccountSettings> List(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.Query<UKassaAccountSettings>(@"
                SELECT Id, AccountId, KassaId, SectionId, CreateDate, DeleteDate, AuthorId
                    FROM UKassaAccountSettings", UnitOfWork.Transaction).ToList();
        }

        public void Update(UKassaAccountSettings entity)
        {
            UnitOfWork.Session.Execute(@"
                UPDATE UKassaAccountSettings
                    SET AccountId = @AccountId, KassaId = @KassaId, SectionId = @SectionId
                        WHERE Id = @Id", entity, UnitOfWork.Transaction);
        }
    }
}
