using System;

namespace Pawnshop.Core.Extensions
{
    public static class DateTimeExtensions
    {
        public static int MonthDifferenceKdn(this DateTime value, DateTime to)
        {
            var monthDiff = Math.Abs((value.Month - to.Month) + 12 * (value.Year - to.Year));

            if (monthDiff == 0)
                return 1;

            return monthDiff;
        }

        public static int MonthDifference(this DateTime value, DateTime to)
        {
            return Math.Abs((value.Month - to.Month) + 12 * (value.Year - to.Year));
        }

        public static int MonthDifferenceWithChangedDate(this DateTime start, DateTime end)
        {
            var diff = Math.Abs((start.Month - end.Month) + 12 * (start.Year - end.Year));
            start = start.AddMonths(diff);
            if ((end - start).TotalDays >= 15)
            {
                diff++;
            }
            return diff;
        }

        public static int MonthDifferenceWithDays(this DateTime value, DateTime to)
        {
            var start = value > to ? to : value;
            var end = value < to ? to : value;

            var monthDiff = Math.Abs((start.Month - end.Month) + 12 * (start.Year - end.Year));
            var dayDiff = end.Day - start.Day;

            if (dayDiff < 0)
                return monthDiff - 1;

            return monthDiff;
        }

        public static int DayDifference(this DateTime value, DateTime to)
        {
            return Math.Abs((value.Date - to.Date).Days);
        }
    }
}
