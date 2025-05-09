using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Clients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pawnshop.Data.Access
{
    public class ClientIncomeCalculationSettingRepository : RepositoryBase, IRepository<ClientIncomeCalculationSetting>
    {
        public ClientIncomeCalculationSettingRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty);
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "DocumentTypeId",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                SELECT COUNT(*)
                FROM ClientIncomeCalculationSettings
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
            throw new NotImplementedException();
        }

        public ClientIncomeCalculationSetting Find(object query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var documentTypeId = query?.Val<int>("DocumentTypeId");

            var incomeCalculationRate = UnitOfWork.Session.Query<ClientIncomeCalculationSetting>(@$"
                    SELECT * FROM ClientIncomeCalculationSettings
                    WHERE DocumentTypeId = @documentTypeId",
                    new { documentTypeId }, UnitOfWork.Transaction).FirstOrDefault();

            if (incomeCalculationRate is null)
                throw new PawnshopApplicationException($"ClientIncomeCalculationSetting для DocumentTypeId = {documentTypeId} не найден");

            return incomeCalculationRate;
        }

        public ClientIncomeCalculationSetting Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<ClientIncomeCalculationSetting>(@"
                SELECT *
                FROM ClientIncomeCalculationSettings
                WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public void Insert(ClientIncomeCalculationSetting entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO ClientIncomeCalculationSettings ( DocumentTypeId, Rate )
                    VALUES ( @DocumentTypeId, @Rate )
                    SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<ClientIncomeCalculationSetting> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var condition = listQuery.Like(string.Empty);
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "DocumentTypeId",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<ClientIncomeCalculationSetting>($@"
                SELECT *
                FROM ClientIncomeCalculationSettings
                {condition} {order} {page}",
                new
                {
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter
                }, UnitOfWork.Transaction).ToList();
        }

        public void Update(ClientIncomeCalculationSetting entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ClientIncomeCalculationSettings
                    SET DocumentTypeId = @DocumentTypeId, Rate = @Rate
                    WHERE Id = @Id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
    }
}
