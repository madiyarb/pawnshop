using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.OnlineTasks
{
    public enum OnlineTaskType
    {
        [Display(Name = "Разрешение ситуации")]
        ResolutionSituation = 0,
        [Display(Name = "Задержка по заявке")]
        DelayApplication = 1,
        [Display(Name = "Взять в работу добор")]
        HireExtras = 2,
        [Display(Name = "Верифицировать")]
        Verify = 3,
        [Display(Name = "Утвердить добор")]
        ApproveAddition = 4,
        [Display(Name = "Закрыть")]
        Close = 5,
        [Display(Name = "Одобрить")]
        Approve = 6,
        [Display(Name = "Рассмотреть заявку")]
        Considerate = 7,
        [Display(Name = "Перезвонить")]
        CallBack = 8,
        [Display(Name = "Сверить биометрию")]
        CheckBiometric = 9,
        [Display(Name = "Проверка реквизитов")]
        RequisiteCheck = 10,
        [Display(Name = "Доработать заявку")]
        Modification = 11,
    }
}
