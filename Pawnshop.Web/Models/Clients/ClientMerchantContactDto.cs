using Pawnshop.Core.Utilities;
using Pawnshop.Data.Models.Clients;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pawnshop.Web.Models.Clients
{
    /// <summary>
    /// Клиент для мерчанта
    /// </summary>
    public class ClientMerchantContactDto
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public Client Client { get; set; }
        public List<ClientContactDto> Contacts{ get; set; }

    }
}
