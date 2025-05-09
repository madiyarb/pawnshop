using Pawnshop.Core;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Domains;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Субъекты с привязкой к позиции
    /// </summary>
    public class PositionSubject : IEntity
    {
        public int Id { get; set; } = 0;
        /// <summary>
        /// Идентификатор позиции
        /// </summary>
        public int PositionId { get; set; }
        /// <summary>
        /// Идентификатор вида субъекта
        /// </summary>
        public int SubjectId { get; set; }
        public LoanSubject? Subject { get; set; }
        /// <summary>
        /// Идентификатор субъекта
        /// </summary>
        public int ClientId { get; set; }
        public Client? Client { get; set; }
        public int AuthorId { get; set; } = 0;
        public DateTime CreateDate { get; set; } = new DateTime();
        public DateTime? DeleteDate { get; set; }
    }
}
