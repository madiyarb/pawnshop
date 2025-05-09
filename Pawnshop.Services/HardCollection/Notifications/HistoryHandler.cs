using MediatR;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using System.Threading.Tasks;
using System.Threading;

namespace Pawnshop.Services.HardCollection.Notifications
{
    public class HistoryHandler : INotificationHandler<HardCollectionNotification>
    {
        private readonly HCActionHistoryRepository _historyRepository;
        private readonly HCContractStatusRepository _repository;

        public HistoryHandler(HCActionHistoryRepository historyRepository, HCContractStatusRepository repository)
        {
            _historyRepository = historyRepository;
            _repository = repository;
        }

        public async Task Handle(HardCollectionNotification request, CancellationToken cancellationToken = default)
        {
            if (request.HistoryNotification != null)
            {
                request.HistoryNotification.HCContractStatusId = (await _repository.GetByContractIdAsync(request.HistoryNotification.ContractId)).Id;
                await _historyRepository.InsertAsync(request.HistoryNotification);
            }
        }
    }
}
