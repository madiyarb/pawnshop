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
    public interface ICBSubject : IEntity
    {
        /// <summary>
        /// Идентификатор договора в KBContracts
        /// </summary>
        [Required(ErrorMessage = "Поле Идентификатор договора в KBContracts обязательно для заполнения")]
         int CBContractId { get; set; }

        /// <summary>
        /// Роль субъекта
        /// </summary>
        [Required(ErrorMessage = "Поле Роль субъекта обязательно для заполнения")]
         int RoleId { get; set; }

        /// <summary>
        /// Физ лицо/Юр лицо
        /// </summary>
        [Required(ErrorMessage = "Признак Физ лицо/Юр лицо обязателен для заполнения")]
         bool IsIndividual { get; set; }

        /// <summary>
        /// Массив адресов
        /// </summary>
        [RequiredId(ErrorMessage = " Массив адресов обязателен для заполнения")]
         long AddressesCollectionId { get; set; }

         List<CBAddress> Addresses { get; set; }

        /// <summary>
        /// Массив документов
        /// </summary>
        [RequiredId(ErrorMessage = "Массив документов обязателен для заполнения")]
         long IdentificationsCollectionId { get; set; }

         List<CBIdentification> Identifications { get; set; }
    }
}