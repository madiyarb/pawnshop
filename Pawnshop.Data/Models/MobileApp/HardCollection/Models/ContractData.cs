using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.ViewModels
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ContractData
    {
        /// <summary>
        /// Данные контракта
        /// </summary>
        [JsonProperty("Contract")]
        public HardCollectionContract Contract { get; set; }

        /// <summary>
        /// Клиент
        /// </summary>
        [JsonProperty("ContractClient")]
        public ClientData ContractClient { get; set; }

        /// <summary>
        /// Автотранспорт
        /// </summary>
        [JsonProperty("ContractCar")]
        public CarData ContractCar { get; set; }
    }
}
