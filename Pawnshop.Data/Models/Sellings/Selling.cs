using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.Sellings
{
    public class Selling : IEntity, IOwnable
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Вид залога
        /// </summary>
        public CollateralType CollateralType { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        [RequiredDate(ErrorMessage = "Поле дата создания обязательно для заполнения")]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Договор
        /// </summary>
        public int ContractId { get; set; }

        /// <summary>
        /// Договор
        /// </summary>
        public Contract Contract { get; set; }

        /// <summary>
        /// Позиция договора
        /// </summary>
        public int? ContractPositionId { get; set; }

        /// <summary>
        /// Позиция договора
        /// </summary>
        public ContractPosition ContractPosition { get; set; }

        /// <summary>
        /// Позиция
        /// </summary>
        [RequiredId(ErrorMessage = "Поле позиция обязательно для заполнения")]
        public int PositionId { get; set; }

        /// <summary>
        /// Позиция
        /// </summary>
        public Position Position { get; set; }

        /// <summary>
        /// Себестоимость
        /// </summary>
        public decimal PriceCost { get; set; }

        /// <summary>
        /// Примечание
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Специфичные поля позиции договора
        /// </summary>
        public GoldContractSpecific PositionSpecific { get; set; }

        /// <summary>
        /// Стоимость продажи
        /// </summary>
        [CustomValidation(typeof(Selling), "SellingCostValidate")]
        public decimal? SellingCost { get; set; }

        /// <summary>
        /// Дата продажи
        /// </summary>
        [CustomValidation(typeof(Selling), "SellingDateValidate")]
        public DateTime? SellingDate { get; set; }

        /// <summary>
        /// Приходный кассовый ордер
        /// </summary>
        public int? CashOrderId { get; set; }

        /// <summary>
        /// Приходный кассовый ордер
        /// </summary>
        public CashOrder CashOrder { get; set; }

        /// <summary>
        /// Статус
        /// </summary>
        public SellingStatus Status { get; set; }

        /// <summary>
        /// Идентификатор владельца
        /// </summary>
        [RequiredId(ErrorMessage = "Поле владелец обязательно для заполнения")]
        public int OwnerId { get; set; }

        /// <summary>
        /// Филиал, в котором создана реализация
        /// </summary>
        [RequiredId(ErrorMessage = "Поле филиал обязательно для заполнения")]
        public int BranchId { get; set; }

        /// <summary>
        /// Филиал, в котором создана реализация
        /// </summary>
        public Group Branch { get; set; }

        /// <summary>
        /// Автор реализации
        /// </summary>
        [RequiredId(ErrorMessage = "Поле автор обязательно для заполнения")]
        public int AuthorId { get; set; }

        /// <summary>
        /// Автор договора
        /// </summary>
        public User Author { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Записи в рамках действия
        /// </summary>
        public List<ContractActionRow> ActionRows { get; set; }

        /// <summary>
        /// Записи в рамках действия по реализации
        /// </summary>
        public List<ContractActionRow> SellingActionRows { get; set; }

        /// <summary>
        /// Записи в рамках реализации
        /// </summary>
        public List<SellingRow> SellingRows { get; set; }

        /// <summary>
        /// Сумма дополнительных расходов
        /// </summary>
        public decimal ExtraExpensesCost { get; set; } = 0;

        public ContractDutyDiscount Discount { get; set; }

        /// <summary>
        /// Остаток на авансе
        /// </summary>
        public decimal PrepaymentBalance { get; set; }

        public static ValidationResult SellingCostValidate(int? value, ValidationContext context)
        {
            var selling = (Selling)context.ObjectInstance;

            if (selling.Status == SellingStatus.Sold && !value.HasValue)
            {
                return new ValidationResult("Поле стоимость продажи ообязательно для заполнения");
            }

            return ValidationResult.Success;
        }

        public static ValidationResult SellingDateValidate(DateTime? value, ValidationContext context)
        {
            var selling = (Selling)context.ObjectInstance;

            if (selling.Status == SellingStatus.Sold && !value.HasValue)
            {
                return new ValidationResult("Поле дата продажи ообязательно для заполнения");
            }

            return ValidationResult.Success;
        }

        public decimal GetDiffPrepaymentBalanceAndPriceCost()
        {
            return Math.Ceiling(Math.Abs(PrepaymentBalance - PriceCost));
        }

        public decimal GetDiffSellingCostAndPriceCost()
        {
            return SellingCost!=null?(decimal)SellingCost- GetDiffPrepaymentBalanceAndPriceCost() : 0;
        }

        public SellingPaymentType GetSellingPaymentType()
        {
            return GetDiffSellingCostAndPriceCost()>=0 ? SellingPaymentType.SellingProfit : SellingPaymentType.SellingLoss;
        }
    }
}