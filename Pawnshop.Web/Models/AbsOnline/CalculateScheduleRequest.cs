using Microsoft.AspNetCore.Mvc;

namespace Pawnshop.Web.Models.AbsOnline
{
    /// <summary>
    /// Параметра запроса на расчет графика погашения платежей
    /// </summary>
    public class CalculateScheduleRequest
    {
        /// <summary>
        /// Параметр шины <b><u>uin</u></b> (ИИН субъекта)
        /// </summary>
        [FromQuery(Name = "iin")]
        public string IIN { get; set; }

        /// <summary>
        /// Параметр шины <b><u>product_id</u></b> (идентификатор продукта)
        /// </summary>
        [FromQuery(Name = "productId")]
        public string ProductId { get; set; }

        /// <summary>
        /// Параметр шины <b><u>summa</u></b> (сумма займа)
        /// </summary>
        [FromQuery(Name = "loanCost")]
        public decimal LoanCost { get; set; }

        /// <summary>
        /// Параметр шины <b><u>pay_count</u></b> (срок займа)
        /// </summary>
        [FromQuery(Name = "period")]
        public int Period { get; set; }

        /// <summary>
        /// Параметр шины <b><u>tranche</u></b> (признак транша)
        /// </summary>
        [FromQuery(Name = "tranche")]
        public bool Tranche { get; set; }

        /// <summary>
        /// Параметр шины <b><u>contract_base</u></b> (номер кредитной линии)
        /// </summary>
        [FromQuery(Name = "baseContractNumber")]
        public string BaseContractNumber { get; set; }

        /// <summary>
        /// Параметр шины <b><u>application_insurance</u></b> (признак использования страховки)
        /// </summary>
        [FromQuery(Name = "insurance")]
        public bool Insurance { get; set; }
    }
}
