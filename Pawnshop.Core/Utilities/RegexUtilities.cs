using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Pawnshop.Core.Utilities
{
    public static class RegexUtilities
    {
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                // Normalize the domain
                email = Regex.Replace(email, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));

                // Examines the domain part of the email and normalizes it.
                string DomainMapper(Match match)
                {
                    // Use IdnMapping class to convert Unicode domain names.
                    var idn = new IdnMapping();
                    // Pull out and process domain name (throws ArgumentException on invalid)0
                    string domainName = idn.GetAscii(match.Groups[2].Value);
                    return match.Groups[1].Value + domainName;
                }
            }
            catch (RegexMatchTimeoutException e)
            {
                return false;
            }
            catch (ArgumentException e)
            {
                return false;
            }

            try
            {
                return Regex.IsMatch(email,
                    @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            try
            {
                return Regex.IsMatch(phone,
                    Constants.PHONE_REGEX,
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public static bool IsValidKazakhstanPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            try
            {
                return Regex.IsMatch(phone,
                    Constants.KAZAKHSTAN_PHONE_REGEX,
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        public static string GetNumbers(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            try
            {
                return Regex.Replace(value, "[^\\d]", "");
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
