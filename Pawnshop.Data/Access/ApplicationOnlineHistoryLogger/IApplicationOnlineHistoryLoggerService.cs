using System.Threading.Tasks;
using Pawnshop.Data.Models.ApplicationOnlineCarLogItems;
using Pawnshop.Data.Models.ApplicationOnlineFileLogItems;
using Pawnshop.Data.Models.ApplicationOnlineLog;
using Pawnshop.Data.Models.ClientAdditionalContactLogItems;
using Pawnshop.Data.Models.ClientAddressLogItems;
using Pawnshop.Data.Models.ClientDocumentLogItems;
using Pawnshop.Data.Models.ClientLogItems;
using Pawnshop.Data.Models.ClientRequisiteLogItems;

namespace Pawnshop.Data.Access.ApplicationOnlineHistoryLogger
{
    public interface IApplicationOnlineHistoryLoggerService
    {
        public Task LogApplicationOnline(ApplicationOnlineLogData applicationOnlineLogData, int? userId);
        public void LogClientData(ClientLogData data, int? userId);
        public void LogClientDocumentData(ClientDocumentLogData data, int? userId);
        public void LogApplicationOnlineCarData(ApplicationOnlineCarLogData data, int? userId);
        public void LogClientAddressData(ClientAddressLogData data, int? userId);
        public void LogClientRequisiteData(ClientRequisiteLogData data, int? userId);
        public Task LogApplicationOnlineFileData(ApplicationOnlineFileLogData data, int? userId);
        public void LogClientAdditionalContactData(ClientAdditionalContactLogData data, int? userId);
    }
}
