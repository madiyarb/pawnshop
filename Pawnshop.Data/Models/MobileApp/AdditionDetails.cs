using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.MobileApp
{
    public class AdditionDetails
    {
        public int? ApplicationId { get; set; }
        public Application? Application { get; set; }
        public decimal? AdditionAmount { get; set; }
        public bool IsChangeCategory { get; set; }
        public ProdKind? ProdKind { get; set; }
        /// <summary>
        /// Признак выдачи траншами: 1-ый транш на погашение долга у другого кредитора
        /// </summary>
        public bool IsFirstTransh { get; set; } = false;

        /// <summary>
        /// Общая сумма договора
        /// </summary>
        public decimal? TotalAmount4AllTransh { get; set; }
    }
}
