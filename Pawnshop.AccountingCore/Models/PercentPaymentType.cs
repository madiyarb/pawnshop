using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pawnshop.AccountingCore.Models
{

    /// <summary>
    /// Вид удержания процентов
    /// </summary>
    public enum PercentPaymentType : short
    {
        /// <summary>
        /// При получении ссуды
        /// </summary>
        [Display(Name = "При получении ссуды")]
        BeginPeriod = 10,
        /// <summary>
        /// По истечении срока
        /// </summary>
        [Display(Name = "По истечении срока")]
        EndPeriod = 20,
        /// <summary>
        /// 12 месяцев
        /// </summary>
        [Display(Name = "12 месяцев")]
        AnnuityTwelve = 30,
        /// <summary>
        /// 24 месяца
        /// </summary>
        [Display(Name = "24 месяца")]
        AnnuityTwentyFour = 31,
        /// <summary>
        /// 36 месяцев
        /// </summary>
        [Display(Name = "36 месяцев")]
        AnnuityThirtySix = 32,
        /// <summary>
        /// Берется из продукта
        /// </summary>
        [Display(Name = "Берется из продукта")]
        Product = 40
    }
}
