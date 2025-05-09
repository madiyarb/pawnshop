using Pawnshop.Data.Models.Notifications;

namespace Pawnshop.Web.Engine.MessageSenders
{
    public class SendResult
    {
        public int ReceiverId { get; set; }
        public bool Success { get; set; }
        public string SendAddress { get; set; }
        public string StatusMessage { get; set; } = "Отправка прошла успешно";
        public NotificationStatus? NotificationStatus { get; set; }

        public int? MessageId { get; set; }
    }
}