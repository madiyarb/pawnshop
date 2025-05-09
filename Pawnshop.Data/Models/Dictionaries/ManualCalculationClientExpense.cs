using System;
using System.ComponentModel.DataAnnotations;
using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Ручной расчет расхода клиента
    /// </summary>
    public class ManualCalculationClientExpense : IEntity
    {
        /// <summary>Идентификатор</summary>
        public int Id { get; set; }

        /// <summary>Задолженность</summary>
        public int Debt { get; set; }

        /// <summary>Идентификатор клиента</summary>
        public int ClientId { get; set; }

        /// <summary>Клиент</summary>
        public Client Client { get; set; }

        /// <summary>Идентификатор автора</summary>
        public int AuthorId { get; set; }

        /// <summary>Автор</summary>
        public User Author { get; set; }

        /// <summary>Дата создания</summary>
        public DateTime Date { get; set; }

        /// <summary>Дата создания</summary>
        public DateTime CreateDate { get; set; }

        /// <summary>Дата удаления</summary>
        public DateTime? DeleteDate { get; set; }
    }
}