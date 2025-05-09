using System.ComponentModel.DataAnnotations;

namespace Pawnshop.AccountingCore.Models
{
    /// <summary>
    /// Статус платежа по графику
    /// </summary>
    public enum ScheduleStatus : short
    {
        [Display(Name = "Предстоящий платеж")]
        FuturePayment = 0,
        [Display(Name = "Реструктуризированный платеж")]
        RestructuredPayment = 1,
        [Display(Name = "Продлен")]
        Prolongated = 5,
        [Display(Name = "Оплачен")]
        Payed = 10,
        [Display(Name = "ЧДП")]
        PartialPayment = 15,
        [Display(Name = "Просрочен")]
        Overdue = 20,
        [Display(Name = "Отменен")]
        Canceled = 30
    }
}
