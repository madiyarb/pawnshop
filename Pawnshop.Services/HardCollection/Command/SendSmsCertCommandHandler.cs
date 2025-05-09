using MediatR;
using Pawnshop.Data.Models.MobileApp.HardCollection.Commands;
using System;
using System.Threading.Tasks;
using System.Threading;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;

namespace Pawnshop.Services.HardCollection.Command
{
    public class SendSmsCertCommandHandler : IRequestHandler<SendSmsCertCommand, bool>
    {
        private readonly IMediator _mediator;

        public SendSmsCertCommandHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<bool> Handle(SendSmsCertCommand request, CancellationToken cancellationToken = default)
        {
            try
            {
                await _mediator.Send(new CheckInHardCollectionContractsQuery() { ContractId = request.ContractId, ClientId = request.ClientId });

                var notification = new HardCollectionNotification()
                {
                    HistoryNotification = request.GetHistoryNotification(request.VerificationId),
                    LogNotification = request.GetLogNotifications($"Отправка смс для подтверждения акта сверки с ClientId={request.ClientId} и ContractId={request.ContractId}")
                };
                await _mediator.Publish(notification);

                return true;
            }
            catch (Exception ex) 
            {
                var notification = new HardCollectionNotification()
                {
                    LogNotification = request.GetLogNotifications($"Ошибка при отправка смс для подтверждения акта сверки с ClientId={request.ClientId} и ContractId={request.ContractId}", ex, Data.Models.Audit.EventStatus.Failed)
                };
                await _mediator.Publish(notification);

                throw;
            }
        }
    }
}
