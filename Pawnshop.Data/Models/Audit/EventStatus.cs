using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Audit
{
    public enum EventStatus
    {
        [Display(Name = "Успешно")]
        Success = 0,
        [Display(Name = "Не удачно")]
        Failed = 1
    }
}