using System;
namespace Pawnshop.Data.Models.Notifications
{
    public class PaymentLastNotificationModel : BaseNotificationModel
    {
        public DateTime PaymentDate { get; set; }
        public string TransportNumber { get; set; }
        public Decimal PaymentCost { get; set; }
    }
}
