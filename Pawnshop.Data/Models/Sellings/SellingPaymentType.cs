using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Sellings
{
    public enum SellingPaymentType : short
    {
        /// <summary>
        /// Реализация с отрицательной маржой
        /// </summary>
        [Display(Name = "Реализация с отрицательной маржой")]
        SellingLoss = 50,
        /// <summary>
        /// Реализация с положительной маржой
        /// </summary>
        [Display(Name = "Реализация с положительной маржой")]
        SellingProfit = 51
    }
}
