using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Access
{
    public class VehicleCountryCodeRepository : RepositoryBase, IRepository<VehicleCountryCode>
    {
        public VehicleCountryCodeRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }        

        public void Insert(VehicleCountryCode entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO VehicleCountryCodes ( Code, CountryId )
VALUES ( @Code, @CountryId )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(VehicleCountryCode entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE VehicleCountryCodes
SET Code = @Code, CountryId = @CountryId
WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE VehicleCountryCodes SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        
        public VehicleCountryCode Get(int id)
        {
            return UnitOfWork.Session.Query<VehicleCountryCode, Country, VehicleCountryCode>(@"
SELECT v.*, c.* 
FROM VehicleCountryCodes v
LEFT JOIN Countries c ON c.Id = v.CountryId
WHERE v.Id=@id", (v, c) =>
            {
                v.Country = c;
                return v;
            }, new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }        

        public VehicleCountryCode Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<VehicleCountryCode> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "v.DeleteDate IS NULL";

            var condition = listQuery.Like(pre, "v.Code","c.NameRus");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "v.Code",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<VehicleCountryCode, Country, VehicleCountryCode>($@"
SELECT v.*, c.*
FROM VehicleCountryCodes v LEFT JOIN Countries c on c.Id=v.CountryId
{condition} {order} {page}", 
            (v, c) =>
            {
                v.Country = c;
                return v;
            },
            new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty, "v.Code", "c.NameRus");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM VehicleCountryCodes v LEFT JOIN Countries c on c.Id=v.CountryId
{condition}", new
            {
                listQuery.Filter
            });
        }

        public int RelationCount(int countryId)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
SELECT COUNT(*)
FROM VehicleWMIs
WHERE VehicleCountryCodeId = @countryId AND DeleteDate IS NULL", new { countryId = countryId });
        }
    }
}