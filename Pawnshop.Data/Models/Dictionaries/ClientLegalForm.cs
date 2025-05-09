using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Data.Models.Clients;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Организационно - правовая форма
    /// </summary>
    public class ClientLegalForm : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Код
        /// </summary>
        [Required(ErrorMessage = "Поле Код обязательно для заполнения")]
        public string Code { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        [Required(ErrorMessage = "Поле Наименование обязательно для заполнения")]
        public string Name { get; set; }

        public string NameKaz { get; set; }

        /// <summary>
        /// Сокращенное наименование
        /// </summary>
        public string Abbreviation { get; set; }
        public string AbbreviationKaz { get; set; }

        /// <summary>
        /// Признак физ. лица
        /// </summary>
        [Required(ErrorMessage = "Поле Признак физ.лица обязательно для заполнения")]
        public bool IsIndividual { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Идентификатор в КБ
        /// </summary>
        public int? CBId { get; set; }
        /// <summary>
        /// Валидация по алгоритму ИИНа
        /// </summary>
        public bool HasIINValidation { get; set; }
        /// <summary>
        /// Валидация дня рождения по алгоритму дня рождения
        /// </summary>
        public bool HasBirthDayValidation { get; set; }
    }
}