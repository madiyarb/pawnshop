using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pawnshop.Services.Models.Calculation.APR
{
    public class APRCalculator
    {
        public APRCalculator(double firstAdvance)
            : this(firstAdvance, new List<Instalment>(), new List<Instalment>())
        {
        }

        internal APRCalculator(double firstAdvance, List<Instalment> advances, List<Instalment> payments)
        {
            _Advances = advances;
            _Payments = payments;
            _Advances.Add(new Instalment() { Amount = firstAdvance, DaysAfterFirstAdvance = 0 });
        }

        public double SinglePaymentCalculation(double payment, int DaysAfterAdvance)
        {
            return Math.Round((Math.Pow(_Advances[0].Amount / payment, (-365 / DaysAfterAdvance)) - 1) * 100, 1, MidpointRounding.AwayFromZero);
        }

        public double Calculate(double guess = 0)
        {
            double rateToTry = guess / 100;
            double difference = 1;
            double amountToAdd = 0.0001d;

            while (difference != 0)
            {
                double advances = _Advances.Sum(a => a.Calculate(rateToTry));
                double payments = _Payments.Sum(p => p.Calculate(rateToTry));

                difference = payments - advances;

                if (difference <= 0.0000001 && difference >= -0.0000001)
                {
                    break;
                }

                if (difference > 0)
                {
                    amountToAdd = amountToAdd * 2;
                    rateToTry = rateToTry + amountToAdd;
                }

                else
                {
                    amountToAdd = amountToAdd / 2;
                    rateToTry = rateToTry - amountToAdd;
                }
            }

            return Math.Round(rateToTry * 100, 2);
        }

        public void AddInstalment(double amount, double daysAfterFirstAdvance, InstalmentType instalmentType = InstalmentType.Payment)
        {
            var instalment = new Instalment() { Amount = amount, DaysAfterFirstAdvance = daysAfterFirstAdvance };
            switch (instalmentType)
            {
                case InstalmentType.Payment:
                    _Payments.Add(instalment);
                    break;
                case InstalmentType.Advance:
                    _Advances.Add(instalment);
                    break;
            }
        }

        private static double getDaysBewteenInstalments(InstalmentFrequency instalmentFrequency)
        {
            switch (instalmentFrequency)
            {
                case InstalmentFrequency.Daily:
                    return 1;
                case InstalmentFrequency.Weekly:
                    return 7;
                case InstalmentFrequency.Fortnightly:
                    return 14;
                case InstalmentFrequency.FourWeekly:
                    return 28;
                case InstalmentFrequency.Monthly:
                    return 365 / 12;
                case InstalmentFrequency.Quarterly:
                    return 365 / 4;
                case InstalmentFrequency.Annually:
                    return 365;
            }
            return 1;
        }

        public void AddRegularInstalments(double amount, int numberOfInstalments, InstalmentFrequency instalmentFrequency, double daysAfterFirstAdvancefirstInstalment = 0)
        {
            double daysBetweenInstalments = getDaysBewteenInstalments(instalmentFrequency);
            if (daysAfterFirstAdvancefirstInstalment == 0)
            {
                daysAfterFirstAdvancefirstInstalment = daysBetweenInstalments;
            }
            for (int i = 0; i < numberOfInstalments; i++)
            {
                _Payments.Add(new Instalment() { Amount = amount, DaysAfterFirstAdvance = daysAfterFirstAdvancefirstInstalment + (daysBetweenInstalments * i) });
            }
        }

        private readonly List<Instalment> _Advances;
        private readonly List<Instalment> _Payments;
    }
}
