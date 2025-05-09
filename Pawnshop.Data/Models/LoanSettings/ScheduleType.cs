using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.LoanSettings
{
    /// <summary>
    /// Вид начисления(графика)
    /// </summary>
    public enum ScheduleType : short
    {
        /// <summary>
        /// Аннуитетный
        /// </summary>
        [Display(Name = "Аннуитетный")]
        Annuity = 0,

        /// <summary>
        /// Дискретный
        /// </summary>
        [Display(Name = "Дискретный")]
        Discrete = 5,

        /// <summary>
        /// Равные платежи
        /// </summary>
        [Display(Name = "Равные платежи")]
        EqualPayment = 10,

        /// <summary>
        /// Дифференцированный
        /// </summary>
        [Display(Name = "Дифференцированный")]
        Differentiated = 15,

        /// <summary>
        /// Смешанный (дискрет + аннуитет)
        /// </summary>
        [Display(Name = "Смешанный (дискрет + аннуитет)")]
        Mixed = 20
    }
}
