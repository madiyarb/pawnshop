using Newtonsoft.Json;

namespace Pawnshop.Data.Models.MobileApp.HardCollection.ViewModels
{
    public class CarData
    {
        /// <summary>
        /// Номер кузова
        /// </summary>
        [JsonProperty("BodyNumber")]
        public string BodyNumber { get; set; }
        /// <summary>
        /// Гос номер
        /// </summary
        [JsonProperty("TransportNumber")]
        public string TransportNumber { get; set; }

        /// <summary>
        /// Марка
        /// </summary>
        [JsonProperty("Mark")]
        public string Mark { get; set; }

        /// <summary>
        /// Модель
        /// </summary>
        [JsonProperty("Model")]
        public string Model { get; set; }

        /// <summary>
        /// Id Статуса стоянки
        /// </summary>
        [JsonProperty("ParkingStatusId")]
        public int ParkingStatusId { get; set; }

        /// <summary>
        /// Год выпуска
        /// </summary>
        [JsonProperty("ReleaseYear")]
        public int ReleaseYear { get; set; }
    }
}
