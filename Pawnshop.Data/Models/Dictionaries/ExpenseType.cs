using System;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Core.Validation;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Вид расходов
    /// </summary>            
    public class ExpenseType : IDictionary
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
        /// Группа расходов
        /// </summary>
        [RequiredId(ErrorMessage = "Поле группа расходов обязательно для заполнения")]
        public int ExpenseGroupId { get; set; }

        /// <summary>
        /// Группа расходов
        /// </summary>
        public ExpenseGroup ExpenseGroup { get; set; }

        /// <summary>
        /// Счет по умолчанию
        /// </summary>
        public int? AccountId { get; set; }

        /// <summary>
        /// Порядок
        /// </summary>
        [RequiredId(ErrorMessage = "Поле порядок обязательно для заполнения")]
        public int OrderBy { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
    }
}