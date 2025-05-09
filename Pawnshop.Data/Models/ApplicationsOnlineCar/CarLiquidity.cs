using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.ApplicationsOnlineCar
{
    public enum CarLiquidity
    {
        [Display(Name = "Низкая")]
        Low = 0,
        [Display(Name = "Средняя")]
        Middle = 1,
        [Display(Name = "Высокая")]
        High = 2
    }
}
