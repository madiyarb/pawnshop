using System;
using Pawnshop.Core;
using Pawnshop.Data.Models.AccountingCore;
using Pawnshop.Data.Models.Contracts.Actions;

namespace Pawnshop.Data.Models.Contracts
{
    /// <summary>
    /// Процентные ставки договора
    /// </summary>
    public class ContractRate : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Идентификатор  договора
        /// </summary>
        public int ContractId { get; set; }
        /// <summary>
        /// Идентификатор счета процентной ставки
        /// </summary>
        public int RateSettingId { get; set; }
        public AccountSetting RateSetting { get; set; }
        /// <summary>
        /// Дата начала действия ставки
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Ставка
        /// </summary>
        public decimal Rate { get; set; }
        /// <summary>
        /// Идентификатор действия, породившего ставку
        /// </summary>
        public int? ActionId { get; set; }
        public ContractAction Action { get; set; }
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