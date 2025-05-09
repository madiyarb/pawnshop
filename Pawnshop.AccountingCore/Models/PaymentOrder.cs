using System;
using Pawnshop.AccountingCore.Abstractions;

namespace Pawnshop.AccountingCore.Models
{
    /// <summary>
    /// Настройки порядка погашения
    /// </summary>
    public class PaymentOrder : IEntity, ICreateLogged
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Порядковый номер
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Счет для погашения
        /// </summary>
        public int AccountSettingId { get; set; }

        /// <summary>
        /// Признак погашения не в контрольную дату
        /// </summary>
        public bool NotOnScheduleDateAllowed { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Признак использования
        /// </summary>
        public bool IsActive { get; set; }
        /// <summary>
        /// Схема очередности погашения
        /// </summary>
        public int PaymentOrderSchema { get; set; }
    }
}
