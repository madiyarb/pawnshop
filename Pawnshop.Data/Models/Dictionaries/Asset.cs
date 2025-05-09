using System;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Основное средство
    /// </summary>
    public class Asset : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Инвентарный номер
        /// </summary>
        [Required(ErrorMessage = "Поле инвентарный номер обязательно для заполнения")]
        public string Number { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        [Required(ErrorMessage = "Поле наименование обязательно для заполнения")]
        public string Name { get; set; }

        /// <summary>
        /// Материально-ответственное лицо
        /// </summary>
        [RequiredId(ErrorMessage = "Поле материально-ответственное лицо обязательно для заполнения")]
        public int ManagerId { get; set; }

        /// <summary>
        /// Материально-ответственное лицо
        /// </summary>
        public User Manager { get; set; }

        /// <summary>
        /// Дата приема
        /// </summary>
        [RequiredDate(ErrorMessage = "Поле дата приема обязательно для заполнения")]
        public DateTime RegisterDate { get; set; }

        /// <summary>
        /// Стоимость
        /// </summary>
        public int Cost { get; set; }

        /// <summary>
        /// Дата списания
        /// </summary>
        public DateTime? DisposalDate { get; set; }

        /// <summary>
        /// Примечание
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Филиал
        /// </summary>
        [RequiredId(ErrorMessage = "Поле филиал обязательно для заполнения")]
        public int BranchId { get; set; }

        /// <summary>
        /// Филиал
        /// </summary>
        public Group Branch { get; set; }

        /// <summary>
        /// Пользователь
        /// </summary>
        [RequiredId(ErrorMessage = "Поле пользователь обязательно для заполнения")]
        public int UserId { get; set; }

        /// <summary>
        /// Пользователь
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
    }
}
