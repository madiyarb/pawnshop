using Pawnshop.Core;
using System;

namespace Pawnshop.Data.Models.Parking
{
    public class ParkingDocument : IEntity
    {
        /// <summary>
        /// Идентификатор
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// История стоянки
        /// </summary>
        public int ParkingHistoryId { get; set; }

        /// <summary>
        /// Тип документов
        /// </summary>
        public string DocumentType { get; set; }

        /// <summary>
        /// Дата
        /// </summary>
        public DateTime Date { get; set; }
    }
}
