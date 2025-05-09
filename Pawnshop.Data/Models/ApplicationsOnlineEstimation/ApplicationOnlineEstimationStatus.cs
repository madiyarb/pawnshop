using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.ApplicationsOnlineEstimation
{
    public enum ApplicationOnlineEstimationStatus
    {
        [Display(Name = "На доработку")]
        NeedCorrection = 0,
        [Display(Name = "Одобрена")]
        Approved = 1,
        [Display(Name = "Отклонена")]
        Decline = 2,
        [Display(Name = "На Оценке")]
        OnEstimation = 3
    }
}
