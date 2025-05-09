using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;
using Pawnshop.Data.Models.Membership;
namespace Pawnshop.Data.Models.Contracts.Inscriptions
{
    /// <summary>
    /// Исполнительная надпись
    /// </summary>
    public class Inscription : IEntity, ILoggableToEntity
    {
        public int Id { get; set; }
        /// <summary>
        /// Идентификатор договора
        /// </summary>
        public int ContractId { get; set; }
        /// <summary>
        /// Общая сумма
        /// </summary>
        public decimal TotalCost { get; set; }
        /// <summary>
        /// Дата
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Статус
        /// </summary>
        public InscriptionStatus Status { get; set; }
        /// <summary>
        /// Действия
        /// </summary>
        public List<InscriptionAction> Actions { get; set; }
        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }
        /// <summary>
        /// Автор
        /// </summary>
        public User Author { get; set; }
        public List<InscriptionRow> Rows { get; set; }
        public int GetLinkedEntityId()
        {
            return ContractId;
        }
    }
}