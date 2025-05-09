using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;
using Pawnshop.Data.Models.Membership;
namespace Pawnshop.Data.Models.Contracts.Inscriptions
{
    public class InscriptionAction : IEntity
    {
        public int Id { get; set; }
        /// <summary>
        /// Идентификатор исполнительной надписи
        /// </summary>
        public int InscriptionId { get; set; }
        /// <summary>
        /// Дата
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Тип действия
        /// </summary>
        public InscriptionActionType ActionType { get; set; }
        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }
        /// <summary>
        /// Автор
        /// </summary>
        public User Author { get; set; }
        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
        public List<InscriptionRow> Rows { get; set; }
    }
}