using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Pawnshop.Services.Models.Calculation
{
    public enum ContractAmountRowType
    {
        [Display(Name = "Продление")]
        ProlongRow = 10,
        [Display(Name = "Ежемесячное погашение")]
        MonthlyRow = 20,
        [Display(Name = "Выкуп")]
        BuyoutRow = 30,
        [Display(Name = "Задолженность")]
        DebtRow = 30
    }
}
