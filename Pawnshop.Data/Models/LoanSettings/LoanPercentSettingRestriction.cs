using System.Collections.Generic;
using Pawnshop.Core;

namespace Pawnshop.Data.Models.LoanSettings
{
    public class LoanPercentSettingRestriction : IEntity
    {
        public int Id { get; set; }
        public int SettingId { get; set; }

        /// <summary>
        /// Ссуда от
        /// </summary>
        public int LoanCostFrom { get; set; }

        /// <summary>
        /// Ссуда до
        /// </summary>
        public int LoanCostTo { get; set; }

        /// <summary>
        /// Процент кредита
        /// </summary>
        public decimal LoanPercent { get; set; }

        /// <summary>
        /// Процент штрафа за просрочку кредита
        /// </summary>
        public decimal PenaltyPercent { get; set; }

        public bool HasPeriods { get; set; }

        public List<LoanPercentSettingPeriod> Periods { get; set; }
    }
}
