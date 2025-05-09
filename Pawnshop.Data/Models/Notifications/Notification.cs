using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Notifications.NotificationTemplates;

namespace Pawnshop.Data.Models.Notifications
{
    /// <summary>
    /// Уведомление
    /// </summary>
    public class Notification : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Тип сообщения
        /// </summary>
        [EnumDataType(typeof(MessageType), ErrorMessage = "Поле тип сообщения обязательно для заполнения")]
        public MessageType MessageType { get; set; } = MessageType.Email;

        /// <summary>
        /// Тема
        /// </summary>
        [Required(ErrorMessage = "Поле тема обязательно для заполнения")]
        public string Subject { get; set; }

        /// <summary>
        /// Сообщение
        /// </summary>
        [Required(ErrorMessage = "Поле сообщение обязательно для заполнения")]
        public string Message { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        [EnumDataType(typeof(NotificationStatus), ErrorMessage = "Поле статус обязательно для заполнения")]
        public NotificationStatus Status { get; set; } = NotificationStatus.Draft;

        /// <summary>
        /// Дата создания
        /// </summary>
        [RequiredDate(ErrorMessage = "Поле дата создания обязательно для заполнения")]
        public DateTime CreateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Филиал
        /// </summary>
        [RequiredId(ErrorMessage = "Поле филиал обязательно для заполнения")]
        public int BranchId { get; set; }

        /// <summary>
        /// Пользователь
        /// </summary>
        [RequiredId(ErrorMessage = "Поле пользователь обязательно для заполнения")]
        public int UserId { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Получатели
        /// </summary>
        public List<NotificationReceiver> Receivers { get; set; } = new List<NotificationReceiver>();

        /// <summary>
        /// Уведомление персональное(не рассылка)
        /// </summary>
        public bool IsPrivate { get; set; } = false;

        public bool IsNonSchedule { get; set; }

        /// <summary>
        /// Идентификатор действия
        /// </summary>
        public int? ContractActionId { get; set; }

        /// <summary>
        /// Действие договора 
        /// </summary>
        public ContractAction ContractAction { get; set; }

        // Ниже хранятся поля не относящиеся к полям таблицы в БД
        /// <summary>
        /// Получатель 
        /// </summary>
        public NotificationReceiver NotificationReceiver { get; set; }

        /// <summary>
        /// Тип уведомления
        /// </summary>
        public NotificationPaymentType? NotificationPaymentType { get; set; }
    }
}