using System;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Core;
using Pawnshop.Core.Exceptions;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.Clients
{
    /// <summary>
    /// Подписанты юр.лиц
    /// </summary>
    public class ClientSigner : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Идентификатор юр. лица
        /// </summary>
        public int CompanyId { get; set; }
        public Client Company { get; set; }
        /// <summary>
        /// Подписант -  физ. лицо
        /// </summary>
        public int SignerId { get; set; }
        public Client Signer { get; set; }
        /// <summary>
        /// Должность подписанта
        /// </summary>
        public int SignerPositionId { get; set; }
        public DomainValue SignerPosition { get; set; }
        /// <summary>
        /// Документ-основание для подписи
        /// </summary>
        [CustomValidation(typeof(ClientSigner), "SignerDocumentValidate")]
        public int DocumentId { get; set; }
        public ClientDocument SignerDocument { get; set; }
        /// <summary>
        /// Дата создания
        /// </summary>
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

        public static ValidationResult SignerDocumentValidate(int id)
        {
            if (id == 0)
                throw new PawnshopApplicationException(
                    $"Не выбран документ на основании которого действует подписант");

            return ValidationResult.Success;
        }
    }
}