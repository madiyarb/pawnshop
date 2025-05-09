using System;
using System.Collections.Generic;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.PayOperations;

namespace Pawnshop.Data.Access
{
    public class PayOperationNumberCounterRepository : RepositoryBase, IRepository<PayOperationNumberCounter>
    {
        public PayOperationNumberCounterRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(PayOperationNumberCounter entity)
        {
            throw new System.NotImplementedException();
        }

        public void Update(PayOperationNumberCounter entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
IF NOT EXISTS (SELECT Id FROM PayOperationNumberCounters WHERE PayTypeId = @PayTypeId AND Year = @Year AND BranchId = @BranchId)
BEGIN
    INSERT INTO PayOperationNumberCounters ( PayTypeId, Year, BranchId, Counter )
    VALUES ( @PayTypeId, @Year, @BranchId, @Counter )
END
ELSE
BEGIN
    UPDATE PayOperationNumberCounters
    SET Counter = @Counter
    WHERE PayTypeId = @PayTypeId AND Year = @Year AND BranchId = @BranchId
END", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            throw new System.NotImplementedException();
        }

        public PayOperationNumberCounter Get(int id)
        {
            throw new System.NotImplementedException();
        }

        public PayOperationNumberCounter Find(object query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var year = query.Val<int?>("Year");
            var branchId = query.Val<int>("BranchId");
            var payTypeId = query.Val<int>("PayTypeId");
            var condition = "WHERE PayTypeId = @payTypeId AND Year = @year AND BranchId = @branchId";

            return UnitOfWork.Session.QuerySingleOrDefault<PayOperationNumberCounter>($@"
SELECT *
FROM PayOperationNumberCounters
{condition}", new
            {
                year,
                branchId,
                payTypeId
            }, UnitOfWork.Transaction);
        }

        public string Next(int payType , int year, int branch, string code, string branchCode)
        {
            var counter = Find(new
            {
                Year = year,
                BranchId = branch,
                PayTypeId = payType
            }) ?? new PayOperationNumberCounter
            {
                PayTypeId = payType,
                Year = year,
                BranchId = branch,
                Counter = 0,
            };
            counter.Counter++;
            Update(counter);

            return $"{counter.Year.ToString().Substring(2, 2)}{(string.IsNullOrEmpty(branchCode) ? "NAN" : branchCode)}-{(string.IsNullOrEmpty(code) ? "__" : code)}{counter.Counter:D4}";
        }

        public List<PayOperationNumberCounter> List(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new System.NotImplementedException();
        }
    }
}