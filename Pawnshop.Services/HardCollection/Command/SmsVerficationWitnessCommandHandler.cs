using MediatR;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.MobileApp.HardCollection.Commands;
using System;
using System.Threading.Tasks;
using System.Threading;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.MobileApp.HardCollection.Entities;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;
using Newtonsoft.Json;

namespace Pawnshop.Services.HardCollection.Command
{
    public class SmsVerficationWitnessCommandHandler: IRequestHandler<SmsVerficationWitnessCommand, bool>
    {
        private readonly IMediator _mediator;
        private readonly VerificationRepository _verificationRepository;

        public SmsVerficationWitnessCommandHandler(IMediator mediator, VerificationRepository verificationRepository)
        {
            _mediator = mediator;
            _verificationRepository = verificationRepository;
        }

        public async Task<bool> Handle(SmsVerficationWitnessCommand request, CancellationToken cancellationToken = default)
        {
            try
            {
                await _mediator.Send(new CheckIsContractInHardCollectionQuery() { ContractId = request.ContractId });

                var verification = _verificationRepository.GetLastVerification(request.ClientId);

                if (verification.ExpireDate <= DateTime.Now)
                    throw new PawnshopApplicationException("Верификация истекла, создайте новую");
                if (verification.ActivationDate.HasValue)
                    throw new PawnshopApplicationException("Верификация уже активирована, создайте новую");
                if (verification.OTP != request.OTP)
                    throw new PawnshopApplicationException("Не совпадает код верификации");

                verification.ActivationDate = DateTime.Now;
                _verificationRepository.Update(verification);

                var notification = new HardCollectionNotification()
                {
                    HistoryNotification = request.GetHistoryNotification(),
                    LogNotification = request.GetLogNotifications("Подтверждение акта сверки ОТП кодом свидетелем")
                };
                await _mediator.Publish(notification);

                return true;
            }
            catch (Exception ex)
            {
                var notification = new HardCollectionNotification()
                {
                    LogNotification = request.GetLogNotifications("Ошибка при подтвердждений ОТП кодом акта сверки свидетелем", ex, Data.Models.Audit.EventStatus.Failed)
                };
                await _mediator.Publish(notification);

                return false;
            }
        }
    }
}
