using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Audit
{
    public enum JobStatus
    {
        [Display(Name = "Успешно")]
        Success = 0,
        [Display(Name = "Не удачно")]
        Failed = 1
    }
}
