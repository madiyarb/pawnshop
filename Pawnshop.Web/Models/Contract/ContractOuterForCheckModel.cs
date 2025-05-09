using Pawnshop.Data.Models.Contracts;
using Pawnshop.Web.Engine.Calculation;
using System;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Services.Calculation;

namespace Pawnshop.Web.Models.Contract
{
    /// <summary>
    /// Внешняя информация о договоре
    /// </summary>
    public class ContractOuterForCheckModel
    {
        /// <summary>
        /// Номер договора
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Дата договора
        /// </summary>
        public DateTime ContractDate { get; set; }

        /// <summary>
        /// Дата ближайшего платежа
        /// </summary>
        public DateTime MaturityDate { get; set; }

        /// <summary>
        /// Основной долг
        /// </summary>
        public decimal LoanCost { get; set; }

        /// <summary>
        /// Суммы для действий
        /// </summary>
        public IContractAmount Amount { get; set; }

        /// <summary>
        /// Ежемесячный платеж
        /// </summary>
        public decimal MonthlyPayment { get; set; } = 0;

        /// <summary>
        /// Просроченные платежи 
        /// </summary>
        public int DelayedMonthlyPayments { get; set; } = 0;

        /// <summary>
        /// Предстоящие платежи
        /// </summary>
        public int FutureMonthlyPayments { get; set; } = 0;

        /// <summary>
        /// Статус договора
        /// </summary>
        public ContractStatus Status { get; set; } = 0;
    }
}
