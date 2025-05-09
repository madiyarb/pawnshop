using System;
using Pawnshop.Core;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.Clients
{
    /// <summary>
    /// Список доступных документов для подписантов юр лиц
    /// </summary>
    public class ClientSignersAllowedDocumentType: IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Правовая форма юр.лица
        /// </summary>
        public int CompanyLegalFormId { get; set; }

        public ClientLegalForm CompanyLegalForm { get; set; }
        /// <summary>
        /// Документ-основание для подписи
        /// </summary>
        public int DocumentTypeId { get; set; }

        public ClientDocumentType DocumentType { get; set; }

        public DateTime CreateDate { get; set; }
        /// <summary>
        /// Идентификатор автора
        /// </summary>
        public int AuthorId { get; set; }
        public User Author { get; set; }
        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
        /// <summary>
        /// Признак обязательности документа
        /// </summary>
        public bool IsMandatory { get; set; }
    }
}