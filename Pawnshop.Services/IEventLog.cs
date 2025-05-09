using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Pawnshop.Core;
using Pawnshop.Data.Models.Audit;

namespace Pawnshop.Services
{
    public interface IEventLog : IService<EventLogItem>
    {
        void Log(EventCode code, EventStatus status, EntityType? entityType, int? entityId = null,
            string requestData = null, string responseData = null, string uri = null, int? userId = null, int? branchId = null);

        Task LogAsync(
            EventCode code,
            EventStatus status,
            EntityType? entityType,
            int? entityId = null,
            string requestData = null,
            string responseData = null,
            string uri = null,
            int? userId = null,
            int? branchId = null);
    }
}
