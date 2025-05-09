using System;
namespace Pawnshop.Web.Engine
{
    public static class DateTimeExtensions
    {
        public static int MonthDifference(this DateTime value, DateTime to)
        {
            return Math.Abs((value.Month - to.Month) + 12 * (value.Year - to.Year));
        }

        public static DateTime DateWithLastDayOfMonth(this DateTime value)
        {
            return new DateTime(value.Year, value.Month, DateTime.DaysInMonth(value.Year, value.Month));
        }

        public static int MonthDifferenceForMaturityDate(this DateTime value, DateTime to)
        {
            return ((value - to).Days - (5 * ((value - to).Days / 365))) / 30;
        }
    }
}
