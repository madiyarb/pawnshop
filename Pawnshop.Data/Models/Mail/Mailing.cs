using System;
using System.Collections.Generic;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Mail
{
    public class Mailing : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Тип рассылки
        /// </summary>
        public MailingType MailingType { get; set; }
        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Тема рассылки
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// Текст рассылки
        /// </summary>
        public string MailingText { get; set; }
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

        public List<MailingAddress> MailingAddresses { get; set; }
    }
}