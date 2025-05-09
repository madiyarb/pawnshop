using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Access
{
    public class CountryRepository : RepositoryBase, IRepository<Country>
    {
        public CountryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(Country entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO Countries (Code, NameRus, NameKaz, CBId)
VALUES(@Code, @NameRus, @NameKaz, @CBId)
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(Country entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE Countries SET Code = @Code, NameRus=@NameRus, NameKaz=@NameKaz, CBId=@CBId
WHERE Id=@id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Countries SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public Country Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<Country>(@"
SELECT * 
FROM Countries
WHERE Id=@id", new { id }, UnitOfWork.Transaction);
        }

        public Country Find(object query)
        {
            if (query == null) 
                throw new ArgumentNullException(nameof(query));

            var code = query?.Val<string>("Code");

            return UnitOfWork.Session.Query<Country>(@"
                    SELECT * FROM Countries
                    WHERE Code = @code",
                new { code }, UnitOfWork.Transaction).FirstOrDefault();
        }


        public List<Country> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) 
                throw new ArgumentNullException(nameof(listQuery));

            var pre = "DeleteDate IS NULL";

            var condition = listQuery.Like(pre, "NameRus");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "NameRus",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<Country>($@"
SELECT *
FROM Countries
{condition} {order} {page}", new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) 
                throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like("DeleteDate IS NULL", "NameRus");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM Countries
{condition}", new
            {
                listQuery.Filter
            }, UnitOfWork.Transaction);
        }

        public int RelationCount(int countryId)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"
SELECT SUM(c)
FROM (
SELECT COUNT(*) as c
FROM VehicleCountryCodes
WHERE CountryId = @countryId AND DeleteDate IS NULL
UNION ALL
SELECT COUNT(*) as c
FROM ClientAddresses 
WHERE CountryId = @countryId AND DeleteDate IS NULL
UNION ALL
SELECT COUNT(*) as c
FROM Clients 
WHERE CitizenshipId = @countryId AND DeleteDate IS NULL) as t", new { countryId = countryId });
        }
    }
}
