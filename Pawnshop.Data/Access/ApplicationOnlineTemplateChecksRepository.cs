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
    public class ApplicationOnlineTemplateChecksRepository : RepositoryBase, IRepository<ApplicationOnlineTemplateCheck>
    {
        public ApplicationOnlineTemplateChecksRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            var predicate = "WHERE DeleteDate IS NULL";

            return UnitOfWork.Session.ExecuteScalar<int>($@"SELECT COUNT(Id)
  FROM ApplicationOnlineTemplateChecks
 {predicate}",
                null, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE ApplicationOnlineTemplateChecks
   SET DeleteDate = @deleteDate
 WHERE Id = @Id",
                   new { id, deleteDate = DateTime.Now }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public ApplicationOnlineTemplateCheck Find(object query)
        {
            throw new NotImplementedException();
        }

        public ApplicationOnlineTemplateCheck GetByCode(string code)
        {
            return UnitOfWork.Session.Query<ApplicationOnlineTemplateCheck, User, User, ApplicationOnlineTemplateCheck>(@"SELECT TOP 1
       aotc.*,
       u.*,
       u2.*
  FROM ApplicationOnlineTemplateChecks aotc
  JOIN Users u ON u.Id = aotc.CreateBy
  LEFT JOIN Users u2 ON u2.Id = aotc.UpdateBy
 WHERE aotc.code = @code",
                    (t, u, u2) =>
                    {
                        t.Author = u;
                        t.UpdateAuthor = u2;

                        return t;
                    },
                    new { code }, UnitOfWork.Transaction)
                .FirstOrDefault();
        }

        public ApplicationOnlineTemplateCheck Get(int id)
        {
            return UnitOfWork.Session.Query<ApplicationOnlineTemplateCheck, User, User, ApplicationOnlineTemplateCheck>(@"SELECT TOP 1
       aotc.*,
       u.*,
       u2.*
  FROM ApplicationOnlineTemplateChecks aotc
  JOIN Users u ON u.Id = aotc.CreateBy
  LEFT JOIN Users u2 ON u2.Id = aotc.UpdateBy
 WHERE aotc.Id = @id",
                (t, u, u2) =>
                {
                    t.Author = u;
                    t.UpdateAuthor = u2;

                    return t;
                },
                new { id }, UnitOfWork.Transaction)
                .FirstOrDefault();
        }

        public void Insert(ApplicationOnlineTemplateCheck entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"INSERT INTO ApplicationOnlineTemplateChecks ( CreateDate, CreateBy, Code, Title, IsActual, IsManual, Stage, ToVerificator, ToManager, ToTranche, AttributeName, AttributeCode )
 VALUES ( @CreateDate, @CreateBy, @Code, @Title, @IsActual, @IsManual, @Stage, @ToVerificator, @ToManager, @ToTranche, @AttributeName, @AttributeCode )

 SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public List<ApplicationOnlineTemplateCheck> List(ListQuery listQuery, object query = null)
        {
            var offset = query.Val<int?>("Offset");
            var limit = query.Val<int?>("Limit");
            var isActual = query.Val<bool?>("IsActual");
            var toTranche = query.Val<bool?>("ToTranche");
            var toManager = query.Val<bool?>("ToManager");
            var toVerificator = query.Val<bool?>("ToVerificator");
            var stage = query.Val<int?>("Stage");
            var isManual = query.Val<bool?>("IsManual");

            #region build predicate
            var predicate = "WHERE aotc.DeleteDate IS NULL";

            if (isActual.HasValue)
                predicate += " AND aotc.IsActual = @isActual";

            if (toTranche.HasValue)
                predicate += " AND aotc.ToTranche = @toTranche";

            if (toManager.HasValue)
                predicate += " AND aotc.ToManager = @toManager";

            if (toVerificator.HasValue)
                predicate += " AND aotc.ToVerificator = @toVerificator";

            if (stage.HasValue)
                predicate += " AND aotc.Stage = @stage";

            if (isManual.HasValue)
                predicate += " AND aotc.IsManual = @IsManual";
            #endregion

            #region build pagination
            var page = string.Empty;

            if (offset.HasValue)
            {
                page = "OFFSET (@offset) ROWS FETCH NEXT @limit ROWS ONLY";

                if (!limit.HasValue)
                    limit = 20;
            }
            #endregion

            return UnitOfWork.Session.Query<ApplicationOnlineTemplateCheck, User, User, ApplicationOnlineTemplateCheck>($@"SELECT aotc.*,
       u.*,
       u2.*
  FROM ApplicationOnlineTemplateChecks aotc
  JOIN Users u ON u.Id = aotc.CreateBy
  LEFT JOIN Users u2 ON u2.Id = aotc.UpdateBy
 {predicate}
 ORDER BY aotc.Id ASC
{page}",
                (t, u, u2) =>
                {
                    t.Author = u;
                    t.UpdateAuthor = u2;

                    return t;
                },
                new
                {
                    offset,
                    limit,
                    isActual,
                    toTranche,
                    toManager,
                    toVerificator,
                    stage,
                    isManual
                }, UnitOfWork.Transaction)
                .ToList();
        }

        public void Update(ApplicationOnlineTemplateCheck entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE ApplicationOnlineTemplateChecks
   SET UpdateDate = @UpdateDate,
       UpdateBy = @UpdateBy,
       DeleteDate = @DeleteDate,
       Code = @Code,
       Title = @Title,
       IsActual = @IsActual,
       IsManual = @IsManual,
       Stage = @Stage,
       ToVerificator = @ToVerificator,
       ToManager = @ToManager,
       ToTranche = @ToTranche,
       AttributeName = @AttributeName,
       AttributeCode = @AttributeCode
 WHERE Id = @Id",
                   entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }
    }
}
