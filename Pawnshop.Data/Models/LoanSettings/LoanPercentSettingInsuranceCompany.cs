using System;
using Pawnshop.Core;
using Pawnshop.Data.Models.Clients;

namespace Pawnshop.Data.Models.LoanSettings
{
    /// <summary>
    /// Продуктовые настройки для СК
    /// </summary>
    public class LoanPercentSettingInsuranceCompany: IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Идентификатор продукта
        /// </summary>
        public int SettingId { get; set; }
        public LoanPercentSetting Setting { get; set; }
        /// <summary>
        /// Идентификатор СК
        /// </summary>
        public int InsuranceCompanyId { get; set; }
        public Client InsuranceCompany { get; set; }
        /// <summary>
        /// Срок страхования (месяцы)
        /// </summary>
        public int InsurancePeriod { get; set; }
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
        /// <summary>
        /// Максимальная сумма премии
        /// </summary>
        public decimal MaxPremium { get; set; }
        /// <summary>
        /// Дельта для сравнивания с суммой доплаты страхового полиса
        /// </summary>
        public decimal PremiumAccuracy { get; set; }
    }
}