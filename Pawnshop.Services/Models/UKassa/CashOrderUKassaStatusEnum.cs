using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Pawnshop.Services.Models.UKassa
{
    public enum CashOrderUKassaStatusEnum
    {
        [Display(Name = "OK")]
        OK = 10,
        [Display(Name = "Не найден в Учет Кассе")]
        NotFoundInUKassa = 20,
        [Display(Name = "Не найден в Фронт базе")]
        NotFoundInFrontBase = 30,
        [Display(Name = "Не совпадают суммы")]
        AmountsNotEqual = 40
    }
}
