using System;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.TasOnline;
using System.Collections.Generic;
using System.Linq;
using Dapper;

namespace Pawnshop.Data.Access
{
    public class TasOnlineRequestRepository : RepositoryBase, IRepository<TasOnlineRequest>
    {
        public TasOnlineRequestRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(TasOnlineRequest entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
                INSERT INTO TasOnlineRequests ( RequestData, ResponseData, Status, PaymentId, CreateDate, AuthorId )
                    VALUES ( @RequestData, @ResponseData, @Status, @PaymentId, @CreateDate, @AuthorId )
                        SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(TasOnlineRequest entity)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
                    UPDATE TasOnlineRequests
                        SET RequestData = @RequestData, ResponseData = @ResponseData, Status = @Status, PaymentId = @PaymentId
                            WHERE Id = @Id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Delete(int id)
        {
            using (var transaction = UnitOfWork.BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE TasOnlineRequests SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id }, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public TasOnlineRequest Find(object query)
        {
            throw new System.NotImplementedException();
        }

        public TasOnlineRequest Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<TasOnlineRequest>(@"
                SELECT *
                    FROM TasOnlineRequests
                        WHERE Id = @id", new { id });
        }

        public List<TasOnlineRequest> List(ListQuery listQuery, object query = null)
        {
            if (listQuery is null) 
                throw new ArgumentNullException(nameof(listQuery));

            var condition = "WHERE DeleteDate IS NULL";

            return UnitOfWork.Session.Query<TasOnlineRequest>($@"
                SELECT *
                  FROM TasOnlineRequests
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
                  FROM TasOnlineRequests
                    {condition}");
        }
    }
}