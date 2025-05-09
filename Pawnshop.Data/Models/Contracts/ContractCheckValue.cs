using Pawnshop.Core;
using Pawnshop.Data.Models.Dictionaries;
using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.Contracts
{
    public class ContractCheckValue : IEntity
    {
        public int Id { get; set; }
        /// <summary>
        /// Договор
        /// </summary>
        public int ContractId { get; set; }
        /// <summary>
        /// Проверка
        /// </summary>
        public int CheckId { get; set; }
        /// <summary>
        /// Проверка
        /// </summary>
        public ContractCheck Check { get; set; }
        /// <summary>
        /// Значение
        /// </summary>
        public bool Value { get; set; }
        /// <summary>
        /// Дата предоставления разрешения
        /// </summary>
        public DateTime? BeginDate { get; set; }
        /// <summary>
        /// Разрешение действительно до
        /// </summary>
        public DateTime? EndDate { get; set; }
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
    }
}
