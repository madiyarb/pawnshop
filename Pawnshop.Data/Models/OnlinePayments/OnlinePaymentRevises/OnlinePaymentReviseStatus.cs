using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.OnlinePayments.OnlinePaymentRevises
{
    public enum OnlinePaymentReviseStatus : short
    {
        [Display(Name = "Черновик")]
        Draft = 0,
        [Display(Name = "Успешно")]
        Success = 10,
        [Display(Name = "Неуспешно")]
        Fail = 20,
    }
}
