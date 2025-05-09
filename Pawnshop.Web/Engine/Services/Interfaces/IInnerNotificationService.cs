using Pawnshop.Data.Models.InnerNotifications;
using Pawnshop.Data.Models.Notifications.NotificationTemplates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Web.Engine.Services.Interfaces
{
    public interface IInnerNotificationService
    {
        InnerNotification CreateNotification(int contractId, NotificationPaymentType type, decimal? notEnoughSum = null);
    }
}
