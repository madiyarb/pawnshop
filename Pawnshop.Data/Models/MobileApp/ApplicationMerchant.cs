using Newtonsoft.Json;
using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp
{
    public class ApplicationMerchant : IEntity
    {
        /// <summary>
        /// Идентификатор продавца
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Имя/Наименование
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Фамилия
        /// </summary>
        public string? Surname { get; set; }
        /// <summary>
        /// Отчество
        /// </summary>
        public string? MiddleName { get; set; }
        /// <summary>
        /// Дата рождения
        /// </summary>
        public DateTime? BirthDay { get; set; }
        /// <summary>
        /// Вид документа
        /// </summary>
        public string DocumentTypeCode { get; set; }
        /// <summary>
        /// Место рождения/регистрации торговца
        /// </summary>
        public string? BirthOfPlace { get; set; }
        /// <summary>
        /// ИИН/БИН
        /// </summary>
        public string IdentityNumber { get; set; }
        /// <summary>
        /// Номер документа
        /// </summary>
        public string? LicenseNumber { get; set; }
        /// <summary>
        /// Дата выдачи документа
        /// </summary>
        public DateTime? LicenseDateOfIssue { get; set; }
        /// <summary>
        /// Срок действия документа
        /// </summary>
        public DateTime? LicenseDateOfEnd { get; set; }
        /// <summary>
        /// Кем выдан документ торговца
        /// </summary>
        public string LicenseIssuer { get; set; }   
        /// <summary>
        /// Признак правовой формы
        /// </summary>
        public string? DefinitionLegalPerson { get; set; }
        /// <summary>
        /// Признак автокредита
        /// </summary>
        public int? IsAutocredit { get; set; }
    }
}
