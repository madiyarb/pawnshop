using System;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.TasOnline
{
    public class TasOnlineRequest : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Отправленные данные
        /// </summary>
        public string RequestData { get; set; }
        /// <summary>
        /// Данные ответа
        /// </summary>
        public string ResponseData { get; set; }
        public object ResponseDataObject { get; set; }
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
        public DateTime DeleteDate { get; set; }
        /// <summary>
        /// Статус запроса
        /// </summary>
        public TasOnlineRequestStatus Status { get; set; }
        /// <summary>
        /// Идентификатор оплаты
        /// </summary>
        public int? PaymentId { get; set; }
        public TasOnlinePayment Payment { get; set; }
    }
}