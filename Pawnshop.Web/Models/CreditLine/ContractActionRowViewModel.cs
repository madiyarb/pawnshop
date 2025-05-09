using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Web.Models.CreditLine
{
    public sealed class ContractActionRowViewModel
    {
        /// <summary>
        /// Идентификатор ContractAction
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Номер контракта 
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Ссылка на действие
        /// </summary>
        public int ActionId { get; set; }

        /// <summary>
        /// Субъект договора
        /// </summary>
        public int? LoanSubjectId { get; set; }

        /// <summary>
        /// Тип погашения
        /// </summary>
        public AmountType PaymentType { get; set; }

        /// <summary>
        /// Период, дн
        /// </summary>
        public int? Period { get; set; }

        /// <summary>
        /// Процент оригинальный
        /// </summary>
        public decimal? OriginalPercent { get; set; }

        /// <summary>
        /// Процент
        /// </summary>
        public decimal? Percent { get; set; }

        /// <summary>
        /// Сумма погашения
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// Ссылка на дебет
        /// </summary>
        public int? DebitAccountId { get; set; }

        public Account DebitAccount { get; set; }

        /// <summary>
        /// Ссылка на кредит
        /// </summary>
        public int? CreditAccountId { get; set; }
        public Account CreditAccount { get; set; }

        /// <summary>
        /// Ссылка на ПКО
        /// </summary>
        public int OrderId { get; set; }


        /// <summary>
        /// Ссылка на настройку бизнес-операции
        /// </summary>
        public int? BusinessOperationSettingId { get; set; }

        /// <summary>
        /// Настройка бизнес-операции
        /// </summary>
        public BusinessOperationSetting BusinessOperationSetting { get; set; }
    }
}
