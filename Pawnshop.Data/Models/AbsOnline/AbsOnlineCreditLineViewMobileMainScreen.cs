using System;

namespace Pawnshop.Data.Models.AbsOnline
{
    public sealed class AbsOnlineCreditLineViewMobileMainScreen
    {
        /// <summary>
        /// Параметр шины <b><u>credit_line</u></b> (номер контракта)
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>rest</u></b> (остаток суммы)
        /// </summary>
        public decimal RemainingAmount { get; set; }

        /// <summary>
        /// Параметр шины <b><u>end_date</u></b> (дата окончания)
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_number</u></b> (номерной знак (гос.номер))
        /// </summary>
        public string CarNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>car_vin</u></b> (VIN)
        /// </summary>
        public string CarVin { get; set; }
    }
}
