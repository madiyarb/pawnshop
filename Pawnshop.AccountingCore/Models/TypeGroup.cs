using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Pawnshop.AccountingCore.Models
{
    public enum TypeGroup : short
    {
        /// <summary>
        /// Сроки
        /// </summary>
        [Display(Name = "Сроки")]
        Terms = 10,
        /// <summary>
        /// Договора/контракты
        /// </summary>
        [Display(Name = "Договора")]
        Contracts = 20,
        /// <summary>
        /// Расходы
        /// </summary>
        [Display(Name = "Расходы")]
        Expenses = 30
    }
}
