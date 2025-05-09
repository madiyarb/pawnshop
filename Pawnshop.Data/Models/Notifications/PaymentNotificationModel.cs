using System;

namespace Pawnshop.Data.Models.Notifications
{
    public class PaymentNotificationModel : BaseNotificationModel
    {
        public DateTime PaymentDate { get; set; }
    }
}
