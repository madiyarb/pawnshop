using System;
using Pawnshop.Data.Models.Notifications;

namespace Pawnshop.Web.Models.Notification
{
    public class NotificationListQueryModel
    {
        public int? BranchId { get; set; }
        
        public int? ClientId { get; set; }

        public DateTime? BeginDate { get; set; }

        public DateTime? EndDate { get; set; }

        public MessageType? MessageType { get; set; }

        public NotificationStatus? Status { get; set; }
    }
}