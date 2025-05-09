using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.Core;
using Pawnshop.Data.Models.Dictionaries;
using Pawnshop.Data.Models.Membership;

namespace Pawnshop.Data.Models.Mintos
{
    public class MintosConfig : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Идентификатор валюты
        /// </summary>
        public int CurrencyId { get; set; }

        /// <summary>
        /// Валюта
        /// </summary>
        public Currency Currency { get; set; }

        /// <summary>
        /// Идентификатор организации
        /// </summary>
        public int OrganizationId { get; set; }

        /// <summary>
        /// Организация
        /// </summary>
        public Organization Organization { get; set; }

        /// <summary>
        /// Разрешать новые выгрузки
        /// </summary>
        public bool NewUploadAllowed { get; set; }

        /// <summary>
        /// Ключ для доступа к API
        /// </summary>
        public string ApiKey { get; set; }

        /// <summary>
        /// Процент инвестора
        /// </summary>
        public decimal InvestorInterestRate { get; set; }

        /// <summary>
        /// Дата удаления
        /// </summary>
        public DateTime? DeleteDate { get; set; }
    }
}
