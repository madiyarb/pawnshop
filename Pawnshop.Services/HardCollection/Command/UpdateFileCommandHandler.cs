using MediatR;
using Pawnshop.Data.Models.MobileApp.HardCollection.Commands;
using Pawnshop.Data.Models.MobileApp.HardCollection.Entities;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using System;
using System.Threading.Tasks;
using System.Threading;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;

namespace Pawnshop.Services.HardCollection.Command
{
    public class UpdateFileCommandHandler : IRequestHandler<UploadFileCommand, bool>
    {
        private readonly IMediator _mediator;

        public UpdateFileCommandHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<bool> Handle(UploadFileCommand request, CancellationToken cancellationToken = default)
        {
            try
            {
                await _mediator.Send(new CheckIsContractInHardCollectionQuery() { ContractId = request.ContractId });

                var notification = new HardCollectionNotification()
                {
                    HistoryNotification = request.GetHistoryNotification(request.FileRowId),
                    LogNotification = request.GetLogNotifications($"Сохранение акта сверки по ContractId={request.ContractId}")
                };
                await _mediator.Publish(notification);

                var geoData = (HCGeoData)request;
                geoData.HCActionHistoryId = notification.HistoryNotification.Id;

                await _mediator.Send(new AddGeoCommand() { GeoData = geoData });

                return true;
            }

            catch (Exception ex)
            {
                var notification = new HardCollectionNotification()
                {
                    LogNotification = request.GetLogNotifications($"Ошибка при сохранений акта сверки по ContractId={request.ContractId}", ex, Data.Models.Audit.EventStatus.Failed)
                };
                await _mediator.Publish(notification);

                return false;
            }
        }
    }
}
