using System.Threading.Tasks;
using Pawnshop.Data.Helpers;
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
    public sealed class ApplicationOnlineHistoryLoggerService : IApplicationOnlineHistoryLoggerService
    {
        private readonly ApplicationOnlineLogItemsRepository _applicationOnlineLogItemsRepository;
        private readonly ClientLogItemsRepository _clientLogItemsRepository;
        private readonly ClientDocumentLogItemsRepository _clientDocumentLogItemsRepository;
        private readonly ApplicationOnlineCarLogItemRepository _applicationOnlineCarLogItemRepository;
        private readonly ClientAddressLogItemsRepository _clientAddressLogItemsRepository;
        private readonly ClientRequisiteLogItemsRepository _clientRequisiteLogItemsRepository;
        private readonly ApplicationOnlineFileLogItemsRepository _applicationOnlineFileLogItemsRepository;
        private readonly ClientAdditionalContactLogItemsRepository _clientAdditionalContactLogItemsRepository;
        public ApplicationOnlineHistoryLoggerService(
            ApplicationOnlineLogItemsRepository applicationOnlineLogItemsRepository,
            ClientLogItemsRepository clientLogItemsRepository,
            ClientDocumentLogItemsRepository clientDocumentLogItemsRepository,
            ApplicationOnlineCarLogItemRepository applicationOnlineCarLogItemRepository,
            ClientAddressLogItemsRepository clientAddressLogItemsRepository,
            ClientRequisiteLogItemsRepository clientRequisiteLogItemsRepository,
            ApplicationOnlineFileLogItemsRepository applicationOnlineFileLogItemsRepository,
            ClientAdditionalContactLogItemsRepository clientAdditionalContactLogItemsRepository)
        {
            _applicationOnlineLogItemsRepository = applicationOnlineLogItemsRepository;
            _clientLogItemsRepository = clientLogItemsRepository;
            _clientDocumentLogItemsRepository = clientDocumentLogItemsRepository;
            _applicationOnlineCarLogItemRepository = applicationOnlineCarLogItemRepository;
            _clientAddressLogItemsRepository = clientAddressLogItemsRepository;
            _clientRequisiteLogItemsRepository = clientRequisiteLogItemsRepository;
            _applicationOnlineFileLogItemsRepository = applicationOnlineFileLogItemsRepository;
            _clientAdditionalContactLogItemsRepository = clientAdditionalContactLogItemsRepository;
        }

        public async Task LogApplicationOnline(ApplicationOnlineLogData applicationOnlineLogData, int? userId)
        {
            await _applicationOnlineLogItemsRepository.Insert(new ApplicationOnlineLogItem(applicationOnlineLogData,
                userId));
        }

        public void LogClientData(ClientLogData data, int? userId)
        {
            _clientLogItemsRepository.Insert(new ClientLogItem(data, userId));
        }

        public void LogClientDocumentData(ClientDocumentLogData data, int? userId)
        {
            _clientDocumentLogItemsRepository.Insert(new ClientDocumentLogItem(data, userId));
        }

        public void LogApplicationOnlineCarData(ApplicationOnlineCarLogData data, int? userId)
        {
            _applicationOnlineCarLogItemRepository.Insert(new ApplicationOnlineCarLogItem(data, userId));
        }

        public void LogClientAddressData(ClientAddressLogData data, int? userId)
        {
            _clientAddressLogItemsRepository.Insert(new ClientAddressLogItem(data, userId));
        }

        public void LogClientRequisiteData(ClientRequisiteLogData data, int? userId)
        {
            _clientRequisiteLogItemsRepository.Insert(new ClientRequisiteLogItem(data, userId));
        }

        public async Task LogApplicationOnlineFileData(ApplicationOnlineFileLogData data, int? userId)
        {
            await _applicationOnlineFileLogItemsRepository.Insert(new ApplicationOnlineFileLogItem(data, userId));
        }

        public void LogClientAdditionalContactData(ClientAdditionalContactLogData data, int? userId)
        {
            _clientAdditionalContactLogItemsRepository.Insert(new ClientAdditionalContactLogItem(data, userId));
        }
    }
}
