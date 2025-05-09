using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pawnshop.Data.Models.Contracts.Discounts
{
    public enum ContractDiscountStatus
    {
        [Display(Name = "Возможна")]
        Accepted = 0,
        [Display(Name = "Предоставлена")]
        Granted = 10,
        [Display(Name = "Отменена")]
        Canceled = 20,
        [Display(Name = "Вне диапазона дат")]
        OutOfDateRange = 30
    }
}
