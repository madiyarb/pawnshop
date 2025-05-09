using MediatR;
using Pawnshop.Data.Models.MobileApp.HardCollection.Commands;
using System.Threading.Tasks;
using System.Threading;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using System;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;

namespace Pawnshop.Services.HardCollection.Command
{
    public class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, bool>
    {
        private readonly IMediator _mediator;
        public AddCommentCommandHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<bool> Handle(AddCommentCommand request, CancellationToken cancellationToken = default)
        {
            try
            {
                await _mediator.Send(new CheckIsContractInHardCollectionQuery() { ContractId = request.ContractId });

                var notification = new HardCollectionNotification()
                {
                    HistoryNotification = request.GetHistoryNotification(),
                    LogNotification = request.GetLogNotifications($"Комментарий по договору ContractId={ request.ContractId } сохранен")
                };
                await _mediator.Publish(notification);

                return true;
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
