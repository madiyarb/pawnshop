using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Files;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pawnshop.Data.Access
{
    public class ClientsBlackListRepository : RepositoryBase, IRepository<ClientsBlackList>
    {
        public ClientsBlackListRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(ClientsBlackList entity)
        {
            using (var transaction = BeginTransaction())
            {
                entity.Id = UnitOfWork.Session.ExecuteScalar<int>(@"
INSERT INTO ClientsBlackList(ClientId, ReasonId, AddedBy, AddReason, AddedAt, AddedFileRowId)
VALUES(@ClientId, @ReasonId, @AddedBy, @AddReason, @AddedAt, @AddedFileRowId)
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }

        public void Update(ClientsBlackList entity)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"
UPDATE ClientsBlackList SET ClientId=@ClientId,ReasonId=@ReasonId,AddedBy=@AddedBy,AddReason=@AddReason,AddedAt=@AddedAt,RemovedBy=@RemovedBy,
                                           RemoveReason=@RemoveReason, RemoveDate=@RemoveDate, AddedFileRowId=@AddedFileRowId, RemovedFileRowId=@RemovedFileRowId  
WHERE Id=@id", entity, UnitOfWork.Transaction);
                transaction.Commit();
            }
        }
        public void Delete(int id)
        {
            using (var transaction = BeginTransaction())
            {
                UnitOfWork.Session.Execute(@"UPDATE ClientsBlackList SET DeleteDate = dbo.GETASTANADATE() WHERE Id = @id", new { id = id }, UnitOfWork.Transaction);

                transaction.Commit();
            }
        }

        public ClientsBlackList Get(int id)
        {
            return UnitOfWork.Session.QuerySingleOrDefault<ClientsBlackList>(@"
SELECT * 
FROM ClientsBlackList
WHERE Id=@id", new { id });
        }

        public List<ClientsBlackList> GetAddedListByClientId(int clientId)
        {
            var list = UnitOfWork.Session.Query<ClientsBlackList, BlackListReason, FileRow, FileRow, ClientsBlackList>(@"
                SELECT *
                FROM ClientsBlackList b
                LEFT JOIN BlackListReasons r ON r.Id=b.ReasonId
                LEFT JOIN FileRows f1 ON b.AddedFileRowId=f1.Id
                LEFT JOIN FileRows f2 ON b.RemovedFileRowId=f2.Id
                WHERE b.ClientId = @clientId AND b.DeleteDate IS NULL",
                (b, r, f1, f2) =>
                {
                    b.BlackListReason = r;
                    b.AddedFile = f1;
                    b.RemovedFile = f2;
                    return b;
                },
                new { clientId }, UnitOfWork.Transaction).ToList();

            return list;
        }
        public List<BlackListReason> GetDisplayedListByClientId(int clientId)
        {
            var list = UnitOfWork.Session.Query<BlackListReason>(@"
                SELECT r.*
                FROM ClientsBlackList b
                LEFT JOIN BlackListReasons r ON r.Id=b.ReasonId
                WHERE b.ClientId = @clientId AND b.DeleteDate IS NULL AND r.IsDisplayed = 1
                AND b.RemoveReason IS NULL",
                new { clientId }, UnitOfWork.Transaction).ToList();

            return list;
        }


        public ClientsBlackList Find(object query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var clientId = query?.Val<int>("ClientId");
            var reasonId = query?.Val<int>("ReasonId");
            var condition = "WHERE DeleteDate IS NULL";

            condition += clientId.HasValue ? " AND ClientId = @clientId" : "";
            condition += reasonId.HasValue ? " AND ReasonId = @reasonId" : "";

            return
                UnitOfWork.Session.Query<ClientsBlackList>($@"
                SELECT * FROM ClientsBlackList {condition}",
                    new { clientId, reasonId }, UnitOfWork.Transaction).FirstOrDefault();
        }

        public List<ClientsBlackList> GetClientsBlackListsByClientId(int clientId)
        {
            return UnitOfWork.Session.Query<ClientsBlackList, BlackListReason, ClientsBlackList>(@"
                    SELECT c.*, b.* FROM ClientsBlackList c 
                    LEFT JOIN BlackListReasons b ON c.ReasonId = b.Id
                    WHERE c.DeleteDate IS NULL AND c.ClientId = @clientId", (c, b) =>
            {
                c.BlackListReason = b;
                return c;
            }, new
            {
                clientId
            }, UnitOfWork.Transaction).ToList();
        }
        
        public async Task<List<ClientsBlackList>> GetClientsBlackListsByClientIdAsync(int clientId)
        {
            var queryResult = await UnitOfWork.Session.QueryAsync<ClientsBlackList, BlackListReason, ClientsBlackList>(
                @"SELECT c.*, b.* FROM ClientsBlackList c 
                        LEFT JOIN BlackListReasons b ON c.ReasonId = b.Id
                        WHERE c.DeleteDate IS NULL AND c.ClientId = @clientId",
                (c, b) =>
                {
                    c.BlackListReason = b;
                    return c;
                },
                new { clientId },
                UnitOfWork.Transaction
            );

            return queryResult?.ToList();
        }

        public List<ClientsBlackList> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "DeleteDate IS NULL";

            var condition = listQuery.Like(pre, "ReasonId");
            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "AddReason",
                Direction = SortDirection.Asc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<ClientsBlackList>($@"
SELECT *
  FROM ClientsBlackList
{condition} {order} {page}", new
            {
                listQuery.Page?.Offset,
                listQuery.Page?.Limit,
                listQuery.Filter
            }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var pre = "DeleteDate IS NULL";

            var condition = listQuery.Like(pre, "ReasonId");

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
  FROM ClientsBlackList
{condition}", new
            {
                listQuery.Filter
            });
        }

        public void LogChanges(ClientsBlackList entity, int userId, bool isNew = false)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var log = new ClientsBlackListLog
            {
                ClientsBlackListId = entity.Id,
                ClientId = entity.ClientId,
                ReasonId = entity.ReasonId,
                AddReason = entity.AddReason,
                AddedBy = entity.AddedBy,
                AddedAt = entity.AddedAt,
                AddedFileRowId = entity.AddedFileRowId,
                RemoveReason = entity.RemoveReason,
                RemoveDate = entity.RemoveDate,
                RemovedBy = entity.RemovedBy,
                RemovedFileRowId = entity.RemovedFileRowId,
                DeleteDate = entity.DeleteDate,
                UpdatedByAuthorId = !isNew ? userId : default(int?),
                UpdateDate = DateTime.Now
            };
            UnitOfWork.Session.Execute(@"
                INSERT INTO ClientsBlackListLogs (ClientsBlackListId, ClientId, ReasonId, AddReason, AddedBy, AddedAt, DeleteDate, AddedFileRowId,
                    RemoveReason, RemoveDate, RemovedBy, RemovedFileRowId, UpdatedByAuthorId, UpdateDate)
                VALUES (@ClientsBlackListId, @ClientId, @ReasonId, @AddReason, @AddedBy, @AddedAt, @DeleteDate, @AddedFileRowId,
                    @RemoveReason, @RemoveDate, @RemovedBy, @RemovedFileRowId, @UpdatedByAuthorId, @UpdateDate)", log, UnitOfWork.Transaction);
        }
    }
}
