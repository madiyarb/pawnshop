using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Pawnshop.Core;
using Pawnshop.Core.Queries;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services.Models.Filters;
using Pawnshop.Services.Models.List;

namespace Pawnshop.Services.Audit
{
    public class EventLogService : IDictionaryWithSearchService<EventLogItem, EventLogFilter>, IEventLog
    {
        private readonly EventLogRepository _repository;
        private readonly UserRepository _userRepository;
        private readonly GroupRepository _groupRepository;
        private readonly ISessionContext _sessionContext;

        public EventLogService(EventLogRepository repository, UserRepository userRepository, GroupRepository groupRepository, ISessionContext sessionContext)
        {
            _repository = repository;
            _userRepository = userRepository;
            _groupRepository = groupRepository;
            _sessionContext = sessionContext;
        }

        public void Delete(int id)
        {
            _repository.Delete(id);
        }

        public Task<EventLogItem> GetAsync(int id)
        {
            return _repository.GetAsync(id);
        }

        public ListModel<EventLogItem> List(ListQueryModel<EventLogFilter> listQuery)
        {
            return new ListModel<EventLogItem>
            {
                Count = _repository.Count(listQuery, listQuery.Model),
                List = _repository.List(listQuery, listQuery.Model)
            };
        }

        public ListModel<EventLogItem> List(ListQuery listQuery)
        {
            return new ListModel<EventLogItem>
            {
                Count = _repository.Count(listQuery),
                List = _repository.List(listQuery)
            };
        }

        public void Log(EventCode code, EventStatus status, EntityType? entityType, int? entityId = null, string requestData = null, string responseData = null, string uri = null, int? userId = null, int? branchId = null)
        {
            var user = new User();
            if (userId.HasValue)
                user = _userRepository.Get(userId.Value);

            var item = new EventLogItem
            {
                EventCode = code,
                EventStatus = status,
                Uri = "Local",
                Address = "Local",
                EntityType = entityType,
                EntityId = entityId,
                RequestData = requestData,
                ResponseData = responseData,
                CreateDate = DateTime.Now
            };

            item.UserId = _sessionContext.IsInitialized ? _sessionContext.UserId : userId.HasValue ? userId : null;
            item.UserName = _sessionContext.IsInitialized ? _sessionContext.UserName : user?.Login;

            if (branchId.HasValue) 
            {
				item.BranchId = branchId.Value;
                item.BranchName = _groupRepository.Get(branchId.Value).Name;
            }
            Save(item);
        }

        public async Task LogAsync(
            EventCode code,
            EventStatus status,
            EntityType? entityType,
            int? entityId = null,
            string requestData = null,
            string responseData = null,
            string uri = null,
            int? userId = null,
            int? branchId = null)
        {
            var user = new User();
            if (userId.HasValue)
            {
                user = await _userRepository.GetAsync(userId.Value);
            }

            var item = new EventLogItem
            {
                EventCode = code,
                EventStatus = status,
                Uri = "Local",
                Address = "Local",
                EntityType = entityType,
                EntityId = entityId,
                RequestData = requestData,
                ResponseData = responseData,
                CreateDate = DateTime.Now,
                UserId = _sessionContext.IsInitialized ? _sessionContext.UserId : userId.HasValue ? userId : null,
                UserName = _sessionContext.IsInitialized ? _sessionContext.UserName : user?.Login
            };

            if (branchId.HasValue) 
            {
                item.BranchId = branchId.Value;
                item.BranchName = (await _groupRepository.GetAsync(branchId.Value)).Name;
            }
            
            await SaveAsync(item);
        }

        public async Task<EventLogItem> SaveAsync(EventLogItem model)
        {
            await _repository.InsertAsync(model);
            return model;
        }

        public EventLogItem Save(EventLogItem model)
        {
            _repository.Insert(model);
            return model;
        }
    }
}
