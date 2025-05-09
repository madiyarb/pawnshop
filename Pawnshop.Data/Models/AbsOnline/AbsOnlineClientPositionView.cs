namespace Pawnshop.Data.Models.AbsOnline
{
    public class AbsOnlineClientPositionView
    {
        /// <summary>
        /// Параметр шины <b><u>type</u></b> (тип позиции залога)
        /// </summary>
        public string PositionType { get; set; }

        /// <summary>
        /// Параметр шины <b><u>vin</u></b> (VIN)
        /// </summary>
        public string Vin { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_brand</u></b> (Марка)
        /// </summary>
        public string CarBrand { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_model</u></b> (Модель)
        /// </summary>
        public string CarModel { get; set; }

        /// <summary>
        /// Параметр шины <b><u>year</u></b> (Год выпуска)
        /// </summary>
        public int? Year { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_id</u></b> (Номер автомобиля)
        /// </summary>
        public string CarNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>rka</u></b> (РКА код)
        /// </summary>
        public string Rka { get; set; }

        /// <summary>
        /// Параметр шины <b><u>address</u></b> (Адрес объекта)
        /// </summary>
        public string Address { get; set; }
    }
}
