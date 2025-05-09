using Pawnshop.Core;
using System.ComponentModel.DataAnnotations;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Категория аналитики
    /// </summary>
    public class Category : IEntity
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
        /// Наименование на казахском языке
        /// </summary>
        public string? NameAlt { get; set; }
        /// <summary>
        /// Код 
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// Вид залога
        /// </summary>
        public CollateralType CollateralType { get; set; }

        /// <summary>
        /// Ссылка на статус стоянки по умолчанию
        /// </summary>
        public int? DefaultParkingStatusId { get; set; }

        public bool IsDisabled { get; set; }
    }
}
