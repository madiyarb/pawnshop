using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Insurances
{
    public enum InsuranceRequestStatus
    {
        [Display(Name = "Создана")]
        New = 0,
        [Display(Name = "Отправлена в СК")]
        Sent = 10,
        [Display(Name = "Отправлен в СК на Отмену")]
        SentCancel = 11,
        [Display(Name = "Подтверждена в СК")]
        Approved = 20,
        [Display(Name = "Отказано в СК")]
        Rejected = 30,
        [Display(Name = "Отказ Отмены в СК")]
        RejectedCancel = 31,
        [Display(Name = "Отменена")]
        Canceled = 40,
        [Display(Name = "Ошибка")]
        Error = 50,
        [Display(Name = "Завершена")]
        Completed = 60,
        [Display(Name = "Аннулирован")]
        Annuled = 70
    }
}