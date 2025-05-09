using System.ComponentModel.DataAnnotations;

namespace Pawnshop.AccountingCore.Models
{
    /// <summary>
    /// Вид суммы
    /// </summary>
    public enum AmountType : short
    {
        /// <summary>
        /// Основной долг
        /// </summary>
        [Display(Name = "Основной долг")]
        Debt = 10,
        /// <summary>
        /// Рефинансирование
        /// </summary>
        [Display(Name = "Рефинансирование")]
        Refinance = 11,
        /// <summary>
        /// Просроченный ОД
        /// </summary>
        [Display(Name = "Просроченный ОД")]
        OverdueDebt = 15,
        /// <summary>
        /// Вознаграждение
        /// </summary>
        [Display(Name = "Вознаграждение")]
        Loan = 20,
        /// <summary>
        /// Просроченное вознаграждение
        /// </summary>
        [Display(Name = "Просроченное вознаграждение")]
        OverdueLoan = 25,
        /// <summary>
        /// Вознаграждение на внебалансе
        /// </summary>
        [Display(Name = "Вознаграждение на внебалансе")]
        OverdueLoanOffBalance = 26,
        /// <summary>
        /// Отсроченное вознаграждение
        /// </summary>
        [Display(Name = "Отсроченное вознаграждение")]
        DefermentLoan = 27,
        /// <summary>
        /// Амортизированное вознаграждение по начисленному вознаграждению
        /// </summary>
        [Display(Name = "Амортизированное вознаграждение по начисленному вознаграждению")]
        AmortizedLoan = 271,
        /// <summary>
        /// Амортизированное вознаграждение по просрочениые вознаграждения
        /// </summary>
        [Display(Name = "Амортизированное вознаграждение по просрочениые вознаграждения")]
        AmortizedOverdueLoan = 272,
        /// <summary>
        /// Штраф/пеня
        /// </summary>
        [Display(Name = "Штраф/пеня")]
        Penalty = 30,
        /// <summary>
        /// Штраф/пеня на основной долг
        /// </summary>
        [Display(Name = "Штраф/пеня на основной долг")]
        DebtPenalty = 31,
        /// <summary>
        /// Амортизированая пеня на долг просроченный
        /// </summary>
        [Display(Name = "Амортизированая пеня на долг просроченный")]
        AmortizedDebtPenalty = 311,
        /// <summary>
        /// Штраф/пеня на вознаграждение/проценты
        /// </summary>
        [Display(Name = "Штраф/пеня на вознаграждение/проценты")]
        LoanPenalty = 32,
        /// <summary>
        /// Амортизированая пеня на проценты просроченные
        /// </summary>
        [Display(Name = "Амортизированая пеня на проценты просроченные")]
        AmortizedLoanPenalty = 321,
        /// <summary>
        /// Штраф за нарушение условий кредитования
        /// </summary>
        [Display(Name = "Штраф за нарушение условий кредитования")]
        Forfeit = 33,
        /// <summary>
        /// Лимит пени
        /// </summary>
        [Display(Name = "Лимит пени")]
        PenaltyLimit = 34,
        [Display(Name = "Списание пени с учетом лимита")]
        PenaltyWriteOffByLimit = 341,
        [Display(Name = "Лимит кредитной линии")]
        CreditLineLimit = 342,
        [Display(Name = "Лимит пени")]
        AmortizedDebtPenaltyLimit = 343,
        [Display(Name = "Лимит пени")]
        AmortizedLoanPenaltyLimit = 344,
        /// <summary>
        /// Госпошлина
        /// </summary>
        [Display(Name = "Госпошлина")]
        Duty = 40,
        /// <summary>
        /// Страховая премия
        /// </summary>
        [Display(Name = "Страховая премия")]
        InsurancePremium = 41,
        /// <summary>
        /// Реализация с отрицательной маржой
        /// </summary>
        [Display(Name = "Реализация с отрицательной маржой")]
        SellingLoss = 50,
        /// <summary>
        /// Реализация с положительной маржой
        /// </summary>
        [Display(Name = "Реализация с положительной маржой")]
        SellingProfit = 51,
        
        /// <summary>
        /// Реализация
        /// </summary>
        [Display(Name = "Фактическая реализация на сумму покупки")]
        Selling = 52,  
        
        /// <summary>
        /// ЧДП/уменьшение ОД
        /// </summary>
        [Display(Name = "ЧДП/уменьшение ОД")]
        PartialPayment = 60,
        /// <summary>
        /// ЧДП/обратный вынос просроченного ОД
        /// </summary>
        [Display(Name = "ЧДП/обратный вынос просроченного ОД ")]
        PartialPaymentOverdueDebtReturn = 61,
        /// <summary>
        /// Добор/увеличение ОД
        /// </summary>
        [Display(Name = "Добор/увеличение ОД")]
        Addition = 70,
        /// <summary>
        /// Добор, оплата ОД за счет аванса
        /// </summary>
        [Display(Name = "Добор, оплата ОД за счет аванса")]
        AdditionDebtPayment = 71,
        /// <summary>
        /// Добор, оплата процентов за счет аванса
        /// </summary>
        [Display(Name = "Добор, оплата процентов за счет аванса")]
        AdditionLoanPayment = 72,

        /// <summary>
        /// Аванс
        /// </summary>
        [Display(Name = "Аванс")]
        Prepayment = 100,

        /// <summary>
        /// Дебиторская задолженность
        /// </summary>
        [Display(Name = "Дебиторская задолженность")]
        Receivable = 110,

        /// <summary>
        /// Переводы между филиалами
        /// </summary>
        [Display(Name = "Перевод между филиалами")]
        Remittance = 900,

        /// <summary>
        /// Расходы связанные с обслуживанием договора
        /// </summary>
        [Display(Name = "Расходы связанные с обслуживанием договора")]
        Expense = 910,
        
        /// <summary>
        /// Ручные проводки
        /// </summary>
        [Display(Name = "Сумма ручной проводки")]
        ManualOrder = 920,

        /// <summary>
        /// Транзитный счет для основного долга
        /// </summary>
        [Display(Name = "Сумма ОД в транзитном счете")]
        TransferredDebt = 200,
    }
}