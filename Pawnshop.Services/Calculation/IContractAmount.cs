using Pawnshop.AccountingCore.Models;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Services.Contracts;
using Pawnshop.Services.Models.Calculation;
using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Abstractions;

namespace Pawnshop.Services.Calculation
{
    public interface IContractAmount : ICalculation
    {
        /// <summary>
        /// Сумма для отображения в онлайн системах
        /// </summary>
        decimal DisplayAmount { get; set; }

        /// <summary>
        /// Сумма для отображения в онлайн системах без учета аванса
        /// </summary>
        decimal DisplayAmountWithoutPrepayment { get; set; }

        /// <summary>
        /// Сумма для продления
        /// </summary>
        decimal ProlongAmount { get; set; }

        ContractAmountRow ProlongRow { get; set; }

        /// <summary>
        /// Сумма для ежемесячного погашения
        /// </summary>
        decimal MonthlyAmount { get; set; }

        ContractAmountRow MonthlyRow { get; set; }

        /// <summary>
        /// Сумма для выкупа
        /// </summary>
        decimal BuyoutAmount { get; set; }

        public ContractAmountRow BuyoutRow { get; set; }

        /// <summary>
        /// Количество просроченных дней
        /// </summary>
        int PenaltyDays { get; set; }

        /// <summary>
        /// Дата следующей оплаты
        /// </summary>
        DateTime? NextPaymentDate { get; set; }

        /// <summary>
        /// Количество просроченных месяцев
        /// </summary>
        int PenaltyMonthCount { get; set; }

        /// <summary>
        /// Сумма аванса
        /// </summary>
        decimal PrepaymentCost { get; set; }

        /// <summary>
        /// Добавляемые или вычитаемые дни
        /// </summary>
        int BlackDays { get; set; }

        /// <summary>
        /// Скидки
        /// </summary>
        ContractDutyDiscount DutyDiscount { get; set; }

        /// <summary>
        /// Признак просроченного договора
        /// </summary>
        bool IsDelayed { get; set; }

        /// <summary>
        /// Сумма доп расходов
        /// </summary>
        decimal ExtraExpensesCost { get; set; }

        /// <summary>
        /// Причина
        /// </summary>
        string Reason { get; set; }

        /// <summary>
        /// Сумма для отображения в онлайн системах без учета аванса и доп расходов
        /// </summary>
        public decimal DisplayAmountWithoutPrepaymentAndExpenses { get; set; }

        /// <summary>
        /// Сумма для отображения в онлайн системах без учета доп расходов
        /// </summary>
        public decimal DisplayAmountWithoutExpenses { get; set; }

        decimal CalculateAmountByAmountType(DateTime date, params AmountType[] amountTypes);
        decimal CalculateAmountByAmountType(DateTime date, bool balanceAccountsOnly = false, params AmountType[] amountTypes);
        decimal GetLoanCostLeft();
        (int, int) CalculatePenaltyDays(IContract contract);
    }
}
