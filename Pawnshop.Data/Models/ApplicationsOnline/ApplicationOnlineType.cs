using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.ApplicationsOnline
{
    public enum ApplicationOnlineType
    {
        [Display(Name = "Базовый транш")]
        BasicTranche,
        [Display(Name = "Добор")]
        AdditionalTranche,
        [Display(Name = "Рефинансирование")]
        Refinance
    }
}
