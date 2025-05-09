using Pawnshop.Data.Models.Notifications.NotificationTemplates;
using Pawnshop.Data.Models.Notifications;
using System.Threading.Tasks;

namespace Pawnshop.Services.Notifications
{
    public interface INotificationTemplateService
    {
        string GetNotificationTextByFilters(int contractId, MessageType messageType, NotificationPaymentType notificationPaymentType, decimal successPaymentCost = -1, decimal failPaymentCost = -1, decimal displayAmount = 0);
        NotificationTemplate GetTemplate(string code);
        Task<NotificationTemplate> GetTemplateAsync(string code);
        bool IsValidTemplate(NotificationTemplate template);
    }
}
