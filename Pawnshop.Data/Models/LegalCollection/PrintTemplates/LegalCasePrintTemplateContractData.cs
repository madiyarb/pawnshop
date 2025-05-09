using System;
using System.Collections.Generic;

namespace Pawnshop.Data.Models.LegalCollection.PrintTemplates
{
    public class LegalCasePrintTemplateContractData
    {
        /// <summary>
        /// ИД контакта в финкоре
        /// </summary>
        public int ContractId { get; set; }
        /// <summary>
        /// Номер контракта в финкоре
        /// </summary>
        public string ContractNumber { get; set; }
        /// <summary>
        /// Дата подписания контракта
        /// </summary>
        public DateTime ContractDate { get; set; }
        /// <summary>
        /// Сумма займа
        /// </summary>
        public decimal LoanCost { get; set; }
        /// <summary>
        /// Срок займа в месяцах
        /// </summary>
        public int LoanPeriod { get; set; }
        /// <summary>
        /// Общая задолжность
        /// </summary>
        public decimal DebtCost { get; set; }
        /// <summary>
        /// Основной долг
        /// </summary>
        public decimal LoanAmountCost { get; set; }
        /// <summary>
        /// Проценты начисленные
        /// </summary>
        public decimal ProfitAmountCost { get; set; }
        /// <summary>
        /// Просроченное вознаграждение
        /// </summary>
        public decimal OverdueLoanCost { get; set; }
        /// <summary>
        /// Просроченное ОД
        /// </summary>
        public decimal OverdueDebtCost { get; set; }
        /// <summary>
        /// Основной долг + Просроченное ОД
        /// </summary>
        public decimal LoanAmountPlusOverdueDebtCost { get { return this.LoanAmountCost + OverdueDebtCost; } }
        /// <summary>
        /// Проценты начисленные + Просроченное вознаграждение
        /// </summary>
        public decimal ProfitAmountPlusOverdueLoanCost { get { return ProfitAmountCost + OverdueLoanCost; } }
        /// <summary>
        /// Пеня
        /// </summary>
        public decimal PenaltyCost { get; set; }
        /// <summary>
        /// Ежедневная процентная ставка
        /// </summary>
        public decimal LoanPercent { get; set; }
        /// <summary>
        /// Годовая процентная ставка
        /// </summary>
        public decimal AnnualEffectiveRate { get; set; }
        /// <summary>
        /// Номинальная процентная ставка
        /// </summary>
        public decimal NominalRate { get; set; }
        /// <summary>
        /// Количество просроченных дней
        /// </summary>
        public int DelayDays { get; set; }
        /// <summary>
        /// Данные созаемщика
        /// </summary>
        public List<LegalCasePrintTemplateClientData> CoborrowerList { get; set; }
        /// <summary>
        /// Данные Гаранта
        /// </summary>
        public List<LegalCasePrintTemplateClientData> GuarantorList { get; set; }
        /// <summary>
        /// Адрес залоговой недвжимости 
        /// </summary>
        public List<string> RealtyAddressList { get; set; }
        /// <summary>
        /// Данные залоговой машины
        /// </summary>
        public List<LegalCasePrintTemplateCarData> CarDataList { get; set; }
    }
}
