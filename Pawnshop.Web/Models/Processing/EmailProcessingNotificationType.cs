using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Web.Models.Processing
{
    public enum EmailProcessingNotificationType : short
    {
        [Display(Name = "Платеж не найден в онлайн системе")]
        NotFoundInProcessing = 10,
        [Display(Name = "Платеж отличается")]
        Different = 20,
        [Display(Name = "Платеж не найден у НАС (TasGroup)")]
        AbsentInTas = 30
    }
}
