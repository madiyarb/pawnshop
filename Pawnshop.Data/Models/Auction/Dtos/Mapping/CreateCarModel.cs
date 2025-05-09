using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Auction.Dtos.Mapping
{
    public class CreateCarModel
    {
        /// <summary>
        /// Id авто в Аукционе
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Id авто предоставленный внешним микросервисом.
        /// </summary>
        public int ExternalCarId { get; set; }
        public string Color { get; set; }
        public string VinCode { get; set; }

        /// <summary>
        /// Номер авто при оформлении микро кредита
        /// </summary>
        public string TransportNumber { get; set; }

        /// <summary>
        /// Марка авто
        /// <example> Kia </example>
        /// </summary>
        public string Brand { get; set; }

        /// <summary>
        /// Модель авто
        /// <example> Rio </example>
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Год выпуска
        /// </summary>
        public int ReleaseYear { get; set; }

        /// <summary>
        /// Id статуса из файла Excel
        /// </summary>
        public int CarStatusId { get; set; }

        /// <summary>
        /// Данные договора при оформлении микро кредита
        /// </summary>
        public CreateContractDto Contract { get; set; }

        /// <summary>
        /// Данные клиента при микро займе
        /// </summary>
        // public ClientDto Client { get; set; }
        public CreateClientDto Client { get; set; }
    }
}
