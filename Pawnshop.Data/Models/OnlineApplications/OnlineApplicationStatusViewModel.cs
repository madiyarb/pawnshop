using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.OnlineApplications
{
    public class OnlineApplicationStatusViewModel
    {
        /// <summary>
        /// Идентификатор записи
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Номер займа
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Статус заявки
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }
    }
}