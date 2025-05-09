using Pawnshop.Core;

namespace Pawnshop.Data.Models.LoanSettings
{
    public class LoanPercentSettingPeriod : IEntity
    {
        public int Id { get; set; }
        public int RestrictionId { get; set; }
        public int PaymentNumber { get; set; }

        /// <summary>
        /// Процент кредита
        /// </summary>
        public decimal LoanPercent { get; set; }

        /// <summary>
        /// Процент штрафа за просрочку кредита
        /// </summary>
        public decimal PenaltyPercent { get; set; }
    }
}
