using Dapper;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Core;
using Pawnshop.Data.Models.Comments;
using Pawnshop.Data.Models.Membership;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Pawnshop.Data.Access
{
    public class CommentsRepository : RepositoryBase, IRepository<Comment>
    {
        public CommentsRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            var appOnlineId = query.Val<Guid?>("ApplicationOnlineId");
            var authorId = query.Val<int?>("AuthorId");
            var authorName = query.Val<string>("AuthorName");
            var clientId = query.Val<int?>("ClientId");

            var predicateList = new List<string> { "WHERE c.DeleteDate IS NULL" };

            if (appOnlineId.HasValue)
                predicateList.Add("aoc.ApplicationOnlineId = @appOnlineId");

            if (authorId.HasValue)
                predicateList.Add("c.AuthorId = @authorId");

            if (!string.IsNullOrEmpty(authorName))
                predicateList.Add("c.AuthorName = @authorName");

            if (clientId.HasValue)
                predicateList.Add("ao.ClientId = @clientId");

            if (predicateList.Count <= 1)
                return 0;

            var predicate = string.Join(" AND ", predicateList.ToArray());

            return UnitOfWork.Session.ExecuteScalar<int>($@"SELECT COUNT(c.Id)
  FROM Comments c
  LEFT JOIN Users u ON u.Id = c.AuthorId
  LEFT JOIN ApplicationOnlineComments aoc ON aoc.CommentID = c.Id
  LEFT JOIN ApplicationsOnline ao ON ao.Id = aoc.ApplicationOnlineId
  LEFT JOIN Clients cl ON cl.Id = ao.ClientId
 {predicate}",
                new { appOnlineId, authorId, authorName, clientId }, UnitOfWork.Transaction);
        }

        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Comments
   SET DeleteDate = @deleteDate
 WHERE Id = @id",
                    new { id, deleteDate = DateTime.Now }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public Comment Find(object query)
        {
            throw new NotImplementedException();
        }

        public Comment Get(int id)
        {
            return UnitOfWork.Session.Query<Comment, User, Comment>(@"SELECT TOP 1 c.*,
       u.*
  FROM Comments c
  LEFT JOIN Users u ON u.Id = c.AuthorId
 WHERE c.DeleteDate IS NULL
   AND c.Id = @id",
                (c, u) =>
                {
                    c.Author = u;
                    return c;
                },
                new { id }, UnitOfWork.Transaction)
                .FirstOrDefault();
        }

        public void Insert(Comment entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.CreateDate = DateTime.Now;
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"INSERT INTO Comments ( CreateDate, AuthorId, AuthorName, CommentText )
 VALUES ( @CreateDate, @AuthorId, @AuthorName, @CommentText )

SELECT SCOPE_IDENTITY()",
                    entity, UnitOfWork.Transaction);

                if (entity.ApplicationOnlineComment != null)
                {
                    UnitOfWork.Session.Execute(@"INSERT INTO ApplicationOnlineComments ( ApplicationOnlineId, CommentId, CommentType ) VALUES ( @appId, @commentId, @commentType )",
                        new
                        {
                            appId = entity.ApplicationOnlineComment.ApplicationOnlineId,
                            commentId = entity.Id,
                            commentType = entity.ApplicationOnlineComment.CommentType
                        }, UnitOfWork.Transaction);
                }

                transaction.Commit();
            }
        }

        public List<Comment> List(ListQuery listQuery, object query = null)
        {
            var appOnlineId = query.Val<Guid?>("ApplicationOnlineId");
            var authorId = query.Val<int?>("AuthorId");
            var authorName = query.Val<string>("AuthorName");
            var clientId = query.Val<int?>("ClientId");
            var offset = query.Val<int?>("Offset");
            var limit = query.Val<int?>("Limit");

            var predicateList = new List<string> { "WHERE c.DeleteDate IS NULL" };

            if (appOnlineId.HasValue)
                predicateList.Add("aoc.ApplicationOnlineId = @appOnlineId");

            if (authorId.HasValue)
                predicateList.Add("c.AuthorId = @authorId");

            if (!string.IsNullOrEmpty(authorName))
                predicateList.Add("c.AuthorName = @authorName");

            if (clientId.HasValue)
                predicateList.Add("ao.ClientId = @clientId");

            if (predicateList.Count <= 1)
                return new List<Comment>();

            var predicate = string.Join(" AND ", predicateList.ToArray());

            var page = string.Empty;

            if (offset.HasValue)
            {
                page = "OFFSET (@offset) ROWS FETCH NEXT @limit ROWS ONLY";

                if (!limit.HasValue)
                    limit = 20;
            }

            return UnitOfWork.Session.Query<Comment, ApplicationOnlineComment, User, Comment>($@"SELECT c.*, aoc.*, u.*
  FROM Comments c
  LEFT JOIN Users u ON u.Id = c.AuthorId
  LEFT JOIN ApplicationOnlineComments aoc ON aoc.CommentID = c.Id
  LEFT JOIN ApplicationsOnline ao ON ao.Id = aoc.ApplicationOnlineId
  LEFT JOIN Clients cl ON cl.Id = ao.ClientId
 {predicate}
 ORDER BY c.CreateDate DESC
 {page}",
                (c, aoc, u) =>
                {
                    c.ApplicationOnlineComment = aoc;
                    c.Author = u;
                    return c;
                },
                new { appOnlineId, authorId, authorName, clientId, offset, limit }, UnitOfWork.Transaction)
                .ToList();
        }

        public Comment GetLastComment(Guid applicationOnlineId)
        {
            return UnitOfWork.Session
                .QueryFirstOrDefault<Comment>($@"
            SELECT c.*
              FROM Comments c
              JOIN ApplicationOnlineComments aoc ON aoc.CommentId = c.Id
             WHERE aoc.ApplicationOnlineId = @app
             ORDER BY c.CreateDate DESC",
                    new { app = applicationOnlineId }, UnitOfWork.Transaction);
        }

        public void Update(Comment entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE Comments
   SET CommentText = @CommentText
 WHERE Id = @Id",
                    entity, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }
    }
}
