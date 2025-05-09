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
    public class UKassaBOSettingsRepository : RepositoryBase, IRepository<UKassaBOSettings>
    {
        public UKassaBOSettingsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }
        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            UnitOfWork.Session.Execute(@"
                UPDATE UKassaBOSettings
                    SET DeleteDate = dbo.GETASTANADATE()
                         WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public UKassaBOSettings Find(object query)
        {
            throw new NotImplementedException();
        }

        public UKassaBOSettings Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<UKassaBOSettings>(@"
                SELECT Id, BusinessOperationSettingId, NomenclatureId, CheckOperationType, CheckStornoOperationType,
                        CashOperationType, CashStornoOperationType, PaymentType, CreateDate, DeleteDate, AuthorId, IsDebit
                    FROM UKassaBOSettings
                        WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public UKassaBOSettings GetByBOId(int BusinessOperationSettingId)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<UKassaBOSettings>(@"
                SELECT Id, BusinessOperationSettingId, NomenclatureId, CheckOperationType, CheckStornoOperationType,
                        CashOperationType, CashStornoOperationType, PaymentType, CreateDate, DeleteDate, AuthorId, IsDebit
                    FROM UKassaBOSettings
                        WHERE BusinessOperationSettingId = @BusinessOperationSettingId", new { BusinessOperationSettingId }, UnitOfWork.Transaction);
        }

        public void Insert(UKassaBOSettings entity)
        {
            entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
            IF NOT EXISTS (SELECT Id FROM UKassaBOSettings WHERE BusinessOperationSettingId = @BusinessOperationSettingId and NomenclatureId = @NomenclatureId and 
                CheckOperationType = @CheckOperationType and CheckStornoOperationType = @CheckStornoOperationType and
                CashOperationType = @CashOperationType and CashStornoOperationType = @CashStornoOperationType and
                PaymentType = @PaymentType)
            BEGIN
                INSERT INTO UKassaBOSettings ( BusinessOperationSettingId, NomenclatureId, CheckOperationType, CheckStornoOperationType,
                        CashOperationType, CashStornoOperationType, PaymentType, CreateDate, DeleteDate, AuthorId, IsDebit )
                VALUES ( @BusinessOperationSettingId, @NomenclatureId, @CheckOperationType @CheckStornoOperationType,
                        @CashOperationType, @CashStornoOperationType, @PaymentType, dbo.GETASTANADATE(), null , @AuthorId, @IsDebit);
            SELECT SCOPE_IDENTITY();
            END
            ELSE SELECT 0", entity, UnitOfWork.Transaction);
        }

        public List<UKassaBOSettings> List(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.Query<UKassaBOSettings>(@"
                SELECT Id, BusinessOperationSettingId, NomenclatureId, CheckOperationType, CheckStornoOperationType,
                        CashOperationType, CashStornoOperationType, PaymentType, CreateDate, DeleteDate, AuthorId, IsDebit
                    FROM UKassaBOSettings", UnitOfWork.Transaction).ToList();
        }

        public void Update(UKassaBOSettings entity)
        {
            UnitOfWork.Session.Execute(@"
                UPDATE UKassaKassas
                    SET BusinessOperationSettingId = @BusinessOperationSettingId, NomenclatureId = @NomenclatureId, 
                        CheckOperationType = @CheckOperationType, CheckStornoOperationType = @CheckStornoOperationType, 
                        CashOperationType = @CashOperationType, CashStornoOperationType = @CashStornoOperationType,
                        PaymentType = @PaymentType, IsDebit = @IsDebit
                        WHERE Id = @Id", entity, UnitOfWork.Transaction);
        }
    }
}
