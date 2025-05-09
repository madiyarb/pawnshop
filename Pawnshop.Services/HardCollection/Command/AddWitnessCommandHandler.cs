using MediatR;
using Pawnshop.Data.Access;
using System;
using System.Threading.Tasks;
using System.Threading;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;

namespace Pawnshop.Services.HardCollection.Command
{
    public class AddWitnessCommandHandler : IRequestHandler<AddWitnessCommand, int>
    {
        private readonly ClientRepository _clientRepository;
        private readonly IMediator _mediator;

        public AddWitnessCommandHandler(ClientRepository clientRepository, IMediator mediator)
        {
            _clientRepository = clientRepository;
            _mediator = mediator;;
        }

        public async Task<int> Handle(AddWitnessCommand request, CancellationToken cancellationToken = default)
        {
            try
            {
                await _mediator.Send(new CheckIsContractInHardCollectionQuery() { ContractId = request.ContractId });

                var newClient = (Client)request;
                var client = _clientRepository.FindByIdentityNumber(newClient.IdentityNumber);
                if (client != null)
                    newClient.Id = client.Id;
                else
                    _clientRepository.Insert(newClient);

                var notification = new HardCollectionNotification()
                {
                    HistoryNotification = request.GetHistoryNotification(newClient.Id),
                    LogNotification = request.GetLogNotifications($"Свидетель в таблицу Clients с clientId={newClient.Id} сохранен")
                };
                await _mediator.Publish(notification);

                return newClient.Id;
            }
            catch(Exception ex)
            {
                var notification = new HardCollectionNotification()
                {
                    LogNotification = request.GetLogNotifications($"Ошибка при добавлений свидетеля", ex, Data.Models.Audit.EventStatus.Failed)
                };
                await _mediator.Publish(notification);

                throw ex;
            }
        }
    }
}
