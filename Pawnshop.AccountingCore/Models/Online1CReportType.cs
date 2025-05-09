using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Pawnshop.AccountingCore.Models
{
    /// <summary>Тип отчета 1C</summary>
    public enum Online1CReportType : short
    {
        /// <summary>
        /// Начисления
        /// </summary>
        [Display(Name = "Начисления")]
        Accruals = 10,
        /// <summary>
        /// Погашения
        /// </summary>
        [Display(Name = "Погашения")]
        Repayment = 20,
        /// <summary>
        /// Поступление денег
        /// </summary>
        [Display(Name = "Поступление денег")]
        CashReceipts = 30,
        /// <summary>
        /// Выдача
        /// </summary>
        [Display(Name = "Выдача")]
        MoneyPayments = 40,
        /// <summary>
        /// Освоение аванса
        /// </summary>
        [Display(Name = "Освоение аванса")]
        AdvanceRepayment = 50,
        /// <summary>
        /// Кассовые операции, не относящиеся к кредитам
        /// </summary>
        [Display(Name = "Кассовые операции, не относящиеся к кредитам")]
        CashOperations = 60
    }
}
