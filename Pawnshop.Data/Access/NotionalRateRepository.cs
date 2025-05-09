using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Data.Access
{
    public class NotionalRateRepository : RepositoryBase, IRepository<NotionalRate>
    {
        public NotionalRateRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty);
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "RateTypeId",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                SELECT COUNT(*)
                FROM NotionalRates
                {condition} {order} {page}",
                new
                {
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter
                }, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("DELETE FROM NotionalRates WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public NotionalRate Find(object query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var notionalRateDate = query?.Val<string>("Date");

            var notionalRate = UnitOfWork.Session.Query<NotionalRate>(@$"
                    SELECT * FROM NotionalRates
                    WHERE Date <= @notionalRateDate",
                    new { notionalRateDate }, UnitOfWork.Transaction).FirstOrDefault();

            return notionalRate;
        }

        public NotionalRate Get(int id)
        {
            var notionalRate = UnitOfWork.Session.QuerySingleOrDefault<NotionalRate>(@"
                SELECT *
                FROM NotionalRates
                WHERE Id = @id", new { id }, UnitOfWork.Transaction);

            if (notionalRate is null)
                throw new PawnshopApplicationException($"NotionalRates с Id = {id} не найден");

            return notionalRate;
        }

        public NotionalRate GetByTypeId(int typeId)
        {
            var notionalRate = UnitOfWork.Session.QuerySingleOrDefault<NotionalRate>(@"
                SELECT *
                FROM NotionalRates
                WHERE RateTypeId = @typeId", new { typeId }, UnitOfWork.Transaction);

            if (notionalRate is null)
                throw new PawnshopApplicationException($"NotionalRates для RateTypeId = {typeId} не найден");

            return notionalRate;
        }

        public NotionalRate GetByTypeOfLastYear(int typeId)
        {
            var notionalRate = UnitOfWork.Session.QueryFirstOrDefault<NotionalRate>(@"
                  SELECT *
	                FROM NotionalRates
	                WHERE RateTypeId = @typeId
	                AND YEAR(Date) = YEAR(dbo.GETASTANADATE())", new { typeId }, UnitOfWork.Transaction);

            return notionalRate;
        }

        public async Task<NotionalRate> GetByTypeOfLastYearAsync(int typeId)
        {
            var notionalRate = await UnitOfWork.Session.QueryFirstOrDefaultAsync<NotionalRate>(@"
                  SELECT *
	                FROM NotionalRates
	                WHERE RateTypeId = @typeId
	                AND YEAR(Date) = YEAR(dbo.GETASTANADATE())", new { typeId }, UnitOfWork.Transaction);

            return notionalRate;
        }

        public void Insert(NotionalRate entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO NotionalRates ( RateTypeId, Date, RateValue, AuthorId, CreateDate )
                    VALUES ( @RateTypeId, @Date, @RateValue, @AuthorId, @CreateDate )
                    SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<NotionalRate> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty);
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "RateTypeId",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<NotionalRate>($@"
                SELECT *
                FROM NotionalRates
                {condition} {order} {page}",
                new
                {
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter
                }, UnitOfWork.Transaction).ToList();
        }

        public void Update(NotionalRate entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE NotionalRates
                    SET RateTypeId = @RateTypeId, Date = @Date, RateValue = @RateValue, AuthorId = @AuthorId
                    WHERE Id = @Id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
    }
}
