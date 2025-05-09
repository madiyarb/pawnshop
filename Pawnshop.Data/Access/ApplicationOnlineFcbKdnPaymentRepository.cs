using Dapper;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.ApplicationOnlineFcbKdnPayment;
using Pawnshop.Data.Models.Membership;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Pawnshop.Data.Access
{
    public class ApplicationOnlineFcbKdnPaymentRepository : RepositoryBase, IRepository<ApplicationOnlineFcbKdnPayment>
    {
        public ApplicationOnlineFcbKdnPaymentRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
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
                UnitOfWork.Session.Execute(@"UPDATE ApplicationOnlineFcbKdnPayments 
   SET DeleteDate = @deleteDate
 WHERE Id = @Id",
                    new { id, deleteDate = DateTime.Now }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public ApplicationOnlineFcbKdnPayment Find(object query)
        {
            throw new NotImplementedException();
        }

        public ApplicationOnlineFcbKdnPayment Get(int id)
        {
            return UnitOfWork.Session.Query<ApplicationOnlineFcbKdnPayment, User, ApplicationOnlineFcbKdnPayment>(@"SELECT p.*,
       u.*
  FROM ApplicationOnlineFcbKdnPayments p
  JOIN Users u ON u.Id = p.CreateBy
 WHERE p.DeleteDate IS NULL
   AND p.Id = @id",
                (p, u) =>
                {
                    p.Author = u;
                    return p;
                },
                new { id }, UnitOfWork.Transaction)
                .FirstOrDefault();
        }

        public void Insert(ApplicationOnlineFcbKdnPayment entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"INSERT INTO ApplicationOnlineFcbKdnPayments ( CreateDate, CreateBy, ApplicationOnlineId, PaymentAmount, Success )
VALUES ( @CreateDate, @CreateBy, @ApplicationOnlineId, @PaymentAmount, @Success )

SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<ApplicationOnlineFcbKdnPayment> List(ListQuery listQuery, object query = null)
        {
            if (query == null)
                return null;

            var applicationOnlineId = query.Val<Guid?>("ApplicationOnlineId");

            if (!applicationOnlineId.HasValue)
                return null;

            return UnitOfWork.Session.Query<ApplicationOnlineFcbKdnPayment, User, ApplicationOnlineFcbKdnPayment>(@"SELECT p.*,
       u.*
  FROM ApplicationOnlineFcbKdnPayments p
  JOIN Users u ON u.Id = p.CreateBy
 WHERE p.DeleteDate IS NULL
   AND p.ApplicationOnlineId = @applicationOnlineId",
                (p, u) =>
                {
                    p.Author = u;
                    return p;
                },
                new { applicationOnlineId }, UnitOfWork.Transaction)
                .ToList();
        }

        public void Update(ApplicationOnlineFcbKdnPayment entity)
        {
            throw new NotImplementedException();
        }
    }
}
