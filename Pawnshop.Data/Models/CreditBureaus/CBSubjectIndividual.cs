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
    /// Физ лица
    /// </summary>
    public class CBSubjectIndividual : ICBSubject
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
        /// Имя
        /// </summary>
        [Required(ErrorMessage = "Поле Имя обязательно для заполнения")]
        public string FirstName { get; set; }

        /// <summary>
        /// Фамилия
        /// </summary>
        [Required(ErrorMessage = "Поле Фамилия обязательно для заполнения")]
        public string Surname { get; set; }

        /// <summary>
        /// Отчество
        /// </summary>
        public string FathersName { get; set; }

        /// <summary>
        /// Пол 
        /// </summary>
        [Required(ErrorMessage = "Поле Пол обязательно для заполнения")]
        public string Gender { get; set; }

        /// <summary>
        /// Классификация
        /// </summary>
        [Required(ErrorMessage = "Поле Классификация обязательно для заполнения")]
        public int Classification { get; set; }

        /// <summary>
        /// Признак резидентства
        /// </summary>
        [Required(ErrorMessage = "Поле Признак резидентства обязательно для заполнения")]
        public int Residency { get; set; }

        /// <summary>
        /// Гражданство
        /// </summary>
        [Required(ErrorMessage = "Поле Гражданство обязательно для заполнения")]
        public int Citizenship { get; set; }

        /// <summary>
        /// Дата рождения
        /// </summary>
        [Required(ErrorMessage = "Поле Дата рождения обязательно для заполнения")]
        public DateTime DateOfBirth { get; set; }
    }
}