using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Insurances
{
    public enum InsuranceActionType : short
    {
        /// <summary>
        /// Подписать
        /// </summary>
        [Display(Name = "Подписать")]
        Sign = 10,
        /// <summary>
        /// Оплатить
        /// </summary>
        [Display(Name = "Оплатить")]
        Pay = 20,
    }
}
