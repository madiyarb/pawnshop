using Pawnshop.Core.Impl;
using Pawnshop.Core;
using Pawnshop.Data.Models.LoanSettings;
using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core.Queries;
using Dapper;
using System.Linq;
using Pawnshop.Data.Models.Domains;

namespace Pawnshop.Data.Access
{
    public class LoanSettingProductTypeLTVRepository : RepositoryBase, IRepository<LoanSettingProductTypeLTV>
    {
        public LoanSettingProductTypeLTVRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            using(var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("UPDATE LoanSettingProductTypeLTV SET DeleteDate = dbo.GETASTANADATE() WHERE Id=@id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public LoanSettingProductTypeLTV Find(object query)
        {
            throw new NotImplementedException();
        }

        public LoanSettingProductTypeLTV Get(int id)
        {
            return UnitOfWork.Session.Query<LoanSettingProductTypeLTV, DomainValue, LoanSettingProductTypeLTV>($@"
SELECT lsptLTV.*, dv.* 
FROM LoanSettingProductTypeLTV lsptLTV
JOIN DomainValues dv ON dv.Id = lsptLTV.SubCollateralTypeId
WHERE lsptLTV.Id = @id
AND lsptLTV.DeleteDate IS NULL
AND dv.DeleteDate IS NULL", (lsptLTV, dv) =>
            {
                lsptLTV.SubCollateralType = dv;
                return lsptLTV;
            },new {id}, UnitOfWork.Transaction).FirstOrDefault();
        }

        public void Insert(LoanSettingProductTypeLTV entity)
        {
            using(var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO LoanSettingProductTypeLTV 
(LoanSettingId, CollateralType, SubCollateralTypeId, LTV, BeginDate, AuthorId, CreateDate)
VALUES
(@LoanSettingId, @CollateralType, @SubCollateralTypeId, @LTV, @BeginDate, @AuthorId, @CreateDate)
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<LoanSettingProductTypeLTV> List(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Update(LoanSettingProductTypeLTV entity)
        {
            throw new NotImplementedException();
        }

        public List<LoanSettingProductTypeLTV> ListForLoanSetting(int loanSettingId)
        {
            var result = UnitOfWork.Session.Query<LoanSettingProductTypeLTV, DomainValue, LoanSettingProductTypeLTV>($@"
SELECT lsptLTV.*, dv.* 
FROM LoanSettingProductTypeLTV lsptLTV
JOIN DomainValues dv ON dv.Id = lsptLTV.SubCollateralTypeId
WHERE LoanSettingId = @loanSettingId
AND lsptLTV.DeleteDate IS NULL
AND dv.DeleteDate IS NULL", (lsptLTV, dv) =>
            {
                lsptLTV.SubCollateralType = dv;
                return lsptLTV;
            }, new { loanSettingId }, UnitOfWork.Transaction).ToList();
            return result;
        }
    }
}
