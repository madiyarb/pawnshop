using Pawnshop.Core;
using Pawnshop.Data.Models.Dictionaries;
using System;
using Pawnshop.Data.Models.Membership;
using Pawnshop.Data.Models.Contracts;

namespace Pawnshop.Data.Models.Parking
{
    public class ParkingHistory : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Договор
        /// </summary>
        public int ContractId { get; set; }

        public Contract Contract { get; set; }

        /// <summary>
        /// Позиция
        /// </summary>
        public int PositionId { get; set; }

        public Position Position { get; set; }

        /// <summary>
        /// Статус до действия
        /// </summary>
        public int? StatusBeforeId { get; set; }

        public ParkingStatus StatusBefore { get; set; }

        /// <summary>
        /// Статус после действия
        /// </summary>
        public int StatusAfterId { get; set; }

        public ParkingStatus StatusAfter { get; set; }

        /// <summary>
        /// Пользователь
        /// </summary>
        public int UserId { get; set; }

        public User User { get; set; }

        /// <summary>
        /// Дни просрочки
        /// </summary>
        public int DelayDays { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        public string[] DocumentTypes { get; set; }

        /// <summary>
        /// Примечание
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Дата постановки на стоянку
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Ссылка на действие постановку на стоянку
        /// </summary>
        public int? ParkingActionId { get; set; }

        public ParkingAction ParkingAction{ get; set; }

        /// <summary>
        /// Ссылка на действие
        /// </summary>
        public int? ActionId { get; set; }
    }
}
