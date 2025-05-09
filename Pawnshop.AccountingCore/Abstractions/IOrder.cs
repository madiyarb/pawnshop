using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.AccountingCore.Abstractions
{
    public interface IOrder : IEntity, IMultiCurrency, IStornoable
    {

        /// <summary>
        /// Тип кассового ордера
        /// </summary>
        public OrderType OrderType { get; set; }

        /// <summary>
        /// Договор
        /// </summary>
        int? ContractId { get; set; }

        /// <summary>
        /// Действие по договору
        /// </summary>
        int? ContractActionId { get; set; }

        /// <summary>
        /// Бизнес-операция
        /// </summary>
        int? BusinessOperationId { get; set; }

        /// <summary>
        /// Настройка бизнес-операции
        /// </summary>
        int? BusinessOperationSettingId { get; set; }

        /// <summary>
        /// Стоимость
        /// </summary>
        decimal OrderCost { get; set; }

        /// <summary>
        /// Стоимость в национальной валюте
        /// </summary>
        decimal OrderCostNC { get; set; }

        /// <summary>
        /// Идентификатор счета дебета
        /// </summary>
        [CustomValidation(typeof(IOrder), "DebitAccountIdValidate")]
        public int? DebitAccountId { get; set; }

        /// <summary>
        /// Идентификатор счета кредита
        /// </summary>
        [CustomValidation(typeof(IOrder), "CreditAccountIdValidate")]
        public int? CreditAccountId { get; set; }

        static ValidationResult DebitAccountIdValidate(int? value, ValidationContext context)
        {
            var order = (IOrder)context.ObjectInstance;

            if (value.HasValue && order.OrderType == OrderType.OffBalanceOut)
            {
                return new ValidationResult($"Расходный внебалансовый ордер не может иметь дебетовый счёт");
            }

            if (!value.HasValue && order.OrderType != OrderType.OffBalanceOut)
            {
                return new ValidationResult($"Дебетовый счёт обязателен к заполнению");
            }

            return ValidationResult.Success;
        }

        static ValidationResult CreditAccountIdValidate(int? value, ValidationContext context)
        {
            var order = (IOrder)context.ObjectInstance;

            if (value.HasValue && order.OrderType == OrderType.OffBalanceIn)
            {
                return new ValidationResult($"Приходный внебалансовый ордер не может иметь счёт кредита");
            }

            if (!value.HasValue && order.OrderType != OrderType.OffBalanceIn)
            {
                return new ValidationResult($"Счёт кредита обязателен к заполнению");
            }

            return ValidationResult.Success;
        }
    }
}
