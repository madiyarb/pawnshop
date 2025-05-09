using Pawnshop.Core.Impl;
using Pawnshop.Core;
using Pawnshop.Data.Models.TasOnline;
using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Data.Models.TMF;
using Dapper;
using Pawnshop.Core.Queries;
using System.Linq;

namespace Pawnshop.Data.Access.TMF
{
    public class TMFRequestRepository : RepositoryBase, IRepository<TMFRequest>
    {
        public TMFRequestRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(TMFRequest entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                INSERT INTO TmfRequests ( RequestData, ResponseData, Status, PaymentId, CreateDate, AuthorId )
                    VALUES ( @RequestData, @ResponseData, @Status, @PaymentId, @CreateDate, @AuthorId )
                        SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(TMFRequest entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE TmfRequests
                        SET RequestData = @RequestData, ResponseData = @ResponseData, Status = @Status, PaymentId = @PaymentId
                            WHERE Id = @Id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE TmfRequests SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public TMFRequest Find(object query)
        {
            throw new System.NotImplementedException();
        }

        public TMFRequest Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<TMFRequest>(@"
                SELECT *
                    FROM TmfRequests
                        WHERE Id = @id", new { id });
        }

        public List<TMFRequest> List(ListQuery listQuery, object query = null)
        {
            if (listQuery is null)
                throw new ArgumentNullException(nameof(listQuery));

            var condition = "WHERE DeleteDate IS NULL";

            return UnitOfWork.Session.Query<TMFRequest>($@"
                SELECT *
                  FROM TmfRequests
                    {condition}"
            ).ToList();
        }
        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery is null)
                throw new ArgumentNullException(nameof(listQuery));

            var condition = "WHERE DeleteDate IS NULL";

            return UnitOfWork.Session.ExecuteScalar<int>($@"
                SELECT COUNT(*)
                  FROM TmfRequests
                    {condition}");
        }
    }
}
