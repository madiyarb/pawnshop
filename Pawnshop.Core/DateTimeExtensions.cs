using System;

namespace Pawnshop.Core
{
    public static class DateTimeExtensions
    {
        public static DateTime MaxOf(this DateTime instance, DateTime dateTime)
        {
            return instance > dateTime ? instance : dateTime;
        }

        public static DateTime MinOf(this DateTime instance, DateTime dateTime)
        {
            return instance < dateTime ? instance : dateTime;
        }

        public static bool CorrectValueForDb(this DateTime dateTime)
        {
            return new DateTime(1753, 1, 1) < dateTime;
        }

        public static DateTime? CompareResultForDb(this DateTime? date1, DateTime? date2)
        {
            return !date1.HasValue || !date1.Value.CorrectValueForDb() ? date2 : date1;
        }
    }
}