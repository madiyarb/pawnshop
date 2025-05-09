using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Contracts
{
    public enum AnnuityType : short
    {
        /// <summary>
        /// Равные платежи
        /// </summary>
        [Display(Name = "Равные платежи")]
        EqualPayments = 0,

        /// <summary>
        /// Аннуитет со стандартной датой
        /// </summary>
        [Display(Name = "Аннуитет со стандартной датой")]
        Annuity = 10,

        /// <summary>
        /// Аннуитет с выбором даты
        /// </summary>
        [Display(Name = "Аннуитет с выбором даты")]
        AnnuityWithDateSelect = 20
    }
}