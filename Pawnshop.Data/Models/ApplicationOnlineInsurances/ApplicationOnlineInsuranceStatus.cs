using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.ApplicationOnlineInsurances
{
    public enum ApplicationOnlineInsuranceStatus
    {
        [Display(Name = "Предоставлена")]
        IsProvided = 0,
        [Display(Name = "Удалена")]
        Deleted = 1,
    }
}
