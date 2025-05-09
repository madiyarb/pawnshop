using MediatR;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.MobileApp.HardCollection.Commands;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;
using System;
using System.Threading.Tasks;
using System.Threading;
using Pawnshop.Data.Models.MobileApp.HardCollection.Enums;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;

namespace Pawnshop.Services.HardCollection.Command
{
    public class AddToDoMyListCommandHandler : IRequestHandler<AddToDoMyListCommand, bool>
    {
        private readonly IMediator _mediator;
        private readonly HCContractStatusRepository _repository;
        public AddToDoMyListCommandHandler(IMediator mediator, HCContractStatusRepository repository)
        {
            _mediator = mediator;
            _repository = repository;
        }

        public async Task<bool> Handle(AddToDoMyListCommand request, CancellationToken cancellationToken = default)
        {
            try
            {
                await _mediator.Send(new CheckIsContractInHardCollectionQuery() { ContractId = request.ContractId });

                var hcModel = await _repository.GetByContractIdAsync(request.ContractId);
                hcModel.StageId = (int)HardCollectionStageEnum.InSearch;
                hcModel.IsActive = true;

                _repository.Update(hcModel);

                var notification = new HardCollectionNotification()
                {
                    HistoryNotification = request.GetHistoryNotification(),
                    LogNotification = request.GetLogNotifications($"Изменена стадия по договору ContractId={request.ContractId}")
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
