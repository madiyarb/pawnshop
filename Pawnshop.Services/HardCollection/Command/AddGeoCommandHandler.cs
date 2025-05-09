using MediatR;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.MobileApp.HardCollection.Commands;
using System;
using System.Threading.Tasks;
using System.Threading;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;

namespace Pawnshop.Services.HardCollection.Command
{
    public class AddGeoCommandHandler : IRequestHandler<AddGeoCommand, int>
    {
        private readonly HCGeoDataRepository _geoDataRepository;
        private readonly IMediator _mediator;
        public AddGeoCommandHandler(HCGeoDataRepository geoDataRepository, IMediator mediator)
        {
            _geoDataRepository = geoDataRepository;
            _mediator = mediator;
        }

        public async Task<int> Handle(AddGeoCommand request, CancellationToken cancellationToken = default)
        {
            try
            {
                _geoDataRepository.Insert(request.GeoData);

                var historyNotification = new HardCollectionNotification()
                {
                    LogNotification = request.GetLogNotifications($"Координаты по договору HistoryId={request.GeoData.HCActionHistoryId} сохранен")
                };
                await _mediator.Publish(historyNotification);

                return request.GeoData.Id;
            }
            catch (Exception ex) 
            {
                var historyNotification = new HardCollectionNotification()
                {
                    LogNotification = request.GetLogNotifications(ex.Message, ex, Data.Models.Audit.EventStatus.Failed)
                };
                await _mediator.Publish(historyNotification);

                throw;
            }
        }
    }
}
