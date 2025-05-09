using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.OnlinePayments.OnlinePaymentRevises.OnlinePaymentRevisePows
{
    public enum OnlinePaymentReviseRowStatus : short
    {
        [Display(Name = "Черновик")]
        Draft = 0,
        [Display(Name = "Успешно")]
        Success = 10,
        [Display(Name = "Неуспешно")]
        Fail = 20,
        [Display(Name = "Создан")]
        Created = 30,
        [Display(Name = "Удален")]
        Deleted = 40,
        [Display(Name = "Ошибка")]
        Error = 900,
    }
}
