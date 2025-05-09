using Pawnshop.Data.Models.Contracts.Inscriptions;
using Pawnshop.Data.Models.Membership;
using System;
using System.Collections.Generic;

namespace Pawnshop.Web.Models.Inscription
{
    public class InscriptionModel
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
        public List<InscriptionRow> RowsToWriteOff { get; set; }
        public int GetLinkedEntityId()
        {
            return ContractId;
        }
    }
}