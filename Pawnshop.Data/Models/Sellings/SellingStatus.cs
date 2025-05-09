using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Sellings
{
    public enum SellingStatus : short
    {
        /// <summary>
        /// В наличии
        /// </summary>
        [Display(Name = "В наличии")]
        InStock = 10,
        /// <summary>
        /// Продано
        /// </summary>
        [Display(Name = "Продано")]
        Sold = 20
    }
}