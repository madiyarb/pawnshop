using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.CashOrders
{
    /// <summary>
    /// Настройки денежных переводов между филиалами
    /// </summary>
    public class RemittanceSetting : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Отправивший филиал
        /// </summary>
        [RequiredId(ErrorMessage = "Поле отправивший филиал обязательно для заполнения")]
        public int SendBranchId { get; set; }

        /// <summary>
        /// Отправивший филиал
        /// </summary>
        public Group SendBranch { get; set; }

        /// <summary>
        /// Получивший филиал
        /// </summary>
        [RequiredId(ErrorMessage = "Поле получивший филиал обязательно для заполнения")]
        public int ReceiveBranchId { get; set; }

        /// <summary>
        /// Получивший филиал
        /// </summary>
        public Group ReceiveBranch { get; set; }

        /// <summary>
        /// Счет РКО Дебет
        /// </summary>
        [RequiredId(ErrorMessage = "Поле счет РКО Дебет обязательно для заполнения")]
        public int CashOutDebitId { get; set; }

        /// <summary>
        /// Счет РКО Дебет
        /// </summary>
        public Account CashOutDebit { get; set; }

        /// <summary>
        /// Счет РКО Кредит
        /// </summary>
        [RequiredId(ErrorMessage = "Поле счет РКО Кредит обязательно для заполнения")]
        public int CashOutCreditId { get; set; }

        /// <summary>
        /// Счет РКО Кредит
        /// </summary>
        public Account CashOutCredit { get; set; }

        /// <summary>
        /// Счет ПКО Дебет
        /// </summary>
        [RequiredId(ErrorMessage = "Поле счет ПКО Дебет обязательно для заполнения")]
        public int CashInDebitId { get; set; }

        /// <summary>
        /// Счет ПКО Дебет
        /// </summary>
        public Account CashInDebit { get; set; }

        /// <summary>
        /// Счет ПКО Кредит
        /// </summary>
        [RequiredId(ErrorMessage = "Поле счет ПКО дебет обязательно для заполнения")]
        public int CashInCreditId { get; set; }

        /// <summary>
        /// Счет ПКО Кредит
        /// </summary>
        public Account CashInCredit { get; set; }

        /// <summary>
        /// Вид расходов
        /// </summary>
        [RequiredId(ErrorMessage = "Поле вид расходов обязательно для заполнения")]
        public int ExpenseTypeId { get; set; }

        /// <summary>
        /// Вид расходов
        /// </summary>
        public ExpenseType ExpenseType { get; set; }

        /// <summary>
        /// Контрагент РКО
        /// </summary>
        public int CashOutUserId { get; set; }

        /// <summary>
        /// Контрагент РКО
        /// </summary>
        public User CashOutUser { get; set; }

        /// <summary>
        /// Контрагент ПКО
        /// </summary>
        public int CashInUserId { get; set; }

        /// <summary>
        /// Контрагент ПКО
        /// </summary>
        public User CashInUser { get; set; }

        /// <summary>
        /// Бизнес-операция РКО
        /// </summary>
        public int CashOutBusinessOperationId { get; set; }

        /// <summary>
        /// Бизнес-операция ПКО
        /// </summary>
        public int CashInBusinessOperationId { get; set; }
    }
}
