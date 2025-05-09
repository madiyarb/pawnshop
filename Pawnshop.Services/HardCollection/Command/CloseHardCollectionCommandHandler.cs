using MediatR;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.MobileApp.HardCollection.Commands;
using System.Threading.Tasks;
using System.Threading;
using Pawnshop.Data.Models.MobileApp.HardCollection.Entities;
using Pawnshop.Data.Models.MobileApp.HardCollection.Enums;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using System;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;

namespace Pawnshop.Services.HardCollection.Command
{
    public class CloseHardCollectionCommandHandler : IRequestHandler<CloseHardCollectionCommand, bool>
    {
        private readonly HCContractStatusRepository _hardCollectionRepository;
        private readonly IMediator _mediator;
        public CloseHardCollectionCommandHandler(HCContractStatusRepository hardCollectionRepository, IMediator mediator)
        {
            _hardCollectionRepository = hardCollectionRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(CloseHardCollectionCommand request, CancellationToken cancellationToken = default)
        {
            try
            {
                await _mediator.Send(new CheckIsContractInHardCollectionQuery() { ContractId = request.ContractId });

                var hcId = await _hardCollectionRepository.GetByContractIdAsync(request.ContractId);
                var model = new HCContractStatus()
                {
                    Id = hcId.Id,
                    ContractId = request.ContractId,
                    StageId = (int)HardCollectionStageEnum.Finished,
                    IsActive = false
                };

                await _hardCollectionRepository.UpdateAsync(model);

                var notification = new HardCollectionNotification()
                {
                    HistoryNotification = request.GetHistoryNotification(),
                    LogNotification = request.GetLogNotifications($"Отправка на стоянку по договору ContractId={request.ContractId}")
                };
                await _mediator.Publish(notification);

                return true;
            }
            catch(Exception ex)
            {
                var notification = new HardCollectionNotification()
                {
                    LogNotification = request.GetLogNotifications($"Ошибка при отправке на стоянку по договору ContractId={request.ContractId}", ex, Data.Models.Audit.EventStatus.Failed)
                };
                await _mediator.Publish(notification);

                throw;
            }
        }
    }
}
