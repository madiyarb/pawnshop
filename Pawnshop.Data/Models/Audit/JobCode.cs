using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Audit
{
    public enum JobCode
    {
        [Display(Name = "Черновик")]
        Draft = 0,
        [Display(Name = "Старт работы")]
        Start = 10,
        [Display(Name = "Начата работа")]
        Begin = 20,
        [Display(Name = "Выполняется работа")]
        Processing = 30,
        [Display(Name = "Ошибка в работе")]
        Error = 90,
        [Display(Name = "Конец работы")]
        End = 100,
        [Display(Name = "Отмена работы")]
        Cancel = 110
    }
}
