using System;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Core;
using Pawnshop.Core.Validation;

namespace Pawnshop.Data.Models.Insurances
{
    /// <summary>
    /// Действие страхового договора
    /// </summary>
    public class InsuranceAction : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Страховой договор
        /// </summary>
        [RequiredId(ErrorMessage = "Поле страховой договор обязательно для заполнения")]
        public int InsuranceId { get; set; }

        /// <summary>
        /// Тип действия
        /// </summary>
        [EnumDataType(typeof(InsuranceActionType), ErrorMessage = "Поле тип действия обязательно для заполнения")]
        public InsuranceActionType ActionType { get; set; }

        /// <summary>
        /// Дата действия
        /// </summary>
        [RequiredDate(ErrorMessage = "Поле дата действия обязательно для заполнения")]
        public DateTime ActionDate { get; set; }

        /// <summary>
        /// Кассовый ордер
        /// </summary>
        [RequiredId(ErrorMessage = "Поле кассовый ордер обязательно для заполнения")]
        public int OrderId { get; set; }

        /// <summary>
        /// Автор действия
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
    }
}
