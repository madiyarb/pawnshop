using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.InnerNotifications
{
    public class InnerNotificationReadUser : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор уведомления
        /// </summary>
        public int InnerNotificationId { get; set; }

        /// <summary>
        /// Пользователь
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Время прочтения
        /// </summary>
        public DateTime ReadDate { get; set; }
    }
}
