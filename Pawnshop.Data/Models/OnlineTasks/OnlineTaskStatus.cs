using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.OnlineTasks
{
    public enum OnlineTaskStatus
    {
        [Display(Name = "Новая")]
        Created = 0,
        [Display(Name = "В обработке")]
        Processing = 1,
        [Display(Name = "Выполнена")]
        Completed = 2
    }
}
