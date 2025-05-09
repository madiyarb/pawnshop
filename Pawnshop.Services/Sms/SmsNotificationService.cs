using Pawnshop.Core;
using Pawnshop.Data.Access;
using Pawnshop.Data.Models.Notifications;
using Pawnshop.Data.Models.Notifications.Interfaces;
using Pawnshop.Data.Models.Sms;
using System;

namespace Pawnshop.Services.Sms
{
    public class SmsNotificationService : ISmsNotificationService
    {
        private readonly NotificationRepository _notificationRepository;
        private readonly NotificationReceiverRepository _notificationReceiverRepository;

        public SmsNotificationService(
            NotificationRepository notificationRepository, 
            NotificationReceiverRepository notificationReceiverRepository)
        {
            _notificationRepository = notificationRepository;
            _notificationReceiverRepository = notificationReceiverRepository;
        }

        public bool CreateSmsNotification(SmsCreateNotificationModel sms)
        {
            if(sms == null)
            {
                return false;
            }

            using (var transaction = _notificationReceiverRepository.BeginTransaction())
            {
                Notification notification = new Notification()
                {
                    BranchId = sms.BranchId,
                    CreateDate = DateTime.Now,
                    Subject = sms.Subject,
                    Message = sms.Message,
                    MessageType = MessageType.Sms,
                    Status = NotificationStatus.ForSend,
                    IsPrivate = true,
                    UserId = Constants.ADMINISTRATOR_IDENTITY
                };
                _notificationRepository.Insert(notification);

                NotificationReceiver notificationReceiver = new NotificationReceiver()
                {
                    ContractId = sms.ContractId,
                    NotificationId = notification.Id,
                    ClientId = sms.ClientId,
                    CreateDate = DateTime.Now,
                    Status = NotificationStatus.ForSend
                };
                _notificationReceiverRepository.Insert(notificationReceiver);

                transaction.Commit();
            }
                
            return true;
        }
    }
}
