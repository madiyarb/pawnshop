using Pawnshop.Core;
using Pawnshop.Core.Validation;
using Pawnshop.Data.Models.Membership;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Черный список винкодов
    /// </summary>
    public class VehiclesBlackListItem: IEntity, IBodyNumber
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Винкод
        /// </summary>
        [Required(ErrorMessage = "Поле винкод обязательно")]
        public string BodyNumber { get; set; }

        /// <summary>
        /// Винкод
        /// </summary>
        [Required(ErrorMessage = "Поле винкод обязательно")]
        public string ConfirmedBodyNumber { get; set; }

        /// <summary>
        /// Номер машины
        /// </summary>
        [Required(ErrorMessage = "Поле номер машины обязательно")]
        public string Number { get; set; }

        /// <summary>
        /// Причина включения в черный список 
        /// </summary>
        [RequiredId(ErrorMessage = "Поле Причина постановки обязательно для заполнения")]
        public int ReasonId { get; set; }

        public BlackListReason BlackListReason { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public int AuthorId { get; set; }

        /// <summary>
        /// Автор
        /// </summary>
        public User Author { get; set; }

        /// <summary>
        /// Примечание
        /// </summary>
        public string Note { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }
    }
}
