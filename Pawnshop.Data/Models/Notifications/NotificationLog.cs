using System;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Core;
using Pawnshop.Core.Validation;

namespace Pawnshop.Data.Models.Notifications
{
    /// <summary>
    /// Журнал отправки уведомления получателю
    /// </summary>
    public class NotificationLog : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Получатель
        /// </summary>
        [RequiredId(ErrorMessage = "Поле получатель обязательно для заполнения")]
        public int NotificationReceiverId { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        [RequiredDate(ErrorMessage = "Поле дата создания обязательно для заполнения")]
        public DateTime CreateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Сообщение статуса
        /// </summary>
        [Required(ErrorMessage = "Поле сообщение статуса обязательно для заполнения")]
        public string StatusMessage { get; set; }
    }
}