using Pawnshop.Core;
using System;
using System.Collections.Generic;

namespace Pawnshop.Data.Models.OnlineApplications
{
    /// <summary>
    /// Онлайн заявка
    /// </summary>
    public class OnlineApplication : IEntity
    {
        /// <summary>
        /// Идентификатор записи
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Номер займа
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Статус заявки
        /// </summary>
        public OnlineApplicationStatusType Status { get; set; }
        
        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Дата выкупа
        /// </summary>
        public DateTime MaturityDate { get; set; }

        /// <summary>
        /// Идентификатор филиала
        /// </summary>
        public int? BranchId { get; set; }

        /// <summary>
        /// Срок займа
        /// </summary>
        public int Period { get; set; }

        /// <summary>
        /// Сумма займа
        /// </summary>
        public decimal LoanCost { get; set; }

        /// <summary>
        /// LTV
        /// </summary>
        public decimal LTV { get; set; }

        /// <summary>
        /// Признак использования страховки
        /// </summary>
        public bool WithInsurance { get; set; }

        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// Идентификатор документа клиента
        /// </summary>
        public int? ClientDocumentId { get; set; }

        /// <summary>
        /// Идентификатор залоговой позиции
        /// </summary>
        public int? OnlinePositionId { get; set; }

        /// <summary>
        /// Позиция
        /// </summary>
        public OnlineApplicationPosition Position { get; set; }

        /// <summary>
        /// Идентификатор продукта
        /// </summary>
        public int SettingId { get; set; }

        /// <summary>
        /// Идентификатор продукта кредитной линии
        /// </summary>
        public int? CreditLineSettingId { get; set; }

        /// <summary>
        /// Код партнера
        /// </summary>
        public string PartnerCode { get; set; }

        /// <summary>
        /// Дата первого платежа
        /// </summary>
        public DateTime? FirstPaymentDate { get; set; }

        /// <summary>
        /// День платежа
        /// </summary>
        public int? PayDay { get; set; }
        
        /// <summary>
        /// Идентификатор займа
        /// </summary>
        public int? ContractId { get; set; }

        /// <summary>
        /// Идентификатор кредитной линии
        /// </summary>
        public int? CreditLineId { get; set; }

        /// <summary>
        /// Признак требования открытия кредитной линии
        /// </summary>
        public bool IsOpeningCreditLine { get; set; }

        /// <summary>
        /// Система откуда подали заявку
        /// </summary>
        public string Source { get; set; }

        /// <summary>
        /// UTM метки
        /// </summary>
        public string UtmTags { get; set; }

        /// <summary>
        /// Список рефинансируемых займов 
        /// </summary>
        public List<OnlineApplicationRefinance> OnlineApplicationRefinances { get; set; } =
            new List<OnlineApplicationRefinance>();
    }
}
