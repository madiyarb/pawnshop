using System;
using Pawnshop.Core;
using Pawnshop.Data.Models.Dictionaries;

namespace Pawnshop.Data.Models.Clients
{
    /// <summary>
    /// Обязательные поля для проверки клиентской карты
    /// </summary>
    public class ClientLegalFormValidationField: IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Идентификатор правовой формы
        /// </summary>
        public int LegalFormId { get; set; }

        public ClientLegalForm LegalForm { get; set; }
        /// <summary>
        /// Код поля
        /// </summary>
        public string FieldCode { get; set; }
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
    }
}