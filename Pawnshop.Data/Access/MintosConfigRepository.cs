using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Investments;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Mintos;

namespace Pawnshop.Data.Access
{
    public class MintosConfigRepository : RepositoryBase, IRepository<MintosConfig>
    {
        public MintosConfigRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(MintosConfig entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO MintosConfigs ( CurrencyId, OrganizationId, NewUploadAllowed, ApiKey, InvestorInterestRate, DeleteDate )
VALUES ( @CurrencyId, @OrganizationId, @NewUploadAllowed, @ApiKey, @InvestorInterestRate, @DeleteDate )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(MintosConfig entity)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE MintosConfigs SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public MintosConfig Get(int id)
        {
            return UnitOfWork.Session.Query<MintosConfig, Currency, Organization, MintosConfig>(@"
SELECT mc.*, c.*, o.*
FROM MintosConfigs mc
JOIN Currencies c ON mc.CurrencyId = c.Id
JOIN Organizations o ON o.Id=mc.OrganizationId
WHERE mc.Id = @id", (mc, c, o) => {
                mc.Currency = c;
                mc.Organization = o;
                return mc;
            }, new { id }).FirstOrDefault();
        }

        public MintosConfig Find(object query)
        {
            var organizationId = query?.Val<int?>("OrganizationId");
            var currencyId = query?.Val<int?>("CurrencyId");


            return UnitOfWork.Session.Query<MintosConfig, Currency, Organization, MintosConfig>($@"
SELECT mc.*, c.*, o.*
FROM MintosConfigs mc
JOIN Currencies c ON mc.CurrencyId = c.Id
JOIN Organizations o ON o.Id=mc.OrganizationId
WHERE mc.OrganizationId = @organizationId
AND mc.CurrencyId = @currencyId
", (mc, c, o) =>
            {
                mc.Currency = c;
                mc.Organization = o;
                return mc;
            }, new
            {
                organizationId,
                currencyId
            }).ToList().FirstOrDefault();
        }

        public List<MintosConfig> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "mc.DeleteDate IS NULL";

            var condition = listQuery.Like(pre);
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<MintosConfig, Currency, Organization, MintosConfig>($@"
SELECT mc.*, c.*, o.*
FROM MintosConfigs mc
JOIN Currencies c ON mc.CurrencyId = c.Id
JOIN Organizations o ON o.Id=mc.OrganizationId
{condition} {page}", (mc, c, o) =>
            {
                mc.Currency = c;
                mc.Organization = o;
                return mc;
            }, new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "mc.DeleteDate IS NULL";

            var condition = listQuery.Like(pre);
            var page = listQuery.Page();

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM MintosConfigs mc
JOIN Currencies c ON mc.CurrencyId = c.Id
JOIN Organizations o ON o.Id=mc.OrganizationId
{condition}", new
            {
                listQuery.Filter
            });
        }
    }
}
