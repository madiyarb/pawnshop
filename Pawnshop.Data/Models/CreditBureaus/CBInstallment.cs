using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;
using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Данные рассроченного платежа
    /// </summary>
    public class CBInstallment : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор договора в KBContracts
        /// </summary>
        [Required(ErrorMessage = "Поле Идентификатор договора в KBContracts обязательно для заполнения")]
        public int CBContractId { get; set; }

        /// <summary>
        /// Форма расчета
        /// </summary>
        [Required(ErrorMessage = "Поле Форма расчета обязательно для заполнения")]
        public int PaymentMethodId { get; set; }

        /// <summary>
        /// Периодичность платежей
        /// </summary>
        [Required(ErrorMessage = "Поле Периодичность платежей обязательно для заполнения")]
        public int PaymentPeriodId { get; set; }

        /// <summary>
        /// Сумма договора
        /// </summary>
        [Required(ErrorMessage = "Поле Сумма договора обязательно для заполнения")]
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Валюта
        /// </summary>
        [Required(ErrorMessage = "Поле Валюта обязательно для заполнения")]
        public string Currency { get; set; }

        /// <summary>
        /// Сумма периодического платежа
        /// </summary>
        [Required(ErrorMessage = "Поле Сумма периодического платежа обязательно для заполнения")]
        public decimal InstallmentAmount { get; set; }

        /// <summary>
        /// Общее количество взносов
        /// </summary>
        [Required(ErrorMessage = "Поле Общее количество взносов обязательно для заполнения")]
        public int InstallmentCount { get; set; }

        public List<CBInstallmentRecord> Records { get; set; } = new List<CBInstallmentRecord>();
    }
}