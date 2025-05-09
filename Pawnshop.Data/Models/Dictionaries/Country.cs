using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;
using System.ComponentModel.DataAnnotations;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Страна
    /// </summary>
    public class Country : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Код
        /// </summary>
        [Required(ErrorMessage = "Поле Код обязательно")]
        public string Code { get; set; }

        /// <summary>
        /// Наименование на русском языке
        /// </summary>
        [Required(ErrorMessage = "Поле Наименование на русском обязательно")]
        public string NameRus { get; set; }

        /// <summary>
        /// Наименование на казахском языке
        /// </summary>
        [Required(ErrorMessage = "Поле Наименование на казахском обязательно")]
        public string NameKaz { get; set; }

        /// <summary>
        /// Идентификатор в КБ
        /// </summary>
        public int? CBId { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
    }
}