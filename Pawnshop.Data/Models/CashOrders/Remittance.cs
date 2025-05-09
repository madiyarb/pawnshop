using System;
using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.CashOrders
{
    /// <summary>
    /// Денежный перевод между филиалами
    /// </summary>
    public class Remittance : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Отправивший филиал
        /// </summary>
        public int SendBranchId { get; set; }

        /// <summary>
        /// Отправивший филиал
        /// </summary>
        public Group SendBranch { get; set; }

        /// <summary>
        /// Отправивший пользователь
        /// </summary>
        public int SendUserId { get; set; }

        /// <summary>
        /// Отправивший пользователь
        /// </summary>
        public User SendUser { get; set; }

        /// <summary>
        /// Дата отправки
        /// </summary>
        [RequiredDate(ErrorMessage = "Поле дата отправки обязательно для заполнения")]
        public DateTime SendDate { get; set; }

        /// <summary>
        /// Отправленная сумма
        /// </summary>
        public int SendCost { get; set; }

        /// <summary>
        /// Расходный кассовый ордер
        /// </summary>
        public int? SendOrderId { get; set; }

        /// <summary>
        /// Получивший филиал
        /// </summary>
        [RequiredId(ErrorMessage = "Поле получивший филиал обязательно для заполнения")]
        public int ReceiveBranchId { get; set; }

        /// <summary>
        /// Получивший филиал
        /// </summary>
        public Group ReceiveBranch { get; set; }

        /// <summary>
        /// Получивший пользователь
        /// </summary>
        public int? ReceiveUserId { get; set; }

        /// <summary>
        /// Получивший пользователь
        /// </summary>
        public User ReceiveUser { get; set; }

        /// <summary>
        /// Дата получения
        /// </summary>
        public DateTime? ReceiveDate { get; set; }

        /// <summary>
        /// Приходный кассовый ордер
        /// </summary>
        public int? ReceiveOrderId { get; set; }

        /// <summary>
        /// Примечание
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        public RemittanceStatusType Status { get; set; } = RemittanceStatusType.Sent;

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// True, если перевод создан сегодня
        /// </summary>
        public bool CreatedToday => CreateDate.Date == DateTime.Now.Date;

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Перевод порожден из договора
        /// </summary>
        public int? ContractId { get; set; }

        /// <summary>
        /// Уведомление о переводе
        /// </summary>
        public int? InnerNotificationId { get; set; }
    }
}
