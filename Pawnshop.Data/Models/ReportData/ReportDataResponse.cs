using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.ReportData
{
    public enum ReportDataResponse : short
    {
        [Display(Name = "Успешно")]
        Success = 0,
        [Display(Name = "Ошибка. Некорректная дата")]
        IncorrectDate = 1,
        [Display(Name = "Ошибка. Пустой массив показателей")]
        EmptyList = 2,
        [Display(Name = "Ошибка.Отрицательные значения в списке показателей")]
        NegativeValues = 3,
        [Display(Name = "Ошибка. Отсутствуют некоторые ключи из списка показателей")]
        MissingKeys = 4,
        [Display(Name = "Другая ошибка.")]
        OtherError = 5,
        [Display(Name = "Ошибка. В списке показателей переданы дублирующие друг друга ключи")]
        DuplicateKeys = 6,
        [Display(Name = "Ошибка. В списке показателей переданы не существующие ключи")]
        WrongKeys = 7,
        [Display(Name = "Ошибка. Некорректное значение даты, ключа или его значения")]
        WrongValues = 8,
        [Display(Name = "Ошибка. Некорректный формат входящих данных.")]
        IncorrectFormat = 9,
    }
}
