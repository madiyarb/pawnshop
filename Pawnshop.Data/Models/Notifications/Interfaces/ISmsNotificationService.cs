using Pawnshop.Data.Models.Notifications.NotificationTemplates;
using Pawnshop.Data.Models.Sms;

namespace Pawnshop.Data.Models.Notifications.Interfaces
{
    public interface ISmsNotificationService
    {
        bool CreateSmsNotification(SmsCreateNotificationModel sms);
    }
}
