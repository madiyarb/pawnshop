using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Investments
{
    /// <summary>
    /// Статус инвестиции
    /// </summary>
    public enum InvestmentStatus : short
    {
        /// <summary>
        /// Черновик
        /// </summary>
        [Display(Name = "Черновик")]
        Draft = 0,
        /// <summary>
        /// Подписан
        /// </summary>
        [Display(Name = "Подписан")]
        Signed = 10,
        /// <summary>
        /// Закрыт
        /// </summary>
        [Display(Name = "Закрыт")]
        Closed = 20
    }
}
