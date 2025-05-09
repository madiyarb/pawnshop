using System;
using Pawnshop.Core;
using System.ComponentModel.DataAnnotations;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Data.Models.Membership
{
    /// <summary>
    /// Настройка процентов кредита
    /// </summary>
    public class AnnuitySetting : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Вид залога
        /// </summary>
        public CollateralType CollateralType { get; set; }

        /// <summary>
        /// Мин период до первого платежа (дней)
        /// </summary>
        public int MinDayCount { get; set; }

        /// <summary>
        /// Макс период до первого платежа (дней)
        /// </summary>
        public int MaxDayCount { get; set; }

        /// <summary>
        /// Филиал, в котором создан договор
        /// </summary>
        [Range(0, 28, ErrorMessage = "Поле дата должна иметь значение от 0 до 28")]
        public int? CertainDay { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Кем создан
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
    }
}