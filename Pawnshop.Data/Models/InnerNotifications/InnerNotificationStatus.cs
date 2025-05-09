using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.InnerNotifications
{
    public enum InnerNotificationStatus : short
    {
        /// <summary>
        /// Отправлено
        /// </summary>
        [Display(Name = "Отправлено")]
        Sent = 0,

        /// <summary>
        /// Прочитано
        /// </summary>
        [Display(Name = "Прочитано")]
        Readen = 10,

        /// <summary>
        /// Принято
        /// </summary>
        [Display(Name = "Принято")]
        Accepted = 20
    }
}