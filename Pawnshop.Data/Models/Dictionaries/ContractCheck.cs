using Pawnshop.Data.Models.Contracts.Actions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Data.Models.Dictionaries
{
    public class ContractCheck : IDictionary
    {
        public int Id { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        [Required(ErrorMessage = "Поле \"Наименование\" обязательно к заполнению")]
        public string Name { get; set; }

        /// <summary>
        /// Текст соглашения
        /// </summary>
        [Required(ErrorMessage = "Поле \"Текст\" обязательно к заполнению")]
        public string Text { get; set; }

        /// <summary>
        /// Уникальный код
        /// </summary>
        [Required(ErrorMessage = "Поле \"Код\" обязательно к заполнению")]
        public string Code { get; set; }

        public bool PeriodRequired { get; set; }

        public int? DefaultPeriodAddedInYears { get; set; }

        public CollateralType? CollateralType { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
    }
}