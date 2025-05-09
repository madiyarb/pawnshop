using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Insurances
{
    public enum InsuranceStatus : short
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
        /// Оплачен
        /// </summary>
        [Display(Name = "Оплачен")]
        Payed = 20,
        /// <summary>
        /// Закрыт
        /// </summary>
        [Display(Name = "Закрыт")]
        Closed = 30
    }
}
