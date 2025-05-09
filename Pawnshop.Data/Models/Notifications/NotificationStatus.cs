using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Notifications
{
    public enum NotificationStatus : short
    {
        /// <summary>
        /// Черновик
        /// </summary>
        [Display(Name = "Черновик")]
        Draft = 0,
        
        /// <summary>
        /// Для отправки
        /// </summary>
        [Display(Name = "Для отправки")]
        ForSend = 10,

        /// <summary>
        /// Отправляется
        /// </summary>
        [Display(Name = "Отправляется")]
        Sending = 11,

        /// <summary>
        /// Отправлено
        /// </summary>
        [Display(Name = "Отправлено")]
        Sent = 20,

        /// <summary>
        /// Доставлено
        /// </summary>
        [Display(Name = "Доставлено")]
        Delivered = 30,

        /// <summary>
        /// Частично доставлено
        /// </summary>
        [Display(Name = "Частично доставлено")]
        PartiallyDelivered = 40,

        /// <summary>
        /// Не отправлено
        /// </summary>
        [Display(Name = "Не доставлено")]
        NotDelivered = 50,

        /// <summary>
        /// Истек
        /// </summary>
        [Display(Name = "Истек")]
        Expired = 60
    }
}