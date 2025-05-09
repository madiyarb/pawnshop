using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.TasOnline
{
    public enum TasOnlineRequestStatus
    {
        [Display(Name = "Новый")]
        New = 10,
        [Display(Name = "Ошибка отправки")]
        Error = 20,
        [Display(Name = "Ошибка обработки")]
        DeserializeError = 30,
        [Display(Name = "Обработан")]
        Done = 40
    }
}