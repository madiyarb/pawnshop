using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.CreditBureaus;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Access
{
    public class CBBatchRepository : RepositoryBase, IRepository<CBBatch>
    {
        public CBBatchRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(CBBatch entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO CBBatches
(BatchDate, BatchId, BatchStatusId, BatchStatusInfo, CBId, OrganizationId, CreateDate, AuthorId, StartsFrom, Size, FileName, FileId)
VALUES
(@BatchDate, @BatchId, @BatchStatusId, @BatchStatusInfo, @CBId, @OrganizationId, @CreateDate, @AuthorId, @StartsFrom, @Size, @FileName, @FileId)
                SELECT SCOPE_IDENTITY()", entity,UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(CBBatch entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE CBBatches SET BatchId=@BatchId, BatchStatusId=@BatchStatusId, BatchStatusInfo=@BatchStatusInfo,
StartsFrom = @StartsFrom, Size = @Size, FileName = @FileName, FileId = @FileId
WHERE Id=@id", entity,UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        public void Delete(int id)
        {
            UnitOfWork.Session.Execute(@"UPDATE CBBatches SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
        }

        public CBBatch Get(int id)
        {
            return UnitOfWork.Session.Query<CBBatch>(@" SELECT * FROM CBBatches WHERE Id=@id", new { id }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public CBBatch Find(object query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var status = query?.Val<CBBatchStatus?>("Status");
            var startsFrom = query?.Val<int?>("StartsFrom");
            var size = query?.Val<int?>("Size");
            var batchDate = query?.Val<DateTime?>("BatchDate");
            var organizationId = query?.Val<int?>("OrganizationId");
            var cbId = query?.Val<CBType?>("CbId");

            var pre = "DeleteDate IS NULL";

            pre += status.HasValue ? " AND BatchStatusId = @status" : string.Empty;
            pre += startsFrom.HasValue ? " AND StartsFrom = @startsFrom" : string.Empty;
            pre += size.HasValue ? " AND Size = @size" : string.Empty;
            pre += batchDate.HasValue ? " AND CAST(BatchDate AS DATE) = CAST(@batchDate AS DATE)" : string.Empty;
            pre += organizationId.HasValue ? " AND OrganizationId = @organizationId" : string.Empty;
            pre += cbId.HasValue ? " AND CBId = @cbId" : string.Empty;

            var condition = new ListQuery().Like(pre);

            return UnitOfWork.Session.Query<CBBatch>($@" SELECT * FROM CBBatches {condition}", new { status, startsFrom, size, batchDate, organizationId, cbId }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<CBBatch> List(ListQuery listQuery, object query = null)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var status = query?.Val<CBBatchStatus?>("Status");

            var pre = "DeleteDate IS NULL";

            pre += status.HasValue ? " AND BatchStatusId = @status" : string.Empty;


            var condition = new ListQuery().Like(pre);

            return UnitOfWork.Session.Query<CBBatch>($@"SELECT * FROM CBBatches {condition}",
                new
                {
                    status
                },
                UnitOfWork.Transaction).ToList();
        }

        public CBBatch GetBatchById(int batchId)
        {
            return UnitOfWork.Session.Query<CBBatch>("SELECT * FROM CBBatches WHERE DeleteDate IS NULL AND Id = @batchId", new { batchId }, UnitOfWork.Transaction).SingleOrDefault();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            return UnitOfWork.Session.ExecuteScalar<int>(@"SELECT COUNT(*) FROM CBBatches", UnitOfWork.Transaction);
        }

        public void Fulfill(CBBatch entity)
        {
            UnitOfWork.Session.Execute(@"dbo.sp_FillDataByBatch", new {BatchId = entity.Id, BatchDate = entity.BatchDate, IsScb = entity.CBId == CBType.SCB ? 1 : 0}, commandTimeout: 150000, commandType: System.Data.CommandType.StoredProcedure);
        }

        public void FillCBBatchesMonthly(DateTime accountingDate, int cb)
        {
            UnitOfWork.Session.Execute(@"dbo.sp_FillCBBatchesMonthly", new { accountingDate = accountingDate, cb }, commandTimeout: 150000, commandType: System.Data.CommandType.StoredProcedure);
        }

        public List<int> FillCBBatchesManually(DateTime accountingDate, int cb, string contractIds, int schemaId, int userId)
        {
            return UnitOfWork.Session.Query<int>(@"dbo.sp_FillCBBatchesManually", new { accountingDate, cb, contractIds, schemaId, userId }, commandTimeout: 150000, commandType: System.Data.CommandType.StoredProcedure).ToList();
        }

        public void FillCBBatchesDaily(int cb)
        {
            UnitOfWork.Session.Execute(@"dbo.sp_FillCBBatchesDaily", new { cb }, commandTimeout: 150000, commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}

