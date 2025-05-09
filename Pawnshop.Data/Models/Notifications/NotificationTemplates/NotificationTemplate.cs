using System;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Notifications.NotificationTemplates
{
    /// <summary>
    /// Уведомление для оплат
    /// </summary>
    public class NotificationTemplate : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Тема уведомление
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Текст уведомление
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Тип уведомление
        /// </summary>
        public MessageType MessageType { get; set; } = MessageType.Sms;

        /// <summary>
        /// Тип действия для уведомление
        /// </summary>
        public NotificationPaymentType NotificationPaymentType { get; set; } = NotificationPaymentType.Draft;

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Активность уведомление
        /// </summary>
        public bool IsActive { get; set; } = false;

        /// <summary>
        /// Описание уведомление
        /// </summary>
        public string Note { get; set; }
    }
}
