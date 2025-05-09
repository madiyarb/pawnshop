using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;
using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Записи периодичных платежей
    /// </summary>
    
    public class CBInstallmentRecord : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор рассроченной записи
        /// </summary>
        [Required(ErrorMessage = "Поле Идентификатор рассроченной записи обязательно для заполнения")]
        public int CBInstallmentId { get; set; }

        /// <summary>
        /// Учетная дата
        /// </summary>
        [Required(ErrorMessage = "Поле Учетная дата обязательно для заполнения")]
        public DateTime AccountingDate { get; set; }

        /// <summary>
        /// Количество предстоящих платежей согласно графику
        /// </summary>
        [Required(ErrorMessage = "Поле Количество предстоящих платежей обязательно для заполнения")]
        public int OutstandingInstallmentCount { get; set; }

        /// <summary>
        /// Сумма предстоящих платежей (Непогашенная сумма)
        /// </summary>
        [Required(ErrorMessage = "Поле Сумма предстоящих платежей обязательно для заполнения")]
        public decimal OutstandingAmount { get; set; }

        /// <summary>
        /// Количество дней просрочки
        /// </summary>
        [Required(ErrorMessage = "Поле Количество дней просрочки обязательно для заполнения")]
        public int OverdueInstallmentCount { get; set; }

        /// <summary>
        /// Сумма просроченных платежей (взносов)
        /// </summary>
        [Required(ErrorMessage = "Поле Сумма просроченных платежей обязательно для заполнения")]
        public decimal OverdueAmount { get; set; }

        /// <summary>
        /// Пеня
        /// </summary>
        [Required(ErrorMessage = "Поле Пеня обязательно для заполнения")]
        public decimal Fine { get; set; }

        /// <summary>
        /// Штраф
        /// </summary>
        [Required(ErrorMessage = "Поле Штраф обязательно для заполнения")]
        public decimal Penalty { get; set; }

        /// <summary>
        /// Дата начала пролонгации
        /// </summary>
        public DateTime? ProlongationStartDate { get; set; }

        /// <summary>
        /// Дата завершения пролонгации
        /// </summary
        public DateTime? ProlongationEndDate { get; set; }
        
        /// <summary>
        /// Доступный лимит КЛ
        /// </summary
        public decimal AvailableLimit { get; set; }

        /// <summary>
        /// Дата поступления последнего платежа
        /// </summary
        public DateTime? LastPaymentDate { get; set; }
    }
}