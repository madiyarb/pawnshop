using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Notifications.NotificationTemplates
{
    /// <summary>
    /// Тип смс уведомления
    /// </summary>
    public enum NotificationPaymentType : short
    {
        [Display(Name = "Черновик")]
        Draft = 0,
        [Display(Name = "Принята оплата")]
        PaymentAccepted = 1,
        [Display(Name = "Продление")]
        Prolong = 10,
        [Display(Name = "Продление с остатком на авансе")]
        ProlongWithPrepayment = 11,
        [Display(Name = "Выкуп")]
        Buyout = 20,
        [Display(Name = "Частичный выкуп")]
        PartialBuyout = 30,
        [Display(Name = "Частичное погашение")]
        PartialPayment = 40,
        [Display(Name = "Выдача")]
        Sign = 50,
        [Display(Name = "Отправка на реализацию")]
        Selling = 60,
        [Display(Name = "Передача")]
        Transfer = 70,
        [Display(Name = "Ежемесячное погашение")]
        MonthlyPayment = 80,
        [Display(Name = "Неполная оплата суммы для ежемесячного погашения")]
        PartialMonthlyPayment = 81,
        [Display(Name = "Последнее погашение по графику")]
        LastPayment = 82,
        [Display(Name = "Погашение по графику")]
        Payment = 83,
        [Display(Name = "Добор")]
        Addition = 90,
        [Display(Name = "Исполнительная надпись")]
        Inscription = 110,
        [Display(Name = "Предварительный взнос")]
        InitialFee = 120,
        [Display(Name = "Неполная оплата суммы для продления")]
        PartialProlong = 130,
        [Display(Name = "Не хватает денег на авансе на погашение доп расходов")]
        NotEnoughForExpense = 131,
        [Display(Name = "Выкуп с остатком баланса на авансе")]
        BuyoutWithExcessPrepayment = 132,
        [Display(Name = "Страховой полис не создан")]
        InsurancePolicyNotCreated = 133,
        [Display(Name = "Денег хватает на ПДП одного из Траншей")]
        MoneyEnoughForOneTranche = 134,
        [Display(Name = "Поздравление с днем рождения")]
        Birthday = 140,
        [Display(Name = "Подписание онлайн договора")]
        ApplicationOnlineSign = 150,
        [Display(Name = "Страховой полис не удалось оформить")]
        InsuranceFail = 160,
        [Display(Name = "Верификация")]
        Verification = 170,
        [Display(Name = "Оплата ранней просрочки")]
        EarlyDelay = 180,
        [Display(Name = "Оплата ранней просрочки")]
        EarlyDelayRus = 181,
        [Display(Name = "Оплата поздней просрочки")]
        LateDelay = 190
    }
}