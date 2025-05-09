using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Dapper;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.CashOrders;

namespace Pawnshop.Data.Access
{
    public class CashOrderNumberCounterRepository : RepositoryBase, IRepository<CashOrderNumberCounter>
    {
        private static int cashInCounter;
        public CashOrderNumberCounterRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(CashOrderNumberCounter entity)
        {
                UnitOfWork.Session.Execute(@"
INSERT INTO CashOrderNumberCounters ( OrderType, Year, BranchId, Counter )
VALUES ( @OrderType, @Year, @BranchId, @Counter )", entity, UnitOfWork.Transaction);
        }

        public void Update(CashOrderNumberCounter entity)
        {
            if (entity.OrderType == OrderType.CashIn)
            {
                if (cashInCounter != entity.Counter)
                    cashInCounter = entity.Counter;
                else
                    entity.Counter = ++cashInCounter;
            }
            UnitOfWork.Session.Execute(@"
UPDATE CashOrderNumberCounters
SET Counter = @Counter
WHERE Id=@Id", entity, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            throw new System.NotImplementedException();
        }

        public CashOrderNumberCounter Get(int id)
        {
            throw new System.NotImplementedException();
        }

        public CashOrderNumberCounter Find(object query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var orderType = query.Val<OrderType?>("OrderType");
            var year = query.Val<int?>("Year");
            var branchId = query.Val<int>("BranchId");
            var condition = "WHERE OrderType = @OrderType AND Year = @year AND BranchId = @branchId";

            return UnitOfWork.Session.Query<CashOrderNumberCounter>($@"
SELECT *
FROM CashOrderNumberCounters
{condition}", new
            {
                orderType,
                year,
                branchId
            }, UnitOfWork.Transaction).FirstOrDefault();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public string Next(OrderType orderType, int year, int branch, string code)
        {
            CashOrderNumberCounter counter = Find(new
            {
                OrderType = orderType,
                Year = year,
                BranchId = branch
            });

            if (counter != null)
            {
                counter.Counter++;
                Update(counter);
            }
            else
            {
                counter = new CashOrderNumberCounter
                {
                    OrderType = orderType,
                    Year = year,
                    BranchId = branch,
                    Counter = 0,
                };
                counter.Counter++;
                Insert(counter);
            }

            return $"{counter.Year.ToString().Substring(2, 2)}{code}{counter.Counter:D4}";
        }

        public List<CashOrderNumberCounter> List(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }
    }
}