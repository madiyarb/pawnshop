using System.ComponentModel.DataAnnotations;


namespace Pawnshop.Data.Models.ApplicationsOnline
{
    public enum ApplicationOnlineRejectionReason
    {
        [Display(Name = "Автоотказ")]
        AutoReject = 0,
        [Display(Name = "Задублированная заявка")]
        DuplicateApplication = 1,
        [Display(Name = "Клиент отказался")]
        ClientRefused = 2,
        [Display(Name = "Арест на автомобиле")]
        CarArrested = 3,
        [Display(Name = "Отрицательная история (внутренняя)")]
        NegativeInternalHistory = 4,
        [Display(Name = "Тех.паспорт оформлен на 3-е лицо")]
        TechnicalPassportIssuedOn3RdPerson = 5,
        [Display(Name = "Превышен лимит (LTV)")]
        LimitExceededLTV = 6,
        [Display(Name = "Не соответствует условиям (Спецтехника)")]
        DoesNotMeetTheConditionsSpecialEquipment = 7,
        [Display(Name = "Авто незаконно ввезен в РК")]
        CarWasIllegallyImportedIntoKazakhstanRepublic = 8,
        [Display(Name = "Клиент находится в реестре должников")]
        TheClientInDebtorsRegister = 9,
        [Display(Name = "Клиента интересовал беззалоговый займ")]
        ClientWasInterestedUnsecuredLoan = 10,
        [Display(Name = "Сумма меньше 200 000 тенге")]
        AmountLessThan200000Tenge = 11,
        [Display(Name = "Подозрение на мошенничество")]
        SuspicionOfFraud = 12,
        [Display(Name = "Негативная информация от контактных лиц")]
        NegativeInformationFromContactPersons = 13,
        [Display(Name = "Автомобиль зарегистрирован не в РК")]
        CarNotRegisteredInKazakhstanRepublic = 14,
        [Display(Name = "Клиент вбил неверные анкетные данные")]
        ClientEnteredIncorrectPersonalData = 15,
        [Display(Name = "Неверный VIN код/ Отсутствует VIN код")]
        WrongOrMissingVinCode = 16,
        [Display(Name = "Не соответствует условиям программы (Возраст)")]
        BadAge = 17,
        [Display(Name = "Клиент решил финансовые трудности")]
        ClientSolvedFinancialDifficulties = 18,
        [Display(Name = "STOP фактор: город, село")]
        StopFactorUrbanRural = 19,
        [Display(Name = "Рекомендовано направить в филиал")]
        RecommendedSendToBranch = 20,
        [Display(Name = "Не прошел по КДН")]
        NotPassKDN = 21,
        [Display(Name = "Отрицательная кредитная история (ПКБ)")]
        NegativeCreditHistoryPKB = 22,
        [Display(Name = "Лудоман")]
        Ludoman = 23,
        [Display(Name = "Клиент не отвечает на звонки")]
        ClientNotAnsweringCalls = 24,
        [Display(Name = "Клиент не прошел биометрию")]
        ClientNotPassBiometric = 25
    }
}
