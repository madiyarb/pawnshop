using System;
using System.Collections.Generic;
using System.Text;
using Pawnshop.AccountingCore.Models;

namespace Pawnshop.Data.Models.Contracts
{
    public class PercentPaymentTypeToLoanPeriodConverter
    {
        public int Convert(PercentPaymentType percentPaymentType)
        {
            switch (percentPaymentType)
            {
                case PercentPaymentType.EndPeriod:
                    return 30;
                case PercentPaymentType.AnnuityTwelve:
                    return 365;
                case PercentPaymentType.AnnuityTwentyFour:
                    return 730;
                case PercentPaymentType.AnnuityThirtySix:
                    return 1095;
                default:
                    throw new ArgumentOutOfRangeException(nameof(PercentPaymentType));
            }
        }
    }
}
