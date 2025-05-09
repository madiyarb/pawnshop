using System;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Contracts.Revisions
{
    /// <summary>
    /// Ревизия
    /// </summary>
    public class Revision : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Номер позиции
        /// </summary>
        public int PositionId { get; set; }
        /// <summary>
        /// Дата ревизии
        /// </summary>
        public DateTime RevisionDate { get; set; }
        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// Статус ревизии
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// Примечание
        /// </summary>
        public string Note { get; set; }
        /// <summary>
        /// Ифентификатор договора
        /// </summary>
        public int ContractId { get; set; }
    }
}