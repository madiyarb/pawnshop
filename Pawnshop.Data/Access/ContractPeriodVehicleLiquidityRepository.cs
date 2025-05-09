using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Access
{
    public class ContractPeriodVehicleLiquidityRepository : RepositoryBase, IRepository<ContractPeriodVehicleLiquidity>
    {
        public ContractPeriodVehicleLiquidityRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ContractPeriodVehicleLiquidity entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.CreateDate = DateTime.Now;
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO ContractPeriodVehicleLiquid ( Age, LiquidValue, MaxMonthsCount, AuthorId, CreateDate)
                    VALUES ( @Age, @LiquidValue, @MaxMonthsCount, @AuthorId, @CreateDate )
                    SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(ContractPeriodVehicleLiquidity entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ContractPeriodVehicleLiquid
                    SET Age = @Age, LiquidValue = @LiquidValue, MaxMonthsCount = @MaxMonthsCount
                    WHERE Id = @Id",
                    entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute("DELETE FROM ContractPeriodVehicleLiquid WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ContractPeriodVehicleLiquidity Get(int id)
        {
            return UnitOfWork.Session.Query<ContractPeriodVehicleLiquidity>(@"
            SELECT *
            FROM ContractPeriodVehicleLiquid
            WHERE Id = @id",
            new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public ContractPeriodVehicleLiquidity Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<ContractPeriodVehicleLiquidity> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "DeleteDate IS NULL";

            var condition = listQuery.Like(pre, "Age");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "LiquidValue",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<ContractPeriodVehicleLiquidity>($@"
                SELECT * FROM ContractPeriodVehicleLiquid
                {condition} {order} {page}",
                new
                {
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter
                }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "DeleteDate IS NULL";

            var condition = listQuery.Like(pre, "Age");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                SELECT COUNT(*)
                FROM ContractPeriodVehicleLiquid
                {condition}",
                new
                {
                    listQuery.Filter
                });
        }

        public ContractPeriodVehicleLiquidity GetPeriod(int releaseYear, int liquidValue)
        {
            if (liquidValue == 0) return null;

            var list = UnitOfWork.Session.Query<ContractPeriodVehicleLiquidity>($@"
                SELECT * FROM ContractPeriodVehicleLiquid
                WHERE DeleteDate IS NULL AND LiquidValue = @liquidValue",
                new { liquidValue }, UnitOfWork.Transaction).ToList();
            
            if (list.Count == 0) return null;

            var orderedList = list.OrderByDescending(x => x.MaxMonthsCount).ToList();
            ContractPeriodVehicleLiquidity contractPeriodVehicleLiquidity = null;
            bool findPeriod = false;
            orderedList.ForEach(x =>
            {
                int carAge = DateTime.Now.Year - releaseYear;
                if (!findPeriod && carAge <= x.Age)
                {
                    contractPeriodVehicleLiquidity = x;
                    findPeriod = true;
                };
            });
            if (contractPeriodVehicleLiquidity is null)
                contractPeriodVehicleLiquidity = orderedList.LastOrDefault();

            return contractPeriodVehicleLiquidity;
        }
    }
}