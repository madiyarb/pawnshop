using Dapper;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.ApplicationsOnline;
using Pawnshop.Data.Models.Membership;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Pawnshop.Data.Access
{
    public class ApplicationOnlineChecksRepository : RepositoryBase, IRepository<ApplicationOnlineCheck>
    {
        public ApplicationOnlineChecksRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }


        public ApplicationOnlineCheck GetCheckByCode(Guid applicationOnlineId, string code)
        {
            return UnitOfWork.Session.Query<ApplicationOnlineCheck, ApplicationOnlineTemplateCheck, User, User, ApplicationOnlineCheck>(@"SELECT aoc.*
       ,aotc.*
       ,u.*
       ,u2.*
  FROM ApplicationOnlineChecks aoc
  JOIN ApplicationOnlineTemplateChecks aotc ON aotc.Id = aoc.TemplateId
  JOIN Users u ON u.Id = aoc.CreateBy
  LEFT JOIN Users u2 ON u2.Id = aoc.UpdateBy
 WHERE aoc.DeleteDate IS NULL
   AND aoc.ApplicationOnlineId = @applicationOnlineId
   AND aotc.Code = @code",
                    (a, t, u, u2) =>
                    {
                        a.TemplateCheck = t;
                        a.Author = u;
                        a.UpdateAuthor = u2;

                        return a;
                    },
                    new { applicationOnlineId, code }, UnitOfWork.Transaction)
                .FirstOrDefault();
        }
        public int Count(ListQuery listQuery, object query = null)
        {
            var appOnlineId = query.Val<Guid?>("ApplicationOnlineId");

            if (!appOnlineId.HasValue)
                return 0;

            var predicate = "WHERE DeleteDate IS NULL AND ApplicationOnlineId = @appOnlineId";

            return UnitOfWork.Session.ExecuteScalar<int>($@"SELECT COUNT(Id)
  FROM ApplicationOnlineChecks
 {predicate}",
                new { appOnlineId }, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE ApplicationOnlineChecks 
   SET DeleteDate = @deleteDate
 WHERE Id = @Id",
                    new { id, deleteDate = DateTime.Now }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public ApplicationOnlineCheck Find(object query)
        {
            throw new NotImplementedException();
        }

        public ApplicationOnlineCheck Get(int id)
        {
            return UnitOfWork.Session.Query<ApplicationOnlineCheck, ApplicationOnlineTemplateCheck, User, User, ApplicationOnlineCheck>(@"SELECT aoc.*
       ,aotc.*
       ,u.*
       ,u2.*
  FROM ApplicationOnlineChecks aoc
  JOIN ApplicationOnlineTemplateChecks aotc ON aotc.Id = aoc.TemplateId
  JOIN Users u ON u.Id = aoc.CreateBy
  LEFT JOIN Users u2 ON u2.Id = aoc.UpdateBy
 WHERE aoc.DeleteDate IS NULL
   AND aoc.Id = @id",
                (a, t, u, u2) =>
                {
                    a.TemplateCheck = t;
                    a.Author = u;
                    a.UpdateAuthor = u2;

                    return a;
                },
                new { id }, UnitOfWork.Transaction)
                .FirstOrDefault();
        }

        public void Insert(ApplicationOnlineCheck entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(
                    @"INSERT INTO ApplicationOnlineChecks ( CreateDate, CreateBy, ApplicationOnlineId, TemplateId, Checked,AdditionalInfo, Note )
                        VALUES ( @CreateDate, @CreateBy, @ApplicationOnlineId, @TemplateId, @Checked, @AdditionalInfo, @Note ) 
                        SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<ApplicationOnlineCheck> List(ListQuery listQuery, object query = null)
        {
            var appOnlineId = query.Val<Guid?>("ApplicationOnlineId");

            if (!appOnlineId.HasValue)
                return new List<ApplicationOnlineCheck>();

            return UnitOfWork.Session.Query<ApplicationOnlineCheck, ApplicationOnlineTemplateCheck, User, User, ApplicationOnlineCheck>($@"SELECT aoc.*
       ,aotc.*
       ,u.*
       ,u2.*
  FROM ApplicationOnlineChecks aoc
  JOIN ApplicationOnlineTemplateChecks aotc ON aotc.Id = aoc.TemplateId
  JOIN ApplicationsOnline ao ON ao.Id = aoc.ApplicationOnlineId AND ao.Stage >= aotc.Stage
  JOIN Users u ON u.Id = aoc.CreateBy
  LEFT JOIN Users u2 ON u2.Id = aoc.UpdateBy
 WHERE aoc.DeleteDate IS NULL
   AND aoc.ApplicationOnlineId = @appOnlineId
 ORDER BY aoc.Id ASC",
                (a, t, u, u2) =>
                {
                    a.TemplateCheck = t;
                    a.Author = u;
                    a.UpdateAuthor = u2;

                    return a;
                },
                new { appOnlineId }, UnitOfWork.Transaction)
                .ToList();
        }

        public void Update(ApplicationOnlineCheck entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE ApplicationOnlineChecks 
   SET UpdateDate = @UpdateDate,
       UpdateBy = @UpdateBy,
       Note = @Note, 
       Checked = @Checked,
       AdditionalInfo = @AdditionalInfo
 WHERE Id = @Id",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }
    }
}
