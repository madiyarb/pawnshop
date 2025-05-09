using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Access
{
    public class AnnuitySettingRepository : RepositoryBase, IRepository<AnnuitySetting>
    {
        public AnnuitySettingRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(AnnuitySetting entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO AnnuitySettings
( CollateralType, MinDayCount, MaxDayCount, CertainDay, CreatedBy, CreateDate )
VALUES ( @CollateralType, @MinDayCount, @MaxDayCount, @CertainDay, @CreatedBy, @CreateDate )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(AnnuitySetting entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE AnnuitySettings
SET CollateralType = @CollateralType, MinDayCount = @MinDayCount, 
MaxDayCount = @MaxDayCount, CertainDay = @CertainDay
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE AnnuitySettings SET DeleteDate=dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public AnnuitySetting Get(int id)
        {
            return UnitOfWork.Session.Query<AnnuitySetting>(@"
SELECT *
FROM AnnuitySettings 
WHERE Id = @id", new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public AnnuitySetting Find(object query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var collateralType = query?.Val<CollateralType>("CollateralType");
            
            if (!collateralType.HasValue) throw new ArgumentNullException(nameof(collateralType));

            return UnitOfWork.Session.Query<AnnuitySetting>(@"
SELECT *
FROM AnnuitySettings 
WHERE DeleteDate IS NULL
AND CollateralType = @collateralType",
                new
                {
                    collateralType,
                }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<AnnuitySetting> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));
            
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "CollateralType",
                Direction = SortDirection.Asc
            });

            return UnitOfWork.Session.Query<AnnuitySetting>($@"
SELECT *
FROM AnnuitySettings l
WHERE DeleteDate IS NULL
{order}", UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM AnnuitySettings
WHERE DeleteDate IS NULL
");
        }
    }
}