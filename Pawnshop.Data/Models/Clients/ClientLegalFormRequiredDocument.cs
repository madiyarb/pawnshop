using System;
using Pawnshop.Core;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Models.Clients
{
    /// <summary>
    /// Обязательные документы для правовых форм
    /// </summary>
    public class ClientLegalFormRequiredDocument: IEntity
    {
        public int Id { get; set; }
        /// <summary>
        /// Идентификатор правовой формы
        /// </summary>
        public int LegalFormId { get; set; }
        public ClientLegalForm LegalForm { get; set; }
        /// <summary>
        /// Идентификатор типа документа
        /// </summary>
        public int DocumentTypeId { get; set; }
        public ClientDocumentType DocumentType { get; set; }
        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }
        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
        /// <summary>
        /// Идентификатор резидентства
        /// </summary>
        public bool IsResident { get; set; }
        /// <summary>
        /// Признак обязательности документа
        /// </summary>
        public bool IsMandatory { get; set; }
    }
}