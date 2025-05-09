using System;
using Pawnshop.Core;
using Pawnshop.Data.Models.Clients;

namespace Pawnshop.Data.Models.Insurances
{
    /// <summary>
    /// Тарифы страхования
    /// </summary>
    public class InsuranceRate : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Идентификатор СК
        /// </summary>
        public int InsuranceCompanyId { get; set; }
        public Client InsuranceCompany { get; set; }
        /// <summary>
        /// Сумма от
        /// </summary>
        public decimal AmountFrom { get; set; }
        /// <summary>
        /// Сумма до
        /// </summary>
        public decimal AmountTo { get; set; }
        /// <summary>
        /// Тариф в процентах
        /// </summary>
        public decimal Rate { get; set; }
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