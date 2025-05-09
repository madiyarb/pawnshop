using System.Threading.Tasks;
using System.Threading;
using MediatR;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.MobileApp.HardCollection.Commands;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using System;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;
using Pawnshop.Core.Exceptions;

namespace Pawnshop.Services.HardCollection.Command
{
    public class AddClientAdditionalContactCommandHandler : IRequestHandler<AddClientAdditionalContactCommand, int>
    {
        private readonly ClientAdditionalContactRepository _clientContactRepository;
        private readonly IMediator _mediator;

        public AddClientAdditionalContactCommandHandler(ClientAdditionalContactRepository clientContactRepository, IMediator mediator)
        {
            _clientContactRepository = clientContactRepository;
            _mediator = mediator;
        }

        public async Task<int> Handle(AddClientAdditionalContactCommand request, CancellationToken cancellationToken = default)
        {
            try
            {
                await _mediator.Send(new CheckInHardCollectionContractsQuery() { ContractId = request.ContractId, ClientId = request.ClientId });

                var contact = (ClientAdditionalContact)request;
                _clientContactRepository.Insert(contact);

                if (contact.Id > 0)
                {
                    var notification = new HardCollectionNotification()
                    {
                        HistoryNotification = request.GetHistoryNotification(contact.Id),
                        LogNotification = request.GetLogNotifications($"Дополнительный контакт успешно сохранен Id={contact.Id}")
                    };
                    await _mediator.Publish(notification);

                    return contact.Id;
                }
                else
                {
                    throw new PawnshopApplicationException($"Дополнительный контакт по клиенту с clientId={request.ClientId} не сохранен");
                }
            }
            catch(Exception ex)
            {
                var notification = new HardCollectionNotification()
                {
                    LogNotification = request.GetLogNotifications(ex.Message, ex, Data.Models.Audit.EventStatus.Failed)
                };
                await _mediator.Publish(notification);

                throw ex;
            }
        }
    }
}
