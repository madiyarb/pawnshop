using MediatR;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.MobileApp.HardCollection.Commands;
using System;
using System.Threading.Tasks;
using System.Threading;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.MobileApp.HardCollection.Notifications;
using Pawnshop.Data.Models.MobileApp.HardCollection.Queries;
using Pawnshop.Data.Models.MobileApp.HardCollection.Entities;

namespace Pawnshop.Services.HardCollection.Command
{
    public class SmsVerificationCertCommandHandler : IRequestHandler<SmsVerificationCertCommand, bool>
    {
        private readonly VerificationRepository _verificationRepository;
        private readonly IMediator _mediator;

        public SmsVerificationCertCommandHandler(VerificationRepository verificationRepository, IMediator mediator)
        {
            _verificationRepository = verificationRepository;
            _mediator = mediator;
        }

        public async Task<bool> Handle(SmsVerificationCertCommand request, CancellationToken cancellationToken = default)
        {
            try
            {
                await _mediator.Send(new CheckInHardCollectionContractsQuery() { ContractId = request.ContractId, ClientId = request.ClientId });

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
                    LogNotification = request.GetLogNotifications($"Подтверждение акта сверки ОТП кодом свидетелем")
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
                    LogNotification = request.GetLogNotifications($"Ошибка при подтверждение акта сверки ОТП кодом свидетелем", ex, Data.Models.Audit.EventStatus.Failed)
                };
                await _mediator.Publish(notification);

                throw;
            }
        }
    }
}
