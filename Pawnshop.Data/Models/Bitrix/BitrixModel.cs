using Pawnshop.Data.Models.Clients;
using Pawnshop.Data.Models.Collection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Bitrix
{
    public class BitrixModel
    {
        /// <summary>
        /// ФИО Клиента
        /// </summary>
        public string ClientFullName { get; set; }

        /// <summary>
        /// Все действующие договоры
        /// </summary>
        public List<BitrixLastPaymentModel> Contracts { get; set; }

    }
}
