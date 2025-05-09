using Dapper;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.AccountingCore;
using Pawnshop.Data.Models.LoanSettings;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Pawnshop.Data.Access
{
    public class LoanSettingRateRepository : RepositoryBase, IRepository<LoanSettingRate>
    {
        public LoanSettingRateRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(LoanSettingRate entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
                    INSERT INTO LoanSettingRates(ProductSettingId, RateSettingId, Rate, CreateDate, AuthorId)
                        VALUES(@ProductSettingId, @RateSettingId, @Rate, @CreateDate, @AuthorId)
                            SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void InsertOrUpdate(List<LoanSettingRate> entities)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    IF EXISTS (SELECT * FROM LoanSettingRates WHERE Id = @Id)
                        BEGIN
                            UPDATE LoanSettingRates SET ProductSettingId = @ProductSettingId, RateSettingId = @RateSettingId, Rate = @Rate, [Index] = @Index
                            WHERE Id=@Id
                        END
                    ELSE
                        BEGIN
                            INSERT INTO LoanSettingRates(ProductSettingId, RateSettingId, Rate, CreateDate, AuthorId, [Index])
                                VALUES(@ProductSettingId, @RateSettingId, @Rate, @CreateDate, @AuthorId, @Index)
                        END", entities, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(LoanSettingRate entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE LoanSettingRates SET ProductSettingId = @ProductSettingId, RateSettingId = @RateSettingId, Rate = @Rate
                            WHERE Id = @id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public LoanSettingRate Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<LoanSettingRate>(@"
                SELECT * 
                    FROM LoanSettingRates
                        WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public LoanSettingRate Find(object query)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE LoanSettingRates SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<LoanSettingRate> List(ListQuery listQuery, object query = null)
        {
            var condition = "WHERE lsr.DeleteDate IS NULL";

            var settingId = query?.Val<int>("SettingId");

            if (settingId.HasValue)
                condition += " AND lsr.ProductSettingId = @settingId";

            return UnitOfWork.Session.Query<LoanSettingRate, AccountSetting, LoanSettingRate>($@"
SELECT lsr.*, acs.*
  FROM LoanSettingRates lsr
  JOIN AccountSettings acs ON acs.Id = lsr.RateSettingId {condition}",
                (lsr, acs) =>
                    {
                        if (lsr != null)
                            lsr.RateSetting = acs;
                        return lsr;
                    },
                new { settingId },
                UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            var condition = "WHERE DeleteDate IS NULL";

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                    SELECT COUNT(*) FROM LoanSettingRates {condition}",
                UnitOfWork.Transaction);
        }
    }
}