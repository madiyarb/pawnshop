using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.OnlinePayments
{
    public enum OnlinePaymentStatus : short
    {
        /// <summary>
        /// Черновик
        /// </summary>
        [Display(Name = "Черновик")]
        Draft = 0,
        /// <summary>
        /// Продление
        /// </summary>
        [Display(Name = "Продление")]
        Prolong = 10,
        /// <summary>
        /// Ежемесячное погашение
        /// </summary>
        [Display(Name = "Ежемесячное погашение")]
        MonthlyPayment = 20,
        /// <summary>
        /// Выкуп
        /// </summary>
        [Display(Name = "Выкуп")]
        Buyout = 30,
        /// <summary>
        /// Не достаточно денег
        /// </summary>
        [Display(Name = "Не достаточно денег")]
        NotEnough = 40,
        /// <summary>
        /// Предварительный взнос
        /// </summary>
        [Display(Name = "Предварительный взнос")]
        InitialFee = 50,
        /// <summary>
        /// Данные договора не подходят
        /// </summary>
        [Display(Name = "Данные договора не подходят для АОДС")]
        Exclusion = 100,
        /// <summary>
        /// Ошибка
        /// </summary>
        [Display(Name = "Ошибка")]
        Error = 900,
    }
}
