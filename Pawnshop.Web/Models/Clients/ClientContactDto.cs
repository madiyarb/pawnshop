using Pawnshop.Core.Utilities;
using Pawnshop.Data.Models.Clients;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Clients
{
    /// <summary>
    /// Контакт
    /// </summary>
    public class ClientContactDto
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Адрес
        /// </summary>
        [Required(ErrorMessage = "Поле 'Адрес(номер) контакта' обязательно для заполнения"), MaxLength(320)]
        public string Address { get; set; }

        /// <summary>
        /// Верификатация годна до
        /// </summary>
        public DateTime? VerificationExpireDate { get; set; }

        /// <summary>
        /// Флаг основного контакта
        /// </summary>
        public bool IsDefault { get; set; }

        [Required(ErrorMessage = "Поле 'Тип контакта' обязательно для заполнения")]
        public int? ContactTypeId { get; set; }

        /// <summary>
        /// Отправка чека УКассы на этот контакт
        /// </summary>
        public bool SendUkassaCheck { get; set; }
        /// <summary>
        /// Идентификатор категории контакта
        /// </summary>
        public int? ContactCategoryId { get; set; }
        /// <summary>
        /// Код категории контакта
        /// </summary>
        public string ContactCategoryCode { get; set; }
        /// <summary>
        /// Флаг актульного контакта 
        /// </summary>
        public bool IsActual { get; set; }
        /// <summary>
        /// Источник
        /// </summary>
        public int? SourceId { get; set; }
        /// <summary>
        /// Комментарий
        /// </summary>
        public string Note { get; set; }
        public bool IsEqualToDBModel(ClientContact clientContact)
        {
            if (clientContact == null)
                throw new ArgumentNullException(nameof(clientContact));

            if (Id != clientContact.Id)
                throw new InvalidOperationException($"Не совпадают Id модели из БД и текущего объекта");

            return Address == clientContact.Address && ContactTypeId == clientContact.ContactTypeId
                && IsDefault == clientContact.IsDefault;
        }
    }
}
