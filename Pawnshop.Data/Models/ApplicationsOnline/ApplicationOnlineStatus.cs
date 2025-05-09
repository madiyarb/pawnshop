using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.ApplicationsOnline
{
    public enum ApplicationOnlineStatus
    {
        [Display(Name = "Новая")]
        Created = 0,
        [Display(Name = "На рассмотрении до")]
        Consideration = 1,
        [Display(Name = "Верификация")]
        Verification = 2,
        [Display(Name = "На оценке")]
        OnEstimation = 3,
        [Display(Name = "Оценка завершена")]
        EstimationCompleted = 4,
        [Display(Name = "Требует корректирования")]
        RequiresCorrection = 5,
        [Display(Name = "Проверка биометрии")]
        BiometricCheck = 6,
        [Display(Name = "Одобрен")]
        Approved = 7,
        [Display(Name = "Договор заключен")]
        ContractConcluded = 8,
        [Display(Name = "Отказ")]
        Declined = 9,
        [Display(Name = "Аннулирован")]
        Canceled = 10,
        [Display(Name = "Биометрия пройдена")]
        BiometricPassed = 11,
        [Display(Name = "Проверка реквизитов для получения денег")]
        RequisiteCheck = 12,
        [Display(Name = "Изменение статуса реквизитов после подписания")]
        ChangeRequisiteAfterContractConcluded = 13,
        [Display(Name = "Доработка от верификатора")]
        ModificationFromVerification = 14,
    }
}
