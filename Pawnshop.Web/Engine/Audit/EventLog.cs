using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Audit;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Services;
using Pawnshop.Services.Audit;

namespace Pawnshop.Web.Engine.Audit
{
    public class EventLog : IEventLog
    {
        private readonly ISessionContext _sessionContext;
        private readonly BranchContext _branchContext;
        private readonly IHttpContextAccessor _accessor;
        private readonly EventLogService _eventLogService;
        private readonly UserRepository _userRepository;

        public EventLog(ISessionContext sessionContext, BranchContext branchContext, IHttpContextAccessor accessor, EventLogService eventLogService, UserRepository userRepository)
        {
            _sessionContext = sessionContext;
            _branchContext = branchContext;
            _accessor = accessor;
            _eventLogService = eventLogService;
            _userRepository = userRepository;
        }

        public void Log(EventCode code, EventStatus status, EntityType? entityType, int? entityId = null, string requestData = null, string responseData = null, string uri = null, int ? userId = null, int? branchId = null)
        {
            var user = new User();
            if (userId.HasValue)
                user = _userRepository.Get(userId.Value);

            var item = new EventLogItem();
            item.EventCode = code;
            item.EventStatus = status;

            item.UserId = _sessionContext.IsInitialized ? _sessionContext.UserId : userId.HasValue ? userId : null;
            item.UserName = _sessionContext.IsInitialized ? _sessionContext.UserName : user?.Login;
            item.BranchId = _branchContext.InBranch ? (int?) _branchContext.Branch.Id : null;
            item.BranchName = _branchContext.InBranch ? _branchContext.Branch.Name : null;
            item.Uri = (_accessor.HttpContext != null) ? _accessor.HttpContext.Request.GetEncodedPathAndQuery() : uri ?? "Local"; 
            item.Address = _accessor.HttpContext != null ? _accessor.HttpContext.Connection.RemoteIpAddress.ToString() : "Local";

            item.EntityType = entityType;
            item.EntityId = entityId;
            item.RequestData = requestData;
            item.ResponseData = responseData;

            item.CreateDate = DateTime.Now;
            _eventLogService.Save(item);
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
                user = await _userRepository.GetAsync(userId.Value);

            var item = new EventLogItem
            {
                EventCode = code,
                EventStatus = status,
                UserId = _sessionContext.IsInitialized ? _sessionContext.UserId : userId.HasValue ? userId : null,
                UserName = _sessionContext.IsInitialized ? _sessionContext.UserName : user?.Login,
                BranchId = _branchContext.InBranch ? (int?) _branchContext.Branch.Id : null,
                BranchName = _branchContext.InBranch ? _branchContext.Branch.Name : null,
                Uri = (_accessor.HttpContext != null) ? _accessor.HttpContext.Request.GetEncodedPathAndQuery() : uri ?? "Local",
                Address = _accessor.HttpContext != null ? _accessor.HttpContext.Connection.RemoteIpAddress.ToString() : "Local",
                EntityType = entityType,
                EntityId = entityId,
                RequestData = requestData,
                ResponseData = responseData,
                CreateDate = DateTime.Now
            };

            await _eventLogService.SaveAsync(item);
        }
    }
}