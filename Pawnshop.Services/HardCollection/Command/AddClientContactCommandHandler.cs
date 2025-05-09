using MediatR;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.MobileApp.HardCollection.Commands;
using System;
using System.Threading.Tasks;
using System.Threading;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;

namespace Pawnshop.Services.HardCollection.Command
{
    public class AddClientContactCommandHandler : IRequestHandler<AddClientContactCommand, int>
    {
        private readonly ClientRepository _clientRepository;
        private readonly ClientContactRepository _clientContactRepository;
        private readonly HCContractStatusRepository _hardCollectionRepository;
        private readonly IMediator _mediator;

        public AddClientContactCommandHandler(ClientRepository clientRepository, 
            ClientContactRepository clientContactRepository, 
            HCContractStatusRepository hardCollectionRepository,
            IMediator mediator)
        {
            _clientRepository = clientRepository;
            _clientContactRepository = clientContactRepository;
            _hardCollectionRepository = hardCollectionRepository;
            _mediator = mediator;
        }

        public async Task<int> Handle(AddClientContactCommand request, CancellationToken cancellationToken = default)
        {
            try
            {
                await _mediator.Send(new CheckInHardCollectionContractsQuery() { ContractId = request.ContractId, ClientId = request.ClientId });

                var clientContact = (ClientContact)request;

                var client = _clientRepository.Get(clientContact.ClientId);
                if (client == null)
                    throw new PawnshopApplicationException($"Нет клиента с ИД {clientContact.ClientId}");

                var hardCollectionModel = _hardCollectionRepository.GetByClientId(clientContact.ClientId);
                if (hardCollectionModel == null)
                    throw new PawnshopApplicationException($"У клиента с ИД {clientContact.ClientId} нет действующих договоров в Хард Коллекшн");

                _clientContactRepository.Insert(clientContact);

                if (clientContact.Id > 0)
                {
                    var notification = new HardCollectionNotification()
                    {
                        HistoryNotification = request.GetHistoryNotification(clientContact.Id),
                        LogNotification = request.GetLogNotifications($"Актуализированный контакт Id={clientContact.Id} сохранен")
                    };
                    await _mediator.Publish(notification);

                    return clientContact.Id;
                }
                else
                {
                    throw new PawnshopApplicationException($"Актуализированный контакт по клиенту с clientId={client.Id} не сохранен");
                }
            }
            catch (Exception ex) 
            {
                var notification = new HardCollectionNotification()
                {
                    LogNotification = request.GetLogNotifications(ex.Message, ex, Data.Models.Audit.EventStatus.Failed)
                };
                await _mediator.Publish(notification);

                throw;
            }
        }
    }
}
