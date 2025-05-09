using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Access
{
    public class RemittanceRepository : RepositoryBase, IRepository<Remittance>
    {
        public RemittanceRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(Remittance entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO Remittances ( SendDate, SendBranchId, SendUserId, SendCost, SendOrderId, 
    ReceiveDate, ReceiveBranchId, ReceiveUserId, ReceiveOrderId, Status, Note, CreateDate, DeleteDate, ContractId, InnerNotificationId )
VALUES ( @SendDate, @SendBranchId, @SendUserId, @SendCost, @SendOrderId, 
    @ReceiveDate, @ReceiveBranchId, @ReceiveUserId, @ReceiveOrderId, @Status, @Note, @CreateDate, @DeleteDate, @ContractId, @InnerNotificationId )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        
        public async Task InsertAsync(Remittance entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                entity.Id = await UnitOfWork.Session.QuerySingleOrDefaultAsync<int>(@"
INSERT INTO Remittances ( SendDate, SendBranchId, SendUserId, SendCost, SendOrderId, 
    ReceiveDate, ReceiveBranchId, ReceiveUserId, ReceiveOrderId, Status, Note, CreateDate, DeleteDate, ContractId, InnerNotificationId )
VALUES ( @SendDate, @SendBranchId, @SendUserId, @SendCost, @SendOrderId, 
    @ReceiveDate, @ReceiveBranchId, @ReceiveUserId, @ReceiveOrderId, @Status, @Note, @CreateDate, @DeleteDate, @ContractId, @InnerNotificationId )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                
                transaction.Commit();
            }
        }

        public void Update(Remittance entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE Remittances
SET SendDate = @SendDate, SendBranchId = @SendBranchId, SendUserId = @SendUserId, SendCost = @SendCost, 
    SendOrderId = @SendOrderId, ReceiveDate = @ReceiveDate, ReceiveBranchId = @ReceiveBranchId, ReceiveUserId = @ReceiveUserId, 
    ReceiveOrderId = @ReceiveOrderId, Status = @Status, Note = @Note, CreateDate = @CreateDate, DeleteDate = @DeleteDate, ContractId = @ContractId,
    InnerNotificationId = @InnerNotificationId
WHERE Id = @Id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        
        public async Task UpdateAsync(Remittance entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                await UnitOfWork.Session.ExecuteAsync(@"
UPDATE Remittances
SET SendDate = @SendDate, SendBranchId = @SendBranchId, SendUserId = @SendUserId, SendCost = @SendCost, 
    SendOrderId = @SendOrderId, ReceiveDate = @ReceiveDate, ReceiveBranchId = @ReceiveBranchId, ReceiveUserId = @ReceiveUserId, 
    ReceiveOrderId = @ReceiveOrderId, Status = @Status, Note = @Note, CreateDate = @CreateDate, DeleteDate = @DeleteDate, ContractId = @ContractId,
    InnerNotificationId = @InnerNotificationId
WHERE Id = @Id", entity, UnitOfWork.Transaction);
                
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Remittances SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public Remittance Get(int id)
        {
            return UnitOfWork.Session.Query<Remittance, Group, User, Group, User, Remittance>(@"
SELECT TOP 1 r.*, sb.*, su.*, rb.*, ru.*
FROM Remittances r
JOIN Groups sb ON r.SendBranchId = sb.Id
JOIN Users su ON r.SendUserId = su.Id
JOIN Groups rb ON r.ReceiveBranchId = rb.Id
LEFT JOIN Users ru ON r.ReceiveUserId = ru.Id
WHERE r.Id = @id", (r, sb, su, rb, ru) => {
                r.SendBranch = sb;
                r.SendUser = su;
                r.ReceiveBranch = rb;
                r.ReceiveUser = ru;
                return r; 
            }, new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }
        
        public async Task<Remittance> GetAsync(int id)
        {
            var sqlQuery = @"
                SELECT TOP 1 r.*, sb.*, su.*, rb.*, ru.*
                FROM Remittances r
                JOIN Groups sb ON r.SendBranchId = sb.Id
                JOIN Users su ON r.SendUserId = su.Id
                JOIN Groups rb ON r.ReceiveBranchId = rb.Id
                LEFT JOIN Users ru ON r.ReceiveUserId = ru.Id
                WHERE r.Id = @id";

            var result = await UnitOfWork.Session.QueryAsync<Remittance, Group, User, Group, User, Remittance>(
                sqlQuery, 
                (r, sb, su, rb, ru) => 
                {
                    r.SendBranch = sb;
                    r.SendUser = su;
                    r.ReceiveBranch = rb;
                    r.ReceiveUser = ru;
                    return r;
                }, 
                new { id }, 
                UnitOfWork.Transaction);

            return result.FirstOrDefault();
        }

        public Remittance Find(object query)
        {
            throw new NotImplementedException();
        }

        public List<Remittance> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var incoming = query?.Val<bool?>("Incoming");
            var branchId = query?.Val<int?>("BranchId");
            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var status = query?.Val<RemittanceStatusType?>("Status");

            if (!incoming.HasValue) throw new ArgumentNullException(nameof(incoming));
            if (!branchId.HasValue) throw new ArgumentNullException(nameof(branchId));

            var condition = @"WHERE r.DeleteDate IS NULL";
            condition += incoming.Value ? " AND r.ReceiveBranchId = @branchId" : " AND r.SendBranchId = @branchId";
            condition += beginDate.HasValue ? " AND r.SendDate >= @beginDate" : string.Empty;
            condition += endDate.HasValue ? " AND r.SendDate <= @endDate" : string.Empty;
            condition += status.HasValue ? " AND r.Status = @status" : string.Empty;

            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "r.SendDate",
                Direction = SortDirection.Desc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<Remittance, Group, User, Group, User, Remittance>($@"
SELECT r.*, sb.*, su.*, rb.*, ru.*
FROM Remittances r
JOIN Groups sb ON r.SendBranchId = sb.Id
JOIN Users su ON r.SendUserId = su.Id
JOIN Groups rb ON r.ReceiveBranchId = rb.Id
LEFT JOIN Users ru ON r.ReceiveUserId = ru.Id
{condition} {order} {page}", (r, sb, su, rb, ru) => {
                r.SendBranch = sb;
                r.SendUser = su;
                r.ReceiveBranch = rb;
                r.ReceiveUser = ru;
                return r;
            }, new
            {
                beginDate,
                endDate,
                status,
                branchId,
                listQuery.Page?.Offset,
                listQuery.Page?.Limit
            }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var incoming = query?.Val<bool?>("Incoming");
            var branchId = query?.Val<int?>("BranchId");
            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var status = query?.Val<RemittanceStatusType?>("Status");

            if (!incoming.HasValue) throw new ArgumentNullException(nameof(incoming));
            if (!branchId.HasValue) throw new ArgumentNullException(nameof(branchId));

            var condition = @"WHERE r.DeleteDate IS NULL";
            condition += incoming.Value ? " AND r.ReceiveBranchId = @branchId" : " AND r.SendBranchId = @branchId";
            condition += beginDate.HasValue ? " AND r.SendDate >= @beginDate" : string.Empty;
            condition += endDate.HasValue ? " AND r.SendDate <= @endDate" : string.Empty;
            condition += status.HasValue ? " AND r.Status = @status" : string.Empty;

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM Remittances r
{condition}", new
            {
                beginDate,
                endDate,
                status,
                branchId
            }, UnitOfWork.Transaction);
        }
    }
}
