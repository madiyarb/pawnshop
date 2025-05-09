using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.Dictionaries
{
    /// <summary>
    /// Валюта
    /// </summary>
    public class Currency : IDictionary
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Полное наименование валюты
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Код валюты, по ISO 4218
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Валюта договоров по умолчанию
        /// Основная валюта
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Обменный курс НацБанка
        /// </summary>
        public decimal CurrentNationalBankExchangeRate { get; set; }

        /// <summary>
        /// Кол-во за курс НацБанка
        /// </summary>
        public decimal CurrentNationalBankExchangeQuantity { get; set; }

        /// <summary>
        /// Дата последнего обновления
        /// </summary>
        public DateTime LastUpdate { get; set; }

        public decimal ExchangeRate => CurrentNationalBankExchangeQuantity / CurrentNationalBankExchangeRate;
    }
}
