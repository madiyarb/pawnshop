using Pawnshop.Data.Models.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Bitrix
{
    public class BitrixLastPaymentModel
    {
        /// <summary>
        /// Номер договора
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Дата выдачи договора
        /// </summary>
        public DateTime ContractDate { get; set; }

        /// <summary>
        /// Сумма последнего проведенного платежа
        /// </summary>
        public decimal? LastPaymentCost { get; set; }

        /// <summary>
        /// Дата последнего платежа
        /// </summary>
        public DateTime? LastPaymentDate { get; set; }

        /// <summary>
        /// Статус договора (просрочен/ не просрочен)
        /// </summary>
        public bool ContractExpired { get; set; }


    }
}
