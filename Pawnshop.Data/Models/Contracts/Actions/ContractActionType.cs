using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Contracts.Actions
{
    public enum ContractActionType : short
    {
        [Display(Name = "Продление")]
        Prolong = 10,
        [Display(Name = "Выкуп")]
        Buyout = 20,
        [Display(Name = "Частичный выкуп")]
        PartialBuyout = 30,
        [Display(Name = "Частичное погашение")]
        PartialPayment = 40,
        [Display(Name = "Выдача")]
        Sign = 50,
        [Display(Name = "Отправка на реализацию")]
        PrepareSelling = 60,
        [Display(Name = "Реализация")]
        Selling = 61,
        [Display(Name = "Передача")]
        Transfer = 70,
        [Display(Name = "Обратный выкуп из передачи")]
        ReTransfer = 75,
        [Display(Name = "Ежемесячное погашение")]
        MonthlyPayment = 80,
        [Display(Name = "Добор")]
        Addition = 90,
        [Display(Name = "Смена категории аналитики кредитной линии")]
        ChangeCreditLineCategory = 91,
        [Display(Name = "Аванс/Первоначальный взнос")]
        Prepayment = 100,
        [Display(Name = "Перенос аванса с договора")]
        PrepaymentToTransit = 101,
        [Display(Name = "Перенос аванса на договор")]
        PrepaymentFromTransit = 102,
        [Display(Name = "Рефинансирование")]
        Refinance = 110,
        [Display(Name = "Возврат аванса/ПВ")]
        PrepaymentReturn = 120,
        [Display(Name = "Оплата")]
        Payment = 130,
        [Display(Name = "Начисление процентов")]
        InterestAccrual = 140,
        [Display(Name = "Начисление процентов на просроченный ОД по дискретам")]
        InterestAccrualOnOverdueDebt = 141,
        [Display(Name = "Вынос на просрочку")]
        MoveToOverdue = 150,
        [Display(Name = "Перенос КД на следующий день в выходной день")]
        MoveScheduleToNextDate = 151,
        [Display(Name = "Начисление штрафов")]
        PenaltyAccrual = 160,
        [Display(Name = "Уменьшение ставки пени")]
        PenaltyRateDecrease = 161,
        [Display(Name = "Увеличение ставки пени")]
        PenaltyRateIncrease = 162,
        [Display(Name = "Моментальное предоставление скидки")]
        InstantDiscount = 170,
        [Display(Name = "Начисление лимита пени")]
        PenaltyLimitAccrual = 180,
        [Display(Name = "Списание лимита пени")]
        PenaltyLimitWriteOff = 181,
        [Display(Name = "Закрытие кредитной линии")]
        CreditLineClose = 182,
        [Display(Name = "Изменение контрольной даты")]
        ControlDateChange = 190,
        [Display(Name = "Реструктуризация")]
        RestructuringCred = 200,
        [Display(Name = "Реструктуризация")]
        RestructuringTranches = 201,
        [Display(Name = "Перенос суммы со счета ОД на транзитный счет")]
        RestructuringTransferToTransitCred = 202,
        [Display(Name = "Перенос суммы со счета ОД на транзитный счет")]
        RestructuringTransferToTransitTranches = 203,
        [Display(Name = "Перенос суммы с тразитного счета на счет ОД")]
        RestructuringTransferToAccountCred = 204,
        [Display(Name = "Перенос суммы с тразитного счета на счет ОД")]
        RestructuringTransferToAccountTranches = 205,
        [Display(Name = "Выкуп реструктуризации")]
        BuyoutRestructuringTranches = 206,
        [Display(Name = "Выкуп реструктуризации")]
        BuyoutRestructuringCred = 207,
        [Display(Name = "Списание по аукциону")]
        WithdrawByAuction = 208
    }
}
