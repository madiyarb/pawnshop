using Newtonsoft.Json;
using System;

namespace Pawnshop.Data.Models.AbsOnline
{
    /// <summary>
    /// Детали графика платежей
    /// </summary>
    public sealed class AbsOnlinePaymentScheduleViewModel
    {
        /// <summary>
        /// Параметр шины <b><u>date</u></b> (дата платежа)
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Параметр шины <b><u>number</u></b> (номер платежа)
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Number { get; set; }

        /// <summary>
        /// Параметр шины <b><u>pay_value</u></b> или <b><u>value</u></b>(сумма платежа)
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Параметр шины <b><u>od_balance</u></b> (остаток основного долга)
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? PrincipalDebtLeft { get; set; }

        /// <summary>
        /// Параметр шины <b><u>od_value</u></b> или <b><u>od</u></b>(сумма платежа основного долга)
        /// </summary>
        public decimal PrincipalDebt { get; set; }

        /// <summary>
        /// Параметр шины <b><u>percent</u></b> (процентная ставка)
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Percent { get; set; }

        /// <summary>
        /// Параметр шины <b><u>percent_value</u></b> (сумма процентов)
        /// </summary>
        public decimal ProfitAmount { get; set; }

        /// <summary>
        /// Параметр шины <b><u>status</u></b> (статус)
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        /// <summary>
        /// Параметр шины <b><u>fine</u></b> (сумма штрафа)
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? FineAmount { get; set; }

        /// <summary>
        /// Параметр шины <b><u>penalties</u></b> (сумма пени)
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? PenaltyAmount { get; set; }

        /// <summary>
        /// Параметр шины <b><u>balance</u></b> (???)
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? Balance { get; set; }
    }
}
