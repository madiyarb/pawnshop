using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.InnerNotifications
{
    /// <summary>
    /// Уведомление сотрудников
    /// </summary>
    public class InnerNotification : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Сообщение
        /// </summary>
        [Required(ErrorMessage = "Поле сообщение обязательно для заполнения")]
        public string Message { get; set; }

        /// <summary>
        /// Создатель
        /// </summary>
        [RequiredId(ErrorMessage = "Поле Создатель обязательно для заполнения")]
        public int CreatedBy { get; set; }

        /// <summary>
        /// Создатель
        /// </summary>
        
        public User Author { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        [RequiredDate(ErrorMessage = "Поле дата создания обязательно для заполнения")]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Филиал получивший уведомление
        /// </summary>
        public int? ReceiveBranchId { get; set; }

        /// <summary>
        /// Пользователь получивший уведомление
        /// </summary>
        public int? ReceiveUserId { get; set; }

        /// <summary>
        /// Тип связанной сущности
        /// </summary>
        public EntityType EntityType { get; set; }

        /// <summary>
        /// Идентификатор связанной сущности
        /// </summary>
        public int? EntityId { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Статус уведомления
        /// </summary>
        public InnerNotificationStatus Status { get; set; }

        /// <summary>
        /// Видевшие сообщение пользователи
        /// </summary>
        public List<InnerNotificationReadUser> ReadenByUsers { get; set; }


    }
}
