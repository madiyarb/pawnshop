using System;
using System.ComponentModel.DataAnnotations;
using Pawnshop.AccountingCore.Models;
using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Contracts;
using Pawnshop.Data.Models.Contracts.Actions;
using Pawnshop.Data.Models.Membership;
using Type = Pawnshop.AccountingCore.Models.Type;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Вид клиентских расходов
    /// </summary>            
    public class Expense : IDictionary, IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>            
        public int Id { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>            
        [Required(ErrorMessage = "Поле наименование обязательно для заполнения")]
        public string Name { get; set; }

        /// <summary>
        /// Стоимость
        /// </summary>
        public int Cost { get; set; }

        /// <summary>
        /// По умолчанию
        /// </summary>
        public bool IsDefault { get; set; }

        // <summary>
        /// Пользователь, на которого будет вешаться расход
        /// </summary>
        public int? UserId { get; set; }

        // <summary>
        /// Пользователь, на которого будет вешаться расход
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Вид действия
        /// </summary>
        public ContractActionType? ActionType { get; set; }

        /// <summary>
        /// Вид залога
        /// </summary>
        [Required(ErrorMessage = "Поле вид залога обязательно для заполнения")]
        public CollateralType CollateralType { get; set; }

        /// <summary>
        /// Кто создал
        /// </summary>
        public int CreatedBy { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Дополнительные расходы
        /// </summary>
        public bool ExtraExpense { get; set; }

        /// <summary>
        /// Группы расходов компании
        /// </summary>
        [CustomValidation(typeof(Expense), "ExpenseTypeValidation")]
        public int? ExpenseTypeId { get; set; }


        /// <summary>
        /// Идентификатор иерархии типов
        /// </summary>
        public int? TypeId { get; set; }
        
        /// <summary>
        /// Иерархия типа
        /// </summary>
        public Type Type { get; set; }

        public bool NotFillUserid { get; set; }

        public string? Code { get; set; }

        public static ValidationResult ExpenseTypeValidation(int? value, ValidationContext context)
        {
            var expense = (Expense)context.ObjectInstance;

            if (expense.ExtraExpense && !value.HasValue)
            {
                return new ValidationResult("Не указан вид расхода");
            }

            return ValidationResult.Success;
        }
    }
}