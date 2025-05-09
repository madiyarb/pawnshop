using System;
using System.Collections.Generic;
using System.Text;

namespace Pawnshop.Services.Models.Calculation.APR
{
    internal class Instalment
    {
        public double Amount { get; set; }
        public double DaysAfterFirstAdvance { get; set; }

        internal double Calculate(double APR)
        {
            double divisor = Math.Pow(1 + APR, DaysToYears);
            var sum = Amount / divisor;
            return sum;
        }

        private double DaysToYears
        {
            get
            {
                return DaysAfterFirstAdvance / 365d;
            }
        }
    }
}
