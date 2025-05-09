using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Contracts.Actions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class PersonalDiscount : IEntity
    {
        public int Id { get; set; }
        
        /// <summary>
        /// Корректировка процента пошлины в порожденном договоре
        /// </summary>
        public decimal PercentAdjustment { get; set; }
        
        /// <summary>
        /// Корректировка процента штрафа в порожденном договоре
        /// </summary>
        public decimal DebtPenaltyAdjustment { get; set; }

        /// <summary>
        /// Процент получаемого вознаграждения
        /// </summary>
        public decimal PercentDiscount { get; set; }
        public decimal PercentDiscountCoefficient => (100 - PercentDiscount) / 100;

        /// <summary>
        /// Процент получаемого штрафа
        /// </summary>
        public decimal DebtPenaltyDiscount { get; set; }
        public decimal DebtPenaltyDiscountCoefficient => (100 - DebtPenaltyDiscount) / 100;

        /// <summary>
        /// Скидка на просроченные проценты
        /// </summary>
        public decimal OverduePercentDiscount { get; set; }

        /// <summary>
        /// Корректировка просроченных процентов
        /// </summary>
        public decimal OverduePercentAdjustment { get; set; }

        /// <summary>
        /// Корректировка штрафа на просроченные проценты
        /// </summary>
        public decimal PercentPenaltyAdjustment { get; set; }

        /// <summary>
        /// Скидка на штраф за просроченные проценты
        /// </summary>
        public decimal PercentPenaltyDiscount { get; set; }

        /// <summary>
        /// Вид залога
        /// </summary>
        [Required(ErrorMessage ="Поле \"Вид залога\" обязательно к заполнению")]
        public CollateralType CollateralType { get; set; }
        
        /// <summary>
        /// Заблокирован
        /// </summary>
        public bool Locked { get; set; }

        /// <summary>
        /// Примечание
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Вид действия
        /// </summary>
        [Required(ErrorMessage = "Поле Вид действия по договору обязательно к заполнению")]
        public ContractActionType ActionType { get; set; }
        
        /// <summary>
        /// Группа скидок
        /// </summary>
        [RequiredId(ErrorMessage = "Поле Группа скидок обязательно к заполнению")]
        public int BlackoutId { get; set; }
        public Blackout Blackout { get; set; }
        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }
        
        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }
    }
}
