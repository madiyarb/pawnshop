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
    /// Юр лица
    /// </summary>
    public class CBSubjectCompany : ICBSubject
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор договора в KBContracts
        /// </summary>
        [Required(ErrorMessage = "Поле Идентификатор договора в KBContracts обязательно для заполнения")]
        public int CBContractId { get; set; }

        /// <summary>
        /// Роль субъекта
        /// </summary>
        [Required(ErrorMessage = "Поле Роль субъекта обязательно для заполнения")]
        public int RoleId { get; set; }

        /// <summary>
        /// Физ лицо/Юр лицо
        /// </summary>
        [Required(ErrorMessage = "Признак Физ лицо/Юр лицо обязателен для заполнения")]
        public bool IsIndividual { get; set; }

        /// <summary>
        /// Массив адресов
        /// </summary>
        [RequiredId(ErrorMessage = " Массив адресов обязателен для заполнения")]
        public long AddressesCollectionId { get; set; }

        public List<CBAddress> Addresses { get; set; } = new List<CBAddress>();

        /// <summary>
        /// Массив документов
        /// </summary>
        [RequiredId(ErrorMessage = "Массив документов обязателен для заполнения")]
        public long IdentificationsCollectionId { get; set; }

        public List<CBIdentification> Identifications { get; set; } = new List<CBIdentification>();
        /// <summary>
        /// Наименование
        /// </summary>
        [Required(ErrorMessage = "Поле Наименование обязательно для заполнения")]
        public string Name { get; set; }

        /// <summary>
        /// Статус юридического лица
        /// </summary>
        [Required(ErrorMessage = "Поле Статус юридического лица обязательно для заполнения")]
        public int Status { get; set; }

        /// <summary>
        /// Торговая марка
        /// </summary>
        [Required(ErrorMessage = "Поле Торговая марка обязательно для заполнения")]
        public string TradeName { get; set; }

        /// <summary>
        /// Сокращенное наименование
        /// </summary>
        [Required(ErrorMessage = "Поле Сокращенное наименование обязательно для заполнения")]
        public string Abbreviation { get; set; }

        /// <summary>
        /// Правовая форма
        /// </summary>
        [Required(ErrorMessage = "Поле Правовая форма обязательно для заполнения")]
        public int LegalForm { get; set; }

        /// <summary>
        /// Страна регистрации
        /// </summary>
        [Required(ErrorMessage = "Поле Страна регистрации обязательно для заполнения")]
        public int Nationality { get; set; }

        /// <summary>
        /// Дата государственной регистрации
        /// </summary>
        [Required(ErrorMessage = "Поле Дата государственной регистрации обязательно для заполнения")]
        public DateTime RegistrationDate { get; set; }

        /// <summary>
        /// Имя первого руководителя
        /// </summary>
        [Required(ErrorMessage = "Поле Имя первого руководителя обязательно для заполнения")]
        public string CEOFirstName { get; set; }

        /// <summary>
        /// Фамилия первого руководителя
        /// </summary>
        [Required(ErrorMessage = "Поле Фамилия первого руководителя обязательно для заполнения")]
        public string CEOSurname { get; set; }

        /// <summary>
        /// Отчество первого руководителя
        /// </summary>
        public string CEOFathersName { get; set; }

        /// <summary>
        /// Дата рождения первого руководителя
        /// </summary>
        [Required(ErrorMessage = "Поле Дата рождения обязательно для заполнения")]
        public DateTime CEODateOfBirth { get; set; }

        /// <summary>
        /// Массив документов первого руководителя
        /// </summary>
        [Required(ErrorMessage = "Массив документов обязателен для заполнения")]
        public long CEOIdentificationsCollectionId { get; set; }

        public List<CBIdentification> CEOIdentifications { get; set; } = new List<CBIdentification>();
    }
}