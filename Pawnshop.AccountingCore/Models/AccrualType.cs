using System.ComponentModel.DataAnnotations;

namespace Pawnshop.AccountingCore.Models
{
    /// <summary>
    /// Тип начисления
    /// </summary>
    public enum AccrualType : short
    {
        [Display(Name = "Начисление штрафов")]
        PenaltyAccrual = 10,
        [Display(Name = "Начисление процентов")]
        InterestAccrual = 20,
        [Display(Name = "Начисление процентов на просроченный ОД")]
        InterestAccrualOnOverdueDebt = 30
    }
}