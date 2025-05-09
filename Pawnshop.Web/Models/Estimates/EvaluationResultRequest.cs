using Newtonsoft.Json;

namespace Pawnshop.Web.Models.Estimates
{
    public class EvaluationResultRequest
    {
        [JsonProperty("app_id")]
        public string ApplicatinId { get; set; }

        [JsonProperty("success")]
        public bool Success { get; set; }

        /// <summary>
        /// Сумма оценки
        /// </summary>
        [JsonProperty("evaluated_amount")]
        public decimal EvaluatedAmount { get; set; }

        /// <summary>
        /// Максимально возможная сумма кредитования
        /// </summary>
        [JsonProperty("issued_amount")]
        public decimal IssuedAmount { get; set; }

        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("author")]
        public string AuthorName { get; set; }
    }
}
