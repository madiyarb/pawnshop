using Newtonsoft.Json;

namespace Pawnshop.Data.Models.Base
{
    /// <summary>
    /// Задолженность
    /// </summary>
    public class DebtInfo
    {
        /// <summary>
        /// Основной долг
        /// </summary>
        public decimal PrincipalDebt { get; set; }

        /// <summary>
        /// Проценты
        /// </summary>
        public decimal Profit { get; set; }

        /// <summary>
        /// Пеня
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal Penalty { get; set; }
    }
}
