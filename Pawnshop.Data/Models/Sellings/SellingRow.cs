using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.CashOrders;
using System;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Data.Models.Sellings
{
    public class SellingRow : IEntity
    {
        public int Id { get; set; }

        /// <summary>
        /// Ссылка на реализацию
        /// </summary>
        public int SellingId { get; set; }

        /// <summary>
        /// Тип погашения
        /// </summary>
        public SellingPaymentType SellingPaymentType { get; set; }

        /// <summary>
        /// Сумма погашения
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// Ссылка на дебет
        /// </summary>
        [RequiredId(ErrorMessage = "Поле \"Дебет\" обязательно для заполнения")]
        public int DebitAccountId { get; set; }

        /// <summary>
        /// Ссылка на кредит
        /// </summary>
        [RequiredId(ErrorMessage = "Поле \"Кредит\" обязательно для заполнения")]
        public int CreditAccountId { get; set; }

        /// <summary>
        /// Ссылка на ПКО
        /// </summary>
        public int OrderId { get; set; }

        /// <summary>
        /// Ссылка на действие
        /// </summary>
        public int ActionId { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Идентификатор скидки по договору
        /// </summary>
        public int? ContractDiscountId { get; set; }

        /// <summary>
        /// Идентификатор скидки по договору
        /// </summary>
        public decimal? ExtraExpensesCost { get; set; }
        
        public OrderType OrderType { get; set; }
    }
}
