using System;

namespace Pawnshop.Data.Models.AbsOnline
{
    /// <summary>
    /// Статус парковки
    /// </summary>
    public class CarStatusView
    {
        /// <summary>
        /// Параметр шины <b><u>credit_line</u></b> (номер контракта)
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>id</u></b> (идентификатор займа)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_status</u></b> (статус автомобиля)
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_status_code</u></b> (код статуса автомобиля)
        /// </summary>
        public string StatusCode { get; set; }

        /// <summary>
        /// Параметр шины <b><u>vin</u></b> (VIN)
        /// </summary>
        public string Vin { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_number</u></b> (гос.номер автомобиля)
        /// </summary>
        public string CarNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>update_date</u></b> (дата обновления статуса)
        /// </summary>
        public DateTime? UpdateDate { get; set; }
    }
}
