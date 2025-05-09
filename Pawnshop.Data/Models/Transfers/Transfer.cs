using System;
using System.Collections.Generic;
using Pawnshop.Core;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.Transfers
{
    /// <summary>
    /// Трансфер
    /// </summary>
    public class Transfer : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Номер пула
        /// </summary>
        public int? PoolNumber { get; set; }

        /// <summary>
        /// Дата трансфера 
        /// </summary>
        public DateTime CreateDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Дата пула
        /// </summary>
        public DateTime? PoolDate { get; set; } 

        /// <summary>
        /// Дата удаления пула
        /// </summary>
        public DateTime? DeleteDate { get; set; }

        /// <summary>
        /// Статус пула
        /// </summary>
        public TransferStatus Status { get; set; } = TransferStatus.Draft;

        /// <summary>
        /// Номер пользователя
        /// </summary>
        public int? UserId { get; set; }

        public User User { get; set; }


    }
}
