using System;
using Pawnshop.Core;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.Investments
{
    /// <summary>
    /// Действие инвестии
    /// </summary>
    public class InvestmentAction : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Инвестиция
        /// </summary>
        public int InvestmentId { get; set; }

        /// <summary>
        /// Дата действия
        /// </summary>
        public DateTime ActionDate { get; set; }

        /// <summary>
        /// Тип действия
        /// </summary>
        public InvestmentActionType ActionType { get; set; }

        /// <summary>
        /// Сумма
        /// </summary>
        public int ActionCost { get; set; }

        /// <summary>
        /// Кассовый ордер
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Пользователь
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Пользователь
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
    }
}
