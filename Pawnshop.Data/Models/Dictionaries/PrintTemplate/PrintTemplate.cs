using System;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.Dictionaries.PrintTemplates
{
    /// <summary>
    /// Шаблон печатной формы
    /// </summary>
    public class PrintTemplate : IDictionary
    {
        public int Id { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Наименование на втором языке
        /// </summary>
        public string NameAlt { get; set; }

        /// <summary>
        /// Код
        /// </summary>
        [Required(ErrorMessage = "Уникальный код обязателен к заполнению")]
        public string Code { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public User Author { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Имеет свою нумерацию
        /// </summary>
        public bool HasNumber { get; set; }

        /// <summary>
        /// Категория для распечатки печатных форм в одной кнопки
        /// </summary>
        public int? CategoryId { get; set; }
        public DomainValue? Category { get; set; }


        /// <summary>
        /// Подкатегория для распечатки печатных форм в одной кнопки
        /// </summary>
        public int? SubCategoryId { get; set; }
        public DomainValue? SubCategory { get; set; }
    }
}
