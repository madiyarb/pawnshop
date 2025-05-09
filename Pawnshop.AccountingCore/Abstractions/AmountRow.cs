using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.AccountingCore.Abstractions
{
    public abstract class AmountRow
    {
        /// <summary>
        /// Сумма основного долг
        /// </summary>
        public decimal DebtAmount { get; set; } = 0;

        /// <summary>
        /// Сумма процентов
        /// </summary>
        public decimal PercentAmount { get; set; } = 0;

        /// <summary>
        /// Количество дней процентов
        /// </summary>
        public int PercentDays { get; set; } = 0;

        /// <summary>
        /// Сумма штрафа
        /// </summary>
        public decimal PenaltyAmount { get; set; } = 0;

        /// <summary>
        /// Сумма дебиторской задолженности
        /// </summary>
        public decimal ReceivableOnlinePaymentAmount { get; set; } = 0;

        /// <summary>
        /// Количество дней штрафа
        /// </summary>
        public int PenaltyDays { get; set; } = 0;

        /// <summary>
        /// Оригинальная сумма процентов
        /// </summary>
        public decimal OriginalPercentAmount { get; set; } = 0;

        /// <summary>
        /// Оригинальное количество дней процентов
        /// </summary>
        public int OriginalPercentDays { get; set; } = 0;

        /// <summary>
        /// Оригинальная сумма штрафа
        /// </summary>
        public decimal OriginalPenaltyAmount { get; set; } = 0;

        /// <summary>
        /// Оригинальное количество дней штрафа
        /// </summary>
        public int OriginalPenaltyDays { get; set; } = 0;

        /// <summary>
        /// Отсроченное вознаграждение
        /// </summary>
        public decimal DefermentLoan { get; set; }

        /// <summary>
        /// Амортизированное вознаграждение
        /// </summary>
        public decimal AmortizedLoan { get; set; }

        /// <summary>
        /// Амортизированая пеня на долг просроченный
        /// </summary>
        public decimal AmortizedDebtPenalty { get; set; }

        /// <summary>
        /// Амортизированая пеня на проценты просроченные
        /// </summary>
        public decimal AmortizedLoanPenalty { get; set; }

    }
}
