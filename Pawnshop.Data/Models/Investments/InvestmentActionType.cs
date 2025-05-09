using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Investments
{
    /// <summary>
    /// Тип действия инвестии
    /// </summary>
    public enum InvestmentActionType
    {
        /// <summary>
        /// Процент
        /// </summary>
        [Display(Name = "Процент")]
        Percent = 10,
        /// <summary>
        /// Внесение
        /// </summary>
        [Display(Name = "Внесение")]
        Deposit = 20,
        /// <summary>
        /// Долг
        /// </summary>
        [Display(Name = "Долг")]
        Debt = 30,
    }
}
