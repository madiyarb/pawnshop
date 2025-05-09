using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pawnshop.Data.Models.PayOperations
{
    public enum PayOperationStatus : short
    {
        [Display(Name="Ожидает проверки")]
        AwaitForCheck = 0,
        [Display(Name = "Проверен")]
        Checked = 10,
        [Display(Name = "Вернули на доработку")]
        ReturnToRepair = 20,
        [Display(Name = "Исправили")]
        Repaired = 30,
        [Display(Name = "Обработан")]
        Executed = 40,
        [Display(Name = "Отменён")]
        Canceled = 50
    }
}
