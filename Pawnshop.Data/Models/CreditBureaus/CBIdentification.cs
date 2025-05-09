using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Pawnshop.Core.Validation;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Субъекты контракта
    /// </summary>
    public class CBIdentification : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор массива
        /// </summary>
        [RequiredId(ErrorMessage = "Поле Идентификатор массива обязательно для заполнения")]
        public long CollectionId { get; set; }

        /// <summary>
        /// Тип документа
        /// </summary>
        [RequiredId(ErrorMessage = "Поле Тип документаобязательно для заполнения")]
        public int TypeId { get; set; }

        /// <summary>
        /// Категория документа
        /// </summary>
        [Required(ErrorMessage = "Поле Категория документа  обязательно для заполнения")]
        public int Rank { get; set; }

        /// <summary>
        /// Номер документа
        /// </summary>
        [Required(ErrorMessage = "Поле Номер документа обязательно для заполнения")]
        public string Number { get; set; }
        
        /// <summary>
        /// Дата выдачи документа
        /// </summary>
        public DateTime? IssueDate { get; set; }
        
        /// <summary>
        /// Дата окончания срока действия документа
        /// </summary>
        public DateTime? ExpirationDate { get; set; }
        
        /// <summary>
        /// Тип документа (доп поле)
        /// </summary>
        public string DocumentTypeText { get; set; }

        /// <summary>
        /// Дата регистрации документа в системе TAS
        /// </summary>
        [Required(ErrorMessage = "Поле Дата регистрации документа обязательно для заполнения")]
        public DateTime RegistrationDate { get; set; }
    }
}