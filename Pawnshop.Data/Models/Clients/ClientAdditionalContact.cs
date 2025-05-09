using Pawnshop.Core;
using System;

namespace Pawnshop.Data.Models.Clients
{
    /// <summary>
    /// Доп контакт клиента
    /// </summary>
    public class ClientAdditionalContact : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Номер телефона
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// Идентификатор типа владельца контакта
        /// </summary>
        public int ContactOwnerTypeId { get; set; }

        /// <summary>
        /// ФИО владельца контакта
        /// </summary>
        public string ContactOwnerFullname { get; set; }

        /// <summary>
        /// Идентфикатор пользователя который создал эту запись
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
        /// <summary>
        /// Заметка
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Признак что контакт есть в списке контактов в МП
        /// </summary>
        public bool FromContactList { get; set; }

        /// <summary>
        /// Имя контакта в списке контактов в МП
        /// </summary>
        public string ContactListName { get; set; }

        /// <summary>
        /// Признак основного плательщика
        /// </summary>
        public bool IsMainPayer { get; set; }

        public bool Equals(ClientAdditionalContact clientAdditionalContact)
        {
            if ((object)clientAdditionalContact == null)
                return false;
            return Id == clientAdditionalContact.Id &&
                   PhoneNumber == clientAdditionalContact.PhoneNumber &&
                   ClientId == clientAdditionalContact.ClientId &&
                   ContactOwnerTypeId == clientAdditionalContact.ContactOwnerTypeId &&
                   ContactOwnerFullname == clientAdditionalContact.ContactOwnerFullname &&
                   AuthorId == clientAdditionalContact.AuthorId &&
                   DeleteDate == clientAdditionalContact.DeleteDate &&
                   Note == clientAdditionalContact.Note &&
                   FromContactList == clientAdditionalContact.FromContactList &&
                   ContactListName == clientAdditionalContact.ContactListName &&
                   IsMainPayer == clientAdditionalContact.IsMainPayer;
        }
    }
}
