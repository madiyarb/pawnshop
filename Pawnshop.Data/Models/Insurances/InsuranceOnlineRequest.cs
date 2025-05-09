using Pawnshop.Core;
using Pawnshop.Data.Models.Contracts;
using System;

namespace Pawnshop.Data.Models.Insurances
{
    /// <summary>
    /// Онлайн-заявки на оформление страхового полиса
    /// </summary>
    public class InsuranceOnlineRequest : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Идентификатор договора
        /// </summary>
        public int ContractId { get; set; }
        public Contract Contract { get; set; }
        /// <summary>
        /// Данные запроса
        /// </summary>
        public InsuranceRequestData RequestData { get; set; }
        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
    }
}