using System;
using Pawnshop.Core;
using Pawnshop.Data.Models.Domains;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.Contracts.LoanFinancePlans
{
    /// <summary>
    /// План финансирования
    /// </summary>
    public class LoanFinancePlan : IEntity
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
        /// Цель кредитования
        /// </summary>
        public int LoanPurposeId { get; set; }
        public DomainValue LoanPurpose { get; set; }
        /// <summary>
        /// Описание
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Собственные средства
        /// </summary>
        public decimal OwnFunds { get; set; }
        /// <summary>
        /// Заемные средства
        /// </summary>
        public decimal DebtFunds { get; set; }
        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }
        public User Author { get; set; }
        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
    }
}