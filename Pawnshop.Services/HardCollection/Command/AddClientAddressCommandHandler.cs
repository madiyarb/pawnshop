using MediatR;
using Pawnshop.Data.Access;
using System;
using System.Threading.Tasks;
using System.Threading;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;

namespace Pawnshop.Services.HardCollection.Command
{
    public class AddClientAddressCommandHandler : IRequestHandler<AddClientAddressCommand, int>
    {
        private readonly ClientRepository _clientRepository;
        private readonly IMediator _mediator;

        public AddClientAddressCommandHandler(ClientRepository clientRepository, IMediator mediator)
        {
            _clientRepository = clientRepository;
            _mediator = mediator;
        }

        public async Task<int> Handle(AddClientAddressCommand request, CancellationToken cancellationToken = default)
        {
            try
            {
                await _mediator.Send(new CheckInHardCollectionContractsQuery() { ContractId = request.ContractId, ClientId = request.ClientId });
                
                var address = (ClientAddress)request;
                var client = _clientRepository.Get(address.ClientId);
                foreach (var addr in client.Addresses)
                {
                    if (addr.AddressTypeId == address.AddressTypeId)
                    {
                        addr.IsActual = false;
                        address.IsActual = true;
                    }
                }
                client.Addresses.Add(address);
                _clientRepository.Update(client);

                if (address.Id > 0)
                {
                    var notification = new HardCollectionNotification()
                    {
                        HistoryNotification = request.GetHistoryNotification(address.Id),
                        LogNotification = request.GetLogNotifications($"Адрес успешно сохранен Id={address.Id}")
                    };

                    await _mediator.Publish(notification);

                    return address.Id;
                }
                else
                {
                    throw new PawnshopApplicationException($"Адрес клиента с clientId={request.ClientId} не сохранен");
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
