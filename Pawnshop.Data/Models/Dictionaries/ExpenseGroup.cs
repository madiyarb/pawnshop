using System;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Core.Validation;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Группа расходов
    /// </summary>            
    public class ExpenseGroup : IDictionary
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