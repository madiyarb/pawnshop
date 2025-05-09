using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Postponements;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Contracts.Postponements
{
    /// <summary>
    /// Отсрочки по договору
    /// </summary>
    public class ContractPostponement : IEntity, ILoggableToEntity
    {
        public int Id { get; set; }

        /// <summary>
        /// Договор
        /// </summary>
        [RequiredId(ErrorMessage = "Не найден идентификатор договора")]
        public int ContractId { get; set; }

        /// <summary>
        /// Вид отсрочки
        /// </summary>
        [RequiredId(ErrorMessage = "Вид отсрочки обязателен к заполнению")] 
        public int PostponementId { get; set; }
        public Postponement Postponement { get; set; }

        /// <summary>
        /// До какого числа отсрочка
        /// </summary>
        [RequiredDate(ErrorMessage ="Дата отсрочки обязательна к заполнению")]
        public DateTime Date { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        public int GetLinkedEntityId()
        {
            return ContractId;
        }
    }
}
