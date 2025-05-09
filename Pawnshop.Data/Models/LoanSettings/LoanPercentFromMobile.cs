using Newtonsoft.Json;
using Pawnshop.Data.Models.Localizations;
using System.Collections.Generic;

namespace Pawnshop.Data.Models.LoanSettings
{
    public class LoanPercentFromMobile
    {
        [JsonProperty("title")]
        public List<LocalizationView> Title { get; set; }

        [JsonProperty("description")]
        public List<LocalizationView> Description { get; set; }

        [JsonProperty("product_id")]
        public int ProductId { get; set; }

        [JsonProperty("insurance")]
        public int Insurance { get; set; }

        [JsonProperty("percent")]
        public decimal Percent { get; set; }

        [JsonProperty("min_percent")]
        public decimal MinPercent { get; set; }

        [JsonProperty("max_percent")]
        public decimal MaxPercent { get; set; }

        [JsonProperty("min_sum")]
        public decimal MinSum { get; set; }

        [JsonProperty("max_sum")]
        public decimal MaxSum { get; set; }

        [JsonProperty("min_tranche_sum", NullValueHandling = NullValueHandling.Ignore)]
        public int? MinTrancheSum { get; set; }

        [JsonProperty("min_month")]
        public int MinMonth { get; set; }

        [JsonProperty("max_month")]
        public int MaxMonth { get; set; }

        [JsonProperty("schedule_type")]
        public int ScheduleType { get; set; }
    }
}
