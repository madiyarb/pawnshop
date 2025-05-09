using Pawnshop.Data.CustomTypes;
using Pawnshop.Data.Models.Membership;
using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Abstractions;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Data.Models.Contracts.Actions
{
    /// <summary>
    /// Результат проверки по рефинансу
    /// </summary>
    public class ContractRefinanceConfig : IJsonObject
    {
        /// <summary>
        /// Статус проверки
        /// </summary>
        public RefinanceCheckStatus CheckStatus { get; set; }
        /// <summary>
        /// Расписание оплат по договору
        /// </summary>
        public List<ContractPaymentSchedule> Schedule { get; set; }
        /// <summary>
        /// Вид удержания процентов
        /// </summary>
        public PercentPaymentType PercentPaymentType { get; set; }
        /// <summary>
        /// Дата закрытия договора
        /// </summary>
        public DateTime MaturityDate { get; set; }
        /// <summary>
        /// Дата первой оплаты
        /// </summary>
        public DateTime? FirstPaymentDate { get; set; }
        /// <summary>
        /// Период договора
        /// </summary>
        public int LoanPeriod { get; set; }
        /// <summary>
        /// Процент пошлины
        /// </summary>
        public decimal LoanPercent { get; set; }
        /// <summary>
        /// Процент штрафа
        /// </summary>
        public decimal PenaltyPercent { get; set; }
        /// <summary>
        /// Основной долг договора
        /// </summary>
        public int LoanCost { get; set; }
        /// <summary>
        /// Филиал, в котором будет открыт договор
        /// </summary>
        public int OldBranchId { get; set; }
        /// <summary>
        /// Филиал, в котором будет открыт договор
        /// </summary>
        public string OldBranchName { get; set; }
        /// <summary>
        /// Филиал, в котором будет открыт договор
        /// </summary>
        public int NewBranchId { get; set; }
        /// <summary>
        /// Филиал, в котором будет открыт договор
        /// </summary>
        public string NewBranchName { get; set; }
        /// <summary>
        /// Выбранное количество оплат
        /// </summary>
        public int PaymentQuantity { get; set; }
        /// <summary>
        /// Варианты количества оплат
        /// </summary>
        public List<int> PossiblePaymentQuantity { get; set; }
        /// <summary>
        /// Список ошибок
        /// </summary>
        public List<string> Errors { get; set; }
        public bool PercentSettingsTakenFromParentContract { get; set; }
    }
}
