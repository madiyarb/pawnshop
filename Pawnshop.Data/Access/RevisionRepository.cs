using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Revisions;

namespace Pawnshop.Data.Access
{
    public class RevisionRepository : RepositoryBase, IRepository<Revision>
    {
        public RevisionRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(Revision entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                    INSERT INTO Revisions (PositionId, RevisionDate, ContractId, CreateDate, Status, Note)
                    VALUES ( @PositionId, @RevisionDate, @ContractId, @CreateDate, @Status, @Note )
                    SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Update(Revision entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE Revisions
                    SET PositionId = @PositionId, RevisionDate = @RevisionDate, ContractId = @ContractId, Status = @Status, Note = @Note
                    WHERE Id = @Id", entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Revisions SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public Revision Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<Revision>(@"
                SELECT * 
                FROM Revisions
                WHERE Id=@id", new { id }, UnitOfWork.Transaction);
        }

        public Revision Find(object query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            var positionId = query?.Val<int>("PositionId");

            return UnitOfWork.Session.QuerySingleOrDefault<Revision>(@"
                SELECT TOP 1 * FROM Revisions WHERE PositionId=@PositionId AND DeleteDate IS NULL", new { positionId }, UnitOfWork.Transaction);
        }

        public List<Revision> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var revisionDate = query?.Val<DateTime?>("RevisionDate");
            var status = query?.Val<string>("Status");
            var contractId = query?.Val<int?>("ContractId");

            string pre = "r.DeleteDate IS NULL";
            pre += revisionDate.HasValue ? " AND CAST(r.RevisionDate AS DATE) = @revisionDate" : string.Empty;
            pre += contractId.HasValue ? " AND cp.ContractId = @contractId" : string.Empty;
            pre += !string.IsNullOrWhiteSpace(status) ? " AND r.Status = @status" : string.Empty;

            var condition = listQuery.Like(pre, "PositionId");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "RevisionDate",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<Revision>($@"
                SELECT DISTINCT 
                  r.* 
                FROM 
                  Revisions r
                JOIN 
                  ContractPositions cp ON cp.PositionId = r.PositionId
                {condition} {order} {page}", new
            {
                revisionDate,
                contractId,
                status,
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }, UnitOfWork.Transaction).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {

            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var revisionDate = query?.Val<DateTime?>("RevisionDate");
            var status = query?.Val<string>("Status");
            var contractId = query?.Val<int?>("ContractId");

            string pre = "r.DeleteDate IS NULL";
            pre += revisionDate.HasValue ? " AND CAST(r.RevisionDate AS DATE) = @revisionDate" : string.Empty;
            pre += contractId.HasValue ? " AND cp.ContractId = @contractId" : string.Empty;
            pre += !string.IsNullOrWhiteSpace(status) ? " AND r.Status = @status" : string.Empty;

            var condition = listQuery.Like(pre, "PositionId");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                SELECT 
                  COUNT(DISTINCT r.PositionId)
                FROM 
                  Revisions r
                JOIN 
                  ContractPositions cp ON cp.PositionId = r.PositionId
                {condition}",
                new
                {
                    revisionDate,
                    contractId,
                    status,
                    listQuery.Filter
                }, UnitOfWork.Transaction);
        }

        public List<int> GetPositionsByContractNumber(int contractId, DateTime revisionDate)
        {
            return UnitOfWork.Session.Query<int>($@"
                SELECT 
                  cp.PositionId
                FROM 
                  ContractPositions cp 
                JOIN 
                  Contracts c ON c.Id = cp.ContractId
                WHERE 
                  c.id = @contractId AND 
                  c.DeleteDate IS NULL AND 
                  NOT EXISTS (SELECT 
                                cr.PositionId 
                              FROM 
                                Revisions cr
                              WHERE 
                                cr.DeleteDate IS NULL AND 
                                cr.PositionId = cp.PositionId AND
                                cr.ContractId = cp.ContractId AND 
                                cr.RevisionDate = @revisionDate)",
                    new { contractId, revisionDate }).ToList();
        }
    }
}