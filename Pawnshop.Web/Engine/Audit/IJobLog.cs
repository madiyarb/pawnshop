using Pawnshop.Core;
using Pawnshop.Data.Models.Audit;

namespace Pawnshop.Web.Engine.Audit
{
    public interface IJobLog
    {
        void Log(string JobName, JobCode code, JobStatus status, EntityType entityType, int? entityId, string requestData, string responseData);
    }
}