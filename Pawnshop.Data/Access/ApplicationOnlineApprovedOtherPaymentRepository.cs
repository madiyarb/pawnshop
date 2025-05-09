using Dapper;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.ApplicationOnlineApprovedOtherPayment;
using Pawnshop.Data.Models.ApplicationOnlineFiles;
using Pawnshop.Data.Models.Membership;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Pawnshop.Data.Access
{
    public class ApplicationOnlineApprovedOtherPaymentRepository : RepositoryBase, IRepository<ApplicationOnlineApprovedOtherPayment>
    {
        public ApplicationOnlineApprovedOtherPaymentRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
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
                UnitOfWork.Session.Execute(@"UPDATE ApplicationOnlineApprovedOtherPayments 
   SET DeleteDate = @deleteDate
 WHERE Id = @Id",
                    new { id, deleteDate = DateTime.Now }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public ApplicationOnlineApprovedOtherPayment Find(object query)
        {
            throw new NotImplementedException();
        }

        public ApplicationOnlineApprovedOtherPayment Get(int id)
        {
            return UnitOfWork.Session.Query<ApplicationOnlineApprovedOtherPayment, User, ApplicationOnlineFile, ApplicationOnlineApprovedOtherPayment>(@"
SELECT p.*,
       u.*,
       f.*
  FROM ApplicationOnlineApprovedOtherPayments p
  JOIN Users u ON u.Id = p.CreateBy
  JOIN ApplicationOnlineFiles f ON f.Id = p.FileId
 WHERE p.DeleteDate IS NULL
   AND p.Id = @id",
                (p, u, f) =>
                {
                    p.Author = u;
                    p.File = f;
                    return p;
                },
                new { id }, UnitOfWork.Transaction)
                .FirstOrDefault();
        }

        public void Insert(ApplicationOnlineApprovedOtherPayment entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"INSERT INTO ApplicationOnlineApprovedOtherPayments ( CreateDate, CreateBy, ApplicationOnlineId, SubjectName,  Amount, FileId )
VALUES ( @CreateDate, @CreateBy, @ApplicationOnlineId, @SubjectName, @Amount, @FileId )

SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<ApplicationOnlineApprovedOtherPayment> List(ListQuery listQuery, object query = null)
        {
            if (query == null)
                return null;

            var applicationOnlineId = query.Val<Guid?>("ApplicationOnlineId");

            if (!applicationOnlineId.HasValue)
                return null;

            return UnitOfWork.Session.Query<ApplicationOnlineApprovedOtherPayment, User, ApplicationOnlineFile, ApplicationOnlineApprovedOtherPayment>(@"
SELECT p.*,
       u.*,
       f.*
  FROM ApplicationOnlineApprovedOtherPayments p
  JOIN Users u ON u.Id = p.CreateBy
  JOIN ApplicationOnlineFiles f ON f.Id = p.FileId
 WHERE p.DeleteDate IS NULL
   AND p.ApplicationOnlineId = @applicationOnlineId",
                (p, u, f) =>
                {
                    p.Author = u;
                    p.File = f;
                    return p;
                },
                new { applicationOnlineId }, UnitOfWork.Transaction)
                .ToList();
        }

        public void Update(ApplicationOnlineApprovedOtherPayment entity)
        {
            throw new NotImplementedException();
        }
    }
}
