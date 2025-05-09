using System;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Mail
{
    public class MailingAddress : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Идентификатор рассылки
        /// </summary>
        public int MailingId { get; set; }
        public Mailing Mailing { get; set; }
        /// <summary>
        /// Наименование получателя
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Адрес электронной почты
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// Основной адрес
        /// </summary>
        public bool IsDefault { get; set; }
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