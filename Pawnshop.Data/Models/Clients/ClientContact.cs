using Pawnshop.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Dapper.Contrib.Extensions;

namespace Pawnshop.Data.Models.Clients
{
    /// <summary>
    /// Контакт клиента
    /// </summary>
    [Table("ClientContacts")]
    public class ClientContact : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Адрес
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// Идентификатор типа контакта
        /// </summary>
        public int ContactTypeId { get; set; }

        /// <summary>
        /// Флаг основного контакта
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Дата экспирации
        /// </summary>
        public DateTime? VerificationExpireDate { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

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
        [Computed]
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

        [Computed]
        public bool isChanged { get; set; } = false;
        public ClientContact() { }
        public void UpdateDefaultPhoneFromMobile(string address, int contactTypeId, bool isDefault, int authorId, bool isActual, DateTime verificationExpireDate)
        {
            Address = address;
            ContactTypeId = contactTypeId;
            IsDefault = isDefault;
            AuthorId = authorId;
            IsActual = isActual;
            VerificationExpireDate = verificationExpireDate;
        }
    }
}
