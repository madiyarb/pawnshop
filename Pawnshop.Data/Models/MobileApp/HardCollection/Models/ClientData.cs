using Newtonsoft.Json;
using Pawnshop.Data.Models.Clients;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.ViewModels
{
    public class ClientData
    {
        /// <summary>
        /// ФИО клиента
        /// </summary>
        [JsonProperty("FullName")]
        public string FullName { get; set; }

        /// <summary>
        /// ИИН клиента
        /// </summary>
        [JsonProperty("IIN")]
        public string IIN { get; set; }

        /// <summary>
        /// Список адресов клиента
        /// </summary>
        [JsonProperty("AddressList")]
        public ClientAddress[] AddressList { get; set; }

        /// <summary>
        /// Список контактов клиента
        /// </summary>
        [JsonProperty("ContactList")]
        public ClientContact[] ContactList { get; set; }

        /// <summary>
        /// Список дополнительных контактов
        /// </summary>
        [JsonProperty("AdditionalContactList")]
        public ClientAdditionalContact[] AdditionalContactList { get; set; }
    }
}
