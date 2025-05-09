using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pawnshop.Data.Models.Contracts.Discounts
{
    public class ContractDiscount : IEntity, ILoggableToEntity
    {
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор договора
        /// </summary>
        [RequiredId(ErrorMessage = "Обязательна привязка к договору")]
        public int ContractId { get; set; }

        /// <summary>
        /// Скидка из шаблона
        /// </summary>
        [Required(ErrorMessage = " Признак \"Типовая скидка\" обязателен")]
        public bool IsTypical { get; set; }

        /// <summary>
        /// Первая дата предоставления скидки
        /// </summary>
        [RequiredDate(ErrorMessage = "Начальная дата предоставления скидки обязательна")]
        public DateTime BeginDate { get; set; }

        /// <summary>
        /// Последняя дата предоставления скидки
        /// </summary>
        [RequiredDate(ErrorMessage = "Начальная дата предоставления скидки обязательна")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Идентификатор типовой скидки
        /// </summary>
        public int? PersonalDiscountId { get; set; }

        /// <summary>
        /// Типовая скидка
        /// </summary>
        public PersonalDiscount PersonalDiscount { get; set; }

        /// <summary>
        /// Сумма пошлины не типовой скидки (Вознаграждение)
        /// </summary>
        public decimal PercentDiscountSum { get; set; }

        /// <summary>
        /// Сумма пошлины не типовой скидки (Штраф/пеня на основной долг)
        /// </summary>
        public decimal DebtPenaltyDiscountSum { get; set; }

        /// <summary>
        /// Скидка на просроченные проценты (Просроченное вознаграждение)
        /// </summary>
        public decimal OverduePercentDiscountSum { get; set; }

        /// <summary>
        /// Скидка на пеню за просроченные проценты (Штраф/пеня на вознаграждение/проценты)
        /// </summary>
        public decimal PercentPenaltyDiscountSum { get; set; }

        /// <summary>
        /// Скидка на Отсроченное вознаграждение
        /// </summary>
        public decimal DefermentLoan { get; set; }

        /// <summary>
        /// Скидка на Амортизированное вознаграждение
        /// </summary>
        public decimal AmortizedLoan { get; set; }

        /// <summary>
        /// Скидка на Амортизированая пеня на долг просроченный
        /// </summary>
        public decimal AmortizedDebtPenalty { get; set; }

        /// <summary>
        /// Скидка на Амортизированая пеня на проценты просроченные
        /// </summary>
        public decimal AmortizedLoanPenalty { get; set; }

        /// <summary>
        /// Примечание
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        public ContractDiscountStatus Status { get; set; }
        
        /// <summary>
        /// Идентификатор типовой скидки
        /// </summary>
        public int? ContractActionId { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        ///  Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        public int GetLinkedEntityId()
        {
            return ContractId;
        }
    }
}
