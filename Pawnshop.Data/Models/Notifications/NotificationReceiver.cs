using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Data.Models.Notifications
{
    /// <summary>
    /// Получатель уведомления
    /// </summary>
    public class NotificationReceiver : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Уведомление
        /// </summary>
        [RequiredId(ErrorMessage = "Поле уведомление обязательно для заполнения")]
        public int NotificationId { get; set; }

        /// <summary>
        /// Уведомление
        /// </summary>
        public Notification Notification { get; set; }

        /// <summary>
        /// Клиент
        /// </summary>
        [RequiredId(ErrorMessage = "Поле клиент обязательно для заполнения")]
        public int ClientId { get; set; }

        /// <summary>
        /// Клиент
        /// </summary>
        public Client Client { get; set; }

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
        /// Количество попыток
        /// </summary>
        public int TryCount { get; set; }

        /// <summary>
        /// Адрес
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Id контракта
        /// </summary>
        public int? ContractId { get; set; }

        /// <summary>
        /// Отправлен в
        /// </summary>
        public DateTime? SentAt { get; set; }

        /// <summary>
        /// Доставлен в
        /// </summary>
        public DateTime? DeliveredAt { get; set; }

        // ниже находится поля которые не относятся к полям таблицы
        /// <summary>
        /// Контракт
        /// </summary>
        public Contract Contract { get; set; }


        /// <summary>
        /// Журнал отправки уведомления получателю
        /// </summary>
        public List<NotificationLog> Logs { get; set; } = new List<NotificationLog>();

        /// <summary>
        /// Идентификатор сообщения для КазИнфоТех
        /// </summary>
        public int? MessageId { get; set; }
    }
}