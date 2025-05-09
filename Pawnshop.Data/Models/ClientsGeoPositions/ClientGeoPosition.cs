using System;
using Dapper.Contrib.Extensions;

namespace Pawnshop.Data.Models.ClientsGeoPositions
{
    [Table("ClientsGeopositions")]
    public sealed class ClientGeoPosition
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        [ExplicitKey]
        public Guid Id { get; set; }

        /// <summary>
        /// Широта
        /// </summary>
        public decimal Latitude { get; set; }

        /// <summary>
        /// Долгота
        /// </summary>
        public decimal Longitude { get; set; }

        /// <summary>
        /// Дата создания
        /// </summary>
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// Идентификатор клиента
        /// </summary>
        public int ClientId { get; set; }

        /// <summary>
        /// Дата сьёма геопозиции
        /// </summary>
        public DateTime? Date { get; set; }
        public ClientGeoPosition()
        {
            
        }

        public ClientGeoPosition(decimal latitude, decimal longitude, int clientId, DateTime? date)
        {
            Id = Guid.NewGuid();
            Latitude = latitude;
            Longitude = longitude;
            ClientId = clientId;
            CreateDate = DateTime.Now;
            Date = date;
        }
    }
}
