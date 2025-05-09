using Pawnshop.Core;
using Pawnshop.Data.Models.CashOrders;
using Pawnshop.Data.Models.TasOnline;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.UKassa
{
    public class UKassaRequest : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор для идемпотентности запросов
        /// </summary>
        public Guid RequestId { get; set; }

        /// <summary>
        /// номер кассового ордера
        /// </summary>
        public int CashOrderId { get; set; }

        /// <summary>
        /// Отправленные данные
        /// </summary>
        public string RequestData { get; set; }

        /// <summary>
        /// Данные ответа
        /// </summary>
        public string ResponseData { get; set; }

        /// <summary>
        /// Номер созданного чека
        /// </summary>
        public string ResponseCheckNumber { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }
        
        /// <summary>
        /// идентификатор кассы
        /// </summary>
        public int KassaId { get; set; }

        /// <summary>
        /// идентификтор отдела/филиала
        /// </summary>
        public int SectionId { get; set; }

        /// <summary>
        /// номер смены
        /// </summary>
        public int ShiftNumber { get; set; }

        /// <summary>
        /// общая сумма чека
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// Тип операции 
        /// </summary>
        public int OperationType { get; set; }

        /// <summary>
        /// Дата создания   
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Статус запроса
        /// </summary>
        public TasOnlineRequestStatus Status { get; set; }

        /// <summary>
        /// URL-Адрес операции
        /// </summary>
        public string RequestUrl { get; set; }

        /// <summary>
        /// Дата ответа   
        /// </summary>
        public DateTime ResponseDate { get; set; }

        /// <summary>
        /// Дата запроса   
        /// </summary>
        public DateTime RequestDate { get; set; }

        public virtual CashOrder CashOrder { get; set; }
    }
}
