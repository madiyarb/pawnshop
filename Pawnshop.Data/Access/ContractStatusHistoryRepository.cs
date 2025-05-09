using Dapper;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Membership;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Data.Access
{
    public class ContractStatusHistoryRepository : RepositoryBase, IRepository<ContractStatusHistory>
    {
        public ContractStatusHistoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            throw new NotImplementedException();
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE ContractStatusHistory SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public ContractStatusHistory Find(object query)
        {
            throw new NotImplementedException();
        }

        public ContractStatusHistory Get(int id)
        {
            var entity = UnitOfWork.Session.Query<ContractStatusHistory>(@"
SELECT csh.*
FROM ContractStatusHistory csh
WHERE csh.Id = @id
AND csh.DeleteDate IS NULL
ORDER BY csh.CreateDate DESC", new { id }, UnitOfWork.Transaction).FirstOrDefault();

            return entity;
        }

        public void Insert(ContractStatusHistory entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO ContractStatusHistory ( ContractId, Status, UserId, Date, AuthorId, CreateDate ) VALUES ( @ContractId, @Status, @UserId, @Date, @AuthorId, @CreateDate )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<ContractStatusHistory> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null)
                throw new ArgumentNullException(nameof(listQuery));

            var status = query?.Val<OrderType?>("Status");
            var date = query?.Val<DateTime?>("Date");
            var userId = query?.Val<int?>("UserId");
            var contractId = query?.Val<int?>("ContractId");

            var pre = " WHERE csh.DeleteDate IS NULL";
            pre += status.HasValue ? " AND csh.Status = @status" : string.Empty;
            pre += date.HasValue ? " AND csh.Date >= @date" : string.Empty;
            pre += userId.HasValue ? " AND csh.UserId = @userId" : string.Empty;
            pre += contractId.HasValue ? " AND csh.ContractId = @contractId" : string.Empty;

            var condition = pre;

            var list = UnitOfWork.Session.Query<ContractStatusHistory>($@"
SELECT csh.*
FROM ContractStatusSchedule csh
{condition}
ORDER BY csh.Date DESC",
                new
                {
                    status,
                    date,
                    userId,
                    contractId
                },
                UnitOfWork.Transaction).ToList();
            return list;
        }

        public void Update(ContractStatusHistory entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE ContractStatusHistory SET ContractId = @contractId, Status = @Status, UserId = @UserId, Date = @Date
WHERE Id = @id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public async Task<ContractStatusHistory> GetLastStatusHistoryItemForContract(int contractId)
        {
            var entity = UnitOfWork.Session.Query<ContractStatusHistory>(@"
SELECT TOP 1 csh.*
FROM ContractStatusHistory csh
WHERE csh.ContractId = @contractId
AND csh.DeleteDate IS NULL
ORDER BY csh.CreateDate DESC", new { contractId }, UnitOfWork.Transaction).FirstOrDefault();

            return entity;
        }
    }
}
