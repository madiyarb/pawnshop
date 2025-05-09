using System;

namespace Pawnshop.Core.Extensions
{
    public static class StringExtensions
    {
        public static string IsNullOrEmpty(this string str1, string str2) =>
            string.IsNullOrEmpty(str1) ? str2 : str1;

        public static string ToString(this DateTime? dt, string format) =>
            dt == null ? "n/a" : ((DateTime)dt).ToString(format);
    }
}
