using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Pawnshop.Core;
using Pawnshop.Core.Impl;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Models.Audit;

namespace Pawnshop.Data.Access
{
    public class EventLogRepository : RepositoryBase, IRepository<EventLogItem>
    {
        public EventLogRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        public void Insert(EventLogItem entity)
        {
                entity.Id = UnitOfWork.Session.QuerySingleOrDefault<int>(@"
INSERT INTO EventLogItems ( EventCode, EventStatus, UserId, UserName, BranchId, BranchName, Uri, Address, EntityType, EntityId, RequestData, ResponseData, CreateDate )
VALUES ( @EventCode, @EventStatus, @UserId, @UserName, @BranchId, @BranchName, @Uri, @Address, @EntityType, @EntityId, @RequestData, @ResponseData, @CreateDate )
SELECT SCOPE_IDENTITY()", entity, UnitOfWork.Transaction);
        }

        public async Task InsertAsync(EventLogItem entity)
        {
            var sqlQuery = @"
                INSERT INTO EventLogItems (EventCode, EventStatus, UserId, UserName, BranchId, BranchName, Uri, Address,
                                           EntityType, EntityId, RequestData, ResponseData, CreateDate)
                VALUES (@EventCode, @EventStatus, @UserId, @UserName, @BranchId, @BranchName, @Uri, @Address, @EntityType,
                        @EntityId, @RequestData, @ResponseData, @CreateDate) ";
            
            var parameters = new
            {
                entity.EventCode,
                entity.EventStatus,
                entity.UserId,
                entity.UserName,
                entity.BranchId,
                entity.BranchName,
                entity.Uri,
                entity.Address,
                entity.EntityType,
                entity.EntityId,
                entity.RequestData,
                entity.ResponseData,
                entity.CreateDate
            };

            await UnitOfWork.Session.QueryAsync(sqlQuery, parameters, UnitOfWork.Transaction);
        }

        public void Update(EventLogItem entity)
        {
            throw new AccessViolationException();
        }

        public void Delete(int id)
        {
            throw new AccessViolationException();
        }

        public EventLogItem Get(int id)
        {
            throw new AccessViolationException();
        }
        public Task<EventLogItem> GetAsync(int id)
        {
            throw new AccessViolationException();
        }

        public EventLogItem Find(object query)
        {
            throw new AccessViolationException();
        }

        public List<EventLogItem> List(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var branchId = query?.Val<int?>("BranchId");
            var eventCode = query?.Val<EventCode?>("EventCode");
            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var entityType = query?.Val<EntityType?>("EntityType");
            var entityId = query?.Val<int?>("EntityId");

            var pre = "Id <> 0";
            pre += branchId.HasValue ? " AND BranchId = @branchId" : string.Empty;
            pre += eventCode.HasValue ? " AND EventCode = @eventCode" : string.Empty;
            pre += beginDate.HasValue ? " AND CreateDate >= @beginDate" : string.Empty;
            pre += endDate.HasValue ? " AND CreateDate <= @endDate" : string.Empty;
            pre += entityType.HasValue ? " AND EntityType = @entityType" : string.Empty;
            pre += entityId.HasValue ? " AND EntityId = @entityId" : string.Empty;

            var condition = listQuery.Like(pre, "UserName", "BranchName", "Address");

            var eventCodes = new List<int>();
            var eventStatuses = new List<int>();

            if (!string.IsNullOrWhiteSpace(listQuery.Filter))
            {
                foreach (EventCode value in Enum.GetValues(typeof(EventCode)))
                {
                    var item = value.GetDisplayName().ToLower();
                    if(item.Contains(listQuery.Filter.ToLower()))
                    eventCodes.Add((int) value);
                }

                foreach (EventStatus value in Enum.GetValues(typeof(EventStatus)))
                {
                    var item = value.GetDisplayName().ToLower();
                    if (item.Contains(listQuery.Filter.ToLower()))
                        eventStatuses.Add((int)value);
                }
            }

            if (eventCodes.Any())
                condition = condition.Replace(")", " OR EventCode IN @eventCodes)");

            if (eventStatuses.Any())
                condition = condition.Replace(")", " OR EventStatus IN @eventStatuses)");

            var order = listQuery.Order(string.Empty, new Sort
            {
                Name = "CreateDate",
                Direction = SortDirection.Desc
            });
            var page = listQuery.Page();

            return UnitOfWork.Session.Query<EventLogItem>($@"
            SELECT *
            FROM EventLogItems
            {condition} {order} {page}", new 
                {
                    branchId,
                    eventCode,
                    beginDate,
                    endDate,
                    entityType,
                    entityId,
                    listQuery.Page?.Offset,
                    listQuery.Page?.Limit,
                    listQuery.Filter,
                    eventCodes,
                    eventStatuses
            }).ToList();
        }

        public int Count(ListQuery listQuery, object query = null)
        {
            if (listQuery == null) throw new ArgumentNullException(nameof(listQuery));

            var branchId = query?.Val<int?>("BranchId");
            var eventCode = query?.Val<EventCode?>("EventCode");
            var beginDate = query?.Val<DateTime?>("BeginDate");
            var endDate = query?.Val<DateTime?>("EndDate");
            var entityType = query?.Val<EntityType?>("EntityType");
            var entityId = query?.Val<int?>("EntityId");

            var pre = "Id <> 0";
            pre += branchId.HasValue ? " AND BranchId = @branchId" : string.Empty;
            pre += eventCode.HasValue ? " AND EventCode = @eventCode" : string.Empty;
            pre += beginDate.HasValue ? " AND CreateDate >= @beginDate" : string.Empty;
            pre += endDate.HasValue ? " AND CreateDate <= @endDate" : string.Empty;
            pre += entityType.HasValue ? " AND EntityType = @entityType" : string.Empty;
            pre += entityId.HasValue ? " AND EntityId = @entityId" : string.Empty;

            var condition = listQuery.Like(pre, "UserName", "BranchName", "Address");

            var eventCodes = new List<int>();
            var eventStatuses = new List<int>();

            if (!string.IsNullOrWhiteSpace(listQuery.Filter))
            {
                foreach (EventCode value in Enum.GetValues(typeof(EventCode)))
                {
                    var item = value.GetDisplayName().ToLower();
                    if (item.Contains(listQuery.Filter.ToLower()))
                        eventCodes.Add((int)value);
                }

                foreach (EventStatus value in Enum.GetValues(typeof(EventStatus)))
                {
                    var item = value.GetDisplayName().ToLower();
                    if (item.Contains(listQuery.Filter.ToLower()))
                        eventStatuses.Add((int)value);
                }
            }

            if (eventCodes.Any())
                condition = condition.Replace(")", " OR EventCode IN @eventCodes)");

            if (eventStatuses.Any())
                condition = condition.Replace(")", " OR EventStatus IN @eventStatuses)");   

            return UnitOfWork.Session.ExecuteScalar<int>($@"
SELECT COUNT(*)
FROM EventLogItems
{condition}", new
            {
                branchId,
                eventCode,
                beginDate,
                endDate,
                entityType,
                entityId,
                listQuery.Filter,
                eventCodes,
                eventStatuses
            });
        }
    }
}