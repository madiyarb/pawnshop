using System;
using Pawnshop.Data.Models.CreditLines;

namespace Pawnshop.Data.Models.ApplicationsOnline.Views
{
    public sealed class ApplicationOnlineCreditLineView
    {
        /// <summary>
        /// Статус парковок машин
        /// </summary>
        public string CarParkingStatus { get; set; }

        /// <summary>
        /// Cтатус кредитной линии
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Дата завершения
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Остаток по кредитной линии
        /// </summary>
        public decimal CreditLineLimit { get; set; }

        /// <summary>
        /// Сумма выкупа
        /// </summary>
        public decimal BuyOutAmount { get; set; }

        /// <summary>
        /// Основной долг по КЛ
        /// </summary>
        public decimal CreditLineMainDebt { get; set; }

        /// <summary>
        /// Кол-во дней просрочки
        /// </summary>
        public int ExpiredDays { get; set; }

        /// <summary>
        /// Дата последнего обновления
        /// </summary>
        public DateTime UpdateDate { get; set; }
        

    }
}
