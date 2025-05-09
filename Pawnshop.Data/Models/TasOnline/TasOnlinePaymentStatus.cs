using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.TasOnline
{
    public enum TasOnlinePaymentStatus
    {
        [Display(Name = "Новый")]
        New = 0,
        [Display(Name = "Оплата проведена")]
        Done = 1 ,
        [Display(Name = "Оплата была проведена ранее")]
        PastPayment = 10, 
        [Display(Name = "Абонент не найден")]
        ClientNotFound = 2,
        [Display(Name = "Недопустимое значение суммы платежа")]
        IncorrectAmount = 3,
        [Display(Name = "Недопустимое значение номера платежа")]
        IncorrectContractId = 4,
        [Display(Name = "Ошибка создания платежа")]
        TransactionCreateError = 5,
        [Display(Name = "Любая другая ошибка")]
        Error = 6,
        [Display(Name = "Транзакция не найдена")]
        TransactionNotFound = 7,
        [Display(Name = "Сторнирование проведено")]
        StonoCreated = 8,
        [Display(Name = "Транзакция в другом опердне")]
        TransactionInOtherOperand = 9
    }
}