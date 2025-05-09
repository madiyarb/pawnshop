using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Data.Models.Contracts.Penalty
{
    public class PenaltyRateComparer : IEqualityComparer<PenaltyRates>
    {
        public bool Equals(PenaltyRates x, PenaltyRates y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                return false;

            return x.Date == y.Date;
        }

        public int GetHashCode(PenaltyRates penaltyRates)
        {
            if (Object.ReferenceEquals(penaltyRates, null)) return 0;

            int hashPenaltyRatesDate = penaltyRates.Date.GetHashCode();

            return hashPenaltyRatesDate;
        }
    }
}
