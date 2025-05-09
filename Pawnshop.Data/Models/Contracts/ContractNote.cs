using System;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.Contracts
{
    /// <summary>
    /// Примечание к договору
    /// </summary>
    public class ContractNote : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор договора
        /// </summary>
        [RequiredId(ErrorMessage = "Поле договор обязательно для заполнения")]
        public int ContractId { get; set; }

        /// <summary>
        /// Дата примечания
        /// </summary>
        [RequiredDate(ErrorMessage = "Поле дата примечания обязательно для заполнения")]
        public DateTime NoteDate { get; set; }

        /// <summary>
        /// Примечание
        /// </summary>
        [Required(ErrorMessage = "Поле примечание обязательно для заполнения")]
        public string Note { get; set; }

        /// <summary>
        /// Автор примечания
        /// </summary>
        [RequiredId(ErrorMessage = "Поле автор обязательно для заполнения")]
        public int AuthorId { get; set; }

        /// <summary>
        /// Автор примечания
        /// </summary>
        public User Author { get; set; }
    }
}