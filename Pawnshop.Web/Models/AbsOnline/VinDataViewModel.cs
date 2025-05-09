namespace Pawnshop.Web.Models.AbsOnline
{
    /// <summary>
    /// Данные по VIN
    /// </summary>
    public class VinDataViewModel
    {
        /// <summary>
        /// Параметр шины <b><u>vin</u></b> (VIN автомобиля)
        /// </summary>
        public string Vin { get; set; }

        /// <summary>
        /// Параметр шины <b><u>od</u></b> (Основной долг)
        /// </summary>
        public decimal PrincipalDebt { get; set; }

        /// <summary>
        /// Параметр шины <b><u>percent</u></b> (Проценты)
        /// </summary>
        public decimal Percent { get; set; }

        /// <summary>
        /// Параметр шины <b><u>fine</u></b> (Пени/штраф)
        /// </summary>
        public decimal Penalty { get; set; }
    }
}
